using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Shell {
	public class Shell {

		/// <summary>
		/// Game reference.
		/// </summary>
		public Game Game { get; private set; }

		Dictionary<string, Type> commands;


		object lockObject = new object();

		List<Command> commandQueue = new List<Command>();
		List<Command> history = new List<Command>();



		/// <summary>
		/// 
		/// </summary>
		public Shell ( Game game )
		{
			Game		=	game;
			commands	=	Command.GatherCommands().ToDictionary( type => type.GetCustomAttribute<CommandAttribute>().Name );
		}

		

		/// <summary>
		/// Executes given string.
		/// </summary>
		/// <param name="command"></param>
		public void ExecuteString ( string commandLine )
		{
			var argList	=	CommandLineParser.SplitCommandLine( commandLine ).ToArray();

			if (!argList.Any()) {
				Log.Warning("Empty command line.");
				return;
			}

			var cmdName	=	argList[0];
			argList		=	argList.Skip(1).ToArray();
			

			lock (lockObject) {
				var command	=	GetCommand( cmdName );

				var parser	=	new CommandLineParser( command, true, cmdName );

				parser.ParseCommandLine( argList );

				commandQueue.Add( command );
			}
		}
		


		/// <summary>
		/// Updates shell, executes enqueued commands.
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
			var delta = (int)gameTime.Elapsed.TotalMilliseconds;

			lock (lockObject) {

				// for each non-delayed commands.
				foreach ( var cmd in commandQueue ) {

					if (cmd.Delay<=0) {
						
						//	execute :
						cmd.Execute();

						//	break execution :
						if (cmd.Terminal) {
							break;
							commandQueue.Clear();
						}

						//	push to history :
						if (!cmd.NoRollback) {
							history.Add( cmd );
						}
					}
				}

				commandQueue.RemoveAll( cmd => cmd.Delay <= 0 );

				var toExecute = commandQueue.Where( cmd => cmd.Delay <= 0 );
			
				foreach ( var cmd in commandQueue ) {
					cmd.Delay -= delta;
				} 
			}
		}



		/// <summary>
		/// Purges all histiry.
		/// </summary>
		public void PurgeHistory ()
		{
			lock (lockObject) {
				history.Clear();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		Command GetCommand ( string name )
		{
			Type cmdType;

			if (commands.TryGetValue( name, out cmdType )) {
				return (Command)Activator.CreateInstance( cmdType, Game );
			}
			
			throw new InvalidOperationException(string.Format("Command {0} not found in current app domain assemblies.", name));
		}
	}
}
