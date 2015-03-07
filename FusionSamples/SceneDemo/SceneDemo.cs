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


		Scene		scene;
		ConstantBuffer	constBuffer;
		Ubershader	uberShader;


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
		/// Add services :
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			constBuffer		=	new ConstantBuffer( GraphicsDevice, typeof(CBData) );

			LoadContent();
			Reloading += (s,e) => LoadContent();
			GetService<Camera>().FreeCamPosition = Vector3.Up * 10;
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
			scene =	Content.Load<Scene>(@"Scenes\testScene");

			foreach ( var mtrl in scene.Materials ) {
				mtrl.Tag	=	Content.Load<Texture2D>( mtrl.TexturePath );
			}

			scene.Bake<VertexColorTextureNormal>( GraphicsDevice, VertexColorTextureNormal.Bake );

			uberShader	=	Content.Load<Ubershader>("render");
			uberShader.Map( typeof(RenderFlags) );

			Log.Message("{0}", scene.Nodes.Count( n => n.MeshIndex >= 0 ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				if (constBuffer!=null) {
					constBuffer.Dispose();
				}
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
			CBData cbData = new CBData();
			var cam	=	GetService<Camera>();

			GraphicsDevice.ClearBackbuffer( Color.CornflowerBlue, 1, 0 );


			uberShader.SetPixelShader(0);
			uberShader.SetVertexShader(0);

			var worldMatricies = new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );


			for (int j = 0; j<1; j++) {
				for ( int i=0; i<scene.Nodes.Count; i++ ) {

					var node = scene.Nodes[i];
				
					if (node.MeshIndex==-1) {
						continue;
					}

					var mesh = scene.Meshes[ node.MeshIndex ];

					cbData.Projection	=	cam.GetProjectionMatrix( stereoEye );
					cbData.View			=	cam.GetViewMatrix( stereoEye );
					cbData.World		=	Matrix.RotationYawPitchRoll(j*0.01f,j*0.02f,j*0.03f) * worldMatricies[ i ] * Matrix.Scaling( (float)Math.Pow(0.9,j) );
					cbData.ViewPos		=	new Vector4( cam.GetCameraMatrix( stereoEye ).TranslationVector, 1 );

					constBuffer.SetData( cbData );

					GraphicsDevice.RasterizerState		= RasterizerState.CullCW ;
					GraphicsDevice.DepthStencilState	= DepthStencilState.Default ;
					GraphicsDevice.BlendState			= BlendState.Opaque ;
					GraphicsDevice.PixelShaderConstants[0]	= constBuffer ;
					GraphicsDevice.VertexShaderConstants[0]	= constBuffer ;
					GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;

					mesh.SetupVertexInput();

					foreach ( var subset in mesh.Subsets ) {   
						//lock (Game
						GraphicsDevice.PixelShaderResources[0]	=	scene.Materials[ subset.MaterialIndex ].Tag as Texture2D ;
						mesh.Draw( subset.StartPrimitive, subset.PrimitiveCount );
					}
				}
			}

			base.Draw( gameTime, stereoEye );
		}
	}
}
