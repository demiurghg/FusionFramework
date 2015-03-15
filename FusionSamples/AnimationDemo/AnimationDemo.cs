using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;

using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;

namespace AnimationDemo {
	public class AnimationDemo : Game {

		class Material {
		}

		class Context {
			public Matrix  View;
			public Matrix  Projection;
			public Vector4 ViewPosition;
		}

		Scene			scene;
		ConstantBuffer	constBuffer;
		Ubershader		uberShader;
		StateFactory	factory;
		SceneDrawer<VertexColorTextureNormal, Material>	sceneDrawer;


		struct CBData {
			public Matrix	Projection;
			public Matrix	View;
			public Matrix	World;
			public Vector4	ViewPos;
		}


		enum RenderFlags {
			None,
		}


		/// <summary>
		/// AnimationDemo constructor
		/// </summary>
		public AnimationDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;
			Parameters.MsaaLevel = 4;
			//Parameters.FullScreen	=	true;

			//	uncomment to enable debug graphics device:
			//	(MS Platform SDK must be installed)
			//	Parameters.UseDebugDevice	=	true;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 0, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );

			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += Game_Exiting;
		}


		/// <summary>
		/// Initializes game :
		/// </summary>
		protected override void Initialize ()
		{
			//	initialize services :
			base.Initialize();

			//	add keyboard handler :
			InputDevice.KeyDown += InputDevice_KeyDown;

			//	load content & create graphics and audio resources here:
			constBuffer		=	new ConstantBuffer( GraphicsDevice, typeof(CBData) );

			LoadContent();
			Reloading += (s,e) => LoadContent();

			GetService<Camera>().FreeCamPosition = Vector3.Up * 5 + Vector3.BackwardRH * 10;
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
			SafeDispose( ref factory );
			SafeDispose( ref sceneDrawer );

			scene		=	Content.Load<Scene>(@"test");
			sceneDrawer	=	new SceneDrawer<VertexColorTextureNormal,Material>( 
							GraphicsDevice, scene, 
							VertexColorTextureNormal.Bake, 
							(m) => new Material() );

			uberShader	=	Content.Load<Ubershader>("render");
			factory		=	new StateFactory( GraphicsDevice, typeof(RenderFlags), uberShader, VertexColorTextureNormal.Elements );

			Log.Message("{0}", scene.Nodes.Count( n => n.MeshIndex >= 0 ) );
		}



		/// <summary>
		/// Disposes game
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				//	dispose disposable stuff here
				//	Do NOT dispose objects loaded using ContentManager.
				SafeDispose( ref sceneDrawer );
				SafeDispose( ref factory );
				SafeDispose( ref constBuffer );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Handle keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Fusion.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
				DevCon.Show( this );
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
		/// Saves configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Game_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}



		/// <summary>
		/// Updates game
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			base.Update( gameTime );

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
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
			frame += gameTime.ElapsedSec * 24;
		}


		float frame = 0;


		Context Prepare ( GameTime gameTime, StereoEye stereoEye )
		{
			var cam = GetService<Camera>();

			return new Context() {
				View			=	cam.GetViewMatrix( stereoEye ),
				Projection		=	cam.GetProjectionMatrix( stereoEye ),
				ViewPosition	=	new Vector4( cam.GetCameraMatrix( stereoEye ).TranslationVector, 1 )
			};
		}


		bool MeshPrepare ( Context context, Node node, Mesh mesh, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, Matrix worldMatrix )
		{
			var cbData	=	new CBData();

			cbData.View			=	context.View;
			cbData.Projection	=	context.Projection;
			cbData.ViewPos		=	context.ViewPosition;
			cbData.World		=	worldMatrix;

			constBuffer.SetData( cbData );

			GraphicsDevice.VertexShaderConstants[0]	=	constBuffer;
			GraphicsDevice.PixelShaderConstants[0]	=	constBuffer;
			GraphicsDevice.PipelineState			=	factory[ 0 ];
			GraphicsDevice.PixelShaderSamplers[0]	=	SamplerState.AnisotropicWrap;
			GraphicsDevice.DepthStencilState		=	DepthStencilState.Default;

			GraphicsDevice.SetupVertexInput( indexBuffer, vertexBuffer );

			return true;
		}


		void SubsetDraw ( Context context, MeshSubset meshSubset, Material material )
		{
			GraphicsDevice.DrawIndexed( Primitive.TriangleList, meshSubset.PrimitiveCount*3, meshSubset.StartPrimitive*3, 0 );
		}


		/// <summary>
		/// Draws game
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			GraphicsDevice.ClearBackbuffer( Color.CornflowerBlue, 1, 0 );

//			var dr	=	GetService<DebugRender>();

			//	Animate :
			/*frame++;
			if (frame>scene.LastFrame) {
				frame = scene.FirstFrame;
			}
			if (frame<scene.FirstFrame) {
				frame = scene.FirstFrame;
			} */

			sceneDrawer.EvaluateScene( frame, AnimationMode.Repeat );

			sceneDrawer.Draw( gameTime, stereoEye, Prepare, MeshPrepare, SubsetDraw );

			base.Draw( gameTime, stereoEye );
		}
	}
}
