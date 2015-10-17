using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Development;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using System.Runtime.InteropServices;


namespace InputDemo {
	class InputDemo : Game {


		/// <summary>
		///	Add services and set options
		/// </summary>
		public InputDemo ()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;

			//	uncomment to enable debug graphics device:
			//	(MS Platform SDK must be installed)
			//	Parameters.UseDebugDevice	=	true;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );

			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += InputDemo_Exiting;
			Activated += InputDemo_Activated;
			Deactivated += InputDemo_Deactivated;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDemo_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}



		/// <summary>
		/// Load stuff here
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			InputDevice.MouseScroll += InputDevice_MouseScroll;
			InputDevice.KeyDown += InputDevice_KeyDown;
			InputDevice.KeyUp += InputDevice_KeyUp;
			InputDevice.FormKeyPress += InputDevice_KeyPress;
			InputDevice.FormKeyDown += InputDevice_FormKeyDown;
			InputDevice.FormKeyUp += InputDevice_FormKeyUp;
		}

		void InputDevice_FormKeyUp ( object sender, InputDevice.KeyEventArgs e )
		{
			Log.Message("Form key up : {0}", e.Key );
		}

		void InputDevice_FormKeyDown ( object sender, InputDevice.KeyEventArgs e )
		{
			Log.Message("Form key down : {0}", e.Key );
		}


		void InputDevice_KeyPress ( object sender, InputDevice.KeyPressArgs e )
		{
			Log.Message("Key press : {0}", e.KeyChar );
		}



		void InputDemo_Activated ( object sender, EventArgs e )
		{
			Log.Message("Game activeted");
		}


		void InputDemo_Deactivated ( object sender, EventArgs e )
		{
			Log.Message("Game deactiveted");
		}


		void InputDevice_KeyUp ( object sender, InputDevice.KeyEventArgs e )
		{
			Log.Message("...key up event : {0}", e.Key );
		}


		void InputDevice_KeyDown ( object sender, InputDevice.KeyEventArgs e )
		{
			Log.Message("...key down event : {0}", e.Key );

			if (e.Key == Keys.F1) {
				//DevCon.Show( this );
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



		int scrollValue;


		void InputDevice_MouseScroll ( object sender, InputDevice.MouseScrollEventArgs e )
		{
			Log.Message("...mouse scroll event : {0}", e.WheelDelta );
			scrollValue += e.WheelDelta;
		}



		/// <summary>
		/// Update stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			base.Update( gameTime );

			var ds = GetService<DebugStrings>();

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );
			ds.Add("");

			if (IsActive) {
				ds.Add(Color.LightGreen, "Active");
			} else {
				ds.Add(Color.Red, "Not active");
			}

			ds.Add(Color.Orange, "");

			ds.Add(Color.Orange, "Keyboard");

			foreach (Keys key in (Keys[])Enum.GetValues(typeof(Keys))) {
				if (InputDevice.IsKeyDown( key ) ) {
					ds.Add(" - {0}", key.ToString() );
				}
			}

			ds.Add("");
			ds.Add(Color.Orange, "Mouse");

			ds.Add(" - position (absolute): {0} {1}", InputDevice.GlobalMouseOffset.X, InputDevice.GlobalMouseOffset.Y );
			ds.Add(" - position (relative): {0} {1}", InputDevice.RelativeMouseOffset.X, InputDevice.RelativeMouseOffset.Y );
			ds.Add(" - total scroll value : {0}", scrollValue );

			ds.Add("");
			ds.Add(Color.Orange, "Gamepad");

			for ( int playerIndex = 0; playerIndex <= 3; playerIndex++ ) {
				var gp = InputDevice.GetGamepad( playerIndex );

				if (gp.IsConnected) {

					ds.Add(Color.LightGreen, "Gamepad #{0} is connected", playerIndex );

					ds.Add(" - Left stick    : {0} {1}"	, gp.LeftStick.X,  gp.LeftStick.Y );
					ds.Add(" - Right stick   : {0} {1}"	, gp.RightStick.X, gp.RightStick.Y );
					ds.Add(" - Left trigger  : {0} (left motor)"		, gp.LeftTrigger );
					ds.Add(" - Right trigger : {0} (right motor)"		, gp.RightTrigger );

					gp.SetVibration( gp.LeftTrigger, gp.RightTrigger );

					if ( gp.IsKeyPressed( GamepadButtons.X ) ) ds.Add(Color.Blue,   "[X]");		
					if ( gp.IsKeyPressed( GamepadButtons.Y ) ) ds.Add(Color.Yellow, "[Y]");		
					if ( gp.IsKeyPressed( GamepadButtons.A ) ) ds.Add(Color.Green,  "[A]");		
					if ( gp.IsKeyPressed( GamepadButtons.B ) ) ds.Add(Color.Red,    "[B]");		

					if ( gp.IsKeyPressed( GamepadButtons.LeftShoulder  ) ) ds.Add( "[LS]");		
					if ( gp.IsKeyPressed( GamepadButtons.RightShoulder ) ) ds.Add( "[RS]");		
					if ( gp.IsKeyPressed( GamepadButtons.LeftThumb     ) ) ds.Add( "[LT]");		
					if ( gp.IsKeyPressed( GamepadButtons.RightThumb    ) ) ds.Add( "[RT]");		
					
					if ( gp.IsKeyPressed( GamepadButtons.Back		   ) ) ds.Add( "[Back]");	
					if ( gp.IsKeyPressed( GamepadButtons.Start		   ) ) ds.Add( "[Start]");	

					if ( gp.IsKeyPressed( GamepadButtons.DPadLeft	   ) ) ds.Add( "[Left]");	
					if ( gp.IsKeyPressed( GamepadButtons.DPadRight	   ) ) ds.Add( "[Right]");	
					if ( gp.IsKeyPressed( GamepadButtons.DPadDown	   ) ) ds.Add( "[Down]");	
					if ( gp.IsKeyPressed( GamepadButtons.DPadUp		   ) ) ds.Add( "[Up]");		

				} else {

					ds.Add(Color.Red, "Gamepad #{0} is diconnected", playerIndex );

				}
			}
		}



		/// <summary>
		/// Draw stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			base.Draw( gameTime, stereoEye );
		}
	}
}
