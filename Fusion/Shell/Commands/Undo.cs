using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Shell.Commands {
	
	[Command("undo", "Undo")]
	public class Undo : Command {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public Undo ( Game game ) : base(game)
		{
		}


		/// <summary>
		/// Force game to exit.
		/// </summary>
		public override void Execute ( Invoker invoker )
		{
			invoker.Undo();
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
