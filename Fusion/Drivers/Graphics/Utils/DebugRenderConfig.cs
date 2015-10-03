using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;
using System.ComponentModel;
using SharpDX;

namespace Fusion.Graphics {

	public class DebugRenderConfig {

		[ Category("Stereo") ]		public bool		SuppressDebugRender	{ get; set; }

		public DebugRenderConfig()
		{
			SuppressDebugRender = false;
		}
	}
}
