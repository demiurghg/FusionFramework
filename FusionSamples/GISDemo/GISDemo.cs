using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fusion;
using Fusion.GIS;
using Fusion.GIS.GlobeMath;
using Fusion.GIS.LayerSpace.Layers;
using Fusion.Mathematics;

using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;
using Fusion.UserInterface;

using DMath = Fusion.GIS.GlobeMath.DMathUtil;

namespace GISDemo {
	public class GISDemo : Game {


		/// <summary>
		/// GISDemo constructor
		/// </summary>
		public GISDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;
			Parameters.MsaaLevel	= 4;

			//	uncomment to enable debug graphics device:
			//	(MS Platform SDK must be installed)
			//	Parameters.UseDebugDevice	=	true;

			//	add services :
			AddService( new SpriteBatch( this ),	false,	false, 0, 9998 );
			AddService( new DebugStrings( this ),	true,	true, 0, 9999 );
			AddService( new LayerService(this),		true,	true, 1, 1 );

			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += Game_Exiting;
		}
		

		/// <summary>
		/// Initializes game :
		/// </summary>
		protected override void Initialize ()
		{
			//	initialize services :
			base.Initialize();

			//	add keyboard handler :
			InputDevice.KeyDown += InputDevice_KeyDown;

			//	load content & create graphics and audio resources here:
			LoadContent();
			Reloading += (s,e) => LoadContent();

			var gl = GetService<LayerService>().GlobeLayer;


			InputDevice.KeyDown		+= (sender, args) => gl.InputDeviceOnMouseDown(sender, new Frame.MouseEventArgs { X = (int)InputDevice.GlobalMouseOffset.X, Y = (int)InputDevice.GlobalMouseOffset.Y, Key = args.Key });
			InputDevice.MouseMove	+= (sender, args) => gl.InputDeviceOnMouseMove(sender, new Frame.MouseEventArgs { X = (int)InputDevice.GlobalMouseOffset.X, Y = (int)InputDevice.GlobalMouseOffset.Y });


			
			//gl.Dots[0] = new GlobeLayer.GeoVert {
			//	Lon = DMath.DegreesToRadians(30.306467),
			//	Lat = DMath.DegreesToRadians(59.944049),
			//	Color = Color.Red,
			//	Position = new Vector3(),
			//	Tex = new Vector4(1, 0, 0.01f, 0)
			//};

			//gl.DotsAddGeoObject(1, new DVector2(30.307467, 59.943049), Color.Tan, 0.05f);

			


			//gl.LinesPolyStart();
			//gl.LinesPolyAdd(new DVector2(30.306467, 59.944049), new DVector2(30.204678, 59.946543), Color.Green, 0.01f);
			//gl.LinesPolyEnd();
		}


		Dictionary<String, GlobeLayer.GeoVert[]> allDataSubway = new Dictionary<string, GlobeLayer.GeoVert[]>();
		SpriteFont sf;
		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
			sf = Content.Load<SpriteFont>(@"Fonts\titleFont");
			var gl = GetService<LayerService>().GlobeLayer;
			int counter = 0;
			string line;

			GlobeLayer.GeoVert[] basic = new GlobeLayer.GeoVert[124];
			// Read the file and display it line by line.
			//System.IO.StreamReader file = new System.IO.StreamReader(@"d:\narc.txt");
			//while((line = file.ReadLine()) != null)
			//{
			//	var str = line.Split(';');
				
			//		//System.Console.WriteLine(str[0]);
			//		basic[counter] =  new GlobeLayer.GeoVert {
			//			Lon = DMath.DegreesToRadians(Double.Parse(str[0])),
			//			Lat = DMath.DegreesToRadians(Double.Parse(str[1])),
			//			Color = new Color(73,97,69),
			//			Position = new Vector3(),
			//			Tex = new Vector4(3, 0,  0.1f, 0)
			//		};
					
			//		counter++;
				
			//}

