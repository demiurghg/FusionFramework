using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Core.Content;
using Fusion.Core.Development;
using Fusion.Engine.Common;
using System.Runtime.InteropServices;
using System.Web;


namespace SpriteDemo {
	class SpriteDemo : Game {

		Texture2D	texture;
		Texture2D	texture2;
		SpriteFont	font1;
		SpriteFont	font2;

		Texture2D	dynamicTexture;

		RenderTarget2D	rt;
		RenderTarget2D	rtMS;
		DepthStencil2D	dsMS;
		TextureAtlas	яatlas;


		/// <summary>
		///	Add services and set options
		/// </summary>
		public SpriteDemo ()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;

			//	uncomment to enable debug graphics device:
			//	(MS Platform SDK must be installed)
			//	Parameters.UseDebugDevice	=	true;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );


			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += SpriteDemo_Exiting;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void SpriteDemo_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				if (dynamicTexture!=null) {
					dynamicTexture.Dispose();
				}
				if (rt!=null) {
					rt.Dispose();
				}
				if (rtMS!=null) {
					rtMS.Dispose();
				}
				if (dsMS!=null) {
					dsMS.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		/// <summary>
		/// Load stuff here
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			LoadContent();

			rt			=	new RenderTarget2D( GraphicsDevice, ColorFormat.Rgba8, 256,256, 1 );
			rtMS		=	new RenderTarget2D( GraphicsDevice, ColorFormat.Rgba8, 256,256, 4 );
			dsMS		=	new DepthStencil2D( GraphicsDevice, DepthFormat.D24S8, 256,256, 4 );

			dynamicTexture	=	new Texture2D( GraphicsDevice, 64,64, ColorFormat.Rgba8, false );

			Reloading += (s,e) => LoadContent();

			InputDevice.KeyDown += InputDevice_KeyDown;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F2) {
				Parameters.ToggleVSync();
			}

			if (e.Key == Keys.F5) {
				Reload();
			}

			if (e.Key == Keys.F12) {
				GraphicsDevice.Screenshot();
			}

			if (e.Key == Keys.Escape) {
				Exit();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			texture		=	Content.Load<Texture2D>("lena");
			texture2	=	Content.Load<Texture2D>("manul" );
			font1		=	Content.Load<SpriteFont>("segoe80");
			font2		=	Content.Load<SpriteFont>("segoe40");

			//atlas		=	Content.Load<TextureAtlas>("sprites");

			/*foreach ( var name in atlas.SubImageNames ) {
				Log.Message( "...{0}", name );
			} */
		}



		/// <summary>
		/// Update stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds = GetService<DebugStrings>();

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );
			ds.Add("");

			if ( InputDevice.IsKeyDown( Keys.F1 ) ) {
				//DevCon.Show(this);
			}

			if ( InputDevice.IsKeyDown( Keys.F5 ) ) {
				Reload();
			}

			if ( InputDevice.IsKeyDown( Keys.PageUp ) ) {
				numSprites += 100;
			}
			if ( InputDevice.IsKeyDown( Keys.PageDown ) ) {
				numSprites -= 100;
			}

			numSprites	=	Math.Max( numSprites, 0 );

			angle	+=	 gameTime.ElapsedSec * MathUtil.DegreesToRadians( 15.0f );
			offset	+=	 gameTime.ElapsedSec * 0.75f;

			base.Update( gameTime );

