using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion;
using Fusion.Build;

namespace TestGame2 {
	class Program {
		[STAThread]
		static void Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );

			try {
				Builder.Build( @"..\..\..\Content", @"Content", @"..\..\..\Temp", false );
			} catch ( Exception e ) {
				Log.Error( e.Message );
				return;
			}

			using (var game = new TestGame2()) {
				game.Run( args );
			}
		}
	}
}
