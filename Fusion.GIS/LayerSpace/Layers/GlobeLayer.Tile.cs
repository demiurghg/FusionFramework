using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Mathematics;

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer
	{
		class GlobeTile : IDisposable
		{
			public Vector2 LeftTopMerc;
			public Vector2 RightBottomMerc;

			public int X;
			public int Y;
			public int Z;


			public double left, top, right, bottom;

			public VertexBuffer VertexBuf;
			public IndexBuffer	IndexBuf;


			public void Dispose()
			{
				VertexBuf.Dispose();
				IndexBuf.Dispose();
			}
		}

		Dictionary<string, GlobeTile> tilesToRender = new Dictionary<string, GlobeTile>();
		Dictionary<string, GlobeTile> tilesOld		= new Dictionary<string, GlobeTile>();
		Dictionary<string, GlobeTile> tilesFree		= new Dictionary<string, GlobeTile>();

		bool updateTiles = false;

		string centerTile = "";

		int tileDensity = 10;

		int CurrentLevel = 8;



		void AddTileToRenderList(int x, int y, int zoom)
		{
			string key = GenerateKey(x, y, zoom);

			if (tilesToRender.ContainsKey(key)) return;

			if (tilesOld.ContainsKey(key)) {
				tilesToRender.Add(key, tilesOld[key]);
				tilesOld.Remove(key);
				return;
			} 
			//if (tilesFree.ContainsKey(key)) {
			//	tilesToRender.Add(key, tilesFree[key]);
			//	tilesFree.Remove(key);
			//	return;
			//}


			long numTiles = 1 << zoom;

			double x0 = ((double)(x + 0) / (double)numTiles);
			double y0 = ((double)(y + 0) / (double)numTiles);
			double x1 = ((double)(x + 1) / (double)numTiles);
			double y1 = ((double)(y + 1) / (double)numTiles);

			
			if (tilesFree.Any()) {

				GlobeTile tile;
				if (tilesFree.ContainsKey(key)) {
					tile = tilesFree[key];
					tilesFree.Remove(key);
				}
				else {
					var temp = tilesFree.First();
					tile = temp.Value;
					tilesFree.Remove(temp.Key);
				}
				

				tile.LeftTopMerc		= new Vector2((float) x0, (float) y0);
				tile.RightBottomMerc	= new Vector2((float) x1, (float) y1);

				tile.X = x;
				tile.Y = y;
				tile.Z = zoom;

				tile.left		= x0;
				tile.right	= x1;
				tile.top		= y0;
				tile.bottom	= y1;

				int[]	indexes;
				GeoVert[]	vertices;

				CalculateVertices(out vertices, out indexes, tileDensity, x0, x1, y0, y1, zoom);

				tile.VertexBuf.SetData(vertices, 0, vertices.Length);
				tile.IndexBuf.SetData(indexes, 0, indexes.Length);

				tilesToRender.Add(key, tile);

			} else {

				var tile = new GlobeTile {
						LeftTopMerc		= new Vector2((float) x0, (float) y0),
						RightBottomMerc = new Vector2((float) x1, (float) y1),
						X	= x,
						Y	= y,
						Z	= zoom,
						left	= x0,
						right	= x1,
						top		= y0,
						bottom	= y1
					};


				GenerateTileGrid(tileDensity, out tile.VertexBuf, out tile.IndexBuf, x0, x1, y0, y1, zoom);

				tilesToRender.Add(key, tile);
			}
		}


		static double[] getGeoFromTile(int x, int y, int zoom)
		{
			double a, c1, c2, c3, c4, g, z, mercX, mercY;
			a = 6378137;
			c1 = 0.00335655146887969;
			c2 = 0.00000657187271079536;
			c3 = 0.00000001764564338702;
			c4 = 0.00000000005328478445;
			mercX = (x * 256 * 2 ^ (23 - zoom)) / 53.5865938 - 20037508.342789;
			mercY = 20037508.342789 - (y * 256 * 2 ^ (23 - zoom)) / 53.5865938;

			g = Math.PI / 2 - 2 * Math.Atan(1 / Math.Exp(mercY / a));
			z = g + c1 * Math.Sin(2 * g) + c2 * Math.Sin(4 * g) + c3 * Math.Sin(6 * g) + c4 * Math.Sin(8 * g);

			return new double[] { mercX / a * 180 / Math.PI, z * 180 / Math.PI };
		}

		static long[] getTileFromGeo(double lat, double lon, int zoom)
		{
			double rLon, rLat, a, k, z;
			rLon = lon * Math.PI / 180;
			rLat = lat * Math.PI / 180;
			a = 6378137;
			k = 0.0818191908426;
			z = Math.Pow( Math.Tan(Math.PI / 4 + rLat / 2) / (Math.Tan(Math.PI / 4 + Math.Asin(k * Math.Sin(rLat)) / 2)), k);
			return new long[] {
                (int) (((20037508.342789 + a * rLon) * 53.5865938 / Math.Pow(2, (23 - zoom))) / 256), (int) (((20037508.342789 - a * Math.Log(z)) * 53.5865938 / Math.Pow(2, (23 - zoom)))) / 256 };
		}

		static int[] getMapTileFromCoordinates(double aLat, double aLon, int zoom)
		{
			int[] outt = new int[2];

			double E2 = (double) aLat*Math.PI/180.0;
			long sradiusa = 6378137;
			long sradiusb = 6356752;
			double J2 = (double) Math.Sqrt(sradiusa*sradiusa - sradiusb*sradiusb)/sradiusa;
			double M2 = (double) Math.Log((1 + Math.Sin(E2)) / (1 - Math.Sin(E2)))/2 - J2 * Math.Log((1 + J2*Math.Sin(E2))/(1 - J2*Math.Sin(E2)))/2;
			double B2 = (double) (1 << zoom);
			outt[0] = (int) Math.Floor(B2/2 - M2*B2/2/Math.PI);

			outt[1] = (int) Math.Floor((aLon + 180)/360*(1 << zoom));

			return outt;
		}

		static double[] tileToMercator(long[] d)
		{
			return new double[] { Math.Round(d[0] / 53.5865938 - 20037508.342789),
                Math.Round(20037508.342789 - d[1] / 53.5865938) };
		}
		
		static double[] geoToMercator(double[] g)
		{
			double d = g[0] * Math.PI / 180, m = g[1] * Math.PI / 180, l = 6378137, k = 0.0818191908426, f = k * Math.Sin(m);
			double h = Math.Tan(Math.PI / 4 + m / 2), j = Math.Pow(Math.Tan(Math.PI / 4 + Math.Asin(f) / 2), k), i = h / j;
			// return new DoublePoint(Math.round(l * d), Math.round(l *
			// Math.log(i)));
			return new double[] { l * d, l * Math.Log(i) };
		}


		static double[] tileCoordinatesToPixels(double[] i, int h)
		{
			double g = Math.Pow(2, (23 - h));
			return new double[] { (int)i[0] / g, (int)i[1] / g };
		}





		//void GetTileIndexByMerc(DVector2 coords, int level, out int x, out int y)
		//{
		//	int numTiles = 1 << level;
		//	
		//	x = (int)(coords.X * numTiles);
		//	y = (int)(coords.Y * numTiles);
		//}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="density"></param>
		/// <param name="vb"></param>
		/// <param name="ib"></param>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <param name="top"></param>
		/// <param name="bottom"></param>
		void GenerateTileGrid(int density, out VertexBuffer vb, out IndexBuffer ib, double left, double right, double top, double bottom, int zoom)
		{
			int[]	indexes; 
			GeoVert[]	vertices;

			CalculateVertices(out vertices, out indexes, density, left, right, top, bottom, zoom);
			
			vb = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), vertices.Length);
			ib = new IndexBuffer(Game.GraphicsDevice,indexes.Length);
			ib.SetData(indexes);
			vb.SetData(vertices, 0, vertices.Length);
		}



		void CalculateVertices(out GeoVert[] vertices, out int[] indeces, int density, double left, double right, double top, double bottom, int zoom)
		{
			int RowsCount		= density + 2;
			int ColumnsCount	= RowsCount;

			var el = Game.GetService<LayerService>().ElevationLayer;
			var ms = Game.GetService<LayerService>().MapLayer.CurrentMapSource;

			var verts = new List<GeoVert>();

			float step = 1.0f / (density + 1);

			double dStep = 1.0 / (double)(density + 1);

			for (int row = 0; row < RowsCount; row++) {
				for (int col = 0; col < ColumnsCount; col++) {

					double xx = left * (1.0 - dStep * col) + right  * dStep * col;
					double yy = top  * (1.0 - dStep * row) + bottom * dStep * row;

					double lon, lat;
					//GeoHelper.TileToWorldPos(xx, yy, 0, out lon, out lat);
					var sc = ms.Projection.TileToWorldPos(xx, yy, 0);

					float elev = 0.0f;
					//if (zoom > 8) elev = el.GetElevation(sc.X, sc.Y) / 1000.0f;

					lon = sc.X * Math.PI / 180.0;
					lat = sc.Y * Math.PI / 180.0;

					//SphericalToCartesian(lon, lat, Config.earthRadius, out x, out y, out z);

					verts.Add(new GeoVert {
							Position	= new Vector3(elev),
							Tex			= new Vector4(step * col, step * row, 0, 0),
							Lon			= lon,
							Lat			= lat
						});
				}

			}
			

			var tindexes = new List<int>();

			for (int row = 0; row < RowsCount - 1; row++) {
				for (int col = 0; col < ColumnsCount - 1; col++) {
					tindexes.Add(col + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + row * ColumnsCount);

					tindexes.Add(col + 1 + row * ColumnsCount);
					tindexes.Add(col + (row + 1) * ColumnsCount);
					tindexes.Add(col + 1 + (row + 1) * ColumnsCount);
				}
			}

			vertices	= verts.ToArray();
			indeces		= tindexes.ToArray();
		}


		int lowestLod	= 9;
		int minLod		= 3;
		/// <summary>
		/// 
		/// </summary>
		void DetermineTiles()
		{
			//SetCurrentLevel();
			var ms = Game.GetService<LayerService>().MapLayer.CurrentMapSource;

			var d = Math.Log((Config.CameraDistance - Config.earthRadius) * 1000.0, 2.0);
			double lod = 28.3 - d;

			var maxLod = ms.MaxZoom;
			
			lowestLod = (int)lod;
			
			if (lowestLod > maxLod.Value) lowestLod = maxLod.Value;
			CurrentLevel = lowestLod;

			if (CurrentLevel < 3) CurrentLevel = 3;


			// Get camera mercator position 
			var lonLat	= GetCameraLonLat();
			lonLat.X	= DMathUtil.RadiansToDegrees(lonLat.X);
			lonLat.Y	= DMathUtil.RadiansToDegrees(lonLat.Y);
			//var merc	= GeoHelper.WorldToTilePos(lonLat.X, lonLat.Y);
			var merc = ms.Projection.WorldToTilePos(lonLat.X, lonLat.Y, CurrentLevel);

			// Get tile index under camera
			int x, y;
			//GetTileIndexByMerc(merc, CurrentLevel, out x, out y);
			x = (int)merc.X;
			y = (int)merc.Y;

			var key = GenerateKey(x, y, CurrentLevel);
			//if (key == centerTile) return;

			centerTile = key;


			if (updateTiles) {
				foreach (var tile in tilesToRender) {
					tilesFree.Add(tile.Key, tile.Value);
				}
				updateTiles = false;
			}
			else {
				foreach (var tile in tilesToRender) {
					tilesOld.Add(tile.Key, tile.Value);
				}
			}

			tilesToRender.Clear();


			var info = new TraversalInfo[2];
				
			var centralNode = new Node { Z = CurrentLevel - 2 };

			var tileUpper = ms.Projection.WorldToTilePos(lonLat.X, lonLat.Y, centralNode.Z);
			centralNode.X = (int)tileUpper.X;
			centralNode.Y = (int)tileUpper.Y;
			//GetTileIndexByMerc(merc, centralNode.Z, out centralNode.X, out centralNode.Y);

			info[0].CentralNode = new Node { X = centralNode.X - 7, Y = centralNode.Y - 7, Z = centralNode.Z };
			info[0].Offset = 4;
			info[0].Length = 7;

			var offNode = new Node { X = info[0].CentralNode.X + info[0].Offset, Y = info[0].CentralNode.Y + info[0].Offset, Z = info[0].CentralNode.Z };
			GetChilds(offNode);

			info[1].CentralNode = offNode.Childs[0];
			info[1].Offset = 3;
			info[1].Length = 8;

			int tilesNum = 1 << info[0].CentralNode.Z;

			for (int i = 0; i < 15; i++) {
				for (int j = 0; j < 15; j++) {

					var nodeX = info[0].CentralNode.X + i;
					var nodeY = info[0].CentralNode.Y + j;

					nodeX = nodeX % tilesNum;
					if (nodeX < 0) nodeX = tilesNum + nodeX;
					if (nodeY < 0 || nodeY >= tilesNum) continue;

					var currNode = new Node {X = nodeX, Y = nodeY, Z = info[0].CentralNode.Z };
					
					QuadTreeTraversalDownTop(info, currNode, 0);
				}
			}

			foreach (var tile in tilesOld) {
				tilesFree.Add(tile.Key, tile.Value);
			}
			tilesOld.Clear();
		}



		static void DoubleToTwoFloats(double value, out float high, out float low)
		{
			if (value >= 0.0) {
				double doubleHigh = Math.Floor(value / 65536.0) * 65536.0;
				high	= (float)doubleHigh;
				low		= (float)(value - doubleHigh);
			} else {
				double doubleHigh = Math.Floor(-value / 65536.0) * 65536.0;
				high	= (float)-doubleHigh;
				low		= (float)(value + doubleHigh);
			}
		}

		static void Vector3DToTwoVector3F(double valueX, double valueY, double valueZ, out Vector3 high, out Vector3 low)
		{
			float highX;
			float highY;
			float highZ;

			float lowX;
			float lowY;
			float lowZ;

			DoubleToTwoFloats(valueX, out highX, out lowX);
			DoubleToTwoFloats(valueY, out highY, out lowY);
			DoubleToTwoFloats(valueZ, out highZ, out lowZ);

			high	= new Vector3(highX, highY, highZ);
			low		= new Vector3(lowX, lowY, lowZ);
		}


		string GenerateKey(int x, int y, int zoom)
		{
			return x + "_" + y + "_" + zoom;
		}



		class Node
		{
			public int X, Y, Z;

			public Node Parent;
			public Node[] Childs;
		}

		struct TraversalInfo
		{
			public Node CentralNode;
			public int Offset, Length;
		}

		Node rootNode = new Node{X = 0, Y = 0, Z = 0, Parent = null};

		// QuadTree
		void QuadTreeTraversalTopDown()
		{
			IsTileVisible(rootNode);
		}



		void QuadTreeTraversalDownTop(TraversalInfo[] info, Node node, int step)
		{
			var ml = Game.GetService<LayerService>().MapLayer;
			int maxLevel = ml.CurrentMapSource.MaxZoom.Value;
			//int minLevel = 3;

			//if (node.Z < minLevel) return;
			if (node.Z > maxLevel) return;
			
			if (step >= info.Length) {
				AddTileToRenderList(node.X, node.Y, node.Z);
				return;
			}


			GetChilds(node);


			int offX = node.X - info[step].CentralNode.X;
			int offY = node.Y - info[step].CentralNode.Y;

			ml.CurrentMapSource.GetTile(node.X, node.Y, node.Z);

			if (offX >= info[step].Offset && offX < info[step].Offset + info[step].Length && 
				offY >= info[step].Offset && offY < info[step].Offset + info[step].Length) {
				
				if (CheckTiles(node)) {
					foreach (var child in node.Childs) {
						QuadTreeTraversalDownTop(info, child, step + 1);
					}
				}
				else {
					AddTileToRenderList(node.X, node.Y, node.Z);
				}

			}
			else {
				AddTileToRenderList(node.X, node.Y, node.Z);
			}
		}



		bool CheckTiles(Node node)
		{
			var ml = Game.GetService<LayerService>().MapLayer;

			if (node.Childs == null) return false;
			if (node.Childs[0].Z > ml.CurrentMapSource.MaxZoom) return false;

			bool check = true;
			foreach (var child in node.Childs) {
				check = check && ml.CurrentMapSource.GetTile(child.X, child.Y, child.Z).IsLoaded;
			}
			return check;
		}


		bool IsTileVisible(Node node)
		{
			// Check if node in range

				// Check if childs in range
					// If all in range return true

					// If some of them in range, add nodes which is not in range to renderList
						
						// return true

				// If none of them in range, add current node to renderList

			// if not in range
			return false;
		}



		void GetChilds(Node node)
		{
			long tilesCurrentLevel	= 1 << node.Z;
			long tilesNextLevel		= 1 << (node.Z + 1);

			int posX = (int)( ((double)node.X / tilesCurrentLevel) * tilesNextLevel );
			int posY = (int)( ((double)node.Y / tilesCurrentLevel) * tilesNextLevel );

			node.Childs = new [] {
					new Node{ X = posX,		Y = posY,		Z = node.Z+1, Parent = node }, 
					new Node{ X = posX +1,	Y = posY,		Z = node.Z+1, Parent = node }, 
					new Node{ X = posX,		Y = posY + 1,	Z = node.Z+1, Parent = node }, 
					new Node{ X = posX + 1,	Y = posY + 1,	Z = node.Z+1, Parent = node }
				};
		}




		void SetCurrentLevel()
		{
			if (Game.InputDevice.IsKeyDown(Keys.NumPad0)) lowestLod = 0;
			if (Game.InputDevice.IsKeyDown(Keys.NumPad1)) lowestLod = 1;
			if (Game.InputDevice.IsKeyDown(Keys.NumPad2)) lowestLod = 2;
			if (Game.InputDevice.IsKeyDown(Keys.NumPad3)) lowestLod = 3;
			if (Game.InputDevice.IsKeyDown(Keys.NumPad4)) lowestLod = 4;
			if (Game.InputDevice.IsKeyDown(Keys.NumPad5)) lowestLod = 5;
			if (Game.InputDevice.IsKeyDown(Keys.NumPad6)) lowestLod = 6;
			if (Game.InputDevice.IsKeyDown(Keys.NumPad7)) lowestLod = 7;
			if (Game.InputDevice.IsKeyDown(Keys.NumPad8)) lowestLod = 8;
			if (Game.InputDevice.IsKeyDown(Keys.NumPad9)) lowestLod = 9;
			if (Game.InputDevice.IsKeyDown(Keys.D0)) lowestLod = 10;
			if (Game.InputDevice.IsKeyDown(Keys.D1)) lowestLod = 11;
			if (Game.InputDevice.IsKeyDown(Keys.D2)) lowestLod = 12;
			if (Game.InputDevice.IsKeyDown(Keys.D3)) lowestLod = 13;
			if (Game.InputDevice.IsKeyDown(Keys.D4)) lowestLod = 14;
			if (Game.InputDevice.IsKeyDown(Keys.D5)) lowestLod = 15;
			if (Game.InputDevice.IsKeyDown(Keys.D6)) lowestLod = 16;
			if (Game.InputDevice.IsKeyDown(Keys.D7)) lowestLod = 17;
			if (Game.InputDevice.IsKeyDown(Keys.D8)) lowestLod = 18;
			if (Game.InputDevice.IsKeyDown(Keys.D9)) lowestLod = 19;
		}
	}
}
