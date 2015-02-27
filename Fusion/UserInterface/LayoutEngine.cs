using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Mathematics;

using Fusion.Input;
using Fusion.Graphics;


namespace Fusion.UserInterface {
	
	public abstract class LayoutEngine {

		public abstract void RunLayout ( Frame targetFrame, bool forceTransitions = false );
		
	}
}
