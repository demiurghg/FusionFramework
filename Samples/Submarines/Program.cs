using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion;
using Fusion.Development;

namespace SubmarinesWars
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
			Trace.Listeners.Add( new ColoredTraceListener() );
            
            using (var game = new SubmarinesWars()) {
                //Console.WriteLine(args.Length);
                if (args.Length != 0)
					game.Run(args);
                else {
					game.Run(args);
				}
            }
        }
    }
}
