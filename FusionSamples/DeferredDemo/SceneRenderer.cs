using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Development;
using System.Threading;

namespace DeferredDemo {

	class SceneRenderer : GameService, LightRenderer.IShadowCaster {

		Scene			scene;
		ConstantBuffer	constBuffer;
		Ubershader		surfaceShader;
		StateFactory	factory;

		Texture2D		defaultDiffuse	;
		Texture2D		defaultSpecular	;
		Texture2D		defaultNormalMap;
		Texture2D		defaultEmission	;

		VertexBuffer[]		vertexBuffers;
		IndexBuffer[]		indexBuffers;
		SurfaceProperties[]	surfaceProps;


		struct CBSurfaceData {
			public Matrix	Projection;
			public Matrix	View;
			public Matrix	World;
			public Vector4	ViewPos			;
			public Vector4	BiasSlopeFar	;
		}



		class SurfaceProperties {
			public Texture2D	Diffuse;
			public Texture2D	Specular;
			public Texture2D	NormalMap;
			public Texture2D	Emission;
		}


		enum SurfaceFlags {
			GBUFFER = 1,
			SHADOW = 2,
		}


		readonly string scenePath;



		/// <summary>
		/// Creates instance of scene renderer
		/// </summary>
		/// <param name="game"></param>
		public SceneRenderer ( Game game, string scenePath ) : base(game)
		{
			this.scenePath = scenePath;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			base.Initialize();

			constBuffer		=	new ConstantBuffer( Game.GraphicsDevice, typeof(CBSurfaceData) );

			defaultDiffuse	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultDiffuse.SetData( Enumerable.Range(0,16).Select( i => Color.Gray ).ToArray() );

			defaultSpecular	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultSpecular.SetData( Enumerable.Range(0,16).Select( i => new Color(0,128,0,255) ).ToArray() );

			defaultNormalMap	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultNormalMap.SetData( Enumerable.Range(0,16).Select( i => new Color(128,128,255,255) ).ToArray() );

			defaultEmission	=	new Texture2D( Game.GraphicsDevice, 4,4, ColorFormat.Rgba8, false );
			defaultEmission.SetData( Enumerable.Range(0,16).Select( i => Color.Black ).ToArray() );
			

			LoadContent();

			Game.Reloading	+= (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="defaultTexture"></param>
		/// <returns></returns>
		Texture2D LoadTexture2D( string path, string postfix, Texture2D defaultTexture )
		{
			//if ( path==null) {
			//	return defaultTexture;
			//}

			path = Path.Combine( Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) ) + postfix;

			if ( Game.Content.Exists( path ) ) {
				return Game.Content.Load<Texture2D>( path );
			} else {
				return defaultTexture;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );
			SafeDispose( ref vertexBuffers );
			SafeDispose( ref indexBuffers );

			surfaceShader	=	Game.Content.Load<Ubershader>("surface");
			factory			=	new StateFactory( surfaceShader, typeof(SurfaceFlags), Primitive.TriangleList, VertexInputElement.FromStructure<VertexColorTextureTBN>() );


			scene =	Game.Content.Load<Scene>(@"Scenes\testScene");

			vertexBuffers	=	scene.Meshes
							.Select( mesh => VertexBuffer.Create( Game.GraphicsDevice, mesh.Vertices.Select( v => VertexColorTextureTBN.Convert( v ) ).ToArray() ) )
							.ToArray();

			indexBuffers	=	scene.Meshes
							.Select( mesh => IndexBuffer.Create( Game.GraphicsDevice, mesh.GetIndices() ) )
							.ToArray();


			surfaceProps	=	scene.Materials
							.Select( mtrl => new SurfaceProperties() {
								Diffuse		=	LoadTexture2D( mtrl.TexturePath, "|srgb"		, defaultDiffuse ),
								Specular	=	LoadTexture2D( mtrl.TexturePath, "_spec"		, defaultSpecular ),
								NormalMap	=	LoadTexture2D( mtrl.TexturePath, "_local"		, defaultNormalMap ),
								Emission	=	LoadTexture2D( mtrl.TexturePath, "_glow|srgb"	, defaultEmission ),
							} )
							.ToArray();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				SafeDispose( ref vertexBuffers );
				SafeDispose( ref indexBuffers );

				SafeDispose( ref defaultDiffuse		);
				SafeDispose( ref defaultSpecular	);
				SafeDispose( ref defaultNormalMap	);
				SafeDispose( ref defaultEmission	);

				SafeDispose( ref constBuffer	);
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			base.Update( gameTime );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		/// <param name="projection"></param>
		/// <param name="depthBuffer"></param>
		/// <param name="hdrTarget"></param>
		/// <param name="diffuse"></param>
		/// <param name="specular"></param>
		/// <param name="normalMap"></param>
		public void RenderGBuffer ( Matrix view, Matrix projection, DepthStencil2D depthBuffer, RenderTarget2D hdrTarget, RenderTarget2D diffuse, RenderTarget2D specular, RenderTarget2D normals )
		{
			if (surfaceShader==null) {
				return;
			}

			var device			= Game.GraphicsDevice;

			device.ResetStates();

			var cbData			= new CBSurfaceData();

			var viewPosition	= Matrix.Invert( view ).TranslationVector;
			var worldMatricies	= new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );


			device.SetTargets( depthBuffer.Surface, hdrTarget.Surface, diffuse.Surface, specular.Surface, normals.Surface );


			device.PipelineState	=	factory[ (int)SurfaceFlags.GBUFFER ];

			for ( int i=0; i<scene.Nodes.Count; i++ ) {

				var node = scene.Nodes[i];
				
				if (node.MeshIndex==-1) {
					continue;
				}

				var mesh = scene.Meshes[ node.MeshIndex ];

				cbData.Projection	=	projection;
				cbData.View			=	view;
				cbData.World		=	worldMatricies[ i ];
				cbData.ViewPos		=	new Vector4(viewPosition,0);

				constBuffer.SetData( cbData );

				device.PixelShaderConstants[0]	= constBuffer ;
				device.VertexShaderConstants[0]	= constBuffer ;
				device.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;

				device.SetupVertexInput( vertexBuffers[ node.MeshIndex ], indexBuffers[ node.MeshIndex ] );

				foreach ( var subset in mesh.Subsets ) {

					var surf = surfaceProps[ subset.MaterialIndex ];
					
					device.PixelShaderResources[0]	=	surf.Diffuse;
					device.PixelShaderResources[1]	=	surf.Specular;
					device.PixelShaderResources[2]	=	surf.NormalMap;
					device.PixelShaderResources[3]	=	surf.Emission;

					device.DrawIndexed( subset.PrimitiveCount * 3, subset.StartPrimitive * 3, 0 );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="shadowView"></param>
		/// <param name="shadowProjection"></param>
		/// <param name="depthBuffer"></param>
		public void RenderShadowMapCascade ( LightRenderer.ShadowRenderContext shadowRenderCtxt )
		{
			if (surfaceShader==null) {
				return;
			}

			var device			= Game.GraphicsDevice;

			var cbData			= new CBSurfaceData();

			var viewPosition	= Matrix.Invert( shadowRenderCtxt.ShadowView ).TranslationVector;
			var worldMatricies	= new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );

			device.SetTargets( shadowRenderCtxt.DepthBuffer, shadowRenderCtxt.ColorBuffer );
			device.SetViewport( shadowRenderCtxt.ShadowViewport );

			device.PipelineState	=	factory[ (int)SurfaceFlags.SHADOW ];

			device.PixelShaderConstants[0]	= constBuffer ;
			device.VertexShaderConstants[0]	= constBuffer ;
			device.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;


			cbData.Projection	=	shadowRenderCtxt.ShadowProjection;
			cbData.View			=	shadowRenderCtxt.ShadowView;
			cbData.BiasSlopeFar	=	new Vector4( shadowRenderCtxt.DepthBias, shadowRenderCtxt.SlopeBias, shadowRenderCtxt.FarDistance, 0 );


			for ( int i=0; i<scene.Nodes.Count; i++ ) {

				var node = scene.Nodes[i];
				
				if (node.MeshIndex==-1) {
					continue;
				}

				var mesh = scene.Meshes[ node.MeshIndex ];

				cbData.World	=	worldMatricies[ i ];

				constBuffer.SetData( cbData );

				device.SetupVertexInput( vertexBuffers[ node.MeshIndex ], indexBuffers[ node.MeshIndex ] );
				device.DrawIndexed( mesh.IndexCount, 0, 0 );
			}
		}
		
		
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="shadowView"></param>
		/// <param name="shadowProjection"></param>
		/// <param name="depthBuffer"></param>
		public void RenderShadowMapSpot	( LightRenderer.ShadowRenderContext shadowRenderCtxt )
		{
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
		}
	}
}
