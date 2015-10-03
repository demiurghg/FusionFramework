using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Shell.Commands {
	
	[Command("help", "Exit")]
	public class Help : Command {


		[CommandLineParser.Required]
		public string CommandName { get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public Help ( Invoker invoker ) : base(invoker)
		{
		}


		/// <summary>
		/// Force game to exit.
		/// </summary>
		public override void Execute ()
		{
			try {
				var cmd = Invoker.GetCommand( CommandName );

				var parser = new CommandLineParser( cmd, cmd.Name );

				Log.Message("");
				Log.Message("Usage: {0} {1}", cmd.Name, string.Join(" ", parser.RequiredUsageHelp.ToArray() ));
				Log.Message("Options:");

				foreach ( var opt in parser.OptionalUsageHelp ) {
					Log.Message("   {0}", opt );
				}

			} catch ( Exception e ) {
				Log.Error( e.Message );
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
