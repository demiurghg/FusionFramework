using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Core;
using Fusion.Utilities;

namespace TestGame2 {
	class GameInterface : IGameInterface {

		readonly GameEngine gameEngine;

			
		SpriteLayer testLayer;
		DiscTexture	texture;
		DiscTexture	debugFont;
		GameConsole	gameConsole;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public GameInterface ( GameEngine gameEngine )
		{
			this.gameEngine		=	gameEngine;
			this.gameConsole	=	new GameConsole( gameEngine, "debugFont", "conback", 10);
		}



		float angle = 0;


		/// <summary>
		/// 
		/// </summary>
		public void Initialize ()
		{
			gameConsole.Initialize();

			testLayer	=	new SpriteLayer( gameEngine.GraphicsEngine, 1024 );
			texture		=	gameEngine.Content.Load<DiscTexture>( "lena" );
			debugFont	=	gameEngine.Content.Load<DiscTexture>( "debugFont" );

			testLayer.Clear();
			testLayer.Draw( texture, 10,10,256,256, Color.White );

			testLayer.DrawDebugString( debugFont, 10,276, "Lenna Soderberg", Color.White );

			gameEngine.GraphicsEngine.SpriteLayers.Add( testLayer );
		}



		public void Shutdown ()
		{
			DisposableBase.SafeDispose( ref gameConsole );
			DisposableBase.SafeDispose( ref testLayer );
		}


		Random rand = new Random();


		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
			gameConsole.Update( gameTime );

			if ( gameEngine.InputDevice.IsKeyDown(Keys.R) ) {
				testLayer.Clear();
				testLayer.DrawDebugString( debugFont, 10, 276, rand.Next().ToString(), Color.White );
			} 

			if ( gameEngine.InputDevice.IsKeyDown(Keys.C) ) {
				testLayer.Color = rand.NextColor(0,1,1);
			} 
			if ( gameEngine.InputDevice.IsKeyDown(Keys.W) ) {
				testLayer.Color = Color.White;
			} 

			if ( gameEngine.InputDevice.IsKeyDown(Keys.Left) ) {
				angle -= 0.01f;
			}
			if ( gameEngine.InputDevice.IsKeyDown(Keys.Right) ) {
				angle += 0.01f;
			}

			testLayer.SetTransform( new Vector2(100,0), new Vector2(128+5,128+5), angle );
		}

		/// <summary>
		/// Draws interface.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public void DrawInterfaceREMOVE ( GameTime gameTime, StereoEye stereoEye )
		{
		}

		/// <summary>
		/// Draws splash screen.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public void DrawSplashScreenREMOVE ( GameTime gameTime, StereoEye stereoEye )
		{
		}

		/// <summary>
		/// Draws loading screen.
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <param name="progress"></param>
		public void DrawLoadingScreenREMOVE ( GameTime gameTime, StereoEye stereoEye, float progress )
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
