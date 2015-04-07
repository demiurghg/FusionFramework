using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.LayerSpace.Layers;
using Fusion.Graphics;
using Fusion.Mathematics;

//using TriangleNet;
//using TriangleNet.Geometry;

namespace Fusion.GIS.DataSystem.GeoObjectsSources
{
	public partial class WikiMapiaSource
	{
		public Game		Game;
		public MapLayer Map;

		//Mesh			triangulator;

		public class WikiMapiaLocation
		{
			public float	Lon;
			public float	Lat;
			public float	North;
			public float	South;
			public float	East;
			public float	West;
			public string	Country;
			public string	State;
			public string	Place;
			public int		CountryAdmId;
		}

		public class WikiMapiaPlace
		{
			public int					Id;
			public string				Title;
			public WikiMapiaLocation	Location;
			public List<Vector2>		Polygon;
			public Dictionary<string, string> Tags;
		}

		public Dictionary<int, WikiMapiaPlace> RamCache = new Dictionary<int, WikiMapiaPlace>(); 


		public WikiMapiaSource(Game game, MapLayer mapLayer)
		{
			Game	= game;
			Map		= mapLayer;

			InitKeys();

			//GetByArea(19140, 9526, 15, false);

			//GetByArea(29.275818f, 59.624714f, 30.899048f, 60.262979f, 2);

			var xmlDoc = new System.Xml.XmlDocument();
			xmlDoc.Load("cache/WikiMapia/area_1.xml");
			
			int elementsCount;
			ParseXmlDocument(xmlDoc, out elementsCount);
			
			
			xmlDoc.Load("cache/WikiMapia/area_2.xml");
			ParseXmlDocument(xmlDoc, out elementsCount);


			List<int> buildings = new List<int>();
			foreach (var p in RamCache) {
				if(p.Value.Tags.ContainsKey("182")) buildings.Add(p.Key);
			}
			
			foreach (var building in buildings) {
				RamCache.Remove(building);
			}

			GetByID(21869405);

			//LoadPlaces();

			//UpdateTriangulator(index);
		}

		int index = 0;

		//void UpdateTriangulator(int ind)
		//{
		//	var place = RamCache.Values.ToArray()[ind];
		//	var p = place.Polygon;
		//
		//	triangulator = new Mesh();
		//	triangulator.Behavior.Algorithm = TriangulationAlgorithm.SweepLine;
		//
		//	InputGeometry ig = new InputGeometry();
		//
		//	ig.AddPoint(p[0].X, p[0].Y);
		//	for (int v = 1; v < p.Count; v++) {
		//		ig.AddPoint(p[v].X, p[v].Y);
		//		ig.AddSegment(v-1, v);
		//	}
		//	ig.AddSegment(p.Count-1, 0);
		//
		//	triangulator.Triangulate(ig);
		//
		//	if(triangulator.Vertices.Count != p.Count) Console.WriteLine("Vetices count not match");
		//}

		public void Update(GameTime gameTime)
		{
			int oldVal = index;
			if (Game.InputDevice.IsKeyDown(Input.Keys.Up))		index++;
			if (Game.InputDevice.IsKeyDown(Input.Keys.Down))	index--;
			if (index < 0) index = 0;
			if (index >= RamCache.Count) index = RamCache.Count-1;

			//if (oldVal != index) UpdateTriangulator(index);

			var rs = Game.GraphicsDevice;
			//var ds = rs.DebugStrings;
			//var dr = rs.DebugRender;
			
			foreach (var wikiPLace in RamCache) {
				var placeN = wikiPLace.Value;
				for (int i = 0; i < placeN.Polygon.Count - 1; i++) {
					var p = placeN.Polygon;
			
					Vector2 w0 = GeoHelper.WorldToTilePos(p[i]);
					Vector2 w1 = GeoHelper.WorldToTilePos(p[i+1]);
			
					Vector2 p0 = w0*Map.Zoom + Map.Offset;
					Vector2 p1 = w1*Map.Zoom + Map.Offset;

					Vector3 pp0 = new Vector3(p0.X, 0.0f, p0.Y);
					Vector3 pp1 = new Vector3(p1.X, 0.0f, p1.Y);
			
					//dr.DrawLine(pp0, pp1, Color.Red);
				}
			}

			//foreach (var tr in triangulator.Triangles) {
			//	var p0 = triangulator.Vertices.ElementAt(tr.P0);
			//	var p1 = triangulator.Vertices.ElementAt(tr.P1);
			//	var p2 = triangulator.Vertices.ElementAt(tr.P2);
			//
			//
			//	Vector2 w0 = GeoHelper.WorldToTilePos(new Vector2((float)p0.X, (float)p0.Y));
			//	Vector2 w1 = GeoHelper.WorldToTilePos(new Vector2((float)p1.X, (float)p1.Y));
			//	Vector2 w2 = GeoHelper.WorldToTilePos(new Vector2((float)p2.X, (float)p2.Y));
			//
			//	Vector2 pp0 = w0 * Map.Zoom + Map.Offset;
			//	Vector2 pp1 = w1 * Map.Zoom + Map.Offset;
			//	Vector2 pp2 = w2 * Map.Zoom + Map.Offset;
			//
			//	Vector3 z0 = new Vector3(pp0, 1.0f);
			//	Vector3 z1 = new Vector3(pp1, 1.0f);
			//	Vector3 z2 = new Vector3(pp2, 1.0f);
			//
			//	dr.DrawLine(z0, z1, Color.Yellow);
			//	dr.DrawLine(z1, z2, Color.Yellow);
			//	dr.DrawLine(z2, z0, Color.Yellow);
			//}

		}


		void LoadPlaces()
		{
			StreamReader reader = new StreamReader(File.OpenRead("MO.txt"));

			WikiMapiaPlace place = new WikiMapiaPlace();
			int counter = 0;

			while (!reader.EndOfStream) {
				var str = reader.ReadLine();

				if(str == "") {
					RamCache.Add(place.Id, place);
				} else if (str[0] == 'М') {
					place = new WikiMapiaPlace();
					place.Title = str;
					place.Id = counter++;
					place.Polygon = new List<Vector2>();
				}
				else {
					var strs = str.Split('	');

					place.Polygon.Add(new Vector2(float.Parse(strs[1]), float.Parse(strs[0])));
				}
			}
		}
	}

}
