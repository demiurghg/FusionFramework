using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Core.Content;
using Fusion.Core.Development;
using Fusion.Engine.Common;

namespace SceneDemo {
	public class SceneDemo : Game {

		/// <summary>
		/// SceneDemo constructor
		/// </summary>
		public SceneDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects		=	false;
			Parameters.VSyncInterval	=	0;
			Parameters.MsaaLevel		=	4;

			//Parameters.StereoMode	=	StereoMode.NV3DVision;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );

			//	load configuration :
			LoadConfiguration();

			//	Force to enable free camera.
			GetService<Camera>().Config.FreeCamEnabled	=	true;

			//	make configuration saved on exit
			Exiting += FusionGame_Exiting;
			InputDevice.KeyDown += InputDevice_KeyDown;
		}



		class Material {
			public Texture2D	Texture;
		}

		class Context {
			public Matrix	View;
			public Matrix	Projection;
			public Vector4	ViewPosition;
		}



		struct CBData {
			public Matrix	Projection;
			public Matrix	View;
			public Matrix	World;
			public Vector4	ViewPos;
		}


		enum RenderFlags {
			None,
		}


		Scene			scene;

		VertexBuffer[]	vertexBuffers;
		IndexBuffer[]	indexBuffers;
		Texture2D[]		textures;
		Matrix[]		worldMatricies;

		ConstantBuffer	constBuffer;
		Ubershader		uberShader;
		StateFactory	factory;
		CBData			constData;

		/// <summary>
		/// Add services :
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			LoadContent();
			Reloading += (s,e) => LoadContent();
			GetService<Camera>().FreeCamPosition = Vector3.Up * 10;

			constBuffer	=	new ConstantBuffer( GraphicsDevice, typeof(CBData) );
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
			SafeDispose( ref factory );
			SafeDispose( ref vertexBuffers );
			SafeDispose( ref indexBuffers );

			uberShader	=	Content.Load<Ubershader>("render");

			factory		=	new StateFactory( 
								uberShader, 
								typeof(RenderFlags), 
								Primitive.TriangleList, 
								VertexColorTextureNormal.Elements,
								BlendState.Opaque,
								RasterizerState.CullCW,
								DepthStencilState.Default 
							);

			scene		=	Content.Load<Scene>(@"Scenes\testScene");


			vertexBuffers	=	scene.Meshes
							.Select( m => VertexBuffer.Create( GraphicsDevice, m.Vertices.Select( v => VertexColorTextureNormal.Convert(v) ).ToArray() ) )
							.ToArray();

			indexBuffers	=	scene.Meshes
							.Select( m => IndexBuffer.Create( GraphicsDevice, m.GetIndices() ) )
							.ToArray();

			textures		=	scene.Materials
							.Select( mtrl => Content.Load<Texture2D>( mtrl.TexturePath ) )
							.ToArray();

			worldMatricies	=	new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );

			//Log.Message("{0}", scene.Nodes.Count( n => n.MeshIndex >= 0 ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref constBuffer );
				SafeDispose( ref factory );
				SafeDispose( ref vertexBuffers );
				SafeDispose( ref indexBuffers );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Handle keys for each demo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
				//DevCon.Show(this);
			}

			if (e.Key == Keys.F2) {
				Parameters.ToggleVSync();
			}

			if (e.Key == Keys.F5) {
				Reload();
			}

			if (e.Key == Keys.F12) {
				GraphicsDevice.Screenshot();
			}

			if (e.Key == Keys.Escape) {
				Exit();
			}
		}



		/// <summary>
		/// Save configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void FusionGame_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			GameTime.AveragingFrameCount = 60;

			ds.Add( Color.Orange, "FPS {0}", gameTime.AverageFrameRate );
			ds.Add( Color.Orange, "FT  {0}", gameTime.AverageFrameTime );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );

			var cam	=	GetService<Camera>();
			var dr	=	GetService<DebugRender>();
			dr.View			=	cam.GetViewMatrix( StereoEye.Mono );
			dr.Projection	=	cam.GetProjectionMatrix( StereoEye.Mono );

			dr.DrawGrid(10);

			base.Update( gameTime );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			var cam	=	GetService<Camera>();
			cam.Config.FreeCamEnabled =  false;

			GraphicsDevice.ClearBackbuffer( Color.CornflowerBlue, 1, 0 );

			constData.View			=	cam.GetViewMatrix( stereoEye );
			constData.Projection	=	cam.GetProjectionMatrix( stereoEye );
			constData.ViewPos		=	cam.GetCameraPosition4( stereoEye );
			constData.World			=	Matrix.Identity;

			for (int j = 0; j<40; j++) {

				GraphicsDevice.PipelineState			=	factory[0];
				GraphicsDevice.PixelShaderSamplers[0]	=	SamplerState.AnisotropicWrap;
				GraphicsDevice.PixelShaderConstants[0]	=	constBuffer;
				GraphicsDevice.VertexShaderConstants[0]	=	constBuffer;


				for (int i=0; i<scene.Nodes.Count; i++) {

					int meshId	=	scene.Nodes[i].MeshIndex;

					if (meshId<0) {
						continue;
					}

					constData.World	=	worldMatricies[ i ];
					constBuffer.SetData( constData );

					GraphicsDevice.SetupVertexInput( vertexBuffers[meshId], indexBuffers[meshId] );

					foreach ( var subset in scene.Meshes[meshId].Subsets ) {
						GraphicsDevice.PixelShaderResources[0]	=	textures[ subset.MaterialIndex ];
						GraphicsDevice.DrawIndexed( subset.PrimitiveCount * 3, subset.StartPrimitive * 3, 0 );
					}
				}
			}

			base.Draw( gameTime, stereoEye );
		}
	}
}
