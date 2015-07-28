using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion;
using Fusion.Mathematics;
using Fusion.Development;

namespace SpriteDemo {
	class Program {

		[STAThread]
		static void Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );

			using ( var game = new SpriteDemo() ) {
				game.Run(args);
			}
		}
	}
}
