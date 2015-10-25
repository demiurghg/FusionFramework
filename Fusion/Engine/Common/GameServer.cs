using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;

namespace Fusion.Engine.Common {

	public abstract partial class GameServer : GameModule {

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
		/// Kills server, stops the game.
		/// This method will be also called when server crashes to clean-up.
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

	}
}
