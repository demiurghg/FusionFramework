using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;

namespace Fusion.Engine.Common {

	public abstract class GameServer : GameModule {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		public GameServer ( GameEngine gameEngine ) : base(gameEngine)
		{
		}

		/// <summary>
		/// Starts server with given map/level.
		/// </summary>
		/// <param name="map"></param>
		public abstract void Start ( string map );

		/// <summary>
		/// Kills server, stops the game
		/// </summary>
		/// <param name="map"></param>
		public abstract void Kill ();

		/// <summary>
		/// Runs one step of server-side world simulation.
		/// </summary>
		/// <param name="gameTime"></param>
		public abstract void Update ( GameTime gameTime );

		/// <summary>
		/// Gets world snapshot for particular client.
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public abstract byte[] GetSnapshot ( int clientId = -1 );

		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		public abstract void FeedCommand ( UserCmd[] commands, int clientId );


		/*---------------------------------------------------------------------
		 * 
		 *	Internal server stuff :
		 * 
		---------------------------------------------------------------------*/

		NetServer server;

		Task serverTask;
		CancellationTokenSource cancelToken;



		internal void StartInternal ( string map, string postCommand )
		{
			if (serverTask!=null) {
				throw new GameException("Server is running or initializing");
			}

			serverTask = new Task( () => ServerTaskFunc(map, postCommand) );
			serverTask.Start();
		}



		/// <summary>
		/// 
		/// </summary>
		internal void KillInternal (bool wait)
		{
			if (cancelToken==null) {
				return;
			}

			cancelToken.Cancel();

			if (wait) {
				Log.Message("Waiting for server task...");
				serverTask.Wait();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="map"></param>
		void ServerTaskFunc ( string map, string postCommand )
		{
			//
			//	configure & start server :
			//
			var	peerCfg = new NetPeerConfiguration("Server");

			peerCfg.Port				=	GameEngine.Network.Config.Port;
			peerCfg.MaximumConnections	=	GameEngine.Network.Config.MaxClients;

			server	=	new NetServer( peerCfg );
			server.Start();

			//
			//	start game specific stuff :
			//
			Start( map );


			Log.Message("Server started : port = {0}", peerCfg.Port );


			//
			//	invoke post-start command :
			//
			if (postCommand!=null) {
				GameEngine.Invoker.Push( postCommand );
			}

			cancelToken	=	new CancellationTokenSource();

			var incomingMessages = new List<NetIncomingMessage>();

			//
			//	do stuff :
			//	
			while ( !cancelToken.IsCancellationRequested ) {

				Thread.Sleep(500);
				Log.Message(".");

				int newIM = server.ReadMessages( incomingMessages );

				foreach ( var im in incomingMessages ) {
					Log.Message("SV: {0}", im.ToString() );
				}

				incomingMessages.Clear();

			}

			server.Shutdown("kill");

			//
			//	kill game specific stuff
			//
			Kill();

			serverTask	=	null;
		}
	}
}
