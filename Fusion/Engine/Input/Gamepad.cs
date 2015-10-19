using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Input;
using DrvGamePad = Fusion.Drivers.Input.Gamepad;

namespace Fusion.Engine.Input {

	/// <summary>
	/// 
	/// </summary>
	public class Gamepad {

		DrvGamePad gamePad;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="drvGamePad"></param>
		internal Gamepad ( DrvGamePad drvGamePad )
		{
			this.gamePad	=	drvGamePad;
		}



		/// <summary>
		/// Returns whether a current gamepad is connected. 
		/// </summary>
		public bool	IsConnected { 
			get { 
				return gamePad.IsConnected;	
			} 
		}


		/// <summary>
		/// Returns whether a specified key is currently being pressed. 
		/// </summary>
		/// <param name="button"></param>
		/// <returns></returns>
		public bool IsButtonPressed(GamepadButtons button)
		{
			return gamePad.IsKeyPressed( (Fusion.Drivers.Input.GamepadButtons)button );
		}



		/// <summary>
		/// Sets vibration
		/// </summary>
		/// <param name="leftMotor"></param>
		/// <param name="rightMotor"></param>
		public void SetVibration ( float leftMotor, float rightMotor )
		{
			gamePad.SetVibration( leftMotor, rightMotor );
		}



		/// <summary>
		/// Gets left trigger position
		/// </summary>
		public float LeftTrigger {
			get {
				return gamePad.LeftTrigger;
			}
		}



		/// <summary>
		/// Gets right trigger position :
		/// </summary>
		public float RightTrigger {
			get {
				return gamePad.RightTrigger;
			}
		}



		/// <summary>
		/// Gets left stick position
		/// </summary>
		public Vector2 LeftStick {
			get {
				return gamePad.LeftStick;
			}
		}


		/// <summary>
		/// Gets right stick position
		/// </summary>
		public Vector2 RightStick {
			get {
				return gamePad.RightStick;
			}
		}
	}
}
