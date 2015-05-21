using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Shell {
	public class Invoker {

		/// <summary>
		/// Game reference.
		/// </summary>
		public Game Game { get; private set; }

		Dictionary<string, Type> commands;


		object lockObject = new object();

		Queue<Command> queue	= new Queue<Command>();
		Queue<Command> delayed	= new Queue<Command>();
		Stack<Command> history	= new Stack<Command>();



		/// <summary>
		/// 
		/// </summary>
		public Invoker ( Game game )
		{
			Game		=	game;
			commands	=	Command.GatherCommands().ToDictionary( type => type.GetCustomAttribute<CommandAttribute>().Name );

			Log.Message("Invoker: {0} commands found", commands.Count);
		}

		

		/// <summary>
		/// Executes given string.
		/// </summary>
		/// <param name="command"></param>
		public void Push ( string commandLine )
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

				queue.Enqueue( command );
			}
		}

		

		/// <summary>
		/// Executes given string.
		/// </summary>
		/// <param name="command"></param>
		public void Push ( Command command )
		{
			lock (lockObject) {
				queue.Enqueue( command );
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

				delayed.Clear();

				while (queue.Any()) {
					
					var cmd = queue.Dequeue();

					if (cmd.Delay<=0) {
						//	execute :
						cmd.Execute( this );

						//	break execution :
						if (cmd.Terminal) {
							queue.Clear();
							break;
						}

						//	push to history :
						if (!cmd.NoRollback) {
							history.Push( cmd );
						}
					} else {

						cmd.Delay -= delta;

						delayed.Enqueue( cmd );

					}

				}

				Misc.Swap( ref delayed, ref queue );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public void Undo ()
		{
			lock (lockObject) {

				if (!history.Any()) {
					throw new Exception("No more commands to undo");
				}

				var cmd = history.Pop();
				cmd.Rollback( this );
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
