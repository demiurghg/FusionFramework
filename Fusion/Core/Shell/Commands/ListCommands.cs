using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Shell.Commands {
	
	[Command("listCmds", "Exit")]
	public class ListCommands : Command {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public ListCommands ( Invoker invoker ) : base(invoker)
		{
		}


		/// <summary>
		/// Force game to exit.
		/// </summary>
		public override void Execute ()
		{
			Log.Message("Commands:");

			var list = Invoker.CommandList;
			
			foreach ( var name in list ) {
				Log.Message("  {0}", name );
			}
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
