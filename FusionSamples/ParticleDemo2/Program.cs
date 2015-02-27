using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Development;

namespace ParticleDemo2 {
	class Program {
		[STAThread]
		static void Main ( string[] args )
		{
			using (var game = new ParticleDemo2()) {
				if (DevCon.Prepare( game, @"..\..\..\Content\Content.xml", "Content" )) {
					game.Run( args );
				}
			}
		}
	}
}
