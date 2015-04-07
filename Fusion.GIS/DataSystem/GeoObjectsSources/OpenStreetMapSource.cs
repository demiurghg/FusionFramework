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


		struct Vertex
		{
			[Vertex("POSITION")]	public Vector3	Position;
			[Vertex("COLOR")]		public Color	Color;
				//public Vector2	TexCoord;
		}


		[StructLayout(LayoutKind.Explicit)]
		struct DrawParams
		{
			[FieldOffset(0)]
			public Matrix viewProjMatrix;
			[FieldOffset(64)]
			public Vector4 Offset;
			[FieldOffset(80)]
			public Vector4 Zoom;
		}

		[Flags]
		public enum DrawFlags : int
		{
			NONE = 0
		}

		Ubershader							DrawShader;
		ConstBufferGeneric<DrawParams>		DrawParameters;

		StateFactory factory;

		VertexBuffer vertexBuf;


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


			var rs = Game.GraphicsDevice;

			DrawShader = Game.Content.Load<Ubershader>("GeoObjects.hlsl");
			SafeDispose(ref factory);
			factory = new StateFactory(DrawShader, typeof(DrawFlags), Primitive.PointList, VertexInputElement.FromStructure(typeof(Vertex)));

			DrawParameters = new ConstBufferGeneric<DrawParams>(rs);

			//GetByArea(new Vector2(29.275818f, 59.624714f), new Vector2(30.899048f, 60.262979f));

			//GetByArea(new Vector2(30.169f, 59.9158f), new Vector2(30.3133f, 59.9677f));

			// Habarovsk
			//GetByArea(new Vector2(134.700623f, 48.253484f), new Vector2(135.350189f, 48.654686f), "Habarovsk");
			
			// Transib
			//GetByRelationId(1154358);
			//GetByRelationId(1758077);

			//var doc = new XmlDocument();
			//doc.Load(Game.Content.OpenStream("Urban/Relation_" + 1154358));
			//ParseXml(doc);
			//doc.Load(Game.Content.OpenStream("Urban/Relation_" + 1758077));
			//ParseXml(doc);




			//XmlDocument doc = new XmlDocument();
			//doc.Load("full_kron.xml");
			//
			//ParseXml(doc);


			//var sw = new StreamWriter(File.Create("OM.txt"));
			//
			//var files = Directory.GetFiles(@"cache\OpenStreetMap\GeoObjects\", "Relation*");
			//foreach (var file in files) {
			//	var stringId = file.Split(new char[] {'_', '.'})[1];
			//
			//	long id = long.Parse(stringId);
			//
			//	GetByRelationId(id);
			//
			//	var name = allRelations.First().Value.Tags.Where(x => x.Key == "name").First().Value;
			//	
			//	sw.WriteLine(name + " ");
			//
			//	Test(sw, allWays.First().Value.nodeRef[0], allWays.First().Value.nodeRef[0], 0);
			//
			//	//foreach (var wayID in allRelations.First().Value.Members) {
			//	//
			//	//	if (wayID.Type == MemberType.Node) continue;
			//	//
			//	//	var way = allWays[wayID.Ref];
			//	//
			//	//	if (way.isBuilding) {
			//	//		continue;
			//	//	}
			//	//
			//	//
			//	//	//foreach (var node in way.nodeRef) {
			//	//	//	sw.WriteLine(allNodes[node].Latitude + "	" + allNodes[node].Longitude);
			//	//	//}
			//	//
			//	//}
			//	sw.WriteLine();
			//
			//}
			//sw.Close();
			//sw.Dispose();

			//CheckRelations();

			//GenerateVertices(true);

			//GenerateRoadNodes();

			//GetAllMO();

			//Game.InputDevice.MouseDown += InputDeviceOnMouseDown;

			//SaveToFile("Buildings.txt");
		}


		public List<DVector2> GetLineFromWays(long firsNode, long lastNode)
		{
			var list = new List<DVector2>();

			long next		= firsNode;
			long cur		= firsNode;
			long prevWayId	= 0;


			while (next != lastNode) {

				int counter = 0;
				foreach (var way in allWays) {
					counter++;
					if (way.Key == prevWayId) continue;

					if (way.Value.nodeRef.First() == next) {
						
						for (int i = 1; i < way.Value.nodeRef.Length; i++) {
							list.Add( new DVector2(allNodes[way.Value.nodeRef[i]].Longitude, allNodes[way.Value.nodeRef[i]].Latitude) );
						}
						next		= way.Value.nodeRef.Last();
						prevWayId	= way.Key;
						counter		= 0;

					} else if (way.Value.nodeRef.Last() == next) {

						for (int i = way.Value.nodeRef.Length - 2; i >= 0 ; i--) {
							list.Add(new DVector2(allNodes[way.Value.nodeRef[i]].Longitude, allNodes[way.Value.nodeRef[i]].Latitude));
						}
						next		= way.Value.nodeRef.First();
						prevWayId	= way.Key;
						counter		= 0;
					}
					else {
						continue;
					}
					break;
				}

				if (counter == allWays.Count) break;
			}

			return list;
		}



		void GetLineRecursive(ref List<DVector2> list,  long nextNode, long lastNode, long prevWayID)
		{
			foreach (var way in allWays) {
				if (way.Key == prevWayID) continue;

				long next = 0;

				if (way.Value.nodeRef.First() == nextNode) {
					
					for (int i = 1; i < way.Value.nodeRef.Length; i++) {
						list.Add( new DVector2(allNodes[way.Value.nodeRef[i]].Longitude, allNodes[way.Value.nodeRef[i]].Latitude) );
					}
					next = way.Value.nodeRef.Last();

				} else if (way.Value.nodeRef.Last() == nextNode) {

					for (int i = way.Value.nodeRef.Length - 2; i >= 0 ; i--) {
						list.Add(new DVector2(allNodes[way.Value.nodeRef[i]].Longitude, allNodes[way.Value.nodeRef[i]].Latitude));
					}
					next = way.Value.nodeRef.First();

				}
				else {
					continue;
				}

				if (next == lastNode) return;

				GetLineRecursive(ref list, next, lastNode, way.Key);
			}
		}



		void Test(StreamWriter sw, long firstNode, long nextNode, long prevWayID)
		{
			foreach (var way in allWays) {
				if (way.Key == prevWayID) continue;

				long next = 0;

				if (way.Value.nodeRef.First() == nextNode) {
					
					for (int i = 1; i < way.Value.nodeRef.Length; i++) {
						sw.WriteLine(allNodes[way.Value.nodeRef[i]].Latitude + "	" + allNodes[way.Value.nodeRef[i]].Longitude);
					}
					next = way.Value.nodeRef.Last();

				} else if (way.Value.nodeRef.Last() == nextNode) {

					for (int i = way.Value.nodeRef.Length - 2; i >= 0 ; i--) {
						sw.WriteLine(allNodes[way.Value.nodeRef[i]].Latitude + "	" + allNodes[way.Value.nodeRef[i]].Longitude);
					}
					next = way.Value.nodeRef.First();

				}
				else {
					continue;
				}

				if (next == firstNode) return;

				Test(sw, firstNode, next, way.Key);
			}
		}


		void InputDeviceOnMouseDown(object sender, Keys keys)
		{
			if (Game.InputDevice.IsKeyDown(Keys.LeftShift)) {
				var ls = Game.GetService<LayerService>();
				if (keys == Keys.LeftButton) 				{
					var pos = (Game.InputDevice.GlobalMouseOffset - ls.MapLayer.Offset) / ls.MapLayer.Zoom;

					osmNode closestNode = allNodes.First().Value;
					foreach (var node in allNodes) {
						var len1 = (pos - node.Value.Position).Length();
						var len2 = (pos - closestNode.Position).Length();

						if (len1 < len2) {
							closestNode = node.Value;
						}
					}

					Console.WriteLine("Closest node : " + closestNode.id);
				}

				if (keys == Keys.RightButton) {
					var pos = (Game.InputDevice.GlobalMouseOffset - ls.MapLayer.Offset) / ls.MapLayer.Zoom;
					
					pos = GeoHelper.TileToWorldPos(pos.X, pos.Y, 0);
					
					long[] ids = GetBuildingIDWithPointInside(pos.X, pos.Y);
					PaintBuildings(ids, Color.SlateGray);
				}
			}
		}



		void GenerateVertices(bool resetColors)
		{
			var rs = Game.GraphicsDevice;

			List<Vertex> vertices = new List<Vertex>();

			foreach (var kpWay in allWays) {
				osmWay way = kpWay.Value;
				//if (!way.isBuilding && !way.isHighway) continue;

				for (int i = 1; i < way.nodeRef.Length; i++) {
					osmNode node1, node2;

					if ( !allNodes.TryGetValue(way.nodeRef[i - 1],	out node1) ) continue;
					if ( !allNodes.TryGetValue(way.nodeRef[i],		out node2) ) continue;

					if (resetColors) {
						#if false
						Color color = Color.Blue;
						if (way.isBuilding) color = Color.Yellow;
						if (way.isHighway) color = Color.Red;
						way.Color = color;
						#else
						Color color = Color.Green;
						if (way.isBuilding) color = Color.Red;
						if (way.isHighway) color = Color.Blue;
						way.Color = color;
						#endif
					}

					vertices.Add(new Vertex { Color = way.Color, Position = new Vector3(node1.Position, 0.0f) });
					vertices.Add(new Vertex { Color = way.Color, Position = new Vector3(node2.Position, 0.0f) });

				}
			}

			var verts = vertices.ToArray();

			if (vertexBuf != null) {
				vertexBuf.Dispose();
				vertexBuf = null;
			}


			vertexBuf = new VertexBuffer(rs, typeof(Vertex), verts.Length);
			vertexBuf.SetData(verts, 0, verts.Length);
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
			#if false
			var rs = Game.GraphicsDevice;
			//var ds	= rs.DebugStrings;
			//var dr	= rs.DebugRender;
			var cam = Game.GetService<Camera>();

			//rs.ResetStates();

			//Vector2 p1 = new Vector2() * Map.Zoom + Map.Offset;
			//Vector2 p2 = new Vector2(1.0f, 1.0f) * Map.Zoom + Map.Offset;
			//dr.DrawLine(new Vector3(p1, 5.0f), new Vector3(p2, 5.0f), Color.Red);

			//string sig;

			rs.PipelineState			= factory[(int) DrawFlags.NONE];
			rs.PipelineState.BlendState = BlendState.AlphaBlend;
			rs.DepthStencilState		= DepthStencilState.Default;

			DrawParameters.Data.Zoom = new Vector4(Map.Zoom);
			DrawParameters.Data.Offset = new Vector4(Map.Offset, 0.0f, 0.0f);
			//DrawParameters.Data.viewProjMatrix = cam.ProjOrthoMatrix;
			DrawParameters.Data.viewProjMatrix = cam.GetViewMatrix(StereoEye.Mono) * cam.GetProjectionMatrix(StereoEye.Mono);
			DrawParameters.UpdateCBuffer();

			DrawParameters.SetCBufferVS(0);

			rs.SetupVertexInput(vertexBuf, null);

			rs.Draw(Primitive.LineList, vertexBuf.Capacity, 0);
			#endif
		}


		public long[] GetBuildingIDWithPointInside(float lon, float lat)
		{
			List<long> ret = new List<long>();

			foreach (var way in allWays) {
				if (!way.Value.isBuilding) continue;
				var mercPos = GeoHelper.WorldToTilePos(lon, lat);

				if (way.Value.BBox.Contains(new Vector3(mercPos, 0.0f)) != ContainmentType.Contains) continue;

				List<Vector2> bound = new List<Vector2>();

				foreach (var reff in way.Value.nodeRef) {
					bound.Add(allNodes[reff].Position);
				}

				if (GeoHelper.IsPointInPolygon(bound, mercPos)) {
					//Console.WriteLine("Way Id : " + way.Key);
					ret.Add(way.Key);
				}
			}

			return ret.ToArray();
		}


		public long[] GetBuildingIDUnderCursor(int x, int y)
		{
			var ls	= Game.GetService<LayerService>();

			var pos = (Game.InputDevice.GlobalMouseOffset - ls.MapLayer.Offset) / ls.MapLayer.Zoom;
				pos = GeoHelper.TileToWorldPos(pos.X, pos.Y, 0);

			return GetBuildingIDWithPointInside(pos.X, pos.Y);
		}


		public void PaintBuildings(long[] ids, Color color)
		{
			foreach (var id in ids) {
				allWays[id].Color = color;
			}
			GenerateVertices(false);
		}


		public void PaintBuildings(long[] ids, Color[] colors)
		{
			for(int i = 0; i < ids.Length; i++) {
				allWays[ids[i]].Color = colors[i];
			}
			GenerateVertices(false);
		}


		void GenerateRoadNodes()
		{
			Dictionary<long, int>	nodeInds	= new Dictionary<long, int>();
			RoadNodes = new List<RoadNode>();
			RoadEdges = new List<RoadEdge>();

			foreach (var way in allWays) {
				if (!way.Value.isHighway) continue;

				foreach (var reff in way.Value.nodeRef) {
					if (!nodeInds.ContainsKey(reff)) {
						var r = allNodes[reff];
						nodeInds.Add(reff, RoadNodes.Count);
						RoadNodes.Add(new RoadNode { Point = new Vector2((float)r.Longitude, (float)r.Latitude) });
					}
				}
			}

			foreach (var way in allWays) {
				if (!way.Value.isHighway) continue;

				for (int i = 0; i < way.Value.nodeRef.Length-1; i++) {
					long l1 = way.Value.nodeRef[i];
					long l2 = way.Value.nodeRef[i+1];

					var id0 = nodeInds[l1];
					var id1 = nodeInds[l2];
					
					RoadEdges.Add(new RoadEdge {Id0 = id0, Id1 = id1});
				}
			}
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


		void CheckRelations()
		{
			foreach (var relat in allRelations) {
				var r = relat.Value;

				foreach (var tag in r.Tags) {
					if (tag.Key == "building" || tag.Key == "building:levels") {
						foreach (var m in r.Members) {
							if (m.Type == MemberType.Way) {
								osmWay way;
								if (allWays.TryGetValue(m.Ref, out way)) {
									way.isBuilding = true;
								}
							}
						}
					}
				}

			}
		}


		public void SaveToFile(string fileName)
		{
			var stream = File.OpenWrite(fileName);

			var sW = new StreamWriter(stream);

			foreach (var way in allWays) {
				if (way.Value.isBuilding) {
					sW.WriteLine("Way ID : " + way.Value.id);
					foreach (var nr in way.Value.nodeRef) {
						sW.WriteLine(allNodes[nr].Position.X + "	" + allNodes[nr].Position.Y);
					}
					sW.WriteLine();
				}
			}

			sW.Close();
			stream.Dispose();
		}
	

	
		void GetAllMO()
		{
			var writer = new StreamWriter(File.Create("MO.txt"));


			foreach (var rel in allRelations) {

				string res;
				if(rel.Value.Tags.TryGetValue("admin_level", out res)) {
					if (String.Equals(res, "8")) {

						//SaveAllMO(writer, rel.Value);
						GetByRelationId(rel.Value.Id);
					}
				}
				
			}

			writer.Close();
			writer.Dispose();
		}

		void SaveAllMO(StreamWriter writer, osmRelation rel)
		{
			writer.Write("Муниципальный округ " + rel.Id + " : ");
			string val;
			if (rel.Tags.TryGetValue("name", out val)) {
				writer.WriteLine(val);
			} else {
				writer.WriteLine();
			}

			foreach (var memb in rel.Members) {
				
				if (memb.Type == MemberType.Node) {
					osmNode nd;

					if(!allNodes.TryGetValue(memb.Ref, out nd)) continue;
					
					writer.WriteLine(nd.Latitude + "	" + nd.Longitude);
				}
				if (memb.Type == MemberType.Way) {

					osmWay way;
					if (!allWays.TryGetValue(memb.Ref, out way)) continue;

					foreach (var nr in way.nodeRef) {
						osmNode nd;
						if (allNodes.TryGetValue(nr, out nd)) {
							writer.WriteLine(nd.Latitude + "	" + nd.Longitude);
						}
						else {
							writer.WriteLine(0 + "	" + 0);
						}
					}
				}

			}

			writer.WriteLine();

		}
	
	}
}
