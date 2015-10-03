using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;

using System.Xml.Serialization;


namespace Fusion.UserInterface {
	public class Config {
		public bool		ShowFrames			{ get; set; }
		public bool		SkipUserInterface	{ get; set; }
		public bool		ShowProfilingInfo	{ get; set; }

	}
}
