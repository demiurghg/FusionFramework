using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Development;
using Fusion.Graphics;

using Fusion.Input;
using Fusion.Mathematics;


namespace TexPumpDemo {
	public class TexPumpDemo : Game {

		string[] images;

		int offset = 0;

		Texture2D	texLoading;
		Texture2D	texFailed;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public TexPumpDemo () 
		{
			AddService( new SpriteBatch(this) );
			AddService( new DebugStrings(this), true, true, 0, 9999 );
			AddService( new WebDownloaderService(this));
			AddService( new TexturePump(this), true, false, 0, 0 );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		protected override void Initialize ()
		{
			// If file with Instagram data exists get urls from it
			var filePath	= "export_VO.csv";
			
			var str			= new StreamReader(File.OpenRead(filePath));
			var urls		= new List<string>();

			while (!str.EndOfStream) {
				urls.Add(str.ReadLine().Split(',')[5]);
			}

			str.Close();

			images = urls.ToArray();

			texFailed	=	Content.Load<Texture2D>("failed2");
			texLoading	=	Content.Load<Texture2D>("loading2");

			base.Initialize();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds = GetService<DebugStrings>();
			ds.Add(Color.Orange, "FPS = {0}", gameTime.Fps );
			ds.Add("========================================================");
			ds.Add("F1     - open editor");
			ds.Add("F5     - reload textures");
			ds.Add("F6     - purge ubershader cache");
			ds.Add("ESC    - exit");
			ds.Add("========================================================");

			ds.Add("Total images : {0}", images.Length );


			if ( InputDevice.IsKeyDown( Keys.F1 ) ) {
				DevCon.Show(this);
			}

			//	exit on ESC :
			if ( InputDevice.IsKeyDown( Keys.Escape ) ) {
				Exit();
			}


			if ( InputDevice.IsKeyDown( Fusion.Input.Keys.Right ) ) {
				offset += InputDevice.IsKeyDown( Keys.LeftShift ) ? 10 : 1;
			}

			if ( InputDevice.IsKeyDown( Fusion.Input.Keys.Left ) ) {
				offset -= InputDevice.IsKeyDown( Keys.LeftShift ) ? 10 : 1;
			}


			if ( InputDevice.IsKeyDown( Keys.Space ) ) {
				GetService<TexturePump>().RemoveAllFailedTextures();
			}

			base.Update( gameTime );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			var sb		=	GetService<SpriteBatch>();
			var tp		=	GetService<TexturePump>();
			var w		=	GraphicsDevice.DisplayBounds.Width;
			var h		=	GraphicsDevice.DisplayBounds.Height;

			int imgWidth	= 128;
			int imgHeight	= 128;

			int cols	=	w / imgWidth;
			int rows	=	h / imgHeight;

			offset		=	Math.Max( 0, offset );
			offset		=	Math.Min( offset, cols * imgWidth );

			sb.Begin();

				for (int i = 0; i < images.Length; i++) {

					int x = ( i / rows ) * imgWidth - offset * 10;
					int y = ( i % rows ) * imgHeight; 

					if (x<-128 || x>w) {
						continue;
					}

					Texture2D	tex;
					var status	=	tp.Load(images[i], out tex);
					
					if (status == TexturePumpStatus.Loading) {
						sb.Draw( texLoading, x, y, imgWidth, imgHeight, Color.Blue );
					}
					if (status == TexturePumpStatus.Failed) {
						sb.Draw( texFailed, x, y, imgWidth, imgHeight, Color.Red );
					}
					if (status == TexturePumpStatus.Ready) {
						sb.Draw( tex, x+2, y+2, imgWidth-4, imgHeight-4, Color.White );
					}
				}

			sb.End();

			base.Draw( gameTime, stereoEye );
		}
	}
}
