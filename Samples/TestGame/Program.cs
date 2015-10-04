using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion;
using Fusion.Core.Development;
using Fusion.Engine.Common;

namespace TestGame {
	class Program {

		static void Main ( string[] args )
		{
			Trace.Listeners.Add( new ColoredTraceListener() );

			using ( var game = new TestGame() ) {
				game.Run(args);
			}
		}
	}
}
