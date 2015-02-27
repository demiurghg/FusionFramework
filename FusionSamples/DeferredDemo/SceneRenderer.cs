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

namespace DeferredDemo {

	class SceneRenderer : GameService, LightRenderer.IShadowCaster {

		Scene			scene;
		ConstantBuffer	constBuffer;
		Ubershader		surfaceShader;

		Texture2D		defaultDiffuse	;
		Texture2D		defaultSpecular	;
		Texture2D		defaultNormalMap;
		Texture2D		defaultEmission	;


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
			scene =	Game.Content.Load<Scene>(@"Scenes\testScene");

			foreach ( var mtrl in scene.Materials ) {

				Log.Message( "...{0} {1}", mtrl.Name, mtrl.TexturePath );

				var surf		=	new SurfaceProperties();
				surf.Diffuse	=	LoadTexture2D( mtrl.TexturePath, ""			, defaultDiffuse );

				surf.Specular	=	LoadTexture2D( mtrl.TexturePath, "_spec"	, defaultSpecular );
				surf.NormalMap	=	LoadTexture2D( mtrl.TexturePath, "_local"	, defaultNormalMap );
				surf.Emission	=	LoadTexture2D( mtrl.TexturePath, "_glow"	, defaultEmission );

				mtrl.Tag		=	surf;
			}

			scene.Bake( Game.GraphicsDevice, VertexColorTextureTBN.Bake );

			surfaceShader	=	Game.Content.Load<Ubershader>("surface");
			surfaceShader.Map( typeof(SurfaceFlags) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
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
			var device			= Game.GraphicsDevice;

			device.ResetStates();

			var cbData			= new CBSurfaceData();

			var viewPosition	= Matrix.Invert( view ).TranslationVector;
			var worldMatricies	= new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );


			device.SetTargets( depthBuffer.Surface, hdrTarget.Surface, diffuse.Surface, specular.Surface, normals.Surface );


			surfaceShader.SetPixelShader((int)SurfaceFlags.GBUFFER);
			surfaceShader.SetVertexShader(0);

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

				device.RasterizerState		= RasterizerState.CullNone ;
				device.DepthStencilState	= DepthStencilState.Default ;
				device.BlendState			= BlendState.Opaque ;
				device.PixelShaderConstants[0]	= constBuffer ;
				device.VertexShaderConstants[0]	= constBuffer ;
				device.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;

				mesh.SetupVertexInput();

				foreach ( var subset in mesh.Subsets ) {

					var surf = scene.Materials[ subset.MaterialIndex ].Tag as SurfaceProperties;

					device.PixelShaderResources[0]	=	surf.Diffuse.SRgb;
					device.PixelShaderResources[1]	=	surf.Specular;
					device.PixelShaderResources[2]	=	surf.NormalMap;
					device.PixelShaderResources[3]	=	surf.Emission.SRgb;

					mesh.Draw( subset.StartPrimitive, subset.PrimitiveCount );
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
			var device			= Game.GraphicsDevice;

			var cbData			= new CBSurfaceData();

			var viewPosition	= Matrix.Invert( shadowRenderCtxt.ShadowView ).TranslationVector;
			var worldMatricies	= new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );

			device.SetTargets( shadowRenderCtxt.DepthBuffer, shadowRenderCtxt.ColorBuffer );
			device.SetViewport( shadowRenderCtxt.ShadowViewport );

			surfaceShader.SetPixelShader((int)SurfaceFlags.SHADOW);
			surfaceShader.SetVertexShader(0);

			device.RasterizerState			= RasterizerState.CullNone ;
			device.DepthStencilState		= DepthStencilState.Default ;
			device.BlendState				= BlendState.Opaque ;
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

				mesh.SetupVertexInput();
				mesh.Draw();
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
