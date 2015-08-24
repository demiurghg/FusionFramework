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
			AddService( new SpriteBatch( this ),	false,	false, 0, 0 );
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




			gl.Dots[0] = new GlobeLayer.GeoVert {
				Lon = DMath.DegreesToRadians(30.306467),
				Lat = DMath.DegreesToRadians(59.944049),
				Color = Color.Red,
				Position = new Vector3(),
				Tex = new Vector4(1, 0, 0.01f, 0)
			};

			gl.DotsAddGeoObject(1, new DVector2(30.307467, 59.943049), Color.Tan, 0.05f);

			gl.DotsUpdate();


			gl.LinesPolyStart();
			gl.LinesPolyAdd(new DVector2(30.306467, 59.944049), new DVector2(30.204678, 59.946543), Color.Green, 0.01f);
			gl.LinesPolyEnd();
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
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



		/// <summary>
		/// Updates game
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			base.Update( gameTime );

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );


			var gl = GetService<LayerService>().GlobeLayer;
			
		}


		/// <summary>
		/// Draws game
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			base.Draw( gameTime, stereoEye );
		}


		void ParseXml()
		{
			//using (var reader = XmlReader.Create(@"E:\Engine\RTIProject\Content\Urban\Relation_1758077.xml")) {
			//	reader.MoveToContent();
			//
			//	while (reader.Read()) {
			//		if(reader.NodeType != XmlNodeType.Element) continue;
			//
			//		Console.WriteLine(reader.Name);
			//		Console.WriteLine(reader.AttributeCount);
			//		Console.WriteLine(reader.GetAttribute("id"));
			//	}
			//}

			//using (var reader = XmlReader.Create(@"E:\GitHub\FusionFramework\FusionSamples\GISDemo\bin\x64\Debug\cache\WikiMapia\id_21869405.xml")) {
			//	reader.MoveToContent();
			//
			//	while (reader.Read()) {
			//		if(reader.NodeType != XmlNodeType.Element) continue;
			//
			//		if (reader.Name == "tags") {
			//			XElement el = XNode.ReadFrom(reader) as XElement;
			//			foreach (var n in el.Elements()) {
			//				//Console.WriteLine(n.Value);
			//				//foreach (var x in n.Elements()) {
			//				//	Console.WriteLine(x.Value);
			//				//}
			//				Console.WriteLine(n.Element("id").Value);
			//			}
			//			
			//		}
			//
			//		Console.WriteLine(reader.Name);
			//		Console.WriteLine(reader.Value);
			//		Console.WriteLine(reader.GetAttribute("id"));
			//	}
			//}
		}
	}
}
