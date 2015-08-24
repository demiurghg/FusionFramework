using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Development;
using System.Diagnostics;

namespace DeferredDemo {
	class Program {
		[STAThread]
		static void Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );

			using (var game = new DeferredDemo()) {
				//if (DevCon.Prepare( game, @"..\..\..\Content\Content.xml", "Content" )) {
					game.Run( args );
				//}
			}
		}
	}
}
