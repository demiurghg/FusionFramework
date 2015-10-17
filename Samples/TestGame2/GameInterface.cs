using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace TestGame2 {
	class GameInterface : IGameInterface {

		public GameEngine Engine { get { return engine; } }
		readonly GameEngine engine;

			
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public GameInterface ( GameEngine engine )
		{
			this.engine	=	engine;
		}


		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
		}

		/// <summary>
		/// Draws interface.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public void DrawInterface ( GameTime gameTime, StereoEye stereoEye )
		{
		}

		/// <summary>
		/// Draws splash screen.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public void DrawSplashScreen ( GameTime gameTime, StereoEye stereoEye )
		{
		}

		/// <summary>
		/// Draws loading screen.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <param name="progress"></param>
		public void DrawLoadingScreen ( GameTime gameTime, StereoEye stereoEye, float progress )
		{
		}


		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public void ShowMessage ( string message )
		{
		}

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public void ShowWarning ( string message )
		{
		}

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public void ShowError ( string message )
		{
		}

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public void ChatMessage ( string message )
		{
		}
	}
}
