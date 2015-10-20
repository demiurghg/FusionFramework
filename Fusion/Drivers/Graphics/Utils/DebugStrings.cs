using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using SharpDX;
using Fusion;
using Fusion.Core;
using Fusion.Core.Configuration;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {
	public class DebugStrings : GameService
	{
		struct Line {
			public	string	text;
			public	Color	color;
		}

		List<Line>			linesAccum	= new List<Line>();
		List<Line>			linesDraw = new List<Line>();


		[Config]
		public DebugStringsConfig Config { get; set; }


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="device"></param>
		/// <param name="fontPath"></param>
		public DebugStrings ( GameEngine game ) : base ( game )
		{
			Config	=	new DebugStringsConfig();
			RequireService<SpriteBatch>();
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}


		/// <summary>
		/// Adds string
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void Add ( string format, params object[] args )
		{
			Line line = new Line();
			line.text		= string.Format( format, args );
			line.color		= Color.White;
			linesAccum.Add( line );
		}



		/// <summary>
		/// Adds string
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void Add ( Color color, string format, params object[] args )
		{
			Line line = new Line();
			line.text		= string.Format( format, args );
			line.color		= color;
			linesAccum.Add( line );
		}



		/// <summary>
		/// Adds string
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void Add ( bool condition, string format, params object[] args )
		{
			if (condition) {
				Add( format, args );
			}
		}


		int maxWidth = 0;



		public override void Update ( GameTime gameTime )
		{
			Misc.Swap( ref linesAccum, ref linesDraw );
			linesAccum.Clear();
		}



		/// <summary>
		/// DrawGatheredStrings
		/// </summary>
		public override void Draw( GameTime gameTime, StereoEye stereoEye )
		{
			var x = 8;
			int y = 8;

			if (Config.SuppressDebugString) {
				linesAccum.Clear();
				return;
			}

			if (!linesDraw.Any()) {
				return;
			}
			
			var sb = GameEngine.GetService<SpriteBatch>();

			sb.Begin();
			
				int w = linesDraw.Max( line => line.text.Length ) * 8 + 16;
				int h = linesDraw.Count * 8 + 16;

				maxWidth	=	Math.Max( w, maxWidth );

				sb.Draw( sb.TextureWhite, new Rectangle(0,0, maxWidth, h), Config.BackgroundColor );

				foreach ( var line in linesDraw ) {
					//font.DrawString( rs.SpriteBatch, line.text, x, y, line.color );
					sb.DrawDebugString( x+1, y+1, line.text, Color.Black );
					sb.DrawDebugString( x+0, y+0, line.text, line.color );
					y += 8;
				}

			sb.End();
		}
	}
}
