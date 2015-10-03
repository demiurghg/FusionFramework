using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Shell.Commands {
	
	[Command("undo", "Undo")]
	public class Undo : Command {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public Undo ( Invoker invoker ) : base(invoker)
		{
		}


		/// <summary>
		/// Force game to exit.
		/// </summary>
		public override void Execute ()
		{
			Invoker.Undo();
		}



		/// <summary>
		/// No rollback.
		/// </summary>
		public override void Rollback ()
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// No rollback.
		/// </summary>
		[CommandLineParser.Ignore]
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