			ds.Add( "PgUp - increase number of sprites" );
			ds.Add( "PgDn - decrease number of sprites" );

		}


		int numSprites = 100;
		Random r = new Random();

		float angle = 0;
		float offset = 0;
		float atlasId = 0;


		/// <summary>
		/// Draw stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			var sb = GetService<SpriteBatch>();
			var rand = new Random();

			var data = Enumerable.Range(0,64*64)
						.Select( i => rand.NextColor() )
						.ToArray();

			dynamicTexture.SetData(0, null, data, 0, 64*64 );


			GraphicsDevice.SetTargets( dsMS.Surface, rtMS.Surface );
			GraphicsDevice.Clear( dsMS.Surface );
			GraphicsDevice.Clear( rtMS.Surface, Color4.Black );
			

			sb.Begin(SpriteBlend.AlphaBlend,null,null, Matrix.OrthoRH(256,256,-1,1));
				sb.DrawSprite( texture, 0,0,256, angle, Color.White );
			sb.End();

			GraphicsDevice.Resolve( rtMS, rt );


			GraphicsDevice.RestoreBackbuffer();


			int w  = GraphicsDevice.DisplayBounds.Width;
			int h  = GraphicsDevice.DisplayBounds.Height;

			//	begin sprite gathering/accumulation :
			sb.Begin();

				if (InputDevice.IsKeyDown(Keys.T)) {
					sb.Draw( texture2, 0,          0, 256, 256, Color.White	);
					sb.Draw( texture , 256+128*0,  0, 128, 128, Color.Red	);
					sb.Draw( texture2, 256+128*1,  0, 128, 128, Color.Lime	);
					sb.Draw( texture , 256+128*2,  0, 128, 128, Color.Blue	);
				} else {
					sb.Draw( rt, 0,          0, 256, 256, Color.White	);
					sb.Draw( rt, 256+128*0,  0, 128, 128, Color.Red	);
					sb.Draw( rt, 256+128*1,  0, 128, 128, Color.Lime	);
					sb.Draw( rt, 256+128*2,  0, 128, 128, Color.Blue	);
				}

			sb.Restart(SpriteBlend.AlphaBlend, SamplerState.PointClamp);
				sb.DrawSprite( texture, 576,192,128, angle, Color.White );


			sb.Restart(SpriteBlend.AlphaBlend, SamplerState.LinearClamp);
				sb.DrawSprite( texture, 704,192,128, angle, Color.White );


			sb.Restart(SpriteBlend.AlphaBlend, SamplerState.PointClamp);
				sb.DrawSprite( texture2, 576,192+128,128, angle, Color.White );


			sb.Restart(SpriteBlend.AlphaBlend, SamplerState.LinearClamp);
				sb.DrawSprite( texture2, 704,192+128,128, angle, Color.White );


			sb.Restart();

				for (int i=0; i<numSprites; i++) {
					//sb.DrawSprite( sb.TextureWhite, r.Next(w), r.Next(h), 7, r.NextFloat(0,360), r.NextColor() ); 
					sb.Draw( sb.TextureWhite, r.Next(w), r.Next(h), 7, 7, r.NextColor() ); 
				}

				var h1 = font1.LineHeight;
				var h2 = font2.LineHeight;

				font1.DrawString( sb, "Lenna Soderberg",      64, 256 + h1,      Color.White  );
				font2.DrawString( sb, "Text (tracking = 0)",  64, 256 + h1+h2,   Color.Red );
				font2.DrawString( sb, "Text (tracking = 2)",  64, 256 + h1+h2*2, Color.Lime, 2 );
				font2.DrawString( sb, "Text (tracking = 4)", 64, 256 + h1+h2*3, Color.Lime, 4 );

				font2.DrawString( sb, string.Format("{1} sprites -> FPS = {0}", gameTime.Fps, numSprites), 64, 256 + h1+h2*4, Color.White );


			sb.Restart( SpriteBlend.Additive );
				sb.Draw( sb.TextureWhite,   256, 128, 128, 128, Color.Gray	);
				sb.Draw( texture, 256, 128, 128, 128, Color.Gray	);


			sb.Restart( SpriteBlend.AlphaBlend );
				sb.Draw( sb.TextureWhite, 256+128, 128, 128, 128, new Color(240,225,160,255)	);


			sb.Restart( SpriteBlend.NegMultiply );
				sb.Draw( texture, 256+128, 128, 128, 128, Color.White	);


			sb.Restart(SpriteBlend.AlphaBlend, SamplerState.AnisotropicWrap);
				sb.DrawBeam( texture2, new Vector2(350,540), new Vector2(750,415), Color.Yellow, Color.LightYellow, 80, 5.5f, offset );

			sb.Restart(SpriteBlend.AlphaBlend, SamplerState.PointClamp);
				sb.Draw( dynamicTexture, 0,0,256,256,new Color(255,255,255,64) );

				sb.Draw( sb.TextureBlack,	0,0,8,8, Color.White);
				sb.Draw( sb.TextureBlue,	8,0,8,8, Color.White);
				sb.Draw( sb.TextureGreen, 	16,0,8,8, Color.White);
				sb.Draw( sb.TextureRed,		24,0,8,8, Color.White);
				sb.Draw( sb.TextureWhite,	32,0,8,8, Color.White);

			sb.Restart();

				/*var name = atlas.SubImageNames[ ((int)atlasId) % atlas.SubImageNames.Length ];
				var ar = atlas.GetSubImageRectangle( name );

				sb.Draw( atlas.Texture, new Rectangle(10,400, ar.Width, ar.Height), ar, Color.White );
				sb.DrawDebugString( 10, 392, name, Color.White );

				atlasId += gameTime.ElapsedSec * 2;*/

			sb.End();

			base.Draw( gameTime, stereoEye );
		}
	}
}
