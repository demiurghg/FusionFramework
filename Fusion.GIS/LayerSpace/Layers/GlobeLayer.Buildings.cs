using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.DataSystem.GeoObjectsSources;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;
using Fusion.Mathematics;
using TriangleNet;
using TriangleNet.Geometry;
using Mesh = TriangleNet.Mesh;

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer
	{
		GeoVert[] simpleBuildings;
		GeoVert[] contourBuildings;
		GeoVert[] buildings;

		VertexBuffer contourBuildingsVB;
		VertexBuffer simpleBuildingsVB;

		VertexBuffer	buildingsVB;
		//IndexBuffer		buildingsIB;

		List<GeoVert> contourBuildingsList = new List<GeoVert>();

		public void AddBuildingContour(DVector2[] contourPoints, Color color)
		{
			if (contourPoints.Length < 3) return;

			var firstInd = contourBuildingsList.Count;
			//write first two
			contourBuildingsList.Add(new GeoVert {
					Lon		= DMathUtil.DegreesToRadians(contourPoints[0].X),
					Lat		= DMathUtil.DegreesToRadians(contourPoints[0].Y),
					Color	= color
				});
			contourBuildingsList.Add(new GeoVert {
					Lon		= DMathUtil.DegreesToRadians(contourPoints[1].X),
					Lat		= DMathUtil.DegreesToRadians(contourPoints[1].Y),
					Color	= color
				});


			for (int i = 1; i < contourPoints.Length-1; i++) {
				var prevInd = contourBuildingsList.Count-1;
				contourBuildingsList.Add(contourBuildingsList[prevInd]);

				contourBuildingsList.Add(new GeoVert {
					Lon		= DMathUtil.DegreesToRadians(contourPoints[i+1].X),
					Lat		= DMathUtil.DegreesToRadians(contourPoints[i+1].Y),
					Color	= color
				});
			}

			contourBuildingsList.Add(contourBuildingsList[contourBuildingsList.Count-1]);
			contourBuildingsList.Add(contourBuildingsList[firstInd]);
		}

		public void UpdateContourBuildings()
		{
			if (contourBuildingsList.Count == 0) return;

			if (contourBuildingsVB != null) {
				contourBuildingsVB.Dispose();
			}
			
			contourBuildingsVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), contourBuildingsList.Count);
			contourBuildingsVB.SetData(contourBuildingsList.ToArray(), 0, contourBuildingsList.Count);
		}

		void InitBuildingsOSM()
		{
			var osm = Game.GetService<LayerService>().OpenStreetMapSource;

			List<GeoVert> lines = new List<GeoVert>();
			List<GeoVert> simple = new List<GeoVert>();

			var nodes = osm.allNodes;
			int k = 0;
			foreach (var way in osm.allWays) {
				if (!way.Value.isBuilding) continue;

				var centerMerc = way.Value.BBox.Center();
				float width		= (way.Value.BBox.Maximum - way.Value.BBox.Minimum).X * 10000.0f;
				float length	= (way.Value.BBox.Maximum - way.Value.BBox.Minimum).Y * 10000.0f;
				double lon, lat;
				GeoHelper.TileToWorldPos(centerMerc.X, centerMerc.Y, 0, out lon, out lat);

				simple.Add(new GeoVert {
						Lon			= DMathUtil.DegreesToRadians(lon),
						Lat			= DMathUtil.DegreesToRadians(lat),
						Color		= Color.White,
						Position	= Vector3.Zero,
						Tex			= new Vector4(width, length, 0, 0)
					});

				List<DVector2> buildingVertices = new List<DVector2>();
				for (int i = 0; i < way.Value.nodeRef.Length - 1; i++) {
					var nInds = way.Value.nodeRef;

					lines.Add(new GeoVert {
							Lon			= DMathUtil.DegreesToRadians(nodes[nInds[i]].Longitude),
							Lat			= DMathUtil.DegreesToRadians(nodes[nInds[i]].Latitude),
							Position	= new Vector3(1.0f, 0.0f, 0.0f),
							Color		= Color.Yellow,
							Tex			= Vector4.Zero
						});

					lines.Add(new GeoVert {
							Lon			= DMathUtil.DegreesToRadians(nodes[nInds[i+1]].Longitude),
							Lat			= DMathUtil.DegreesToRadians(nodes[nInds[i+1]].Latitude),
							Position	= new Vector3(1.0f, 0.0f, 0.0f),
							Color		= Color.Yellow,
							Tex			= Vector4.Zero
						});

					buildingVertices.Add(new DVector2(nodes[nInds[i]].Longitude, nodes[nInds[i]].Latitude));
				}
				buildingVertices.Add(new DVector2(nodes[way.Value.nodeRef[way.Value.nodeRef.Length - 1]].Longitude, nodes[way.Value.nodeRef[way.Value.nodeRef.Length - 1]].Latitude));

				/////////////////////////////////////////

				var mesh = new Mesh();
				mesh.Behavior.Quality	= false;
				mesh.Behavior.MinAngle	= 25;
				mesh.Behavior.Convex	= false;

				var ig = new InputGeometry();

				ig.AddPoint(buildingVertices[0].X, buildingVertices[0].Y);
				for (int v = 1; v < buildingVertices.Count; v++) {
					ig.AddPoint(buildingVertices[v].X, buildingVertices[v].Y);
					ig.AddSegment(v - 1, v);
				}
				ig.AddSegment(buildingVertices.Count - 1, 0);

				mesh.Triangulate(ig);

				int n = mesh.Vertices.Count;

				mesh.Renumber();

				buildings = new GeoVert[mesh.Triangles.Count*3];
				
				int ind = 0;
				foreach (var triangle in mesh.Triangles) {
					buildings[ind++] = new GeoVert {
						Lon = DMathUtil.DegreesToRadians(mesh.Vertices.ElementAt(triangle.P0).X),
						Lat = DMathUtil.DegreesToRadians(mesh.Vertices.ElementAt(triangle.P0).Y),
						Position = new Vector3(0.1f, 0.0f, 0.0f),
						Color = Color.Green,
						Tex = Vector4.Zero
					};
					buildings[ind++] = new GeoVert {
						Lon = DMathUtil.DegreesToRadians(mesh.Vertices.ElementAt(triangle.P1).X),
						Lat = DMathUtil.DegreesToRadians(mesh.Vertices.ElementAt(triangle.P1).Y),
						Position = new Vector3(0.1f, 0.0f, 0.0f),
						Color = Color.Green,
						Tex = Vector4.Zero
					};
					buildings[ind++] = new GeoVert {
						Lon = DMathUtil.DegreesToRadians(mesh.Vertices.ElementAt(triangle.P2).X),
						Lat = DMathUtil.DegreesToRadians(mesh.Vertices.ElementAt(triangle.P2).Y),
						Position = new Vector3(0.1f, 0.0f, 0.0f),
						Color = Color.Green,
						Tex = Vector4.Zero
					};
				}

				/////////////////////////////////////////


				k++;
				if (k >= 1) break;
			}

			simpleBuildings		= simple.ToArray();
			contourBuildings	= lines.ToArray();

			contourBuildingsVB = new VertexBuffer(Game.GraphicsDevice, typeof (GeoVert), contourBuildings.Length);
			contourBuildingsVB.SetData(contourBuildings, 0, contourBuildings.Length);

			simpleBuildingsVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), simpleBuildings.Length);
			simpleBuildingsVB.SetData(simpleBuildings, 0, simpleBuildings.Length);


			buildingsVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), buildings.Length);
			buildingsVB.SetData(buildings, 0, buildings.Length);
		}





	}
}
