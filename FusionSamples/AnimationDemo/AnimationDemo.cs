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


		Scene			scene;
		ConstantBuffer	constBuffer;
		Ubershader		uberShader;


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
			Parameters.StereoMode	=	StereoMode.Interlaced;
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
			scene =	Content.Load<Scene>(@"test");

			scene.Bake<VertexColorTextureNormal>( GraphicsDevice, VertexColorTextureNormal.Bake );

			uberShader	=	Content.Load<Ubershader>("render");
			uberShader.Map( typeof(RenderFlags) );

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

		/// <summary>
		/// Draws game
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


			var dr	=	GetService<DebugRender>();
			
			var worldMatricies = new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );


			//	Animate :
			/*frame++;
			if (frame>scene.LastFrame) {
				frame = scene.FirstFrame;
			}
			if (frame<scene.FirstFrame) {
				frame = scene.FirstFrame;
			} */

			scene.GetAnimSnapshot( frame, scene.FirstFrame, scene.LastFrame, AnimationMode.Repeat, worldMatricies );
			scene.ComputeAbsoluteTransforms( worldMatricies, worldMatricies );


			for (int j=1; j<worldMatricies.Length; j++) {
				dr.DrawLine( worldMatricies[j-1].TranslationVector, worldMatricies[j].TranslationVector, Color.LightYellow );
			}
			foreach ( var wm in worldMatricies ) {
				dr.DrawBasis( wm, 0.3f );
			}



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
						//GraphicsDevice.PSShaderResources[0]	=	mesh.Materials[ subset.MaterialIndex ].Tag as Texture2D ;
						mesh.Draw( subset.StartPrimitive, subset.PrimitiveCount );
					}
				}
			}

			base.Draw( gameTime, stereoEye );
		}
	}
}
