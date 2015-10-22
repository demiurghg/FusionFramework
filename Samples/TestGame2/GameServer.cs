using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;

namespace TestGame2 {
	class GameServer : Fusion.Engine.Common.GameServer {
			
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public GameServer ( GameEngine gameEngine ) : base(gameEngine)
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}


		/// <summary>
		/// Starts server with given map/level.
		/// </summary>
		/// <param name="map"></param>
		public override void Start ( string map )
		{
		}


		/// <summary>
		/// Kills server
		/// </summary>
		public override void Kill ()
		{
		}


		/// <summary>
		/// Runs one step of server-side world simulation.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
		}

		/// <summary>
		/// Gets world snapshot for particular client.
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public override byte[] GetSnapshot ( int clientId = -1 )
		{
			return null;
		}

		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		public override void FeedCommand ( UserCmd[] commands, int clientId )
		{
		}
	}
}
