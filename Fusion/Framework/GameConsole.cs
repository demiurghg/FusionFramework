﻿using System;
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
using Fusion.Engine.Input;

namespace Fusion.Framework {
	
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
		/// Console fall speed.
		/// </summary>
		public float Speed { get; set; }

		/// <summary>
		/// Show/Hide console.
		/// </summary>
		public bool Show { get; set; }


		public float CursorBlinkRate { get; set; }

		public Color MessageColor	{ get; set; }
		public Color ErrorColor		{ get; set; }
		public Color WarningColor	{ get; set; }
		public Color CmdLineColor	{ get; set; }



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
			CursorBlinkRate	=	2.0f;

			editBox		=	new EditBox();

			MessageColor	=	Color.White;
			ErrorColor		=	Color.Red;
			WarningColor	=	Color.Yellow;
			CmdLineColor	=	Color.Orange;
		}



		/// <summary>
		/// 
		/// </summary>
		public void Initialize ()
		{
			consoleLayer	=	new SpriteLayer( gameEngine.GraphicsEngine, 1024 );
			editLayer		=	new SpriteLayer( gameEngine.GraphicsEngine, 1024 );
			consoleLayer.Order = 9999;
			consoleLayer.Layers.Add( editLayer );

			gameEngine.GraphicsEngine.SpriteLayers.Add( consoleLayer );

			LoadContent();
			gameEngine.Reloading += (s,e) => LoadContent();

			gameEngine.GraphicsDevice.DisplayBoundsChanged += GraphicsDevice_DisplayBoundsChanged;
			TraceRecorder.TraceRecorded += TraceRecorder_TraceRecorded;
			gameEngine.Keyboard.KeyDown += Keyboard_KeyDown;
			gameEngine.Keyboard.FormKeyPress += Keyboard_FormKeyPress;
			gameEngine.Keyboard.FormKeyDown += Keyboard_FormKeyDown;

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
			consoleFont			=	gameEngine.Content.Load<SpriteFont>(font);
			consoleBackground	=	gameEngine.Content.Load<DiscTexture>(conback);
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
			var vp		=	gameEngine.GraphicsDevice.DisplayBounds;

			RefreshConsoleLayer();

			if (Show) {
				showFactor = MathUtil.Clamp( showFactor + Speed * gameTime.ElapsedSec, 0,1 );
			} else {															   
				showFactor = MathUtil.Clamp( showFactor - Speed * gameTime.ElapsedSec, 0,1 );
			}

			//Log.Message("{0} {1}", showFactor, Show);
			float offset	=	(int)(- (vp.Height / 2) * (1 - showFactor));

			consoleLayer.SetTransform( new Vector2(0, offset), Vector2.Zero, 0 );
			editLayer.SetTransform( 0, vp.Height/2 - 8 );

			Color cursorColor = CmdLineColor;
			cursorColor.A = (byte)( cursorColor.A * (0.5 + 0.5 * Math.Cos( 2 * CursorBlinkRate * Math.PI * gameTime.Total.TotalSeconds ) > 0.5 ? 1 : 0 ) );

			editLayer.Clear();

			consoleFont.DrawString( editLayer, "]" + editBox.Text, 0,0, CmdLineColor );
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
			var vp		=	gameEngine.GraphicsDevice.DisplayBounds;

			int cols	=	vp.Width / charWidth;
			int rows	=	vp.Height / charHeight / 2;

			int count = 1;

			consoleLayer.Clear();

			consoleLayer.Draw( consoleBackground, 0,0, vp.Width, vp.Height/2, Color.White );

			scroll	=	MathUtil.Clamp( scroll, 0, TraceRecorder.Lines.Count() );

			foreach ( var line in TraceRecorder.Lines.Reverse().Skip(scroll) ) {

				Color color = Color.Gray;

				switch (line.EventType) {
					case TraceEventType.Information : color = MessageColor; break;
					case TraceEventType.Error		: color = ErrorColor;	break;
					case TraceEventType.Warning		: color = WarningColor; break;
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
				gameEngine.Invoker.Push(cmd);
			} catch ( Exception e ) {
				Log.Error(e.Message);
			}
		}



		void TabCmd ()
		{
			editBox.Text = gameEngine.Invoker.AutoComplete( editBox.Text );
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
