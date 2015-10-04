using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Input;
using Fusion.Drivers.Graphics;


namespace Fusion.UserInterface.Layouts {

	public class StackLayout : LayoutEngine {

		public int	Offset			{ get; set; }
		public int	Interval		{ get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetFrame"></param>
		/// <param name="forceTransitions"></param>
		public override void RunLayout ( Frame targetFrame, bool forceTransitions = false )
		{
			int offset = Offset;
			var gp = targetFrame.GetPaddedRectangle(false);

			foreach ( var child in targetFrame.Children ) {

				child.X = gp.X;
				child.Y = gp.Y + offset;

				offset += child.Height;
				offset += Interval;

			}

		}

		
	}
}
