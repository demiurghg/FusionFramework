using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Common {

	public abstract class GameInterface : GameModule {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		public GameInterface ( GameEngine gameEngine ) : base(gameEngine)
		{
		}

		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public abstract void Update ( GameTime gameTime );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public abstract void ShowMessage ( string message );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public abstract void ShowWarning ( string message );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public abstract void ShowError ( string message );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public abstract void ChatMessage ( string message );
	}
}
