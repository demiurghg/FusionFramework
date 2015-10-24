using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Input;
using Fusion.Engine.Graphics;
using Fusion.Core;
using Fusion.Core.Configuration;
using Fusion.Framework;

namespace TestGame2 {
	class CustomGameInterface : Fusion.Engine.Common.GameInterface {

		
		[Config]
		public UIConfig	UIConfig { get; set; }

		[GameModule("Console", "con")]
		public GameConsole Console { get { return console; } }
		public GameConsole console;
		
		SpriteLayer testLayer;
		DiscTexture	texture;
		DiscTexture	debugFont;




		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameInterface ( GameEngine gameEngine ) : base(gameEngine)
		{
			UIConfig			=	new UIConfig();
			console				=	new GameConsole( gameEngine, "courier", "conback");
		}



		float angle = 0;


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			testLayer	=	new SpriteLayer( GameEngine.GraphicsEngine, 1024 );
			texture		=	GameEngine.Content.Load<DiscTexture>( "lena" );
			debugFont	=	GameEngine.Content.Load<DiscTexture>( "debugFont" );

			testLayer.Clear();
			testLayer.Draw( texture, 10,10 + 384,256,256, Color.White );

			testLayer.DrawDebugString( debugFont, 10,276, "Lenna Soderberg", Color.White );

			GameEngine.GraphicsEngine.SpriteLayers.Add( testLayer );
		}



		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref testLayer );
			}
			base.Dispose( disposing );
		}


		Random rand = new Random();


		/// <summary>
		/// Updates internal state of interface.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			console.Update( gameTime );

			testLayer.Color	=	UIConfig.LenaColor;

			/*if ( gameEngine.Keyboard.IsKeyDown(Keys.R) ) {
				testLayer.Clear();
				testLayer.DrawDebugString( debugFont, 10, 276, rand.Next().ToString(), Color.White );
			} */

			if ( GameEngine.Keyboard.IsKeyDown(Keys.Left) ) {
				angle -= 0.01f;
			}
			if ( GameEngine.Keyboard.IsKeyDown(Keys.Right) ) {
				angle += 0.01f;
			}

			testLayer.SetTransform( new Vector2(100,0), new Vector2(128+5,128+5), angle );
		}


		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public override void ShowMessage ( string message )
		{
		}

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public override void ShowWarning ( string message )
		{
		}

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public override void ShowError ( string message )
		{
		}

		/// <summary>
		/// Shows message to user.
		/// </summary>
		/// <param name="message"></param>
		public override void ChatMessage ( string message )
		{
		}
	}
}
