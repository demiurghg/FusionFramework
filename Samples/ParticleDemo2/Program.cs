using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Core.Content;
using Fusion.Core.Development;
using Fusion.Build;
using Fusion.Engine.Common;

namespace ParticleDemo2 {
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

			using (var game = new ParticleDemo2()) {
				//if (DevCon.Prepare( game, @"..\..\..\Content\Content.xml", "Content" )) {
					game.Run( args );
				//}
			}
		}
	}
}
