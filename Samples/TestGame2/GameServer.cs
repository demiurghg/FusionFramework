using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;

namespace TestGame2 {
	class GameServer : IGameServer {


		public GameEngine Engine { get { return engine; } }
		readonly GameEngine engine;

			
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public GameServer ( GameEngine engine )
		{
			this.engine	=	engine;
		}


		/// <summary>
		/// Starts server with given map/level.
		/// </summary>
		/// <param name="map"></param>
		public void Start ( string map )
		{
		}

		/// <summary>
		/// Runs one step of server-side world simulation.
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
		}

		/// <summary>
		/// Gets world snapshot for particular client.
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public byte[] GetSnapshot ( int clientId = -1 )
		{
			return null;
		}

		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		public void FeedCommand ( UserCmd[] commands, int clientId )
		{
		}
	}
}
