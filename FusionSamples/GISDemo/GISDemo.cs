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
	public class GISDemo: Game {


		/// <summary>
		/// GISDemo constructor
		/// </summary>
		public GISDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;
			Parameters.MsaaLevel = 4;

			//	uncomment to enable debug graphics device:
			//	(MS Platform SDK must be installed)
			//	Parameters.UseDebugDevice	=	true;

			//	add services :
			AddService(new SpriteBatch(this), false, false, 0, 9998);
			AddService(new DebugStrings(this), true, true, 0, 9999);
			AddService(new LayerService(this), true, true, 1, 1);

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
			Reloading += (s, e) => LoadContent();

			var gl = GetService<LayerService>().GlobeLayer;


			InputDevice.KeyDown += (sender, args) => gl.InputDeviceOnMouseDown(sender, new Frame.MouseEventArgs { X = (int) InputDevice.GlobalMouseOffset.X, Y = (int) InputDevice.GlobalMouseOffset.Y, Key = args.Key });
			InputDevice.MouseMove += (sender, args) => gl.InputDeviceOnMouseMove(sender, new Frame.MouseEventArgs { X = (int) InputDevice.GlobalMouseOffset.X, Y = (int) InputDevice.GlobalMouseOffset.Y });



			//gl.Dots[0] = new GlobeLayer.GeoVert {
			//	Lon = DMath.DegreesToRadians(30.306467),
			//	Lat = DMath.DegreesToRadians(59.944049),
			//	Color = Color.Red,
			//	Position = new Vector3(),
			//	Tex = new Vector4(1, 0, 0.01f, 0)
			//};

			//gl.DotsAddGeoObject(1, new DVector2(30.307467, 59.943049), Color.Tan, 0.05f);





		}


		Dictionary<String, GlobeLayer.GeoVert[]> allDataSubway = new Dictionary<string, GlobeLayer.GeoVert[]>();
		GlobeLayer.GeoVert[] complains = new GlobeLayer.GeoVert[870];
		GlobeLayer.GeoVert[] suic2013 = new GlobeLayer.GeoVert[710];
		GlobeLayer.GeoVert[] suic2014 = new GlobeLayer.GeoVert[343];
		GlobeLayer.GeoVert[] toxic = new GlobeLayer.GeoVert[768];
		GlobeLayer.GeoVert[] migr = new GlobeLayer.GeoVert[880];

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
			//Subway
			
			GlobeLayer.GeoVert[] basic = new GlobeLayer.GeoVert[118];

			System.IO.StreamReader file = new System.IO.StreamReader(@"d:\coords.txt");
			while ( ( line = file.ReadLine() ) != null ) {
				var str = line.Split(',');
				if ( !str[1].Equals("0") ) {
					//System.Console.WriteLine(str[0]);
					basic[counter] = new GlobeLayer.GeoVert {
						Lon = DMath.DegreesToRadians(Double.Parse(str[2])),
						Lat = DMath.DegreesToRadians(Double.Parse(str[1])),
						Color = new Color(207, 71, 60),
						Position = new Vector3(),
						Tex = new Vector4(1, 0, 0.08f, (float) Double.Parse(str[3]))
					};
					//Console.WriteLine(basic[counter].Tex);
					
					basic[++counter] = new GlobeLayer.GeoVert {
						Lon = DMath.DegreesToRadians(Double.Parse(str[2])),
						Lat = DMath.DegreesToRadians(Double.Parse(str[1])),
						Color = new Color(23, 60, 120),
						Position = new Vector3(),
						Tex = new Vector4(2, 0, 0.08f, (float) Double.Parse(str[3]))
					};

					counter++;
				}
			}
			file.Close();
			

			Console.WriteLine("There were {0} lines.", counter);

			System.IO.StreamReader fileTime = new System.IO.StreamReader(@"d:\times.txt");
			string[] times = new string[96];
			int id = 0;
			while ( ( line = fileTime.ReadLine() ) != null ) {
				times[id] = line;
				id++;
			}
			fileTime.Close();


			//for (int i = 0; i < gl.MaxDotsToDraw; i++){
			//			var d = basic[i];
			//			gl.Dots[i] = d;
			//		}
			//		gl.DotsUpdate();


			System.IO.StreamReader data = new System.IO.StreamReader(@"d:\all-data.txt");

			int c = 0;
			string date = "";
			int t = 0;
			while ( ( line = data.ReadLine() ) != null ) {

				if ( c < counter / 2 ) {
					//if(!str[1].Equals("0")){
					var str = line.Split(';');

					basic[2 * c].Tex = new Vector4(14, 0, (float) Double.Parse(str[3]), 0);
					basic[2 * c + 1].Tex = new Vector4(13, 0, (float) Double.Parse(str[4]), 0);

					c++;
					//}
					date = str[0];
				} else {
					GlobeLayer.GeoVert[] arr = new GlobeLayer.GeoVert[basic.Length];
					basic.CopyTo(arr, 0);
					allDataSubway.Add(date.TrimMatchingQuotes('"') + ";" + times[t], arr);
					t = t == 95 ? 0 : t + 1;
					c = 0;
					//continue;
				}

			}
			data.Close();

			// Complains about drugs
			System.IO.StreamReader comp = new System.IO.StreamReader(@"d:\narc.txt");
			counter = 0;
			while ( ( line = comp.ReadLine() ) != null ) {
				var str = line.Split(';');
				float compl = (float) Double.Parse(str[3]);
				compl = compl > 10 ? 0.95f : compl / 10;
				//System.Console.WriteLine(str[0]);
				complains[counter] = new GlobeLayer.GeoVert {
					Lon = DMath.DegreesToRadians(Double.Parse(str[0])),
					Lat = DMath.DegreesToRadians(Double.Parse(str[1])),
					Color = new Color(73, 97, 69),
					Position = new Vector3(),
					Tex = new Vector4(15, compl, 0.1f, 0)
				};

				counter++;

			}
			Console.WriteLine("There were {0} lines.", counter);
			comp.Close();

			//Suicides in 2013
			System.IO.StreamReader s13 = new System.IO.StreamReader(@"d:\suic2013.txt");
			counter = 0;
			while ( ( line = s13.ReadLine() ) != null ) {
				var str = line.Split(';');
				int tex = 16;
				//System.Console.WriteLine(str[0]);
				//switch ( (int) Double.Parse(str[4]) ) {
				//	case 1:
				//		tex = 12;
				//		break;
				//	case 2:
				//		tex = 11;
				//		break;
				//	case 3:
				//		tex = 7;
				//		break;
				//	case 4:
				//		tex = 8;
				//		break;
				//	case 5:
				//		tex = 4;
				//		break;
				//}

				suic2013[counter] = new GlobeLayer.GeoVert {
					Lon = DMath.DegreesToRadians(Double.Parse(str[0])),
					Lat = DMath.DegreesToRadians(Double.Parse(str[1])),
					Color = Color.White,
					Position = new Vector3(),
					Tex = new Vector4(tex, (float) Double.Parse(str[3]) / 70, 0.2f, (float) Double.Parse(str[2]))
				};
				//Console.WriteLine(suic2013[counter].Tex.W);

				counter++;

			}
			s13.Close();
			Console.WriteLine("There were {0} lines.", counter);

			System.IO.StreamReader s14 = new System.IO.StreamReader(@"d:\suic2014.txt");
			counter = 0;
			while ( ( line = s14.ReadLine() ) != null ) {
				var str = line.Split(';');
				int tex = 16;
				//System.Console.WriteLine(str[0]);
				//switch ( (int) Double.Parse(str[4]) ) {
				//	case 1:
				//		tex = 12;
				//		break;
				//	case 2:
				//		tex = 11;
				//		break;
				//	case 3:
				//		tex = 7;
				//		break;
				//	case 4:
				//		tex = 8;
				//		break;
				//	case 5:
				//		tex = 4;
				//		break;
				//}

				suic2014[counter] = new GlobeLayer.GeoVert {
					Lon = DMath.DegreesToRadians(Double.Parse(str[0])),
					Lat = DMath.DegreesToRadians(Double.Parse(str[1])),
					Color = Color.White,
					Position = new Vector3(),
					Tex = new Vector4(tex, (float) Double.Parse(str[3]) / 70, 0.2f, (float) Double.Parse(str[2]))
				};

				counter++;
			}
			s14.Close();
			Console.WriteLine("There were {0} lines.", counter);

			System.IO.StreamReader tox = new System.IO.StreamReader(@"d:\toks13.txt");
			counter = 0;
			while ( ( line = tox.ReadLine() ) != null ) {
				var str = line.Split(';');
				int tex = 16;
				//System.Console.WriteLine(str[0]);
				//switch ( (int) Double.Parse(str[4]) ) {
				//	case 1:
				//		tex = 12;
				//		break;
				//	case 2:
				//		tex = 11;
				//		break;
				//	case 3:
				//		tex = 7;
				//		break;
				//	case 4:
				//		tex = 8;
				//		break;
				//	case 5:
				//		tex = 4;
				//		break;
				//}

				toxic[counter] = new GlobeLayer.GeoVert {
					Lon = DMath.DegreesToRadians(Double.Parse(str[0])),
					Lat = DMath.DegreesToRadians(Double.Parse(str[1])),
					Color = Color.White,
					Position = new Vector3(),
					Tex = new Vector4(tex, (float) Double.Parse(str[3]) / 70, 0.1f, (float) Double.Parse(str[2]))
				};

				counter++;
			}
			tox.Close();
			Console.WriteLine("There were {0} lines.", counter);

			System.IO.StreamReader mig = new System.IO.StreamReader(@"d:\migran.txt");
			counter = 0;
			while ( ( line = mig.ReadLine() ) != null ) {
				var str = line.Split(';');
				
				
				migr[counter] = new GlobeLayer.GeoVert {
					Lon = DMath.DegreesToRadians(Double.Parse(str[0])),
					Lat = DMath.DegreesToRadians(Double.Parse(str[1])),
					Color = Color.White,
					Position = new Vector3(),
					Tex = new Vector4(3, (float) Double.Parse(str[2]) / 50, 0.1f, (float) Double.Parse(str[2]))
				};

				counter++;
			}
			mig.Close();
			Console.WriteLine("There were {0} lines.", counter);
			gl.MaxDotsToDraw = basic.Length + complains.Length + suic2013.Length + suic2014.Length + toxic.Length + migr.Length;


		}



		/// <summary>
		/// Disposes game
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose (bool disposing)
		{
			if ( disposing ) {
				//	dispose disposable stuff here
				//	Do NOT dispose objects loaded using ContentManager.
			}
			base.Dispose(disposing);
		}


		bool pause = true;
		bool ShowSubway = false;
		bool ShowSuicide13 = false;
		bool ShowGender13 = false;
		bool ShowGender14 = false;
		bool ShowSuicide14 = false;
		bool ShowToxic = false;
		bool ShowToxicGender = false;
		bool ShowComplains = false;
		bool ShowMigrants = false;
		/// <summary>
		/// Handle keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown (object sender, Fusion.Input.InputDevice.KeyEventArgs e)
		{
			if ( e.Key == Keys.F1 ) {
				DevCon.Show(this);
			}

			if ( e.Key == Keys.F2 ) {
				Parameters.ToggleVSync();
			}

			if ( e.Key == Keys.F5 ) {
				Reload();
			}

			if ( e.Key == Keys.F12 ) {
				GraphicsDevice.Screenshot();
			}

			if ( e.Key == Keys.Escape ) {
				Exit();
			}
			if ( e.Key == Keys.R ) {
				ShowToxicGender = !ShowToxicGender;
			}
			if ( e.Key == Keys.T ) {
				ShowToxic = !ShowToxic;
			}
			if ( e.Key == Keys.G ) {
				ShowGender13 = !ShowGender13;
			}
			if ( e.Key == Keys.H ) {
				ShowGender14 = !ShowGender14;
			}
			if ( e.Key == Keys.M ) {
				ShowSubway = !ShowSubway;
			}
			if ( e.Key == Keys.C ) {
				ShowComplains = !ShowComplains;
			}
			if ( e.Key == Keys.P ) {
				pause = !pause;
			}
			if ( e.Key == Keys.S ) {
				ShowSuicide13 = !ShowSuicide13;
			}
			if ( e.Key == Keys.D ) {
				ShowSuicide14 = !ShowSuicide14;
			}
			if ( e.Key == Keys.Q ) {
				ShowMigrants = !ShowMigrants;
			}
			if ( e.Key == Keys.Up || e.Key == Keys.Add ) {
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
			if ( e.Key == Keys.Down || e.Key == Keys.Subtract ) {
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
		void Game_Exiting (object sender, EventArgs e)
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
		protected override void Update (GameTime gameTime)
		{
			var ds	=	GetService<DebugStrings>();

			base.Update(gameTime);

			//ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			//ds.Add( "F1   - show developer console" );
			//ds.Add( "F2   - toggle vsync" );
			//ds.Add( "F5   - build content and reload textures" );
			//ds.Add( "F12  - make screenshot" );
			//ds.Add( "ESC  - exit" );


		}

		int v = 0;
		string timer = "";
		//DateTime timer = new DateTime( 2014, 1, 13, 3, 0, 0 );
		/// <summary>
		/// Draws game
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw (GameTime gameTime, StereoEye stereoEye)
		{
			base.Draw(gameTime, stereoEye);

			double step = 900000 / velocity;
			var gl = GetService<LayerService>().GlobeLayer;

			var sb = GetService<SpriteBatch>();
			if ( ShowSubway ) {
				sf.DrawString(sb, str, GraphicsDevice.DisplayBounds.Width / 3 - 150, GraphicsDevice.DisplayBounds.Height / 2 + sf.LineHeight, Color.Black);
				sf.DrawString(sb, timer, GraphicsDevice.DisplayBounds.Width / 3 - 100, GraphicsDevice.DisplayBounds.Height / 2 + sf.LineHeight * 2, Color.Black);
				addSubwayLines();
			
			} else{
				gl.LinesPolyStart();
				gl.LinesPolyEnd();
				gl.LinesPolyClear();
			}

			if ( index < allDataSubway.Count ) {
				var data = allDataSubway.ElementAt(index);
				var dataNext = allDataSubway.ElementAt(index + 1);
				for ( int i = 0; i < data.Value.Length; i++ ) {
					var d = data.Value[i];
					var dN = dataNext.Value[i];
					float radD = sizeCoeff(d.Tex.Z);
					float radDN = sizeCoeff(dN.Tex.Z);
					gl.Dots[i] = d;
					gl.Dots[i].Tex.Z = radD + ( radDN - radD ) * v / velocity;
					if ( !ShowSubway ) {
						gl.Dots[i].Color.A = 0;
					}
				}



				if ( !pause ) {
					if ( v == velocity ) {
						index++;
						velocity = nextVel;
						v = 0;
						var stri = data.Key.Split(';')[1];//.Split(':');
						//var hour = (int) Double.Parse( stri[0] );
						//var minute = (int) Double.Parse( stri[1] );
						timer = stri;//new DateTime( 2014, 1, 13, hour, minute, 0 );
						if ( strId == 95 ) {
							str = dates[idx];
							idx++;
							strId = 0;
						} else {
							strId++;
						}


					} else {
						v++;
					}

				}

				//v++;

				//v = v == velocity ? 0 : v + 1;
			}


			for ( int i = 0; i < complains.Length; i++ ) {
				gl.Dots[i + 118] = complains[i];
				if ( !ShowComplains ) {
					gl.Dots[i + 118].Color.A = 0;
				}
			}

			for ( int i = 0; i < suic2013.Length; i++ ) {
				gl.Dots[i + 118 + complains.Length] = suic2013[i];

				//

				if ( ShowGender13 ) {
					gl.Dots[i + complains.Length + 118].Tex.Y = 0;
					if ( suic2013[i].Tex.W == 1 ) {
						gl.Dots[i + complains.Length + 118].Color = new Color(112, 65, 91);
					} else {
						gl.Dots[i + complains.Length + 118].Color = new Color(61, 105, 166);
					}

				}
				if ( !ShowSuicide13 ) {
					gl.Dots[i + 118 + complains.Length].Color.A = 0;
				}
			}

			for ( int i = 0; i < suic2014.Length; i++ ) {
				gl.Dots[i + 118 + complains.Length + suic2013.Length] = suic2014[i];

				//

				if ( ShowGender14 ) {
					gl.Dots[i + complains.Length + 118 + suic2013.Length].Tex.Y = 0;
					if ( suic2014[i].Tex.W == 1 ) {
						gl.Dots[i + complains.Length + 118 + suic2013.Length].Color = new Color(112, 65, 91);
					} else {
						gl.Dots[i + complains.Length + 118 + suic2013.Length].Color = new Color(61, 105, 166);
					}

				}
				if ( !ShowSuicide14 ) {
					gl.Dots[i + 118 + complains.Length + suic2013.Length].Color.A = 0;
				}
			}

			for ( int i = 0; i < toxic.Length; i++ ) {
				gl.Dots[i + 118 + complains.Length + suic2013.Length + suic2014.Length] = toxic[i];

				//

				if ( ShowToxicGender ) {
					gl.Dots[i + complains.Length + 118 + suic2013.Length + suic2014.Length].Tex.Y = 0;
					if ( toxic[i].Tex.W == 1 ) {
						gl.Dots[i + complains.Length + 118 + suic2013.Length + suic2014.Length].Color = new Color(112, 65, 91);
					} else {
						gl.Dots[i + complains.Length + 118 + suic2013.Length + suic2014.Length].Color = new Color(61, 105, 166);
					}

				}
				if ( !ShowToxic ) {
					gl.Dots[i + 118 + complains.Length + suic2013.Length + suic2014.Length].Color.A = 0;
				}
			}

			for ( int i = 0; i < migr.Length; i++ ) {
				gl.Dots[i + 118 + complains.Length + suic2013.Length + suic2014.Length + toxic.Length] = migr[i];
								
				if ( !ShowMigrants ) {
					gl.Dots[i + 118 + complains.Length + suic2013.Length + suic2014.Length + toxic.Length].Color.A = 0;
				}
				
			}
			//Console.WriteLine( gl.Dots[234 + 118 + complains.Length + suic2013.Length + suic2014.Length + toxic.Length].Color );
			gl.DotsUpdate();

			if ( InputDevice.RelativeMouseOffset == Vector2.Zero ) {
				//sb.Draw(sb.TextureWhite, InputDevice.MousePosition.X, InputDevice.MousePosition.Y, 150, 50, Color.White);
			}
		}

		void addSubwayLines(){
			var gl = GetService<LayerService>().GlobeLayer;
			Color[] metro = new Color[5];
			metro[0] = new Color(222, 61, 69);
			metro[1] = new Color(0, 118, 188);
			metro[2] = new Color(0, 150, 79);
			metro[3] = new Color(230, 116, 2);
			metro[4] = new Color(131, 44, 146);
			float width = 0.04f;
			gl.LinesPolyStart();
			//red
			gl.LinesPolyAdd(new DVector2(30.250652, 59.842094), new DVector2(30.268471, 59.851758), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.268471, 59.851758), new DVector2(30.261382, 59.867249), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.261382, 59.867249), new DVector2(30.261911, 59.8797),	metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.261911, 59.8797),	new DVector2(30.274903, 59.901208), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.274903, 59.901208), new DVector2(30.299517, 59.907214), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.299517, 59.907214), new DVector2(30.318725, 59.915481), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.318725, 59.915481), new DVector2(30.329669, 59.920689), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.329669, 59.920689), new DVector2(30.347961, 59.927539), metro[0], width); //to vlad
			gl.LinesPolyAdd(new DVector2(30.347961, 59.927539), new DVector2(30.360567, 59.931561), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.360567, 59.931561), new DVector2(30.359967, 59.944556), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.359967, 59.944556), new DVector2(30.355547, 59.957094), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.355547, 59.957094), new DVector2(30.347389, 59.971047), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.347389, 59.971047), new DVector2(30.344325, 59.984811), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.344325, 59.984811), new DVector2(30.366158, 59.999744), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.366158, 59.999744), new DVector2(30.370892, 60.008942), metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.370892, 60.008942), new DVector2(30.396016, 60.01277),	metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.396016, 60.01277), new DVector2(30.418397, 60.034675),	metro[0], width);
			gl.LinesPolyAdd(new DVector2(30.418397, 60.034675), new DVector2(30.442169, 60.050236), metro[0], width);
			//blue
			gl.LinesPolyAdd(new DVector2(30.375556,59.829444), new DVector2(30.349492, 59.833233),	metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.349492, 59.833233), new DVector2(30.322206, 59.852192), metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.322206, 59.852192), new DVector2(30.321803, 59.866361), metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.321803, 59.866361), new DVector2(30.318644, 59.879203), metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.318644, 59.879203), new DVector2(30.317881, 59.891808), metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.317881, 59.891808), new DVector2(30.31745, 59.906281),	metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.31745, 59.906281), new DVector2(30.318725, 59.915481),	metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.318725, 59.915481), new DVector2(30.320425, 59.927092), metro[1], width); //to sennaya
			gl.LinesPolyAdd(new DVector2(30.320425, 59.927092), new DVector2(30.330833, 59.934444), metro[1], width); //to gost
			gl.LinesPolyAdd(new DVector2(30.330833, 59.934444), new DVector2(30.318753, 59.956147), metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.318753, 59.956147), new DVector2(30.311556, 59.966378), metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.311556, 59.966378), new DVector2(30.300819, 59.985478), metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.300819, 59.985478), new DVector2(30.296667, 60.0025),	metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.296667, 60.0025), new DVector2(30.315647, 60.016661),	metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.315647, 60.016661), new DVector2(30.321517, 60.03715),	metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.321517, 60.03715), new DVector2(30.332489, 60.051442),	metro[1], width);
			gl.LinesPolyAdd(new DVector2(30.332489, 60.051442), new DVector2(30.334233, 60.067139), metro[1], width);

			//green
			gl.LinesPolyAdd(new DVector2(30.234, 59.9485), new DVector2(30.278278, 59.942639),		metro[2], width);
			gl.LinesPolyAdd(new DVector2(30.278278, 59.942639), new DVector2(30.330833, 59.934444), metro[2], width);
			gl.LinesPolyAdd(new DVector2(30.330833, 59.934444), new DVector2(30.360567, 59.931561), metro[2], width);
			gl.LinesPolyAdd(new DVector2(30.360567, 59.931561), new DVector2(30.384989, 59.924369), metro[2], width); // to aleks
			gl.LinesPolyAdd(new DVector2(30.384989, 59.924369), new DVector2(30.4235, 59.8967),		metro[2], width);
			gl.LinesPolyAdd(new DVector2(30.4235, 59.8967), new DVector2(30.441686, 59.877317),		metro[2], width);
			gl.LinesPolyAdd(new DVector2(30.441686, 59.877317), new DVector2(30.470272, 59.865194), metro[2], width);
			gl.LinesPolyAdd(new DVector2(30.470272, 59.865194), new DVector2(30.457742, 59.848711), metro[2], width);
			gl.LinesPolyAdd(new DVector2(30.457742, 59.848711), new DVector2(30.500117, 59.830878), metro[2], width);

			//orange
			gl.LinesPolyAdd(new DVector2(30.483356, 59.907444), new DVector2(30.46675, 59.919917),	metro[3], width);
			gl.LinesPolyAdd(new DVector2(30.46675, 59.919917), new DVector2(30.439306, 59.932444),	metro[3], width);
			gl.LinesPolyAdd(new DVector2(30.439306, 59.932444), new DVector2(30.411778, 59.929056), metro[3], width);
			gl.LinesPolyAdd(new DVector2(30.411778, 59.929056), new DVector2(30.384989, 59.924369), metro[3], width);
			gl.LinesPolyAdd(new DVector2(30.384989, 59.924369), new DVector2(30.355222, 59.920778), metro[3], width);
			gl.LinesPolyAdd(new DVector2(30.355222, 59.920778), new DVector2(30.347961, 59.927539), metro[3], width);
			gl.LinesPolyAdd(new DVector2(30.347961, 59.927539), new DVector2(30.320425, 59.927092), metro[3], width);

			//purple
			gl.LinesPolyAdd(new DVector2(30.379722, 59.87), new DVector2(30.369444, 59.883611),		metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.369444, 59.883611), new DVector2(30.358475, 59.896053), metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.358475, 59.896053), new DVector2(30.34815, 59.914686),	metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.34815, 59.914686), new DVector2(30.329669, 59.920689),	metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.329669, 59.920689), new DVector2(30.320425, 59.927092), metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.320425, 59.927092), new DVector2(30.314747, 59.936097), metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.314747, 59.936097), new DVector2(30.290611, 59.952083), metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.290611, 59.952083), new DVector2(30.292028, 59.961056), metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.292028, 59.961056), new DVector2(30.259442, 59.971781), metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.259442, 59.971781), new DVector2(30.255111, 59.989611), metro[4], width);
			gl.LinesPolyAdd(new DVector2(30.255111, 59.989611), new DVector2(30.256975, 60.009978), metro[4], width);
			


			gl.LinesPolyEnd();
		}

		private float sizeCoeff (float d)
		{
			//if ( d < 10 ) return 1;
			if ( d < 50 ) return 0.1f;
			if ( d < 100 ) return 0.12f;
			if ( d < 500 ) return 0.15f;
			if ( d < 1000 ) return 0.2f;
			if ( d < 2000 ) return 0.4f;
			if ( d > 4000 ) return 0.5f;
			return 0.05f;
		}


		int idx = 1;
		int strId = 12;
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
