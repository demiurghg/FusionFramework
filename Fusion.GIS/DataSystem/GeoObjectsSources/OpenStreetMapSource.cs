using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Fusion.GIS.GlobeMath;
using Fusion.Mathematics;
using Fusion.GIS.LayerSpace.Layers;
using Fusion.Input;
using Fusion.Graphics;

namespace Fusion.GIS.DataSystem.GeoObjectsSources
{
	public class OpenStreetMapSource : DisposableBase
	{
		public Game     Game;
		public MapLayer Map;
		
		WebDownloader downloader = new WebDownloader("", @"cache\OpenStreetMap\GeoObjects");

		readonly string[] Formats = new string[]
			{
				"http://www.openstreetmap.org/api/0.6/map?{0}", 
				"http://www.overpass-api.de/api/map?{0}",
				"http://overpass.osm.rambler.ru/cgi/xapi_meta?*[{0}]"
			};
		string bbox = "bbox={0},{1},{2},{3}";

		public class osmNode
		{
			public long id;
			public double Longitude;
			public double Latitude;
			public Vector2 Position;
		}

		public class osmWay
		{
			public long			id;
			public long[]		nodeRef;
			public bool			isBuilding;
			public bool			isHighway;
			public Highway		highwayType;
			public Color		Color;
			public BoundingBox	BBox;
		}

		public class osmRelation
		{
			public long							Id;
			public RelationMember[]				Members;
			public Dictionary<string, string>	Tags;
		}


		public class RelationMember
		{
			public MemberType	Type;
			public long			Ref;
			public MemberRole	Role;
		}

		public enum MemberType
		{
			Node,
			Way,
			Relation
		}

		public enum MemberRole
		{
			None,
			Outer,
			Inner,
		}

		public enum Highway
		{
			residential,
			tertiary,
			service,
			unclassified,
			footway,
			path
		}


		public struct RoadNode {
			public Vector2 Point;
		}
		public struct RoadEdge {
			public int Id0, Id1;
		}

		public Dictionary<long, osmNode>		allNodes		= new Dictionary<long, osmNode>();
		public Dictionary<long, osmWay>			allWays			= new Dictionary<long, osmWay>();
		public Dictionary<long, osmRelation>	allRelations	= new Dictionary<long, osmRelation>();

		public List<RoadNode>			RoadNodes;
		public List<RoadEdge>			RoadEdges; 

		public OpenStreetMapSource(Game game, MapLayer mapLayer)
		{
			Game	= game;
			Map		= mapLayer;

			if (!Directory.Exists(@"cache\OpenStreetMap")) {
				Directory.CreateDirectory(@"cache\OpenStreetMap");
			}
			if (!Directory.Exists(@"cache\OpenStreetMap\GeoObjects")) {
				Directory.CreateDirectory(@"cache\OpenStreetMap\GeoObjects");
			}

			// Habarovsk
			//GetByArea(new Vector2(134.700623f, 48.253484f), new Vector2(135.350189f, 48.654686f), "Habarovsk");
			
			// Transib
			//GetByRelationId(1154358);
			
		}


		string GenerateUrl(float leftLon, float bottomLat, float rightLon, float topLat)
		{
			string coords = String.Format(bbox, leftLon, bottomLat, rightLon, topLat);

			return String.Format(Formats[1], coords);
		}


		void GetByArea(Vector2 leftBottom, Vector2 rightTop, string fileName)
		{
			string filePath = @"cache\OpenStreetMap\GeoObjects\" + fileName + ".xml";

			XmlDocument xmlDoc;
			if (File.Exists(filePath)) {
				xmlDoc = new XmlDocument();
				xmlDoc.Load(filePath);
			}
			else {
				xmlDoc = downloader.DownloadXml(GenerateUrl(leftBottom.X, leftBottom.Y, rightTop.X, rightTop.Y), filePath);
			}

			ParseXml(xmlDoc);
		}


		void GetByRelationId(long Id)
		{
			string url = "http://api.openstreetmap.org/api/0.6/relation/" + Id + "/full";

			string filePath = @"cache\OpenStreetMap\GeoObjects\Relation_" + Id + ".xml";

			XmlDocument xmlDoc;
			if (File.Exists(filePath)) {
				xmlDoc = new XmlDocument();
				xmlDoc.Load(filePath);
			}
			else {
				xmlDoc = downloader.DownloadXml(url, filePath);
			}

			ParseXml(xmlDoc);
		}


