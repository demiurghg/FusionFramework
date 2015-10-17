using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion;
using Fusion.Build;
using Fusion.Engine.Common;
using Fusion.Core.Shell;

namespace TestGame2 {

	class Program {
		[STAThread]
		static int Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );

			//
			//	Build content on startup :
			//
			try {
				Builder.Build( @"..\..\..\Content", @"Content", @"..\..\..\Temp", false );
			} catch ( Exception e ) {
				Log.Error( e.Message );
				return 1;
			}


			//
			//	Parse command line :
			//

			using ( var engine = new GameEngine() ) {

				var sv	=	new GameServer(engine);
				var cl	=	new GameClient(engine);
				var ui	=	new GameInterface(engine);

				engine.Run( sv, cl, ui );				
			}

			return 0;
		}
	}
}
