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

	public class Mouse : GameModule {
		
		InputDevice device;

		public event MouseMoveHandlerDelegate	Move;
		public event MouseScrollEventHandler	Scroll;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		internal Mouse ( GameEngine gameEngine ) : base(gameEngine)
		{
			this.device	=	gameEngine.InputDevice;

			device.MouseScroll += device_MouseScroll;
			device.MouseMove += device_MouseMove;
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
				device.MouseScroll -= device_MouseScroll;
				device.MouseMove -= device_MouseMove;
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Difference between last and current mouse position.
		/// Use it for shooters.
		/// </summary>
		public Vector2 PositionDelta { 
			get { return device.RelativeMouseOffset; } 
		}



		/// <summary>
		/// Mouse position relative to top-left corner
		/// </summary>
		public Point Position { 
			get { return new Point( (int)device.GlobalMouseOffset.X, (int)device.GlobalMouseOffset.Y ); }
		}

		/// <summary>
		/// Force mouse to main window center on each frame.
		/// </summary>
		public	bool IsMouseCentered { 
			get { return device.IsMouseCentered; }
			set { device.IsMouseCentered = value; }
		}
		
		/// <summary>
		/// Clip mouse by main window's client area border.
		/// </summary>
		public	bool IsMouseClipped { 
			get { return device.IsMouseClipped; }
			set { device.IsMouseClipped = value; }
		}
		
		/// <summary>
		/// Set and get mouse visibility
		/// </summary>
		public	bool IsMouseHidden { 
			get { return device.IsMouseHidden; }
			set { device.IsMouseHidden = value; }
		}

		/// <summary>
		/// System value: MouseWheelScrollLines
		/// </summary>
		public	int	MouseWheelScrollLines	{ get { return device.MouseWheelScrollLines; } }

		/// <summary>
		/// System value: MouseWheelScrollDelta
		/// </summary>
		public	int	MouseWheelScrollDelta	{ get { return device.MouseWheelScrollDelta; } }



		void device_MouseMove ( object sender, InputDevice.MouseMoveEventArgs e )
		{
			var handler = Move;
			if (handler!=null) {
				handler( sender, new MouseMoveEventArgs(){ Position = e.Position } );	
			}
		}


		void device_MouseScroll ( object sender, InputDevice.MouseScrollEventArgs e )
		{
			var handler = Scroll;
			if (handler!=null) {
				handler( sender, new MouseScrollEventArgs(){ WheelDelta = e.WheelDelta } );	
			}
		}




	}
}
