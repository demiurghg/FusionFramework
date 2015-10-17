using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Common {

	public interface IGameInterface {

		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		void Update ( GameTime gameTime );

		/// <summary>
		/// Draws interface.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		void DrawInterface ( GameTime gameTime, StereoEye stereoEye );

		/// <summary>
		/// Draws splash screen.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		void DrawSplashScreen ( GameTime gameTime, StereoEye stereoEye );

		/// <summary>
		/// Draws loading screen.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <param name="progress"></param>
		void DrawLoadingScreen ( GameTime gameTime, StereoEye stereoEye, float progress );


		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		void ShowMessage ( string message );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		void ShowWarning ( string message );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		void ShowError ( string message );

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		void ChatMessage ( string message );
	}
}