			System.IO.StreamReader file = new System.IO.StreamReader( @"d:\coords.txt" );
			while ( ( line = file.ReadLine() ) != null ) {
				var str = line.Split( ',' );
				if ( !str[1].Equals( "0" ) ) {
					//System.Console.WriteLine(str[0]);
					basic[counter] = new GlobeLayer.GeoVert {
						Lon = DMath.DegreesToRadians( Double.Parse( str[2] ) ),
						Lat = DMath.DegreesToRadians( Double.Parse( str[1] ) ),
						Color = new Color( 207, 71, 60 ),
						Position = new Vector3(),
						Tex = new Vector4( 1, 0, 0.08f, 0 )
					};
					basic[++counter] = new GlobeLayer.GeoVert {
						Lon = DMath.DegreesToRadians( Double.Parse( str[2] ) ),
						Lat = DMath.DegreesToRadians( Double.Parse( str[1] ) ),
						Color = new Color( 23, 60, 120 ),
						Position = new Vector3(),
						Tex = new Vector4( 2, 0, 0.08f, 0 )
					};
					counter++;
				}
			}
			file.Close();
			
			Console.WriteLine("There were {0} lines.", counter);

			System.IO.StreamReader fileTime = new System.IO.StreamReader( @"d:\times.txt" );
			string[] times = new string[96];
			int id = 0;
			while ( ( line = fileTime.ReadLine() ) != null ) {
				times[id] = line;
				id++;
			}
			fileTime.Close();
			gl.MaxDotsToDraw = basic.Length;

			for (int i = 0; i < gl.MaxDotsToDraw; i++){
						var d = basic[i];
						gl.Dots[i] = d;
					}
					gl.DotsUpdate();


					System.IO.StreamReader data = new System.IO.StreamReader( @"d:\data-01.txt" );

