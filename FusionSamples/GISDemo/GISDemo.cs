using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.GIS;
using Fusion.Mathematics;

using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;
using Fusion.UserInterface;

namespace GISDemo {
	public class GISDemo : Game {


		/// <summary>
		/// GISDemo constructor
		/// </summary>
		public GISDemo ()
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
			AddService( new SpriteBatch( this ),	false,	false, 0, 0 );
			AddService( new DebugStrings( this ),	true,	true, 0, 9999 );
			//AddService( new DebugRender( this ),	true,	true, 9998, 9998 );
			AddService( new LayerService(this),		true,	true, 1, 1 );

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
			LoadContent();
			Reloading += (s,e) => LoadContent();

			var lay = GetService<LayerService>();

			InputDevice.KeyDown += (sender, args) => lay.GlobeLayer.InputDeviceOnMouseDown(sender, new Frame.MouseEventArgs { X = (int)InputDevice.GlobalMouseOffset.X, Y = (int)InputDevice.GlobalMouseOffset.Y, Key = args.Key });
			InputDevice.MouseMove += (sender, args) => lay.GlobeLayer.InputDeviceOnMouseMove(sender, new Frame.MouseEventArgs { X = (int)InputDevice.GlobalMouseOffset.X, Y = (int)InputDevice.GlobalMouseOffset.Y });
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
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

			//var cam	=	GetService<Camera>();
			//var dr	=	GetService<DebugRender>();
			//dr.View			=	cam.GetViewMatrix( StereoEye.Mono );
			//dr.Projection	=	cam.GetProjectionMatrix( StereoEye.Mono );

			//dr.DrawGrid(10);
			frame += gameTime.ElapsedSec * 24;

			//Console.WriteLine(InputDevice.GlobalMouseOffset);
		}


		float frame = 0;


		/// <summary>
		/// Draws game
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			//GraphicsDevice.ClearBackbuffer( Color.CornflowerBlue, 1, 0 );

			base.Draw( gameTime, stereoEye );
		}
	}
}
