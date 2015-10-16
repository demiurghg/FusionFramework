using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Core.Development;
using Fusion.Build;

namespace SpriteDemo {
	class Program {

		[STAThread]
		static void Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );

			Fusion.Core.Test.Tester.Run();

			try {
				Builder.Build( @"..\..\..\Content", @"Content", @"..\..\..\Temp", false );
			} catch ( Exception e ) {
				Log.Error( e.Message );
				return;
			}

			using ( var game = new SpriteDemo() ) {
				game.Run(args);
			}
		}
	}
}
