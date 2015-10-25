using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using System.Threading;


namespace Fusion.Engine.Common {
	public abstract partial class GameClient : GameModule {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		public GameClient ( GameEngine gameEngine ) : base(gameEngine) 
		{
		}

		/// <summary>
		/// Called when connection request accepted by server.
		/// Client could start loading models, textures, models etc.
		/// </summary>
		/// <param name="host"></param>
		public abstract void Connect ( string host, int port );

		/// <summary>
		///	Called when client disconnected, dropped, kicked or timeouted.
		///	Client must purge all level-associated content.
		///	Reason???
		/// </summary>
		public abstract void Disconnect ();

		/// <summary>
		/// Runs one step of client-side simulation and render world state.
		/// </summary>
		/// <param name="gameTime"></param>
		public abstract void Update ( GameTime gameTime );

		/// <summary>
		/// Performs update of visual and audial objects.
		/// The method is synchronized with UI/Graphics thread.
		/// </summary>
		/// <param name="gameTime"></param>
		public abstract void UpdateGfx ( GameTime gameTime );

		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// </summary>
		/// <param name="snapshot"></param>
		public abstract void FeedSnapshot ( byte[] snapshot );

		/// <summary>
		/// Gets command from client.
		/// Returns null if no command is available.
		/// Called on each frame. 
		/// This method should not send command twice, 
		/// i.e. after call command must be purged.
		/// </summary>
		/// <returns></returns>
		public abstract UserCmd[] GetCommands ();
	}
}
