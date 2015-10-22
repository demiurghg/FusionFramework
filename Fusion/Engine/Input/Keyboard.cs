using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;

namespace Fusion.Engine.Input {
	public class Keyboard : GameModule {

		InputDevice device;

		/// <summary>
		/// ctor
		/// </summary>
		/// <param name="gameEngine"></param>
		internal Keyboard ( GameEngine gameEngine ) : base(gameEngine)
		{
			this.device	=	gameEngine.InputDevice;

			device.KeyDown += device_KeyDown;
			device.KeyUp += device_KeyUp;

			device.FormKeyDown += device_FormKeyDown;
			device.FormKeyUp += device_FormKeyUp;
			device.FormKeyPress += device_FormKeyPress;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				device.KeyDown -= device_KeyDown;
				device.KeyUp -= device_KeyUp;

				device.FormKeyDown -= device_FormKeyDown;
				device.FormKeyUp -= device_FormKeyUp;
				device.FormKeyPress -= device_FormKeyPress;
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Returns whether a specified key is currently being pressed. 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsKeyDown ( Keys key )
		{
			return ( device.IsKeyDown( (Fusion.Drivers.Input.Keys)key ) );
		}
		

		/// <summary>
		/// Returns whether a specified key is currently not pressed. 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsKeyUp ( Keys key )
		{
			return ( device.IsKeyUp( (Fusion.Drivers.Input.Keys)key ) );
		}


		public event KeyDownEventHandler		KeyDown;
		public event KeyUpEventHandler			KeyUp;
		public event KeyDownEventHandler		FormKeyDown;
		public event KeyUpEventHandler			FormKeyUp;
		public event KeyPressEventHandler		FormKeyPress;



		void device_KeyDown ( object sender, InputDevice.KeyEventArgs e )
		{
			var handler = KeyDown;
			if (handler!=null) {
				handler( sender, new KeyEventArgs(){ Key = (Keys)e.Key } );
			}
		}

		void device_KeyUp ( object sender, InputDevice.KeyEventArgs e )
		{
			var handler = KeyUp;
			if (handler!=null) {
				handler( sender, new KeyEventArgs(){ Key = (Keys)e.Key } );
			}
		}


		void device_FormKeyDown ( object sender, InputDevice.KeyEventArgs e )
		{
			var handler = FormKeyDown;
			if (handler!=null) {
				handler( sender, new KeyEventArgs(){ Key = (Keys)e.Key } );
			}
		}

		void device_FormKeyUp ( object sender, InputDevice.KeyEventArgs e )
		{
			var handler = FormKeyUp;
			if (handler!=null) {
				handler( sender, new KeyEventArgs(){ Key = (Keys)e.Key } );
			}
		}

		void device_FormKeyPress ( object sender, InputDevice.KeyPressArgs e )
		{
			var handler = FormKeyPress;
			if (handler!=null) {
				handler( sender, new KeyPressArgs(){ KeyChar = e.KeyChar } );
			}
		}
		



	}
}
