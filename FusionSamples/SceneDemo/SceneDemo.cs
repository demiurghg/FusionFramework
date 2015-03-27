using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Development;

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



		class MySceneDrawer : SceneDrawer<Context,Material,VertexColorTextureNormal> {

			ConstantBuffer	constBuffer;
			Ubershader		uberShader;
			StateFactory	factory;
			
			CBData			constData;


			public MySceneDrawer ( GraphicsDevice device, Scene scene ) : base(device, scene)
			{
				constBuffer	=	new ConstantBuffer( GraphicsDevice, typeof(CBData) );
				uberShader	=	Game.Content.Load<Ubershader>("render");
				factory		=	new StateFactory( uberShader, typeof(RenderFlags), Primitive.TriangleList, VertexColorTextureNormal.Elements );
			}


			protected override void Dispose ( bool disposing )
			{
				if (disposing) {
					SafeDispose( ref factory );
					SafeDispose( ref constBuffer );
				}
				base.Dispose( disposing );
			}


			public override Material Convert ( MeshMaterial material )
			{
				return new Material(){ Texture = Game.Content.Load<Texture2D>( material.TexturePath ) };
			}


			public override VertexColorTextureNormal Convert ( MeshVertex vertex )
			{
				return VertexColorTextureNormal.Convert( vertex );
			}


			public override Context Prepare ( GameTime gameTime, StereoEye stereoEye )
			{
				var cam = Game.GetService<Camera>();

				return new Context() {
					View			=	cam.GetViewMatrix( stereoEye ),
					Projection		=	cam.GetProjectionMatrix( stereoEye ),
					ViewPosition	=	cam.GetCameraPosition4( stereoEye )
				};
			}


			public override void PrepareMesh ( Context context, Mesh mesh, VertexBuffer vb, IndexBuffer ib )
			{
				GraphicsDevice.SetupVertexInput( vb, ib );
			}


			public override bool PrepareNode ( Context context, Node node, Matrix worldMatrix )
			{
				constData.View			=	context.View;
				constData.Projection	=	context.Projection;
				constData.ViewPos		=	context.ViewPosition;
				constData.World			=	worldMatrix;

				constBuffer.SetData( constData );

				GraphicsDevice.PipelineState			=	factory[0];
				GraphicsDevice.PixelShaderSamplers[0]	=	SamplerState.AnisotropicWrap;
				GraphicsDevice.VertexShaderConstants[0]	=	constBuffer;

				return true;
			}


			public override void DrawSubset ( Context context, MeshSubset subset, Material material )
			{
				GraphicsDevice.PixelShaderResources[0]	=	material.Texture;
				GraphicsDevice.DrawIndexed( subset.PrimitiveCount * 3, subset.StartPrimitive, 0 );
			}
		}



		Scene			scene;
		MySceneDrawer	sceneDrawer;


		/// <summary>
		/// Add services :
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			LoadContent();
			Reloading += (s,e) => LoadContent();
			GetService<Camera>().FreeCamPosition = Vector3.Up * 10;
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
			SafeDispose( ref sceneDrawer );

			scene		=	Content.Load<Scene>(@"Scenes\testScene");
			sceneDrawer	=	new MySceneDrawer( GraphicsDevice, scene );

			Log.Message("{0}", scene.Nodes.Count( n => n.MeshIndex >= 0 ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref sceneDrawer );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Handle keys for each demo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Fusion.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
				DevCon.Show(this);
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

			GraphicsDevice.ClearBackbuffer( Color.CornflowerBlue, 1, 0 );


			sceneDrawer.EvaluateScene();

			for (int j = 0; j<30; j++) {
				sceneDrawer.Draw( gameTime, stereoEye );
			}

			base.Draw( gameTime, stereoEye );
		}
	}
}
