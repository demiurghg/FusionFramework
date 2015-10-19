using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.XInput;
using XIGamePad = SharpDX.XInput.Gamepad;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Input
{
	internal class Gamepad
	{
		public bool	IsConnected { get { return controller.IsConnected;	} }
		Controller	controller;
		State		state;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		protected Gamepad(int index)
		{
			controller	= new Controller( (UserIndex)((int)index) );
			UpdateState();
		}



		/// <summary>
		/// Updates controller state
		/// </summary>
		protected void UpdateState()
		{
			if (!controller.IsConnected) return;

			state = controller.GetState();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="button"></param>
		/// <returns></returns>
		public bool IsKeyPressed(GamepadButtons button)
		{
			if (((int) state.Gamepad.Buttons & (int) button) != 0) return true;

			return false;
		}



		/// <summary>
		/// Sets vibration
		/// </summary>
		/// <param name="leftMotor"></param>
		/// <param name="rightMotor"></param>
		public void SetVibration ( float leftMotor, float rightMotor )
		{
			if (!controller.IsConnected) return;

			var uLeftMotor  = (ushort)( leftMotor	* 65535.0f );
			var uRightMotor = (ushort)( rightMotor	* 65535.0f );

			controller.SetVibration(new Vibration {LeftMotorSpeed = uLeftMotor, RightMotorSpeed = uRightMotor});
		}



		/// <summary>
		/// Gets left trigger position
		/// </summary>
		public float LeftTrigger {
			get {
				byte trig = state.Gamepad.LeftTrigger;

				if (trig > XIGamePad.TriggerThreshold)
					return trig/255.0f;

				return 0.0f;
			}
		}



		/// <summary>
		/// Gets right trigger position :
		/// </summary>
		public float RightTrigger {
			get {
				byte trig = state.Gamepad.RightTrigger;

				if (trig > XIGamePad.TriggerThreshold)
					return trig / 255.0f;

				return 0.0f;
			}
		}



		/// <summary>
		/// Gets left stick position
		/// </summary>
		public Vector2 LeftStick {
			get {
				return GetStickDirection(state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY, XIGamePad.LeftThumbDeadZone);
			}
		}


		/// <summary>
		/// Gets right stick position
		/// </summary>
		public Vector2 RightStick {
			get {
				return GetStickDirection(state.Gamepad.RightThumbX, state.Gamepad.RightThumbY, XIGamePad.RightThumbDeadZone);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sX"></param>
		/// <param name="sY"></param>
		/// <param name="threshold"></param>
		/// <returns></returns>
		Vector2 GetStickDirection ( short sX, short sY, short threshold )
		{
			float tX = sX;
			float tY = sY;

			//determine how far the controller is pushed
			float magnitude = (float) Math.Sqrt(tX * tX + tY * tY);

			//determine the direction the controller is pushed
			var ret = Vector2.Zero;
			if (magnitude > 0.0f) {
				ret.X = tX/magnitude;
				ret.Y = tY/magnitude;
			}

			float normalizedMagnitude = 0;

			//check if the controller is outside a circular dead zone
			if (magnitude > threshold) {
				//clip the magnitude at its expected maximum value
				if (magnitude > 32767) magnitude = 32767;

				//adjust magnitude relative to the end of the dead zone
				magnitude -= threshold;

				//optionally normalize the magnitude with respect to its expected range
				//giving a magnitude value of 0.0 to 1.0
				normalizedMagnitude = magnitude / (32767 - threshold);
			} else { //if the controller is in the deadzone zero out the magnitude
				magnitude = 0.0f;
				normalizedMagnitude = 0.0f;
			}

			ret *= normalizedMagnitude;

			return ret;
		}


		static Gamepad[] gamepads = new Gamepad[] { new Gamepad(0), new Gamepad(1), new Gamepad(2), new Gamepad(3) };

		/// <summary>
		/// Gets gamepad state.
		/// </summary>
		/// <param name="playerIndex">Player index from 0 to 3 inclusive</param>
		/// <returns></returns>
		internal static Gamepad GetGamePad(int playerIndex)
		{
			var gpad = gamepads[playerIndex];

			return gpad;
		}


		/// <summary>
		/// 
		/// </summary>
		internal static void Update()
		{
			foreach (var gamepad in gamepads) {
				gamepad.UpdateState();
			}
		}
	}
}
