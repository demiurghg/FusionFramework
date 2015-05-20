using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Shell {

	/// <summary>
	///		set /delay:1000 /norollback /var:"d3d_fullscr" /value:"true"
	/// </summary>
	public abstract class Command {

		public Game Game { get; private set; }

		/// <summary>
		/// Execute action
		/// </summary>
		public abstract void Execute ();

		/// <summary>
		/// Rollback action
		/// </summary>
		public abstract void Rollback ();


		/// <summary>
		/// Execution delay in milliseconds.
		/// </summary>
		[CommandLineParser.Name("delay")]
		public virtual int Delay { get; set; }


		/// <summary>
		/// Execution delay in milliseconds.
		/// </summary>
		[CommandLineParser.Name("norollback")]
		public virtual bool NoRollback { get; set; }


		/// <summary>
		/// Breaks execution of command queue.
		/// </summary>
		[CommandLineParser.Name("terminal")]
		public virtual bool Terminal { get; set; }


		/// <summary>
		/// 
		/// </summary>
		public Command ( Game game )
		{
			Game		=	game;
			Delay		=	0;
			NoRollback	=	false;
			Terminal	=	false;
		}


		/// <summary>
		/// Gets all avaialble commands.
		/// Command must be inherited from Command class and have attribute [Command]
		/// </summary>
		/// <returns></returns>
		internal static Type[] GatherCommands ()
		{
			return Misc.GetAllSubclassedOf( typeof(Command) )
				.Where( t => t.HasAttribute<CommandAttribute>() )
				.ToArray();
		}
	}
}
