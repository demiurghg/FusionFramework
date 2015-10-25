using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;

namespace TestGame2 {
	class CustomGameClient : Fusion.Engine.Common.GameClient {

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameClient ( GameEngine gameEngine )	: base(gameEngine)
		{
		}


		/// <summary>
		/// Initializes game
		/// </summary>
		public override void Initialize ()
		{
		}


		/// <summary>
		/// Called when connection request accepted by server.
		/// Client could start loading models, textures, models etc.
		/// </summary>
		/// <param name="map"></param>
		public override void Connect ( string host, int port )
		{
		}

		/// <summary>
		///	Called when client disconnected, dropped, kicked or timeouted.
		///	Client must purge all level-associated content.
		///	Reason???
		/// </summary>
		public override void Disconnect ()
		{
		}

		/// <summary>
		/// Runs one step of client-side simulation and render world state.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void UpdateGfx (GameTime gameTime )
		{
		}



		/// <summary>
		/// Feed server snapshot to client.
		/// Called when fresh snapshot arrived.
		/// </summary>
		/// <param name="snapshot"></param>
		public override void FeedSnapshot ( byte[] snapshot ) 
		{
		}

		/// <summary>
		/// Gets command from client.
		/// Returns null if no command is available.
		/// Called on each frame. 
		/// This method should not send command twice, 
		/// i.e. after call command must be purged.
		/// </summary>
		/// <returns></returns>
		public override UserCmd[] GetCommands ()
		{
			return null;
		}
	}
}