					int c = 0;
					string date = "";
					int t = 0;
					while ( ( line = data.ReadLine() ) != null ) {

						if ( c < counter / 2 ) {
							//if(!str[1].Equals("0")){
							var str = line.Split( ';' );

							basic[2 * c].Tex = new Vector4( 2, 0, (float) Double.Parse( str[3] ), 0 );
							basic[2 * c + 1].Tex = new Vector4( 1, 0, (float) Double.Parse( str[4] ), 0 );

							c++;
							//}
							date = str[0];
						}
						else {
							GlobeLayer.GeoVert[] arr = new GlobeLayer.GeoVert[basic.Length];
							basic.CopyTo( arr, 0 );
							allDataSubway.Add( date.TrimMatchingQuotes( '"' ) + ";" + times[t] + ":00 ", arr );
							t = t == 95 ? 0 : t + 1;
							c = 0;
							//continue;
						}

					}
					data.Close();
		}



		/// <summary>
		/// Disposes game
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				//	dispose disposable stuff here
				//	Do NOT dispose objects loaded using ContentManager.
			}
			base.Dispose( disposing );
		}


		bool pause = true;
		/// <summary>
		/// Handle keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Fusion.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
				DevCon.Show( this );
			}

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
			if ( e.Key == Keys.P ) {
				pause = !pause;
			}
			if ( e.Key == Keys.Up || e.Key == Keys.Add) {
				switch ( velocity ) {
					case 9:
						nextVel = 1;
						break;
					case 15:
						nextVel = 9;
						break;
					case 30:
						nextVel = 15;
						break;
					case 60:
						nextVel = 30;
						break;

				}
			}
			if (e.Key == Keys.Down || e.Key == Keys.Subtract){
				switch ( velocity ) {
					case 1:
						nextVel = 9;
						break;
					case 9:
						nextVel = 15;
						break;
					case 15:
						nextVel = 30;
						break;
					case 30:
						nextVel = 60;
						break;
					
				}
			}

		}



		/// <summary>
		/// Saves configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Game_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}


		Random r = new Random();
		int velocity = 15;
		int nextVel = 15;
		int index = 0;
		/// <summary>
		/// Updates game
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			base.Update( gameTime );

			//ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			//ds.Add( "F1   - show developer console" );
			//ds.Add( "F2   - toggle vsync" );
			//ds.Add( "F5   - build content and reload textures" );
			//ds.Add( "F12  - make screenshot" );
			//ds.Add( "ESC  - exit" );


		}

		int v = 0;
		DateTime timer = new DateTime( 2014, 1, 13, 3, 0, 0 );
		/// <summary>
		/// Draws game
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			base.Draw( gameTime, stereoEye );

			double step = 900000 / velocity;
			var gl = GetService<LayerService>().GlobeLayer;

			var sb = GetService<SpriteBatch>();
			sf.DrawString( sb, str, GraphicsDevice.DisplayBounds.Width / 3 - 150, GraphicsDevice.DisplayBounds.Height / 2 + sf.LineHeight, Color.Black );

			if ( pause ) {
				if ( index < allDataSubway.Count ) {
					var data = allDataSubway.ElementAt( index );
					var dataNext = allDataSubway.ElementAt( index + 1 );
					for ( int i = 0; i < 124; i++ ) {
						var d = data.Value[i];
						var dN = dataNext.Value[i];
						float radD = sizeCoeff( d.Tex.Z );
						float radDN = sizeCoeff( dN.Tex.Z );
						gl.Dots[i] = d;
						gl.Dots[i].Tex.Z = radD + ( radDN - radD ) * v / velocity;
					}

					
					gl.DotsUpdate();
					var t1 = timer.AddMilliseconds( step );
					if ( !timer.Day.Equals( t1.Day ) ) {
						str = dates[++idx];
					}
					timer = t1;
					sf.DrawString( sb, timer.ToString( "T" ), GraphicsDevice.DisplayBounds.Width / 3 - 100, GraphicsDevice.DisplayBounds.Height / 2 + sf.LineHeight * 2, Color.Black );

					//sf.DrawString(sb, data.Key, 0, GraphicsDevice.DisplayBounds.Height - sf.LineHeight, Color.Black);
					if ( v == velocity ) {
						index++;
						velocity = nextVel;
						v = 0;
						var stri = data.Key.Split( ';' )[1].Split(':');
						var hour = (int) Double.Parse( stri[0].TrimMatchingQuotes( '0' ) );
						var minute = (int) Double.Parse( stri[1] );
						timer = new DateTime( 2014, 1, 13, hour, minute, 0 );

					}
					else {
						v++;
					}

				}

				//v++;

				//v = v == velocity ? 0 : v + 1;
			}
			
			
		}

		private float sizeCoeff(float d) {
			//if ( d < 10 ) return 1;
			if ( d < 50 ) return 0.1f;
			if ( d < 100 ) return 0.12f;
			if ( d < 500 ) return 0.15f;
			if ( d < 1000 ) return 0.2f;
			if ( d < 2000 ) return 0.4f;
			if ( d > 4000 ) return 0.5f;
			return 0.05f;
		}

		
		int idx = 0;
		string str = "Monday, January";
		string[] dates = new string[]{ "Monday, January"
						, "Tuesday, January"
						, "Wednesday, January"
						, "Thursday, January"
						, "Friday, January"
						, "Saturday, January"
						, "Sunday, January"
						, "Monday, February"
						, "Tuesday, February"
						, "Wednesday, February"
						, "Thursday, February"
						, "Friday, February"
						, "Saturday, February"
						, "Sunday, February"
						, "Monday, March"
						, "Tuesday, March"
						, "Wednesday, March"
						, "Thursday, March"
						, "Friday, March"
						, "Saturday, March"
						, "Sunday, March"
						, "Monday, April"
						, "Tuesday, April"
						, "Wednesday, April"
						, "Thursday, April"
						, "Friday, April"
						, "Saturday, April"
						, "Sunday, April"
						, "Monday, May"
						, "Tuesday, May"
						, "Wednesday, May"
						, "Thursday, May"
						, "Friday, May"
						, "Saturday, May"
						, "Sunday, May"
						, "Monday, June"
						, "Tuesday, June"
						, "Wednesday, June"
						, "Thursday, June"
						, "Friday, June"
						, "Saturday, June"
						, "Sunday, June"
						, "Monday, July"
						, "Tuesday, July"
						, "Wednesday, July"
						, "Thursday, July"
						, "Friday, July"
						, "Saturday, July"
						, "Sunday, July"
						, "Monday, August"
						, "Tuesday, August"
						, "Wednesday, August"
						, "Thursday, August"
						, "Friday, August"
						, "Saturday, August"
						, "Sunday, August"
						, "Monday, September"
						, "Tuesday, September"
						, "Wednesday, September"
						, "Thursday, September"
						, "Friday, September"
						, "Saturday, September"
						, "Sunday, September"
						, "Monday, October"
						, "Tuesday, October"
						, "Wednesday, October"
						, "Thursday, October"
						, "Friday, October"
						, "Saturday, October"
						, "Sunday, October"
						, "Monday, November"
						, "Tuesday, November"
						, "Wednesday, November"
						, "Thursday, November"
						, "Friday, November"
						, "Saturday, November"
						, "Sunday, November"
						, "Monday, December"
						, "Tuesday, December"
						, "Wednesday, December"
						, "Thursday, December"
						, "Friday, December"
						, "Saturday, December"
						, "Sunday, December"
			};
	}
}
