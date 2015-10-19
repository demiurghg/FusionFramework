using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.RawInput;
using Device = SharpDX.RawInput.Device;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
//using FRBTouch.MultiTouch;
//using FRBTouch.MultiTouch.Win32Helper;
//using SharpDX.DirectInput;


namespace Fusion.Drivers.Input {

	
	internal class InputDevice : DisposableBase {

		public	Vector2		RelativeMouseOffset		{ get; protected set; }
		public	Vector2		GlobalMouseOffset		{ get; protected set; }
		public	Point		MousePosition			{ get { return new Point( (int)GlobalMouseOffset.X, (int)GlobalMouseOffset.Y ); } }
		public	int			TotalMouseScroll		{ get; protected set; }
		public	bool		IsMouseCentered			{ get; set; }
		public	bool		IsMouseClipped			{ get; set; }
		public	bool		IsMouseHidden			{ get; set; }
		public	int			MouseWheelScrollLines	{ get { return System.Windows.Forms.SystemInformation.MouseWheelScrollLines; } }
		public	int			MouseWheelScrollDelta	{ get { return System.Windows.Forms.SystemInformation.MouseWheelScrollDelta; } }
		
		HashSet<Keys>		pressed = new HashSet<Keys>();

		public delegate void MouseMoveHandlerDelegate	( object sender, MouseMoveEventArgs e );
		public delegate void MouseScrollEventHandler	( object sender, MouseScrollEventArgs e );
		public delegate void KeyDownEventHandler		( object sender, KeyEventArgs e );
		public delegate void KeyUpEventHandler			( object sender, KeyEventArgs e );
		public delegate void KeyPressEventHandler		( object sender, KeyPressArgs e );


		public class KeyEventArgs : EventArgs {
			public Keys	Key;
		}

		public class KeyPressArgs : EventArgs {
			public char	KeyChar;
		}

		public class MouseScrollEventArgs : EventArgs {
			/// <summary>
			/// See: InputDevice.MouseWheelScrollDelta.
			/// </summary>
			public int WheelDelta;
		}

		public class MouseMoveEventArgs : EventArgs {
			public Vector2	Position;
		}

		public event MouseMoveHandlerDelegate	MouseMove;
		public event MouseScrollEventHandler	MouseScroll;
		public event KeyDownEventHandler		KeyDown;
		public event KeyUpEventHandler			KeyUp;
		public event KeyDownEventHandler		FormKeyDown;
		public event KeyUpEventHandler			FormKeyUp;
		public event KeyPressEventHandler		FormKeyPress;



		static class NativeMethods {
			public static Forms.Cursor LoadCustomCursor(string path) 
			{
				IntPtr hCurs =	LoadCursorFromFile(path);

				if (hCurs == IntPtr.Zero) {
					throw new Win32Exception();
				}
				
				var curs	=	new Forms.Cursor(hCurs);
				// Note: force the cursor to own the handle so it gets released properly
				var fi		=	typeof(Forms.Cursor).GetField("ownHandle", BindingFlags.NonPublic | BindingFlags.Instance);
				
				fi.SetValue(curs, true);
				return curs;
			}

			[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			private static extern IntPtr LoadCursorFromFile(string path);
		}		



		public readonly GameEngine GameEngine;


		/// <summary>
		/// Constrcutor
		/// </summary>
		internal InputDevice ( GameEngine game )
		{
			this.GameEngine		=	game;
		}



		internal void Initialize ()
		{
			IsMouseHidden	=	false;
			IsMouseCentered	=	false;
			IsMouseClipped	=	false;

            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericMouse, DeviceFlags.None);
            Device.MouseInput += new EventHandler<MouseInputEventArgs>(MouseHandler);

            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
            Device.KeyboardInput += new EventHandler<KeyboardInputEventArgs>(KeyboardHandle);

			if (GameEngine.GraphicsDevice.Display.Window != null && !GameEngine.GraphicsDevice.Display.Window.IsDisposed) {
				var p				= GameEngine.GraphicsDevice.Display.Window.PointToClient(Forms.Cursor.Position);
				GlobalMouseOffset	= new Vector2(p.X, p.Y);
			}
		}


		
		/// <summary>
		/// Disposes stuff
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose( bool disposing )
		{
			Device.KeyboardInput	-= KeyboardHandle;
			Device.MouseInput		-= MouseHandler;

			SetCursorVisibility(true);
			Forms.Cursor.Clip		=	new Drawing.Rectangle( int.MinValue, int.MinValue, int.MaxValue, int.MaxValue );

			base.Dispose(disposing);
		}



