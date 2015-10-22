using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion.Core;
using Fusion.Core.Utils;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;

namespace Fusion.Framework {
	
	public class GameConsoleConfig {

		public float FallSpeed { get; set; }

		public float CursorBlinkRate { get; set; }

		public Color MessageColor	{ get; set; }
		public Color ErrorColor		{ get; set; }
		public Color WarningColor	{ get; set; }
		public Color CmdLineColor	{ get; set; }

		public GameConsoleConfig ()
		{
			FallSpeed		=	5;
			
			CursorBlinkRate	=	3;
			
			MessageColor	=	Color.White;
			ErrorColor		=	Color.Red;
			WarningColor	=	Color.Yellow;
			CmdLineColor	=	Color.Orange;
		}
	}
}
