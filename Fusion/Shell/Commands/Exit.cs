using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Shell.Commands {
	
	[Command("exit", "Exit")]
	public class Exit : Command {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public Exit ( Game game ) : base(game)
		{
		}


		/// <summary>
		/// Force game to exit.
		/// </summary>
		public override void Execute ( Invoker invoker )
		{
			Game.Exit();
		}



		/// <summary>
		/// No rollback.
		/// </summary>
		public override void Rollback ( Invoker invoker )
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// No rollback.
		/// </summary>
		public override bool NoRollback
		{
			get	{
				return true;
			}
			set	{
			}
		}
		
	}
}
