using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Development;

namespace SubmarinesWars
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            
            using (var game = new SubmarinesWars()) {
                //Console.WriteLine(args.Length);
                if (args.Length != 0)
                    game.Run(args);
                else
                    if (DevCon.Prepare(game, @"..\..\..\Content\Content.xml", "Content"))
                    {
                        game.Run(args);
                    }
            }
        }
    }
}
