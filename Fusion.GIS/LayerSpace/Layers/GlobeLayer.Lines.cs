using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.DataSystem.GeoObjectsSources;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;
using Fusion.Mathematics;

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer
	{
		VertexBuffer	municipalLinesVB;
		GeoVert[]		lines;

		VertexBuffer	airLinesVB;
		GeoVert[]		airLines;


		VertexBuffer	railRoadsVB;
		Texture2D		railRoadsTex;


		VertexBuffer roadsVB;
		VertexBuffer debugLinesVB;


		class AirDirection
		{
			public int		Id;
			public int		Type;
			public string	Name;
			public double	Latitude;
			public double	Longitude;
		}

		List<AirDirection> airDirections;


		class RailroadPoint
		{
			public DVector2 LonLat;
			public double	Distance;
		}

		private RailroadPoint[] railroadCpu;


		List<VertexBuffer>	linesBatch = new List<VertexBuffer>();
		List<GeoVert>		linesBatchVertices; 

		List<VertexBuffer>	linesPolyBatch = new List<VertexBuffer>();
		List<GeoVert>		linesPolyBatchVertices; 

		public void LinesStart()
		{
			linesBatchVertices = new List<GeoVert>();
		}


		public void LinesAdd(DVector2 lonLatPoint0, DVector2 lonLatPoint1, Color color)
		{
			linesBatchVertices.Add(new GeoVert {
				Lon = DMathUtil.DegreesToRadians(lonLatPoint0.X),
				Lat = DMathUtil.DegreesToRadians(lonLatPoint0.Y),
				Color		= color,
				Position	= new Vector3(),
				Tex			= new Vector4()
			});
			linesBatchVertices.Add(new GeoVert {
				Lon = DMathUtil.DegreesToRadians(lonLatPoint1.X),
				Lat = DMathUtil.DegreesToRadians(lonLatPoint1.Y),
				Color		= color,
				Position	= new Vector3(),
				Tex			= new Vector4()
			});
		}


		public void LinesEnd()
		{
			if (linesBatchVertices.Count == 0) return;

			var vb = new VertexBuffer(Game.GraphicsDevice, typeof (GeoVert), linesBatchVertices.Count);
			vb.SetData(linesBatchVertices.ToArray());

			linesBatch.Add(vb);

			linesBatchVertices.Clear();
		}


		public void LinesPolyStart()
		{
			linesPolyBatchVertices = new List<GeoVert>();
		}


		public void LinesPolyAdd(DVector2 lonLatPoint0, DVector2 lonLatPoint1, Color color, float width)
		{
			linesPolyBatchVertices.Add(new GeoVert {
				Lon = DMathUtil.DegreesToRadians(lonLatPoint0.X),
				Lat = DMathUtil.DegreesToRadians(lonLatPoint0.Y),
				Color		= color,
				Position	= new Vector3(),
				Tex			= new Vector4(width, 0, 0, 0)
			});
			linesPolyBatchVertices.Add(new GeoVert {
				Lon = DMathUtil.DegreesToRadians(lonLatPoint1.X),
				Lat = DMathUtil.DegreesToRadians(lonLatPoint1.Y),
				Color		= color,
				Position	= new Vector3(),
				Tex			= new Vector4(width, 0, 0, 0)
			});
		}


		public void LinesPolyEnd()
		{
			if (linesPolyBatchVertices.Count == 0) return;

			var vb = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), linesPolyBatchVertices.Count);
			vb.SetData(linesPolyBatchVertices.ToArray());

			linesPolyBatch.Add(vb);
			
			linesPolyBatchVertices.Clear();
		}
		public void LinesPolyClear()
		{
			linesPolyBatch.Clear();
			linesPolyBatchVertices.Clear();
		}



		void CreateRoadFromLine(RailroadPoint[] line, double width, out VertexBuffer vb, out IndexBuffer ib)
		{
			if (line.Length == 0) {
				vb = null;
				ib = null;
				return;
			}

			float distMul = 4.0f;

			List<GeoVert>	vertices	= new List<GeoVert>();
			List<int>		indeces		= new List<int>();
			
			for (int i = 0; i < line.Length-1; i++) {
				var p0 = line[i];
				var p1 = line[i+1];

				var cPos0 = SphericalToCartesian(new DVector2(p0.LonLat.X * (Math.PI / 180.0), p0.LonLat.Y * (Math.PI / 180.0)), Config.earthRadius);
				var cPos1 = SphericalToCartesian(new DVector2(p1.LonLat.X * (Math.PI / 180.0), p1.LonLat.Y * (Math.PI / 180.0)), Config.earthRadius);

				var normal = cPos0;
				normal.Normalize();

				DVector3 dir = cPos1 - cPos0;

				DVector3 sideVec = DVector3.Cross(normal, dir);
				sideVec.Normalize();

				DVector3 sideOffset = sideVec * width;


				// Plane
				var finalPosRight	= cPos0	+ sideOffset;
				var finalPosLeft	= cPos0	- sideOffset;

				var lonLatRight = CartesianToSpherical(finalPosRight);
				var lonLatLeft	= CartesianToSpherical(finalPosLeft);

				vertices.Add(new GeoVert {
						Lon			= lonLatRight.X,
						Lat			= lonLatRight.Y,
						Color		= Color.Yellow,
						Tex			= new Vector4((float)p0.Distance * distMul, 0.0f, 0.0f, 0.0f),
						Position	= Vector3.Zero
					});

				vertices.Add(new GeoVert {
						Lon			= lonLatLeft.X,
						Lat			= lonLatLeft.Y,
						Color		= Color.Yellow,
						Tex			= new Vector4((float)p0.Distance * distMul, 1.0f, 0.0f, 0.0f),
						Position	= Vector3.Zero
					});

				indeces.Add(i * 2);
				indeces.Add(i * 2 + 1);
				indeces.Add((i+1) * 2);

				indeces.Add(i * 2 + 1);
				indeces.Add((i + 1) * 2 + 1);
				indeces.Add((i + 1) * 2);

			}

			{
				var p0 = line[line.Length-1];
				var p1 = line[line.Length-2];

				var cPos0 = SphericalToCartesian(new DVector2(p0.LonLat.X * (Math.PI / 180.0), p0.LonLat.Y * (Math.PI / 180.0)), Config.earthRadius);
				var cPos1 = SphericalToCartesian(new DVector2(p1.LonLat.X * (Math.PI / 180.0), p1.LonLat.Y * (Math.PI / 180.0)), Config.earthRadius);

				var normal = cPos0;
				normal.Normalize();

				DVector3 dir = cPos1 - cPos0;

				DVector3 sideVec = DVector3.Cross(normal, -dir);
				sideVec.Normalize();

				DVector3 sideOffset = sideVec * width;


				// Plane
				var finalPosRight = cPos0 + sideOffset;
				var finalPosLeft = cPos0 - sideOffset;

				var lonLatRight = CartesianToSpherical(finalPosRight);
				var lonLatLeft = CartesianToSpherical(finalPosLeft);

				vertices.Add(new GeoVert
				{
					Lon			= lonLatRight.X,
					Lat			= lonLatRight.Y,
					Color		= Color.Yellow,
					Tex			= new Vector4((float)p0.Distance * distMul, 0.0f, 0.0f, 0.0f),
					Position	= Vector3.Zero
				});

				vertices.Add(new GeoVert
				{
					Lon			= lonLatLeft.X,
					Lat			= lonLatLeft.Y,
					Color		= Color.Yellow,
					Tex			= new Vector4((float)p0.Distance * distMul, 1.0f, 0.0f, 0.0f),
					Position	= Vector3.Zero
				});
			}


			vb = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), vertices.Count);
			vb.SetData(vertices.ToArray(), 0, vertices.Count);

			ib = new IndexBuffer(Game.GraphicsDevice, indeces.Count);
			ib.SetData(indeces.ToArray(), 0, indeces.Count);
		}



		VertexBuffer	railroadsPolyVB;
		IndexBuffer		railroadsPolyIB;

		void DrawRailroadPoly()
		{
			if (railroadsPolyVB == null) return;

			Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_POLY | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_TEXTURED | GlobeFlags.DRAW_VERTEX_POLY,
					Primitive.TriangleList,
					BlendState.AlphaBlend,
					RasterizerState.CullCW,
					DepthStencilState.None);

			constBuffer.Data.Temp = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
			constBuffer.UpdateCBuffer();

			Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;


			Game.GraphicsDevice.PixelShaderResources[0] = railRoadsTex;
			Game.GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearWrap;

			Game.GraphicsDevice.SetupVertexInput(railroadsPolyVB, railroadsPolyIB);
			Game.GraphicsDevice.DrawIndexed(railroadsPolyIB.Capacity, 0, 0);
		}


		void InitAirLines()
		{
			if (!File.Exists("Airlines.txt")) return;

			airDirections = new List<AirDirection>();

			var sr = new StreamReader("Airlines.txt");

			var dir = new AirDirection();
			while (!sr.EndOfStream) {
				var line = sr.ReadLine();
				if(line.Length == 0) continue;

				var lines = line.Split(new char[] {':', '\'', ' ', ','}, StringSplitOptions.RemoveEmptyEntries);

				switch (lines[0]) {
					case "id": {
							dir.Id = int.Parse(lines[1]);
							break;
						}
					case "type": {
							dir.Type = int.Parse(lines[1]);
							break;
						}
					case "name": {
							dir.Name = lines[1];
							break;
						}
					case "latitude": {
							dir.Latitude = double.Parse(lines[1]);
							break;
						}
					case "longitude": {
							dir.Longitude = double.Parse(lines[1]);

							airDirections.Add(dir);
							dir = new AirDirection();
							break;
						}
				}
			}

			sr.Close();


			airLines	= new GeoVert[2 * airDirections.Count(x => x.Type == 1)];
			airLinesVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), airLines.Length);
			
			var saintPetersburgPos = new DVector2(DMathUtil.DegreesToRadians(30.270424), DMathUtil.DegreesToRadians(59.800073));

			var saintVert = new GeoVert {
					Lon			= saintPetersburgPos.X,
					Lat			= saintPetersburgPos.Y,
					Position	= Vector3.Zero,
					Tex			= new Vector4(1.0f, 0.0f, 0, 0),
					Color		= new Color(0.01f, 0.01f, 0.01f, 0.01f)
				};


			int i = 0;
			foreach(var airDir in airDirections.Where(x => x.Type == 1)) {
				airLines[2*i + 0] = saintVert;
				airLines[2*i + 1] = new GeoVert {
						Lon			= DMathUtil.DegreesToRadians(airDir.Longitude),
						Lat			= DMathUtil.DegreesToRadians(airDir.Latitude),
						Position	= Vector3.Zero,
						Tex			= new Vector4(10.0f, 0.0f, 0, 0),
						Color		= new Color(0.8f, 0.8f, 0.8f, 0.8f)
					};
				i++;
			}

			airLinesVB.SetData(airLines, 0, airLines.Length);
		}



		void SetupRoads(GeoVert[] geoVerts)
		{
			if (roadsVB == null || roadsVB.Capacity != geoVerts.Length) {
				if(roadsVB != null) roadsVB.Dispose();

				roadsVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), geoVerts.Length);
			}

			roadsVB.SetData(geoVerts, 0, geoVerts.Length);
		}




		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		List<CartVert> debugVertsCPU = new List<CartVert>(); 
		void AddDebugLine(DVector3 pos0, Color color0, DVector3 pos1, Color color1)
		{
			debugVertsCPU.Add(new CartVert {
					Color	= color0, 
					Data	= Vector4.Zero,
					X		= pos0.X,
					Y		= pos0.Y,
					Z		= pos0.Z
				});
			debugVertsCPU.Add(new CartVert {
					Color	= color1, 
					Data	= Vector4.Zero,
					X		= pos1.X,
					Y		= pos1.Y,
					Z		= pos1.Z
				});
		}

		void DrawDebugLines()
		{
			if (debugVertsCPU.Count == 0) return;

			if (debugLinesVB == null) {
				debugLinesVB = new VertexBuffer(Game.GraphicsDevice, typeof(CartVert), debugVertsCPU.Count);
				debugLinesVB.SetData(debugVertsCPU.ToArray(), 0, debugVertsCPU.Count);
			}

			if (debugLinesVB.Capacity != debugVertsCPU.Count) {
				debugLinesVB.Dispose();
				debugLinesVB = new VertexBuffer(Game.GraphicsDevice, typeof(CartVert), debugVertsCPU.Count);
				debugLinesVB.SetData(debugVertsCPU.ToArray(), 0, debugVertsCPU.Count);
			}


			Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_POLY | GlobeFlags.USE_CARTCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_COLOR | GlobeFlags.DRAW_VERTEX_POLY,
					Primitive.LineList,
					BlendState.AlphaBlend,
					RasterizerState.CullCCW,
					DepthStencilState.None);

			constBuffer.Data.Temp = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
			constBuffer.UpdateCBuffer();

			Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;

			Game.GraphicsDevice.SetupVertexInput(debugLinesVB, null);
			Game.GraphicsDevice.Draw(debugLinesVB.Capacity, 0);
		}

	}
}
