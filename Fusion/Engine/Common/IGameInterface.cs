using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Common {

	public interface IGameInterface {

		/// <summary>
		/// Called after all the systems have been initialized.
		/// </summary>
		void Initialize ();

		/// <summary>
		/// Shuts down user interface. 
		/// Dispose disposable objects here.
		/// </summary>
		void Shutdown ();

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
		void DrawInterfaceREMOVE ( GameTime gameTime, StereoEye stereoEye );

		/// <summary>
		/// Draws splash screen.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		void DrawSplashScreenREMOVE ( GameTime gameTime, StereoEye stereoEye );

		/// <summary>
		/// Draws loading screen.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <param name="progress"></param>
		void DrawLoadingScreenREMOVE ( GameTime gameTime, StereoEye stereoEye, float progress );


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