		/// <summary>
		/// Adds key to hash list and fires KeyDown event
		/// </summary>
		/// <param name="key"></param>
		void AddPressedKey ( Keys key )
		{
			if (!GameEngine.IsActive) {
				return;
			}

			pressed.Add( key );
			if ( KeyDown!=null ) {
				KeyDown( this, new KeyEventArgs(){ Key = key } );
			}
		}



		/// <summary>
		/// Removes key from hash list and fires KeyUp event
		/// </summary>
		/// <param name="key"></param>
		void RemovePressedKey ( Keys key )
		{
			if (pressed.Contains(key)) {
				pressed.Remove( key );
				if ( KeyUp!=null ) {
					KeyUp( this, new KeyEventArgs(){ Key = key } );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		internal void RemoveAllPressedKey()
		{
			foreach ( var key in pressed ) {
				if ( KeyUp!=null ) {
					KeyUp( this, new KeyEventArgs(){ Key = key } );
				}
			}
			pressed.Clear();
		}


		/// <summary>
		/// Loads cursor image from file
		/// </summary>
		/// <param name="path"></param>
		public void SetCursorImage ( string path )
		{
			NativeMethods.LoadCustomCursor( path );
		}



		/// <summary>
		///	Mouse handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void MouseHandler ( object sender, MouseInputEventArgs e )
		{
			if (GameEngine.GraphicsDevice.Display.Window != null && !GameEngine.GraphicsDevice.Display.Window.IsDisposed) {

				var p				= GameEngine.GraphicsDevice.Display.Window.PointToClient(Forms.Cursor.Position);
			
				GlobalMouseOffset	= new Vector2(p.X, p.Y);
				
				if (MouseMove!=null) {
					MouseMove(this, new MouseMoveEventArgs(){ Position = GlobalMouseOffset });
				}
			}


			//Console.WriteLine( "{0} {1} {2}", e.X, e.Y, e.ButtonFlags.ToString() );

			RelativeMouseOffset += new Vector2( e.X, e.Y );

			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.LeftButtonDown		) ) AddPressedKey( Keys.LeftButton );
			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.RightButtonDown	) )	AddPressedKey( Keys.RightButton );
			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.MiddleButtonDown	) )	AddPressedKey( Keys.MiddleButton );
			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.Button4Down		) )	AddPressedKey( Keys.MouseButtonX1 );
			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.Button5Down		) )	AddPressedKey( Keys.MouseButtonX2 );
			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.LeftButtonUp		) ) RemovePressedKey( Keys.LeftButton );
			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.RightButtonUp		) )	RemovePressedKey( Keys.RightButton );
			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.MiddleButtonUp		) )	RemovePressedKey( Keys.MiddleButton );
			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.Button4Up			) )	RemovePressedKey( Keys.MouseButtonX1 );
			if ( e.ButtonFlags.HasFlag( MouseButtonFlags.Button5Up			) )	RemovePressedKey( Keys.MouseButtonX2 );

			if ( GameEngine.IsActive ) {
				if ( MouseScroll!=null && e.WheelDelta!=0 ) {
					MouseScroll( this, new MouseScrollEventArgs(){ WheelDelta = e.WheelDelta } );
				}
				TotalMouseScroll	+=	e.WheelDelta;
			}
		}


		
		/// <summary>
		/// Keyboard handler
		/// In general: http://molecularmusings.wordpress.com/2011/09/05/properly-handling-keyboard-input/ 
		/// L/R shift:  http://stackoverflow.com/questions/5920301/distinguish-between-left-and-right-shift-keys-using-rawinput
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void KeyboardHandle ( object sender, KeyboardInputEventArgs e )
		{
		    Keys	key = (Keys)e.Key;

			bool E0		=	e.ScanCodeFlags.HasFlag( ScanCodeFlags.E0 );
			bool E1		=	e.ScanCodeFlags.HasFlag( ScanCodeFlags.E1 );
			bool Make	=	e.ScanCodeFlags.HasFlag( ScanCodeFlags.Make );
			bool Break	=	e.ScanCodeFlags.HasFlag( ScanCodeFlags.Break );

			if (e.Key==Forms.Keys.Menu) {
				key = E0 ? Keys.RightAlt : Keys.LeftAlt;
			}

			if (e.Key==Forms.Keys.ControlKey) {
				key = E0 ? Keys.RightControl : Keys.LeftControl;
			}

			if (e.Key==Forms.Keys.ShiftKey) {
				if ( e.MakeCode==0x2a ) key = Keys.LeftShift;
				if ( e.MakeCode==0x36 ) key = Keys.RightShift;
			}

			if (!E0) {
				if ( e.Key==Forms.Keys.Insert	)	key	=	Keys.NumPad0;
				if ( e.Key==Forms.Keys.End		)	key	=	Keys.NumPad1;
				if ( e.Key==Forms.Keys.Down		)	key	=	Keys.NumPad2;
				if ( e.Key==Forms.Keys.PageDown	)	key	=	Keys.NumPad3;
				if ( e.Key==Forms.Keys.Left		)	key	=	Keys.NumPad4;
				if ( e.Key==Forms.Keys.Clear	)	key	=	Keys.NumPad5;
				if ( e.Key==Forms.Keys.Right	)	key	=	Keys.NumPad6;
				if ( e.Key==Forms.Keys.Home		)	key	=	Keys.NumPad7;
				if ( e.Key==Forms.Keys.Up		)	key	=	Keys.NumPad8;
				if ( e.Key==Forms.Keys.PageUp	)	key	=	Keys.NumPad9;
			}

			if (Enum.IsDefined(typeof(Keys), key)) {
				if (Break) {
					if ( pressed.Contains( key ) ) RemovePressedKey( key );
				} else {
					if ( !pressed.Contains( key ) ) AddPressedKey( key );
				}
			}
		}



		bool cursorHidden = false;

		/// <summary>
		/// Sets cursor visibility
		/// </summary>
		/// <param name="visible"></param>
		void SetCursorVisibility ( bool visible )
		{
			if (visible) {
				if (cursorHidden) {
					Forms.Cursor.Show();
					cursorHidden = false;
				}
			} else {
				if (!cursorHidden) {
					Forms.Cursor.Hide();
					cursorHidden = true;
				}
			}
		}



		/// <summary>
		/// Frame
		/// </summary>
		internal void UpdateInput ()
		{
			if ( GameEngine.GraphicsDevice.Display.Window!=null ) {

			    if ( GameEngine.IsActive ) {

			        System.Drawing.Rectangle rect = GameEngine.GraphicsDevice.Display.Window.ClientRectangle;

					if (IsMouseCentered) {
						Forms.Cursor.Position	=	GameEngine.GraphicsDevice.Display.Window.PointToScreen( new Drawing.Point( rect.Width/2, rect.Height/2 ) );
					}

					if (IsMouseClipped) {
						Forms.Cursor.Clip		=	GameEngine.GraphicsDevice.Display.Window.RectangleToScreen( rect );
					}

					SetCursorVisibility( !IsMouseHidden );

			    } else {

			        Forms.Cursor.Clip		=	new Drawing.Rectangle( int.MinValue, int.MinValue, int.MaxValue, int.MaxValue );
					RelativeMouseOffset		=	Vector2.Zero;
					SetCursorVisibility( true );

					RemoveAllPressedKey();
			    }
			}

			Gamepad.Update();
		}




		/// <summary>
		/// Should be called after everything is updated
		/// </summary>
		internal void EndUpdateInput ()
		{
			RelativeMouseOffset = Vector2.Zero;
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Form handling :
		 * 
		-----------------------------------------------------------------------------------------*/

		internal void NotifyKeyDown ( Keys key, bool alt, bool shift, bool control )
		{
			var formKeyDown = FormKeyDown;
			if (formKeyDown!=null) {
				formKeyDown( this, new KeyEventArgs(){ Key = key } );
			}
		}


		internal void NotifyKeyUp  ( Keys key, bool alt, bool shift, bool control )
		{
			var formKeyUp = FormKeyUp;
			if (formKeyUp!=null) {
				formKeyUp( this, new KeyEventArgs(){ Key = key } );
			}
		}


		internal void NotifyKeyPress ( char keyChar )
		{
			var keyPress = FormKeyPress;
			if (keyPress!=null) {
				keyPress( this, new KeyPressArgs(){ KeyChar = keyChar } );
			}
		}


		/// <summary>
		/// Checks whether key is down
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsKeyDown ( Keys key, bool ignoreInputMode = true )
		{
			return ( pressed.Contains( key ) );
		}



		/// <summary>
		/// Checks whether key is down
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsKeyUp ( Keys key )
		{
			return !IsKeyDown( key );
		}



		/// <summary>
		/// Gets player's gamepad
		/// </summary>
		/// <param name="playerIndex">Player index from 0 to 3 inclusivly</param>
		/// <returns></returns>
		public Gamepad	GetGamepad ( int playerIndex )
		{
			return Gamepad.GetGamePad( playerIndex );
		}
	}
}
