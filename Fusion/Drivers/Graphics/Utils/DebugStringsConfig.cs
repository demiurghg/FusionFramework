using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {

	public class DebugStringsConfig {

		public DebugStringsConfig() {
			BackgroundColor		=	new Color(0,0,0,0);
			SuppressDebugString	=	false;
		}
			
		public bool		SuppressDebugString	{ get; set; }
		public Color	BackgroundColor		{ get; set; }
	}
}
