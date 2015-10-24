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
using Fusion.Core.Utils;

namespace TestGame2 {

	class Program {
		[STAThread]
		static int Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );
			Trace.Listeners.Add( new TraceRecorder() );

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

				engine.GameServer		=	new CustomGameServer(engine);
				engine.GameClient		=	new CustomGameClient(engine);
				engine.GameInterface	=	new CustomGameInterface(engine);

				engine.LoadConfiguration("Config.ini");

				//	apply command-line options here:
				//	...

				engine.Run();
				
				engine.SaveConfiguration("Config.ini"); 				
			}

			return 0;
		}
	}
}