		public void Update(GameTime gameTime)
		{
			
		}


		void ParseXml(XmlDocument xmlDoc)
		{
			XmlNode mainNode = xmlDoc.SelectSingleNode("osm");

			var nodes = mainNode.SelectNodes("node");

			if (allNodes == null) {
				allNodes = new Dictionary<long, osmNode>(nodes.Count);
			}

			for (int i = 0; i < nodes.Count; i++) {
				var lat = nodes[i].Attributes["lat"].Value;
				var lon = nodes[i].Attributes["lon"].Value;
				var id  = nodes[i].Attributes["id"].Value;
				
				var node		= new osmNode {id = long.Parse(id), Latitude = double.Parse(lat), Longitude = double.Parse(lon)};
				node.Position	= GeoHelper.WorldToTilePos(node.Longitude, node.Latitude);

				if (!allNodes.ContainsKey(node.id)) {
					allNodes.Add(node.id, node);
				}
			}

			var ways = mainNode.SelectNodes("way");

			if (allWays == null) {
				allWays = new Dictionary<long, osmWay>(ways.Count);
			}

			foreach (XmlNode way in ways) {
				long id = long.Parse(way.Attributes["id"].Value);

				var refNodes = way.SelectNodes("nd");
				long[] refs = new long[0];
				if (refNodes != null) {
					refs = new long[refNodes.Count];
					for (int i = 0; i < refNodes.Count; i++) {
						refs[i] = long.Parse(refNodes[i].Attributes["ref"].Value);
					}
				}

				float minX = float.MaxValue;
				float minY = float.MaxValue;
				float maxX = float.MinValue;
				float maxY = float.MinValue;

				foreach (var reff in refs) {
					osmNode nd;
					if (allNodes.TryGetValue(reff, out nd)) {
						var p = nd.Position;
						if (p.X < minX) minX = p.X;
						if (p.Y < minY) minY = p.Y;
						if (p.X > maxX) maxX = p.X;
						if (p.Y > maxY) maxY = p.Y;
					}
				}

				var bbox = new BoundingBox(new Vector3(minX, minY, -1.0f), new Vector3(maxX, maxY, 1.0f));


				osmWay osmWay = new osmWay {
						id		= id,
						nodeRef = refs,
						BBox	= bbox
					};
				

				var tags = way.SelectNodes("tag");
				if (tags != null) {
					foreach (XmlNode tag in tags) {
						var key = tag.Attributes["k"].Value;
						var val = tag.Attributes["v"].Value;

						if (key == "building" || key == "building:levels") {
							osmWay.isBuilding = true;
						}
						if (key == "highway") {
							osmWay.isHighway = true;
						}
					}
				}

				if (!allWays.ContainsKey(id)) {
					allWays.Add(id, osmWay);
				}
			}


			var relations	= mainNode.SelectNodes("relation");

			if (allRelations == null) {
				allRelations = new Dictionary<long, osmRelation>();
			}

			foreach (XmlNode rel in relations) {
				long id = long.Parse(rel.Attributes["id"].Value);

				var refMembers = rel.SelectNodes("member");
				var members = new RelationMember[0];
				if (refMembers != null) {
					members = new RelationMember[refMembers.Count];
					for (int i = 0; i < refMembers.Count; i++) {

						var rm = refMembers[i];

						var type = rm.Attributes["type"].Value;
						MemberType mType = MemberType.Way;
						if (type == "node") mType = MemberType.Node;

						long refff = long.Parse(rm.Attributes["ref"].Value);

						MemberRole mRole = MemberRole.None;
						var role = rm.Attributes["role"].Value;
						if (role == "inner") mRole = MemberRole.Inner;
						if (role == "outer") mRole = MemberRole.Outer;

						members[i] = new RelationMember {Ref = refff, Role = mRole, Type = mType};
					}
				}


				var refTags = rel.SelectNodes("tag");
				Dictionary<string, string> tags = new Dictionary<string, string>();
				if (refTags != null) {
					foreach (XmlNode tag in refTags) {
						var key = tag.Attributes["k"].Value;
						var val = tag.Attributes["v"].Value;

						tags.Add(key, val);
					}
				}

				if (!allRelations.ContainsKey(id)) {
					allRelations.Add(id, new osmRelation {Id = id, Members = members, Tags = tags});
				}
			}
		}
	
	}
}
