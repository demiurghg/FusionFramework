using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Common {
	public interface IGameServer {

		/// <summary>
		/// Starts server with given map/level.
		/// </summary>
		/// <param name="map"></param>
		void Start ( string map );

		/// <summary>
		/// Runs one step of server-side world simulation.
		/// </summary>
		/// <param name="gameTime"></param>
		void Update ( GameTime gameTime );

		/// <summary>
		/// Gets world snapshot for particular client.
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		byte[] GetSnapshot ( int clientId = -1 );

		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		void FeedCommand ( UserCmd[] commands, int clientId );

	}
}
