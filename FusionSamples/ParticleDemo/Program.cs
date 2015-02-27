using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;

namespace ParticleDemo {
	class Program {

		[STAThread]
		static void Main ( string[] args )
		{
			using ( var game = new ParticleDemo() ) {
				if (Fusion.Development.DevCon.Prepare(game, @"..\..\..\Content\Content.xml", "Content")) {
					game.Run(args);
				}
			}
		}
	}
}
