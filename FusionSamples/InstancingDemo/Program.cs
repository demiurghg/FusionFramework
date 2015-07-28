using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion;
using Fusion.Mathematics;

namespace InstancingDemo2D {
	class Program {

		[STAThread]
		static void Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );

			using ( var game = new InstancingDemo() ) {
				game.Run(args);
			}
		}
	}
}
