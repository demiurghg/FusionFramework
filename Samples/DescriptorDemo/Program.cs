using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion.Development;
using Fusion;
using Fusion.Mathematics;

namespace DescriptorDemo {
	class Program {

		[STAThread]
		static void Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );

			using ( var game = new DescriptorDemo() ) {
				//if (DevCon.Prepare(game, @"..\..\..\Content\Content.xml", "Content")) {
				game.Run(args);
				//}
			}
		}
	}
}
