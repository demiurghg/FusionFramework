using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;
using Fusion.Mathematics;

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer
	{
		VertexBuffer				dotsVB;
		ConstBufferGeneric<DotsConstData>	dotsBuf;

		public GeoVert[]	Dots;
		Dictionary<int, Texture2D>	textures = new Dictionary<int, Texture2D>();

		Texture2D socioClasses;
		Texture2D geoObjects;
		Texture2D infectionDot;

		int geoObjectStart	= 500000;
		int geoObjectOffset = 500000;
		public int infectionStart	= 300000;

		[StructLayout(LayoutKind.Explicit)]
		public unsafe struct DotsConstData 
		{
			[FieldOffset(0)]	public	Matrix 	View;
			[FieldOffset(64)]	public	Matrix	Proj;
			[FieldOffset(128)]	public	Vector4	TexWHST;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16*4)]
			[FieldOffset(144)]	public	float[]	Colors;
		}


		VertexBuffer simpleRailroadsVB;


		void InitDots()
		{
			dotsBuf = new ConstBufferGeneric<DotsConstData>(Game.GraphicsDevice);
			dotsBuf.Data.Colors = new float[16*4];

			//var r = new Random();
			
			Dots = new GeoVert[500100];
			
			dotsVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), Dots.Length);

			for (int v = 0; v < Dots.Length; v++) {
				var vert = new GeoVert {
					Lon = 0,//DMathUtil.DegreesToRadians(30.301419 + 0.125 * r.NextDouble()),
					Lat = 0,//DMathUtil.DegreesToRadians(59.942562 + 0.125 * r.NextDouble()),
					Position	= Vector3.Zero,
					Color		= Color.White,
					Tex			= new Vector4(1, 0, 0, 0)
				};

				Dots[v] = vert;
			}

			// Metro
			AddGeoObject(1, new DVector2(30.291136, 59.951796), Color.White, 0.05f); // Metro
			AddGeoObject(1, new DVector2(30.278251, 59.942602), Color.White, 0.05f); // Metro
			AddGeoObject(1, new DVector2(30.234627, 59.948470), Color.White, 0.05f); // Metro


			// IO Dots
			AddGeoObject(2, new DVector2(30.286614, 59.949759), Color.White, 0.05f); // Tuchkov
			AddGeoObject(2, new DVector2(30.303522, 59.947314), Color.White, 0.05f); // Birj
			AddGeoObject(2, new DVector2(30.309391, 59.940275), Color.White, 0.05f); // Dvorc
			AddGeoObject(2, new DVector2(30.290841, 59.933460), Color.White, 0.05f); // Blago


			// railroads
			AddGeoObject(3, new DVector2(30.298743, 59.906812), Color.Yellow, 0.05f); // balt
			AddGeoObject(3, new DVector2(30.356464, 59.956350), Color.Yellow, 0.05f); // fin
			AddGeoObject(3, new DVector2(30.329428, 59.919639), Color.Yellow, 0.05f); // viteb
			AddGeoObject(3, new DVector2(30.361930, 59.929369), Color.Yellow, 0.05f); // moscow
			AddGeoObject(3, new DVector2(30.440599, 59.932079), Color.Yellow, 0.05f); // ladoj



			var balt0 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.298743), Lat = DMathUtil.DegreesToRadians(59.906812), Color = new Color(1.0f, 1.0f, 1.0f, 1.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			var balt1 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.288105), Lat = DMathUtil.DegreesToRadians(59.851217), Color = new Color(1.0f, 1.0f, 1.0f, 0.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };


			var mosc0 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.361930), Lat = DMathUtil.DegreesToRadians(59.929369), Color = new Color(1.0f, 1.0f, 1.0f, 1.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			var mosc1 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.365911), Lat = DMathUtil.DegreesToRadians(59.912719), Color = new Color(1.0f, 1.0f, 1.0f, 1.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			var mosc2 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.550232), Lat = DMathUtil.DegreesToRadians(59.781460), Color = new Color(1.0f, 1.0f, 1.0f, 0.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };


			var fin0 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.356464), Lat = DMathUtil.DegreesToRadians(59.956350), Color = new Color(1.0f, 1.0f, 1.0f, 1.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			var fin1 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.357671), Lat = DMathUtil.DegreesToRadians(59.967750), Color = new Color(1.0f, 1.0f, 1.0f, 1.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			var fin2 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.294199), Lat = DMathUtil.DegreesToRadians(60.052353), Color = new Color(1.0f, 1.0f, 1.0f, 0.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			//var fin3 = new Vert { Lon = DMathUtil.DegreesToRadians(30.298743), Lat = DMathUtil.DegreesToRadians(59.906812), Color = new Color(1.0f, 1.0f, 1.0f, 0.0f), Position = Vector3.Zero, Tex = new Vector2(0.06f, 0.0f) };


			var vit0 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.329428), Lat = DMathUtil.DegreesToRadians(59.919639), Color = new Color(1.0f, 1.0f, 1.0f, 1.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			var vit1 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.334539), Lat = DMathUtil.DegreesToRadians(59.913923), Color = new Color(1.0f, 1.0f, 1.0f, 1.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			var vit2 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.419769), Lat = DMathUtil.DegreesToRadians(59.742191), Color = new Color(1.0f, 1.0f, 1.0f, 0.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };


			var lad0 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.440599), Lat = DMathUtil.DegreesToRadians(59.932079), Color = new Color(1.0f, 1.0f, 1.0f, 1.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			var lad1 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.448480), Lat = DMathUtil.DegreesToRadians(59.936919), Color = new Color(1.0f, 1.0f, 1.0f, 1.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };
			var lad2 = new GeoVert { Lon = DMathUtil.DegreesToRadians(30.558472), Lat = DMathUtil.DegreesToRadians(59.929517), Color = new Color(1.0f, 1.0f, 1.0f, 0.0f), Position = Vector3.Zero, Tex = new Vector4(0.06f, 0.0f, 0, 0) };


			simpleRailroadsVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), 18);

			simpleRailroadsVB.SetData(new GeoVert[] { balt0, balt1, mosc0, mosc1, mosc1, mosc2, fin0, fin1, fin1, fin2, vit0, vit1, vit1, vit2, lad0, lad1, lad1, lad2 }, 0, 18);

			dotsBuf.Data.Colors[0] = 1.0f;
			dotsBuf.Data.Colors[1] = 1.0f;
			dotsBuf.Data.Colors[2] = 1.0f;
			dotsBuf.Data.Colors[3] = 1.0f;


			dotsVB.SetData(Dots, 0, Dots.Length);

			//textures.Add(0, Game.Content.Load<Texture2D>("Urban/Burdened_Singles.tga"));
			//textures.Add(1, Game.Content.Load<Texture2D>("Urban/Comfortably_off.tga"));
			//textures.Add(2, Game.Content.Load<Texture2D>("Urban/Educated_urbanites.tga"));
			//textures.Add(3, Game.Content.Load<Texture2D>("Urban/immigrants.tga"));
			//textures.Add(4, Game.Content.Load<Texture2D>("Urban/Moderate_means.tga"));
			//textures.Add(5, Game.Content.Load<Texture2D>("Urban/pensioneer.tga"));
			//textures.Add(6, Game.Content.Load<Texture2D>("Urban/Struggling_families.tga"));
			//textures.Add(7, Game.Content.Load<Texture2D>("Urban/Sudents.tga"));
			//textures.Add(8, Game.Content.Load<Texture2D>("Urban/urban_prosperity.tga"));
			//textures.Add(9, Game.Content.Load<Texture2D>("Urban/Wealthy_achievers.tga"));

			socioClasses	= Game.Content.Load<Texture2D>("Urban/SocioClasses.tga");
			geoObjects		= Game.Content.Load<Texture2D>("Urban/geoObjects.tga");
			infectionDot	= Game.Content.Load<Texture2D>("Urban/InfectionDot.tga");
		}



		public void AddGeoObject(int objectClass, DVector2 lonLat, Color color, float size)
		{
			if (geoObjectOffset >= Dots.Length) return;

			Dots[geoObjectOffset++] = new GeoVert {
					Color		= color,
					Lon			= DMathUtil.DegreesToRadians(lonLat.X),
					Lat			= DMathUtil.DegreesToRadians(lonLat.Y),
					Position	= Vector3.Zero,
					Tex			= new Vector4(objectClass, 0, size, 0)
				};
		}


		public void UpdateDots(GameTime gameTime)
		{
			//for (int i = 0; i < Dots.Length; i++) {
			//	Dots[i].Lon += 0.000001;// * Math.Sin(Math.PI * gameTime.Total.TotalSeconds) * r.NextDouble(-1, 1);
			//	Dots[i].Lat += 0.000001;// *Math.Cos(Math.PI * gameTime.Total.TotalSeconds) * r.NextDouble(-1, 1);
			//}

			dotsVB.SetData(Dots, 0, Dots.Length);
		}


		public void UpdateColors(Color[] cols)
		{
			int count = cols.Length > 15 ? 15  : cols.Length;

			for (int i = 1; i < count+1; i++) {
				var v = cols[i-1].ToVector4();
			
				dotsBuf.Data.Colors[i * 4 + 0] = v.X;
				dotsBuf.Data.Colors[i * 4 + 1] = v.Y;
				dotsBuf.Data.Colors[i * 4 + 2] = v.Z;
				dotsBuf.Data.Colors[i * 4 + 3] = v.W;
			}
			
			dotsBuf.UpdateCBuffer();
		}
	}
}
