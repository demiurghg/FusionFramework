using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Engine.Common {
	public interface IGameClient {

		/// <summary>
		/// Called when connection request accepted by server.
		/// Client could start loading models, textures, models etc.
		/// </summary>
		/// <param name="map"></param>
		void Connect ( string map );

		/// <summary>
		///	Called when client disconnected, dropped, kicked or timeouted.
		///	Client must purge all level-associated content.
		///	Reason???
		/// </summary>
		void Disconnect ();

		/// <summary>
		/// Runs one step of client-side simulation and render world state.
		/// </summary>
		/// <param name="gameTime"></param>
		void Update ( GameTime gameTime );

		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// </summary>
		/// <param name="snapshot"></param>
		void FeedSnapshot ( byte[] snapshot );

		/// <summary>
		/// Gets command from client.
		/// Returns null if no command is available.
		/// Called on each frame. 
		/// This method should not send command twice, 
		/// i.e. after call command must be purged.
		/// </summary>
		/// <returns></returns>
		UserCmd[] GetCommands ();

	}
}
