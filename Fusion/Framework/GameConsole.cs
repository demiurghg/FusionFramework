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
using Fusion.Core.Configuration;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;

namespace Fusion.Framework {
	
	public class GameConsole : GameModule {

		//readonly GameEngine gameEngine;
		[Config]
		public GameConsoleConfig Config { get; set; }


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


		SpriteFont	consoleFont;
		DiscTexture consoleBackground;
		SpriteLayer consoleLayer;
		SpriteLayer editLayer;
		

		float showFactor = 0;
		string font;
		string conback;

		EditBox	editBox;


		int scroll = 0;

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
		public GameConsole ( GameEngine gameEngine, string font, string conback ) : base(gameEngine)
		{
			Config			=	new GameConsoleConfig();
			
			this.font		=	font;
			this.conback	=	conback;

			editBox		=	new EditBox();
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			consoleLayer	=	new SpriteLayer( GameEngine.GraphicsEngine, 1024 );
			editLayer		=	new SpriteLayer( GameEngine.GraphicsEngine, 1024 );
			consoleLayer.Order = 9999;
			consoleLayer.Layers.Add( editLayer );

			GameEngine.GraphicsEngine.SpriteLayers.Add( consoleLayer );

			LoadContent();
			GameEngine.Reloading += (s,e) => LoadContent();

			GameEngine.GraphicsDevice.DisplayBoundsChanged += GraphicsDevice_DisplayBoundsChanged;
			TraceRecorder.TraceRecorded += TraceRecorder_TraceRecorded;
			GameEngine.Keyboard.KeyDown += Keyboard_KeyDown;
			GameEngine.Keyboard.FormKeyPress += Keyboard_FormKeyPress;
			GameEngine.Keyboard.FormKeyDown += Keyboard_FormKeyDown;

			RefreshConsole();
			RefreshEdit();
		}


		int charHeight { get { return consoleFont.LineHeight; } }
		int charWidth { get { return consoleFont.SpaceWidth; } }


		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			consoleFont			=	GameEngine.Content.Load<SpriteFont>(font);
			consoleBackground	=	GameEngine.Content.Load<DiscTexture>(conback);
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
				SafeDispose( ref editLayer );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update ( GameTime gameTime )
		{
			var vp		=	GameEngine.GraphicsDevice.DisplayBounds;

			RefreshConsoleLayer();

			if (Show) {
				showFactor = MathUtil.Clamp( showFactor + Config.FallSpeed * gameTime.ElapsedSec, 0,1 );
			} else {															   
				showFactor = MathUtil.Clamp( showFactor - Config.FallSpeed * gameTime.ElapsedSec, 0,1 );
			}

			//Log.Message("{0} {1}", showFactor, Show);
			float offset	=	(int)(- (vp.Height / 2) * (1 - showFactor));

			consoleLayer.SetTransform( new Vector2(0, offset), Vector2.Zero, 0 );
			editLayer.SetTransform( 0, vp.Height/2 - 8 );

			Color cursorColor = Config.CmdLineColor;
			cursorColor.A = (byte)( cursorColor.A * (0.5 + 0.5 * Math.Cos( 2 * Config.CursorBlinkRate * Math.PI * gameTime.Total.TotalSeconds ) > 0.5 ? 1 : 0 ) );

			editLayer.Clear();

			consoleFont.DrawString( editLayer, "]" + editBox.Text, 0,0, Config.CmdLineColor );
			consoleFont.DrawString( editLayer, "_", charWidth + charWidth * editBox.Cursor, 0, cursorColor );
		}



		/// <summary>
		/// Refreshes edit box.
		/// </summary>
		void RefreshEdit ()
		{
		}


		bool dirty = true;


		void RefreshConsoleLayer ()
		{
			if (!dirty) {
				return;
			}

			var vp		=	GameEngine.GraphicsDevice.DisplayBounds;

			int cols	=	vp.Width / charWidth;
			int rows	=	vp.Height / charHeight / 2;

			int count = 1;

			consoleLayer.Clear();

			consoleLayer.Draw( consoleBackground, 0,0, vp.Width, vp.Height/2, Color.White );

			scroll	=	MathUtil.Clamp( scroll, 0, TraceRecorder.Lines.Count() );

			/*var info = gameEngine.GetReleaseInfo();
			consoleFont.DrawString( consoleLayer, info, vp.Width - consoleFont.MeasureString(info).Width, vp.Height/2 - 1 * charHeight, ErrorColor );*/


			foreach ( var line in TraceRecorder.Lines.Reverse().Skip(scroll) ) {

				Color color = Color.Gray;

				switch (line.EventType) {
					case TraceEventType.Information : color = Config.MessageColor; break;
					case TraceEventType.Error		: color = Config.ErrorColor;   break;
					case TraceEventType.Warning		: color = Config.WarningColor; break;
				}
				

				consoleFont.DrawString( consoleLayer, line.Message, 0, vp.Height/2 - (count+2) * charHeight, color );

				if (count>rows) {
					break;
				}

				count++;
			}

			dirty = false;
		}


		/// <summary>
		/// Refreshes console layer.
		/// </summary>
		void RefreshConsole ()
		{
			dirty	=	true;
		}




		void ExecCmd ()
		{
			try {
				var cmd  = editBox.Text;
				Log.Message("]{0}", cmd);
				GameEngine.Invoker.Push(cmd);
			} catch ( Exception e ) {
				Log.Error(e.Message);
			}
		}



		void TabCmd ()
		{
			editBox.Text = GameEngine.Invoker.AutoComplete( editBox.Text );
		}



		void Keyboard_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Key==Keys.OemTilde) {
				Show = !Show;
				return;
			}
		}


		void Keyboard_FormKeyDown ( object sender, KeyEventArgs e )
		{
			if (!Show) {
				return;
			}
			switch (e.Key) {
				case Keys.End		: editBox.Move(int.MaxValue/2); break;
				case Keys.Home		: editBox.Move(int.MinValue/2); break;
				case Keys.Left		: editBox.Move(-1); break;
				case Keys.Right		: editBox.Move( 1); break;
				case Keys.Delete	: editBox.Delete(); break;
				case Keys.Up		: editBox.Prev(); break;
				case Keys.Down		: editBox.Next(); break;
				case Keys.PageUp	: scroll += 2; dirty = true; break;
				case Keys.PageDown	: scroll -= 2; dirty = true; break;
			}

			RefreshEdit();
		}

		

		const char Backspace = (char)8;
		const char Enter = (char)13;
		const char Escape = (char)27;
		const char Tab = (char)9;


		void Keyboard_FormKeyPress ( object sender, KeyPressArgs e )
		{
			if (!Show) {
				return;
			}
			if (e.KeyChar=='`') {
				return;
			}
			switch (e.KeyChar) {
				case Backspace	: editBox.Backspace(); break;
				case Enter		: ExecCmd(); editBox.Enter(); break;
				case Escape		: break;
				case Tab		: TabCmd(); break;
				default			: editBox.TypeChar( e.KeyChar ); break;
			}

			//Log.Message("{0}", (int)e.KeyChar);

			RefreshEdit();
		}


		void TraceRecorder_TraceRecorded ( object sender, EventArgs e )
		{
			RefreshConsole();
			scroll	=	0;
		}


		void GraphicsDevice_DisplayBoundsChanged ( object sender, EventArgs e )
		{
			RefreshConsole();
		}
	}
}
