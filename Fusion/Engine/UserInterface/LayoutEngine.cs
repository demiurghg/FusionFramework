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


namespace Fusion.UserInterface {
	
	public abstract class LayoutEngine {

		public abstract void RunLayout ( Frame targetFrame, bool forceTransitions = false );
		
	}
}
