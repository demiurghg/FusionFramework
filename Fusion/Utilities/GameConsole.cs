using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Fusion.Core;
using Fusion.Core.Utils;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Drivers.Input;

namespace Fusion.Utilities {
	
	public class GameConsole : DisposableBase {

		readonly GameEngine gameEngine;


		class Line {
			public readonly TraceEventType EventType;
			public readonly string Message;

			public Line ( TraceEventType eventType, string message ) 
			{
				EventType	=	eventType;
				Message		=	message;
			}
		}
		
		List<string> lines = new List<string>();


		DiscTexture	consoleFont;
		DiscTexture consoleBackground;
		SpriteLayer consoleLayer;
		

		float showFactor = 0;
		string font;
		string conback;

		/// <summary>
		/// Console fall speed.
		/// </summary>
		public float Speed { get; set; }

		/// <summary>
		/// Show/Hide console.
		/// </summary>
		public bool Show { get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		/// <param name="font">Font texture. Must be 128x128.</param>
		/// <param name="conback">Console background texture</param>
		/// <param name="speed">Console fall speed</param>
		public GameConsole ( GameEngine gameEngine, string font, string conback, float speed )
		{
			this.gameEngine	=	gameEngine;
			this.font		=	font;
			this.conback	=	conback;
			this.Speed		=	speed;
		}



		/// <summary>
		/// 
		/// </summary>
		public void Initialize ()
		{
			consoleLayer	=	new SpriteLayer( gameEngine.GraphicsEngine, 1024 );
			consoleLayer.Order = 9999;

			gameEngine.GraphicsEngine.SpriteLayers.Add( consoleLayer );

			LoadContent();
			gameEngine.Reloading += (s,e) => LoadContent();

			gameEngine.GraphicsDevice.DisplayBoundsChanged += GraphicsDevice_DisplayBoundsChanged;
			TraceRecorder.TraceRecorded += TraceRecorder_TraceRecorded;

			gameEngine.InputDevice.KeyDown += InputDevice_KeyDown;

			Refresh();
		}

		void InputDevice_KeyDown ( object sender, InputDevice.KeyEventArgs e )
		{
			if (e.Key==Keys.OemTilde) {
				Show = !Show;
			}
			Log.Message("{0}", e.Key);
		}



		void TraceRecorder_TraceRecorded ( object sender, EventArgs e )
		{
			Refresh();
		}


		void Refresh ()
		{
			var vp		=	gameEngine.GraphicsDevice.DisplayBounds;

			int cols	=	vp.Width / 8;
			int rows	=	vp.Height / 16;

			int count = 1;

			consoleLayer.Clear();

			consoleLayer.Draw( consoleBackground, 0,0, vp.Width, vp.Height/2, Color.White );

			foreach ( var line in TraceRecorder.Lines.Reverse() ) {
				
				consoleLayer.DrawDebugString( consoleFont, 0, vp.Height/2 - (count + 1) * 8, line.Message, Color.White );

				if (count>rows) {
					break;
				}

				count++;
			}
		}



		void GraphicsDevice_DisplayBoundsChanged ( object sender, EventArgs e )
		{
			Refresh();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			//
			//	Build content on startup :
			//
			consoleFont			=	gameEngine.Content.Load<DiscTexture>(font);
			consoleBackground	=	gameEngine.Content.Load<DiscTexture>(conback);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
			var vp		=	gameEngine.GraphicsDevice.DisplayBounds;

			if (Show) {
				showFactor = MathUtil.Clamp( showFactor + Speed * gameTime.ElapsedSec, 0,1 );
			} else {															   
				showFactor = MathUtil.Clamp( showFactor - Speed * gameTime.ElapsedSec, 0,1 );
			}

			//Log.Message("{0} {1}", showFactor, Show);
			float offset	=	(int)(- vp.Height / 2 * (1 - showFactor));

			consoleLayer.SetTransform( new Vector2(0, offset), Vector2.Zero, 0 );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				TraceRecorder.TraceRecorded -= TraceRecorder_TraceRecorded;
				SafeDispose( ref consoleLayer );

			}

			base.Dispose( disposing );
		}

	}
}
