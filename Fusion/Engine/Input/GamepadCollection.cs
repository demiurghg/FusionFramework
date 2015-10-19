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
	public class GamepadCollection {

		
		Gamepad[] gamepads;
		

		internal GamepadCollection ( GameEngine gameEngine )
		{
			gamepads	=	new Gamepad[4];
			gamepads[0]	=	new Gamepad( gameEngine.InputDevice.GetGamepad( 0 ) );
			gamepads[1]	=	new Gamepad( gameEngine.InputDevice.GetGamepad( 1 ) );
			gamepads[2]	=	new Gamepad( gameEngine.InputDevice.GetGamepad( 2 ) );
			gamepads[3]	=	new Gamepad( gameEngine.InputDevice.GetGamepad( 3 ) );
		}
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="playerIndex"></param>
		/// <returns></returns>
		public Gamepad this[int playerIndex] {
			get {							  
				if (playerIndex>gamepads.Length) {
					throw new ArgumentOutOfRangeException("playerIndex must be in 0,1,2 or 3");
				}

				return gamepads[playerIndex];
			}
		}
	}
}
