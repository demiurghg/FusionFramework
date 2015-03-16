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

#pragma warning disable 612

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer
	{
		VertexBuffer	linesVB;
		GeoVert[]			lines;

		VertexBuffer	airLinesVB;
		GeoVert[]			airLines;


		VertexBuffer	railRoadsVB;
		Texture2D		railRoadsTex;
		Texture2D		trainTex;


		VertexBuffer roadsVB;

		VertexBuffer trainVB;

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

		// Train stuff
		public class TrainShceduleLine
		{
			public string	City;
			public DVector2	LonLat;
			public DateTime ArrivalTime;
			public DateTime DepartureTime;
			public int		RailroadPointInd;

			public double[] SickPeople;
			public double[] RecoveredPeople;
			public double	MaximumPeople;
			public int Sick = 0;
			public int Recovered = 0;
		}

        List<TrainShceduleLine> trainShcedule; 


		class RailroadPoint
		{
			public DVector2 LonLat;
			public double	Distance;
		}

		private RailroadPoint[] railroadCpu;




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

		public void DrawRailroadPoly()
		{
			if (railroadsPolyVB == null) return;

			var dev	= Game.GraphicsDevice;
			shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER));
			shader.SetPixelShader((int)GlobeFlags.DRAW_TEXTURED | (int)GlobeFlags.DRAW_POLY);

			Game.GraphicsDevice.BlendState			=	BlendState.AlphaBlend;
			Game.GraphicsDevice.DepthStencilState	=	DepthStencilState.None;

			ConstData cbData = new ConstData();

			cbData.Temp = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);

			constBuffer.SetData(cbData);

			dev.VertexShaderConstants[0]	= constBuffer;
			dev.PixelShaderConstants[0]		= constBuffer;
			

	//		railRoadsTex.SetPS(0);
			dev.PixelShaderResources[0]	= railRoadsTex; // ??? maybe

			dev.PixelShaderSamplers[0]	= SamplerState.LinearWrap;
			dev.RasterizerState			= RasterizerState.CullCW;

			var VertInLayout	= new VertexInputLayout(dev, typeof(GeoVert) );
			Game.GraphicsDevice.SetupVertexInput( VertInLayout, railroadsPolyVB, railroadsPolyIB );
			Game.GraphicsDevice.DrawIndexed(Primitive.TriangleList, railroadsPolyIB.Capacity, 0, 0);
		}


		public void InitLines(int maxLinesCount)
		{
			if (linesVB != null) linesVB.Dispose();
	//		linesVB = Game.GraphicsDevice.CreateVertexBuffer(typeof (GeoVert), maxLinesCount*2);
			linesVB	= new VertexBuffer( Game.GraphicsDevice, typeof(GeoVert), maxLinesCount*2 );
			lines	= new GeoVert[maxLinesCount * 2];


			linesVB.SetData(lines, 0, 2);
		}



		public void UpdateLine(int i, DVector2 lonLatDeg0, DVector2 lonLatDeg1, float width0, float width1, Color color0, Color color1)
		{
			if (linesVB == null || i*2 + 1 >= lines.Length) return;

			lines[i*2 + 0] = new GeoVert {
				Color = color0,
				Position = Vector3.Zero,
				Lon = DMathUtil.DegreesToRadians(lonLatDeg0.X),
				Lat = DMathUtil.DegreesToRadians(lonLatDeg0.Y),
				Tex = new Vector4(width0, 0.0f, 0, 0)
			};

			lines[i*2 + 1] = new GeoVert {
				Color = color1,
				Position = Vector3.Zero,
				Lon = DMathUtil.DegreesToRadians(lonLatDeg1.X),
				Lat = DMathUtil.DegreesToRadians(lonLatDeg1.Y),
				Tex = new Vector4(width1, 0.0f, 0, 0)
			};
		}



		public void UpdateAllLines()
		{
			if (linesVB == null) return;
			linesVB.SetData(lines, 0, lines.Length);
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
	//		airLinesVB	= Game.GraphicsDevice.CreateVertexBuffer(typeof (GeoVert), airLines.Length);
			airLinesVB	= new VertexBuffer( Game.GraphicsDevice, typeof(GeoVert), airLines.Length );
			
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



		public void SetTrainSchedule(List<TrainShceduleLine> schedule)
		{
			trainShcedule = schedule;

			foreach (var line in trainShcedule) {
				AddGeoObject(8, line.LonLat, new Color(1.0f, 0.0f, 0.0f, 0.5f), 15.0f);
			}

			//GeoHelper.GetNewPointWithDistance()
			List<GeoVert> charts = new List<GeoVert>();
			foreach (var line in trainShcedule) {
				charts.Add(new GeoVert {
					Color		= Color.Red,
					Lon			= DMathUtil.DegreesToRadians(line.LonLat.X),
					Lat			= DMathUtil.DegreesToRadians(line.LonLat.Y),
					Position	= Vector3.Zero,
					Tex			= new Vector4(2.7f, 0.0f, 2.7f, 0.0f)
				});
				charts.Add(new GeoVert {
					Color		= Color.Blue,
					Lon			= DMathUtil.DegreesToRadians(line.LonLat.X),
					Lat			= DMathUtil.DegreesToRadians(line.LonLat.Y),
					Position	= Vector3.Zero,
					Tex			= new Vector4(2.7f, 0.0f, 2.7f, 0.0f)
				});
			}

			UpdateCharts(charts);

			InitRailroadsOSM();
		}



		void UpdateTransibCitiesCharts(DateTime time)
		{
			for (int i = 0; i < trainShcedule.Count; i++) {
				if(trainShcedule[i].SickPeople.Length <= 1) continue;

				var arriveTime	= trainShcedule[i].ArrivalTime;
				var maxDays		= trainShcedule[i].SickPeople.Length;
				var param		= (time - arriveTime).TotalDays / maxDays;

				if (param < 0) {
					trainShcedule[i].Sick		= 0;
					trainShcedule[i].Recovered	= 0;
					continue;
				}

				param = DMathUtil.Clamp(param, 0.0, 1.0);

				var ind = (int)((maxDays-1)*param);

				double f = (maxDays-1)*param - ind;

				double sick = 0;
				double rec	= 0;

				if (ind == maxDays - 1) {
					sick = trainShcedule[i].SickPeople[ind];
					rec = trainShcedule[i].RecoveredPeople[ind];
				}
				else {
					sick	= DMathUtil.Lerp(trainShcedule[i].SickPeople[ind], trainShcedule[i].SickPeople[ind + 1], f);
					rec		= DMathUtil.Lerp(trainShcedule[i].RecoveredPeople[ind], trainShcedule[i].RecoveredPeople[ind + 1], f);
				}

				trainShcedule[i].Sick		= (int) sick;
				trainShcedule[i].Recovered	= (int) rec;

				sick /= trainShcedule[i].MaximumPeople;
				rec /= trainShcedule[i].MaximumPeople;

				float stickHeight = 25.0f;

				var s = chartsCPU[i * 2];
				s.Tex.Y = (float)sick * stickHeight;
				chartsCPU[i*2] = s;

				var r = chartsCPU[i*2 + 1];
				r.Tex.Y = (float)rec * stickHeight;
				r.Tex.W = (float)sick * stickHeight;
				chartsCPU[i*2 + 1] = r;
			}

			UpdateCharts(chartsCPU);
		}



		public DVector2 UpdateTrainPosition(DateTime time)
		{
			if (trainVB == null) return DVector2.Zero;

			DVector2 position = DVector2.Zero;

			int		start	= 0;
			int		end		= 0;
			double	param	= 0.0;

			if (time <= trainShcedule[0].DepartureTime) {
				position = railroadCpu[trainShcedule[0].RailroadPointInd].LonLat;
			} else if (time >= trainShcedule.Last().ArrivalTime) {
				position = railroadCpu[trainShcedule.Last().RailroadPointInd].LonLat;
			} else {
				for (int i = 0; i < trainShcedule.Count-1; i++) {
					if (time >= trainShcedule[i].ArrivalTime && time <= trainShcedule[i].DepartureTime) {
						position = railroadCpu[trainShcedule[i].RailroadPointInd].LonLat;
						break;
					} else if (time > trainShcedule[i].DepartureTime && time < trainShcedule[i + 1].ArrivalTime) {
						start = trainShcedule[i].RailroadPointInd;
						end = trainShcedule[i + 1].RailroadPointInd;

						double total = (trainShcedule[i + 1].ArrivalTime - trainShcedule[i].DepartureTime).TotalSeconds;
						double curre = (time - trainShcedule[i].DepartureTime).TotalSeconds;

						param = curre/total;

						break;
					}

				}

				if (start != end) {
					DMathUtil.Clamp(param, 0, 1);

					if (start > end) {
						var t = end;
						end = start;
						start = t;
						param = 1.0f - param;
					}

					double currentDistance = (railroadCpu[end].Distance - railroadCpu[start].Distance) * param + railroadCpu[start].Distance;

					

					for (int i = start; i <= end-1; i++) {
						if (currentDistance >= railroadCpu[i].Distance && currentDistance <= railroadCpu[i + 1].Distance) {

							double p = (currentDistance - railroadCpu[i].Distance) / (railroadCpu[i + 1].Distance - railroadCpu[i].Distance);

							position = DVector2.Lerp(railroadCpu[i].LonLat, railroadCpu[i + 1].LonLat, p);

							break;
						}
					}

				}

			}


			double height = (Config.CameraDistance - Config.earthRadius) * 1000.0;
			double f = (height - 1000.0) / (5000000.0 - 1000.0);

			f = DMathUtil.Clamp(f, 0, 1);

			trainVB.SetData(new[] { new GeoVert {
					Lon			= position.X * (Math.PI / 180.0),
					Lat			= position.Y * (Math.PI / 180.0),
					Position	= Vector3.Zero,
					Tex			= new Vector4(1, 0, 0.05f + 80.0f * (float)f, 0),
					Color		= Color.Orange
				}}, 0, 1);


			UpdateTransibCitiesCharts(time);

			return position;
		}



		void DrawTrain()
		{
			if (trainVB == null) return;

			dotsConstData.TexWHST	= new Vector4(trainTex.Width, trainTex.Height, 0.0f, 0);
			dotsBuf.SetData(dotsConstData);

	//		Game.GraphicsDevice.SetVSConstant(1, dotsBuf);
	//		Game.GraphicsDevice.SetGSConstant(1, dotsBuf);
			Game.GraphicsDevice.VertexShaderConstants[1]	= dotsBuf;
			Game.GraphicsDevice.GeometryShaderConstants[1]	= dotsBuf;

			Game.GraphicsDevice.BlendState			= BlendState.AlphaBlend;
			Game.GraphicsDevice.DepthStencilState	= DepthStencilState.None;

			shader.SetVertexShader((int)(GlobeFlags.DRAW_DOTS | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER));
			shader.SetPixelShader((int)GlobeFlags.DRAW_DOTS);
			shader.SetGeometryShader((int)(GlobeFlags.DRAW_DOTS | GlobeFlags.DOTS_SCREENSPACE));

	//		Game.GraphicsDevice.SetPSSamplerState(0, SamplerState.LinearClamp);
	//		Game.GraphicsDevice.SetPSResource(0, trainTex);

			Game.GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearClamp;
			Game.GraphicsDevice.PixelShaderResources[0]	= trainTex;

			var VertInLayout	= new VertexInputLayout(Game.GraphicsDevice, typeof(GeoVert) );
			Game.GraphicsDevice.SetupVertexInput( VertInLayout, trainVB, null );

			Game.GraphicsDevice.Draw(Primitive.PointList, trainVB.Capacity, 0);

			Game.GraphicsDevice.GeometryShader = null;
		}



		void InitRailroadsOSM()
		{
			if(railRoadsVB	!= null)	railRoadsVB.Dispose();
			if(trainVB		!= null)	trainVB.Dispose();
			
			
			var osm = Game.GetService<LayerService>().OpenStreetMapSource;
			var lines = new List<GeoVert>();

			var transib = osm.GetLineFromWays(60725029, 916507771);
			
			railroadCpu = new RailroadPoint[transib.Count];
			
			for (int i = 0; i < transib.Count; i++) {
				lines.Add(new GeoVert {
					Lon			= DMathUtil.DegreesToRadians(transib[i].X),
					Lat			= DMathUtil.DegreesToRadians(transib[i].Y),
					Position	= new Vector3(1.0f, 0.0f, 0.0f),
					Color		= Color.Yellow,
					Tex			= new Vector4(0.06f, 0.0f, 0, 0)
				});

				if (trainShcedule == null) continue;
				for (int j = 0; j < trainShcedule.Count; j++) {
					if ((transib[trainShcedule[j].RailroadPointInd] - trainShcedule[j].LonLat).Length() > (transib[i] - trainShcedule[j].LonLat).Length()) {
						trainShcedule[j].RailroadPointInd = i;
					}
				}
			}

			
			railroadCpu[0] = new RailroadPoint();
			railroadCpu[0].LonLat	= transib[0];
			railroadCpu[0].Distance = 0;
			
			for (int i = 1; i < transib.Count; i++) {
				railroadCpu[i] = new RailroadPoint();
				railroadCpu[i].LonLat = transib[i];
				railroadCpu[i].Distance = railroadCpu[i - 1].Distance + DistanceBetweenTwoPoints(transib[i - 1] * (Math.PI / 180.0), transib[i] * (Math.PI / 180.0));
			}


			trainVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), 1);
			trainVB.SetData(new [] { new GeoVert {
					Lon			= railroadCpu[0].LonLat.X * (Math.PI / 180.0),
					Lat			= railroadCpu[0].LonLat.Y * (Math.PI / 180.0),
					Position	= Vector3.Zero,
					Tex			= new Vector4(1, 0, 10, 0),
					Color		= Color.Orange
				}}, 0, 1);

				
			var railroads = lines.ToArray();

			railRoadsVB	= new VertexBuffer( Game.GraphicsDevice, typeof(GeoVert), railroads.Length );
			railRoadsVB.SetData(railroads, 0, railroads.Length);


			CreateRoadFromLine(railroadCpu, 0.025, out railroadsPolyVB, out railroadsPolyIB);
		}



		public void SetupRoads(GeoVert[] geoVerts)
		{
			if (roadsVB == null || roadsVB.Capacity != geoVerts.Length) {
				if(roadsVB != null) roadsVB.Dispose();

				roadsVB	= new VertexBuffer( Game.GraphicsDevice, typeof(GeoVert), geoVerts.Length );
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

			shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY | GlobeFlags.USE_CARTCOORDS | GlobeFlags.VERTEX_SHADER) );
			shader.SetPixelShader((int)GlobeFlags.DRAW_COLOR | (int)GlobeFlags.DRAW_POLY);
			
			Game.GraphicsDevice.BlendState			= BlendState.AlphaBlend;
			Game.GraphicsDevice.DepthStencilState	= DepthStencilState.None;


			ConstData cbData = new ConstData();
			cbData.Temp	= new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
			constBuffer.SetData(cbData);

			Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;

			var VertInLayout	= new VertexInputLayout(Game.GraphicsDevice, typeof(CartVert) );
			Game.GraphicsDevice.SetupVertexInput( VertInLayout, debugLinesVB, null );
			Game.GraphicsDevice.Draw(Primitive.LineList, debugLinesVB.Capacity, 0);
		}

	}
}
