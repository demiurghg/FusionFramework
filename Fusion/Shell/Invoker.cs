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


		/// <summary>
		/// Invoker's context object to target invoker and commands to particular object.
		/// </summary>
		public object Context { get; private set; }

		Dictionary<string, Type> commands;

		object lockObject = new object();

		Queue<Command> queue	= new Queue<Command>();
		Queue<Command> delayed	= new Queue<Command>();
		Stack<Command> history	= new Stack<Command>();



		/// <summary>
		/// Creates instance of Invoker.
		/// </summary>
		/// <param name="game">Game instance</param>
		public Invoker ( Game game )
		{
			Initialize( game, Command.GatherCommands() );
		}



		/// <summary>
		/// Creates instance of Invoker with specified command types.
		/// </summary>
		/// <param name="game">Game instance</param>
		/// <param name="types">Specified Command types</param>
		/// <param name="context">Invoker's context</param>
		public Invoker ( Game game, Type[] types, object context = null )
		{
			Initialize( game, types );
			Context = context;
		}

		

		void Initialize ( Game game, Type[] types )
		{
			Context		=	null;
			Game		=	game;
			commands	=	types
						.Where( t1 => t1.IsSubclassOf(typeof(Command)) )
						.Where( t2 => t2.HasAttribute<CommandAttribute>() )
						.ToDictionary( t3 => t3.GetCustomAttribute<CommandAttribute>().Name );
						
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
			} else {
				Log.Message("> {0}", commandLine );
			}

			var cmdName	=	argList[0];
			argList		=	argList.Skip(1).ToArray();
			

			lock (lockObject) {
				var command	=	GetCommand( cmdName );

				var parser	=	new CommandLineParser( command, true, cmdName );

				parser.ParseCommandLine( argList );

				Push( command );
			}
		}

		

		/// <summary>
		/// Executes given string.
		/// </summary>
		/// <param name="command"></param>
		public void Push ( Command command )
		{
			lock (lockObject) {
				if (queue.Any() && queue.Last().Terminal) {
					Log.Warning("Attempt to push command after terminal one. Ignored.");
					return;
				}
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
						//if (cmd.Terminal) {
						//	queue.Clear();
						//	break;
						//}

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
			
			throw new InvalidOperationException(string.Format("Unknown command '{0}'.", name));
		}
	}
}
