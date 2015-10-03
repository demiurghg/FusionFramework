using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.DirectInput;
using DirectJoystick = SharpDX.DirectInput.Joystick;

namespace Fusion.Input {
	public class Joystick : DisposableBase
	{
		public DirectJoystick	DJoystick		{ get; private set; }
		public JoystickState	JoystickState	{ get; private set; }

		DirectInput directInput;

		public Joystick()
		{
			JoystickState = new JoystickState();
			ObtainJoystick();
		}


		public void ObtainJoystick()
		{
			if (DJoystick != null) {
				DJoystick.Dispose();
			}
			
			directInput = new DirectInput();
			
			// Find a Joystick Guid
			var joystickGuid = Guid.Empty;
			
			foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices)) {
				joystickGuid = deviceInstance.InstanceGuid;
			}
			
			if (joystickGuid == Guid.Empty) {
				Log.LogMessage("No joystick connected");
				return;
			}
			
			// Instantiate the joystick
			DJoystick = new DirectJoystick(directInput, joystickGuid);
			
			Log.LogMessage("Found Joystick with GUID: {0}		Name: {1}", joystickGuid, DJoystick.Information.ProductName);
			
			// Set BufferSize in order to use buffered data.
			DJoystick.Properties.BufferSize = 128;
			
			// Acquire the joystick
			DJoystick.Acquire();
		}


		public void Update()
		{
			if (DJoystick == null) return;

			DJoystick.Poll();
			var datas = DJoystick.GetBufferedData();
			foreach (var state in datas) {
				JoystickState.Update(state);
				//Log.WriteLine(state.ToString());
			}
		}


		public void Acquire()
		{
			if (DJoystick!=null) {
				DJoystick.Acquire();
			}
		}


		public void Unacquire()
		{
			if (DJoystick!=null) {
				DJoystick.Unacquire();
			}
		}


		protected override void Dispose( bool disposing )
		{
			if (disposing) {
				Unacquire();
				if (DJoystick != null) DJoystick.Dispose();
				directInput.Dispose();
			}

			base.Dispose( disposing );
		}
	}
}
