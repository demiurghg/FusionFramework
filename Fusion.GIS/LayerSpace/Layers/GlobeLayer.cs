using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.UserInterface;
using Fusion.GIS.DataSystem.MapSources.Projections;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Mathematics;

#pragma warning disable 612

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer : BaseLayer
	{
		public struct GeoVert
		{
			[Vertex("Position")]	public Vector3	Position;
			[Vertex("Color")]		public Color	Color;
			[Vertex("TEXCOORD", 0)] public Vector4	Tex;
			[Vertex("TEXCOORD", 1)] public double	Lon;
			[Vertex("TEXCOORD", 2)] public double	Lat;
		}


		public struct CartVert
		{
			[Vertex("Color")]		public Color	Color;
			[Vertex("TEXCOORD", 0)] public Vector4	Data;
			[Vertex("TEXCOORD", 1)] public double	X;
			[Vertex("TEXCOORD", 2)] public double	Y;
			[Vertex("TEXCOORD", 3)] public double	Z;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct ConstData 
		{
			[FieldOffset(0)]	public	Matrix 	ViewProj		;
			[FieldOffset(64)]	public	double	ViewPositionX	;
			[FieldOffset(72)]	public	double	ViewPositionY	;
			[FieldOffset(80)]	public	double	ViewPositionZ	;
			[FieldOffset(96)]	public	Vector4 Temp			;
			[FieldOffset(112)]	public	Vector4 ViewDir			;
		}

		Ubershader		shader;
		ConstantBuffer	constBuffer;


		[Flags]
		public enum GlobeFlags : int
		{
			NONE					= 0x0000, // 0
			SHOW_FRAMES				= 0x0001, // 1
			DRAW_COLOR				= 0x0002, // 2
			DRAW_TEXTURED			= 0x0004, // 4
			DRAW_PALETTE			= 0x0008, // 8
			DRAW_POLY				= 0x0010, // 16
			DRAW_DOTS				= 0x0020, // 32
			DRAW_HEAT				= 0x0040, // 64
			DRAW_LINES				= 0x0080, // 128
			DRAW_ARCS				= 0x0100, // 256
			DRAW_SEGMENTED_LINES	= 0x0200, // 512
			DRAW_ATMOSPHERE			= 0x0400, // 1024
			DRAW_SIMPLE_BUILDINGS	= 0x0800, // 2048
			
			USE_GEOCOORDS			= 0x1000, // 4096
			USE_CARTCOORDS			= 0x2000, // 8192

			VERTEX_SHADER			= 0x4000, // 16384
			DRAW_CHARTS				= 0x8000, // 32768

			DOTS_SCREENSPACE		= 0x10000, // 65536
			DOTS_WORLDSPACE			= 0x20000, // 131072
		}


		BlendState blendState = new BlendState
		{
			Enabled		= true,
			WriteMask	= ColorChannels.Alpha,
			SrcColor	= Blend.SrcAlpha,
			DstColor	= Blend.InvSrcAlpha,
			SrcAlpha	= Blend.One,
			DstAlpha	= Blend.Zero,
		};


		Texture2D	frame;
//		Texture2D	beamTexture;
		

		public enum Places
		{
			SaintPetersburg_VO,
			Vladivostok,
		}



		double pitch = 0.0f;
		double maxPitch = 0;
		double minPitch = 0;

		public double Yaw	{ set; get; }
		public double Pitch { set { 
			pitch = value;
			if (pitch > maxPitch) pitch = maxPitch;
			if (pitch < minPitch) pitch = minPitch;
		} get { return pitch; } }

		public DQuaternion Rotation { get { return DQuaternion.RotationAxis(DVector3.UnitY, Yaw)*DQuaternion.RotationAxis(DVector3.UnitX, Pitch); } }
		

		


		// Camera stuff
		DMatrix		viewMatrix;
		DMatrix		projMatrix;
		DVector3	cameraPosition;
		float viewportWidth = 1280, viewportHeight = 720;
		double camNear = 10, camFar = 90000.0;
		double camFov = 20;

		double frustumWidth;
		double frustumHeight;
		double frustumZNear;
		double frustumZFar;

		float t = 0.0f;


		SpriteFont	font;
		SpriteFont	labelFont;
		SpriteFont	labelLightFont;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public GlobeLayer(Game game, LayerServiceConfig config) : base(game, config)
		{
	//		shader		= Game.GraphicsDevice.CreateUberShader(@"Core\Globe.hlsl", typeof(GlobeFlags));
			shader		= Game.Content.Load<Ubershader>(@"Core\Globe.hlsl");
			shader.Map( typeof(GlobeFlags) );

			constBuffer	= new ConstantBuffer( Game.GraphicsDevice, typeof(ConstData) ); 
			

			frame			= Game.Content.Load<Texture2D>("redframe.png");
	//		heatMapPalette	= Game.Content.Load<Texture2D>("palette.tga");

			railRoadsTex	= Game.Content.Load<Texture2D>("Urban/Railroads.png");
			trainTex		= Game.Content.Load<Texture2D>("Urban/trainMarker.png");

			paletteMunDiv0	= Game.Content.Load<Texture2D>("palette3.png");
			paletteMunDiv1	= Game.Content.Load<Texture2D>("palette2.png");
			paletteMunDiv	= paletteMunDiv0;

			font = Game.Content.Load<SpriteFont>(@"Fonts\headerFont.bmfc");
			labelFont = Game.Content.Load<SpriteFont>(@"Fonts\labelFontAccent.bmfc");
			labelLightFont = Game.Content.Load<SpriteFont>(@"Fonts\labelMapCityFont.bmfc");

			UpdateProjectionMatrix(Game.GraphicsDevice.DisplayBounds.Width, Game.GraphicsDevice.DisplayBounds.Height);

			//Game.InputDevice.KeyDown	+= InputDeviceOnMouseDown;
			//Game.InputDevice.MouseMove	+= InputDeviceOnMouseMove;

			maxPitch = DMathUtil.DegreesToRadians(87.5);
			minPitch = DMathUtil.DegreesToRadians(-87.5);

			GoToPlace(Places.SaintPetersburg_VO);


			//var divisions = Game.GetService<LayerService>().GeoObjectsLayer.MunicipalDivisions;
			//foreach (var division in divisions) {
			//	AddMunicipalDivision(division.Name, division.Contour);				
			//}

			InitDots();

			InitAirLines();

			//InitBuildings();
			//InitRailroadsOSM();

		//	InitHeatMap(48);
		//	SetHeatMapCoordinates(30.155066037597368, 30.324324155276081, 59.966725082678039, 59.916127185943942);
			//UpdateHeatMapData();

			//InitAtmosphere();

			Game.GetService<LayerService>().MapLayer.OnProjectionChanged += (s) => { updateTiles = true; };

			CreateSphere(100, 100);


			//AddDebugLine(new DVector3(10000, 0, 0), Color.Red, new DVector3(0, 0, 0), Color.Red);
			//AddDebugLine(new DVector3(0, 10000, 0), Color.Green, new DVector3(0, 0, 0), Color.Green);
			//AddDebugLine(new DVector3(0, 0, 10000), Color.Blue, new DVector3(0, 0, 0), Color.Blue);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="place"></param>
		public void GoToPlace(Places place)
		{
			switch (place) {
				case Places.SaintPetersburg_VO: 
					Yaw		= DMathUtil.DegreesToRadians(30.301419);
					Pitch	= -DMathUtil.DegreesToRadians(59.942562);
					break;
				case Places.Vladivostok:
					Yaw		= DMathUtil.DegreesToRadians(131.881642);
					Pitch	= -DMathUtil.DegreesToRadians(43.111248);
					break;
			}
		}


		Vector2 previousMousePosition;
		public void InputDeviceOnMouseMove(object sender, Frame.MouseEventArgs args)
		{
			if (Game.InputDevice.IsKeyDown(Keys.LeftButton)) {
				DVector2 before, after;
				var beforeHit	= ScreenToSpherical(previousMousePosition.X,	previousMousePosition.Y,	out before);
				var afterHit	= ScreenToSpherical(args.X,			args.Y,			out after);

				if (beforeHit && afterHit) {
					Yaw		-= after.X - before.X;
					Pitch	+= after.Y - before.Y;
				}
				else {
					
				}
			}
			if (Game.InputDevice.IsKeyDown(Keys.RightButton)) {
				var dif = new Vector2(args.X, args.Y) - previousMousePosition;

				if (dif.Y > 0) {
					Config.CameraDistance -= (Config.CameraDistance - Config.earthRadius) * 0.04;
				} else if (dif.Y < 0) {
					Config.CameraDistance += (Config.CameraDistance - Config.earthRadius) * 0.04;
				}
			}
			if (Game.InputDevice.IsKeyDown(Keys.Up)) {
				Config.CameraDistance -= 0.01;
			}

			previousMousePosition = new Vector2(args.X, args.Y);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void InputDeviceOnMouseDown(object sender, Frame.MouseEventArgs args)
		{
			if (args.Key != Keys.LeftButton) return;

			var w = viewportWidth;
			var h = viewportHeight;

			var	pos	= new Vector2(args.X, args.Y);

			var nearPoint	= new DVector3(pos.X, pos.Y,	frustumZNear);
			var farPoint	= new DVector3(pos.X, pos.Y,	frustumZFar);

			var vm	= DMatrix.LookAtRH(cameraPosition, DVector3.Zero, DVector3.UnitY);
			var mVP = vm * projMatrix;

			var		near		= DVector3.Unproject(nearPoint,	0, 0, w, h, frustumZNear, frustumZFar, mVP);
			var		far			= DVector3.Unproject(farPoint,	0, 0, w, h, frustumZNear, frustumZFar, mVP);

			DVector3[] res;

			if (LineIntersection(near, far, Config.earthRadius, out res)) {
				if(res.Length > 0) {
					double lon, lat;

					CartesianToSpherical(res[0], out lon, out lat);
					//Console.WriteLine(DMathUtil.RadiansToDegrees(lon) + "	" + DMathUtil.RadiansToDegrees(lat));

					var lonLat = new DVector2(lon, lat);


					//var osm = Game.GetService<LayerService>().OpenStreetMapSource;
					//long nearest = osm.allNodes.First().Key;
					//
					//foreach (var node in osm.allNodes) {
					//	if ((lonLat - new DVector2(osm.allNodes[nearest].Longitude, osm.allNodes[nearest].Latitude)).Length() > (lonLat - new DVector2(node.Value.Longitude, node.Value.Latitude)).Length()) {
					//		nearest = node.Key;
					//	}
					//}

					//Console.WriteLine(nearest);


					// Calc cartesian coords
					//double x, y, z;
					//SphericalToCartesian(lonLat.X, lonLat.Y, Config.earthRadius, out x, out y, out z);
					//
					//var pointPos = new DVector3(x, y, z);
					//
					//var normal = pointPos;
					//normal.Normalize();
					//
					////var xAxis = DVector3.Transform(DVector3.UnitX, DQuaternion.RotationAxis(DVector3.UnitY, lonLat.X));
					//var xAxis = DVector3.TransformNormal(DVector3.UnitX, DMatrix.RotationAxis(DVector3.UnitY, lonLat.X));
					//xAxis.Normalize();
					//
					//var zAxis = DVector3.Cross(xAxis, normal);
					//
					//AddDebugLine(pointPos, Color.Green, pointPos + normal*5, Color.Green);
					//AddDebugLine(pointPos, Color.Red, pointPos + xAxis * 5, Color.Red);
					//AddDebugLine(pointPos, Color.Blue, pointPos + zAxis * 5, Color.Blue);

				
					if (trainShcedule != null) {
						int minInd = 0;

						for (int i = 0; i < trainShcedule.Count; i++) {
							if ((lonLat - trainShcedule[minInd].LonLat).Length() > (lonLat - trainShcedule[i].LonLat).Length()) {
								minInd = i;
							}
						}

						if((lonLat - trainShcedule[minInd].LonLat).Length() < 1.0)
							Console.WriteLine(trainShcedule[minInd].City);
					}
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update(GameTime gameTime)
		{
			if (Game.InputDevice.IsKeyDown(Keys.Right)) {
				t += 1;
				//if (t > 1.0f) t = 1.0f;
			}

			if (Game.InputDevice.IsKeyDown(Keys.Left)) {
				t -= 1;
				if (t < 0.0f) t = 0.0f;
			}


			UpdateAtmosphere(gameTime);


			//if (trainShcedule != null) {
			//	var pos = UpdateTrainPosition(trainShcedule.First().ArrivalTime + new TimeSpan(0, 0, (int)t, 0));
			//	if (Game.InputDevice.IsKeyDown(Keys.Space)) {
			//		Yaw		= DMathUtil.DegreesToRadians(pos.X);
			//		Pitch	= -DMathUtil.DegreesToRadians(pos.Y);
			//	}
			//
			//	var ds = Game.GetService<DebugStrings>();
			//
			//	ds.Add(trainShcedule.First().ArrivalTime.ToString());
			//	ds.Add((trainShcedule.First().ArrivalTime + new TimeSpan(0, 0, (int)(t), 0)).ToString());
			//}

		

			// Update camera position
			cameraPosition = DVector3.Transform( new DVector3(0, 0, Config.CameraDistance), Rotation);

			DetermineTiles();

			Game.GetService<DebugStrings>().Add("Tiles count : " + tilesToRender.Count);

			//UpdateDots(gameTime);

			UpdateCamera();


			//if (chartsCPU != null) {
			//	for (int i = 0; i < chartsCPU.Count; i++) {
			//		var r = chartsCPU[i];
			//
			//		//r.Tex.Y += (float)Math.Sin(Math.PI*gameTime.Total.TotalSeconds)*0.01f;
			//
			//		chartsCPU[i] = r;
			//	}
			//
			//	//UpdateCharts(chartsCPU);
			//}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime, StereoEye stereoEye)
		{
			UpdateProjectionMatrix(Game.GraphicsDevice.DisplayBounds.Width, Game.GraphicsDevice.DisplayBounds.Height);
			
			DrawInternal(gameTime);

		}


		double FreeCamYaw	= Math.PI;
		double FreeCamPitch = -Math.PI/10.0;

		void UpdateCamera()
		{
			var input = Game.InputDevice;

			if (input.IsKeyDown(Keys.LeftShift) && input.IsKeyDown(Keys.MiddleButton)) {
				FreeCamYaw		+= input.RelativeMouseOffset.X*0.003;
				FreeCamPitch	-= input.RelativeMouseOffset.Y*0.003;

				FreeCamPitch = DMathUtil.Clamp(FreeCamPitch, -Math.PI/2.01, 0.0);

				//Console.WriteLine(DMathUtil.RadiansToDegrees(FreeCamPitch));
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		void DrawInternal(GameTime gameTime)
		{

			var dev = Game.GraphicsDevice;

			viewMatrix = DMatrix.LookAtRH(cameraPosition, DVector3.Zero, DVector3.UnitY);

			DMatrix vvvM = viewMatrix;

			viewMatrix.TranslationVector = DVector3.Zero;

			var camPos = cameraPosition;

			if (Game.InputDevice.IsKeyDown(Keys.LeftShift)) {

				DVector3 cameraUp		= cameraPosition/cameraPosition.Length();
				DVector3 lookAtPoint	= cameraUp*Config.earthRadius;

				double length = Config.CameraDistance - Config.earthRadius;

				var quat = DQuaternion.RotationAxis(DVector3.UnitY, FreeCamYaw) * DQuaternion.RotationAxis(DVector3.UnitX, FreeCamPitch);

				var qRot	= DMatrix.RotationQuaternion(quat);
				var mat		= DMatrix.Identity;

				var xAxis = DVector3.TransformNormal(DVector3.UnitX, DMatrix.RotationAxis(DVector3.UnitY, Yaw));
				xAxis.Normalize();

				mat.Up = cameraUp;
				mat.Right = xAxis;
				mat.Forward = DVector3.Cross(xAxis, cameraUp);
				mat.Forward.Normalize();

				var matrix = qRot*mat;

				var c = DVector3.Transform(new DVector3(0, 0, length), matrix);

				var camPoint = new DVector3(c.X, c.Y, c.Z) + lookAtPoint;

				camPos = camPoint;

				viewMatrix = DMatrix.LookAtRH(camPoint, lookAtPoint, cameraUp);
				vvvM = viewMatrix;
				viewMatrix.TranslationVector = DVector3.Zero;

				//Console.WriteLine(FreeCamPitch);
			}

			var vm		= DMatrix.ToFloatMatrix(viewMatrix);
			var proj	= DMatrix.ToFloatMatrix(projMatrix);

			var viewDir = cameraPosition / cameraPosition.Length();

			ConstData cbData = new ConstData();

			cbData.ViewProj			= vm * proj;
			cbData.ViewPositionX	= camPos.X;
			cbData.ViewPositionY	= camPos.Y;
			cbData.ViewPositionZ	= camPos.Z;
			cbData.Temp				= new Vector4(t, (float)Config.earthRadius, 0, 0);
			cbData.ViewDir			= new Vector4((float)viewDir.X, (float)viewDir.Y, (float)viewDir.Z, 0);

			constBuffer.SetData( cbData );
			dev.VertexShaderConstants[0]	= constBuffer;

			shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
			shader.SetPixelShader( (int)GlobeFlags.DRAW_TEXTURED | (Config.ShowFrames ? (int)GlobeFlags.SHOW_FRAMES | (int)GlobeFlags.DRAW_POLY : (int)GlobeFlags.DRAW_POLY) );

			dev.PixelShaderSamplers[0] = SamplerState.LinearClamp;

			dev.RasterizerState = RasterizerState.CullCW;

			if (Game.InputDevice.IsKeyDown(Keys.Tab)) {
				dev.RasterizerState = RasterizerState.Wireframe;
			}

			dev.BlendState	=	BlendState.AlphaBlend;
			

	//		frame.SetPS(1);
			dev.PixelShaderResources[1] = frame; // ??? maybe

			if (vBuf != null) {
				dev.DepthStencilState = DepthStencilState.None;

	//			eTex.SetPS(0);
				dev.PixelShaderResources[0] = eTex; // ??? maybe

				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, vBuf, iBuf );
				dev.DrawIndexed(Primitive.TriangleList, iBuf.Capacity, 0, 0);
			}



			dev.DepthStencilState = DepthStencilState.Default;

#if false  /// this section is very slow
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
			sw.Reset();
			sw.Start();


			int debugCounter = 1;
			foreach (var globeTile in tilesToRender) {
				var tex = Game.GetService<LayerService>().MapLayer.CurrentMapSource.GetTile(globeTile.Value.X, globeTile.Value.Y, globeTile.Value.Z).Tile;

	//			tex.SetPS(0);
				dev.PixelShaderResources[0] = tex; // ??? maybe
				
				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, globeTile.Value.VertexBuf, globeTile.Value.IndexBuf );
				dev.DrawIndexed(Primitive.TriangleList, globeTile.Value.IndexBuf.Capacity, 0, 0);
				++debugCounter;
			}

			Console.WriteLine( "tiles to render: " + debugCounter );
			sw.Stop();
			Console.WriteLine( "2 " + sw.ElapsedMilliseconds );
			sw.Reset();
			sw.Start();

#endif
			dotsConstData.View	= vm;
			dotsConstData.Proj	= proj;

			if (Config.ShowMunicipalDivisions) {
				shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY/*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int)GlobeFlags.DRAW_PALETTE | (int)GlobeFlags.DRAW_POLY);
				
				dev.BlendState = BlendState.AlphaBlend;
				dev.DepthStencilState = DepthStencilState.None;


	//			paletteMunDiv.SetPS(0);
				dev.PixelShaderResources[0] = paletteMunDiv; // ??? maybe
				dev.PixelShaderSamplers[0] = SamplerState.LinearClamp;
				dev.RasterizerState = RasterizerState.CullNone;

				foreach (var mo in municipalDivisions) {
					cbData.Temp	= new Vector4(mo.Value.Value, Config.MunicipalDivisionTransparency, 0.0f, 0.0f);
					constBuffer.SetData(cbData);

					dev.VertexShaderConstants[0]	= constBuffer;
					dev.PixelShaderConstants[0]		= constBuffer;

					var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) ); 
					dev.SetupVertexInput( VertInLayout, mo.Value.Vertices, mo.Value.Indeces );
					dev.DrawIndexed(Primitive.TriangleList, mo.Value.Indeces.Capacity, 0, 0);
				}

				// Draw contours
				shader.SetPixelShader((int)GlobeFlags.DRAW_COLOR | (int)GlobeFlags.DRAW_POLY);

				cbData.Temp	= new Vector4(Config.MunicipalDivisionTransparency, 0.0f, 0.0f, 0.0f);
				constBuffer.SetData(cbData);

				dev.VertexShaderConstants[0]	= constBuffer;

	//			constBuffer.SetCBufferVS(0);
				dev.PixelShaderConstants[0]		= constBuffer;


				foreach (var mo in municipalDivisions) {
					var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
					dev.SetupVertexInput( VertInLayout, mo.Value.Contour, null);
					dev.Draw(Primitive.LineList, mo.Value.Contour.Capacity, 0);
				}


				//Draw lines
				if (linesVB != null && Config.ShowEdges) {
					shader.SetVertexShader((int)(GlobeFlags.DRAW_LINES/*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
					shader.SetPixelShader(		(int) GlobeFlags.DRAW_LINES					);
					shader.SetGeometryShader(	(int) GlobeFlags.DRAW_LINES					);

					dev.BlendState	= BlendState.AlphaBlend;
					dev.DepthStencilState	= DepthStencilState.None;

					dotsConstData.View	= vm;
					dotsConstData.Proj	= proj;
					dotsBuf.SetData(dotsConstData);

		//			dotsBuf.SetCBufferVS(1);
					dev.VertexShaderConstants[1]	= dotsBuf;
		//			dotsBuf.SetCBufferGS(1);
					dev.GeometryShaderConstants[1]	= dotsBuf;

		//			constBuffer.SetCBufferGS(0);
					dev.GeometryShaderConstants[0]	= constBuffer;

					dev.RasterizerState		= RasterizerState.CullNone;

					var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
					dev.SetupVertexInput( VertInLayout, linesVB, null );
					dev.Draw(Primitive.LineList, linesVB.Capacity, 0);
				}

				dev.GeometryShader = null;
			}

			// Draw simple railroads
			if (simpleRailroadsVB != null && Config.ShowRailroads) {
				shader.SetVertexShader(		(int)(GlobeFlags.DRAW_LINES/*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader(		(int)GlobeFlags.DRAW_SEGMENTED_LINES		);
				shader.SetGeometryShader(	(int)GlobeFlags.DRAW_SEGMENTED_LINES		);

				dev.BlendState	= BlendState.AlphaBlend;
				dev.DepthStencilState	=	DepthStencilState.None;

				cbData.Temp	= new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				constBuffer.SetData(cbData);

				dotsConstData.View	= vm;
				dotsConstData.Proj	= proj;
				dotsBuf.SetData(dotsConstData);

		
				dev.VertexShaderConstants[1]	= dotsBuf;
				dev.GeometryShaderConstants[1]	= dotsBuf;
				dev.GeometryShaderConstants[0]	= constBuffer;
				dev.VertexShaderConstants[0]	= constBuffer;
				dev.PixelShaderConstants[0]		= constBuffer;

				dev.RasterizerState	= RasterizerState.CullNone;

	//			railRoadsTex.SetPS(0);
				dev.PixelShaderResources[0]	= railRoadsTex; // ??? maybe

				dev.PixelShaderSamplers[0]	= SamplerState.LinearWrap;
				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, simpleRailroadsVB, null );

				dev.Draw(Primitive.LineList, simpleRailroadsVB.Capacity, 0);

				dev.GeometryShader = null;
			}


			// Draw railroads
			if (railRoadsVB != null && Config.ShowRailroads) {
				shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int)GlobeFlags.DRAW_COLOR | (int)GlobeFlags.DRAW_POLY);

				dev.BlendState	= BlendState.AlphaBlend;
				dev.DepthStencilState	= DepthStencilState.None;

				cbData.Temp	= new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				constBuffer.SetData(cbData);

	//			constBuffer.SetCBufferVS(0);
				dev.VertexShaderConstants[0] = constBuffer;
	//			constBuffer.SetCBufferPS(0);
				dev.PixelShaderConstants[0]	= constBuffer;

				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, railRoadsVB, null );
				dev.Draw(Primitive.LineStrip, railRoadsVB.Capacity, 0);
			}


			// Draw Roads
			if (roadsVB != null && Config.ShowRoads) {
				shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY/*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int)GlobeFlags.DRAW_COLOR | (int)GlobeFlags.DRAW_POLY);

				dev.BlendState			= BlendState.AlphaBlend;
				dev.DepthStencilState	= DepthStencilState.None;

		
				cbData.Temp	= new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				constBuffer.SetData(cbData);
				dev.VertexShaderConstants[0] = constBuffer;
				dev.PixelShaderConstants[0]	= constBuffer;

				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, roadsVB, null );
				dev.Draw(Primitive.LineList, roadsVB.Capacity, 0);
			}


			// Draw buildings
			if (contourBuildingsVB != null && Config.ShowBuildingsContours) {
				shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int)GlobeFlags.DRAW_COLOR | (int)GlobeFlags.DRAW_POLY);
			
				dev.BlendState	= BlendState.AlphaBlend;
				dev.DepthStencilState	= DepthStencilState.None;
			
				cbData.Temp	= new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				constBuffer.SetData(cbData);
				dev.VertexShaderConstants[0] = constBuffer;
				dev.PixelShaderConstants[0]	= constBuffer;
			
				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, contourBuildingsVB, null );
				dev.Draw(Primitive.LineList, contourBuildingsVB.Capacity, 0);
			}


			/////////////////////////////////////////////////////////////////////////////////////////////
			//if (simpleBuildings != null) {
			//	shader.SetVertexShader((int)GlobeFlags.DRAW_LINES, out signature);
			//	shader.SetPixelShader((int)GlobeFlags.DRAW_SIMPLE_BUILDINGS);
			//	shader.SetGeometryShader((int)GlobeFlags.DRAW_SIMPLE_BUILDINGS);
			//
			//	dev.SetBlendState(BlendState.Opaque);
			//	Game.GraphicsDevice.SetDepthStencilState(DepthStencilState.Default);
			//
			//	dotsBuf.Data.View = vm;
			//	dotsBuf.Data.Proj = proj;
			//	dotsBuf.UpdateCBuffer();
			//	dotsBuf.SetCBufferVS(1);
			//	dotsBuf.SetCBufferGS(1);
			//
			//
			//	dev.SetRasterizerState(RasterizerState.CullCW);
			//
			//
			//	dev.SetupVertexInput(simpleBuildingsVB, null, signature);
			//	dev.Draw(Primitive.PointList, simpleBuildingsVB.Capacity, 0);
			//
			//	shader.ResetGeometryShader();
			//
			//}
			///////////////////////////////////////////////////////////////////////////////////////////////

			if (Config.ShowPOI) {

				dotsConstData.View		= vm;
				dotsConstData.Proj		= proj;
				dotsConstData.TexWHST	= new Vector4(geoObjects.Width, geoObjects.Height, 164.0f, 0.05f);
				dotsBuf.SetData(dotsConstData);

				dev.VertexShaderConstants[1]	= dotsBuf;
				dev.GeometryShaderConstants[1]	= dotsBuf;
				dev.GeometryShaderConstants[0]	= constBuffer;

				dev.BlendState			= BlendState.AlphaBlend;
				dev.DepthStencilState	= DepthStencilState.None;

				shader.SetVertexShader((int)(GlobeFlags.DRAW_LINES /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int)GlobeFlags.DRAW_DOTS);
				shader.SetGeometryShader((int)(GlobeFlags.DRAW_DOTS /*| GlobeFlags.DOTS_WORLDSPACE*/));

				dev.PixelShaderSamplers[0]	= SamplerState.LinearClamp;

	//			geoObjects.SetPS(0);
				dev.PixelShaderResources[0]	= geoObjects; // ??? maybe


				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, dotsVB, null);
			
				dev.Draw(Primitive.PointList, dotsVB.Capacity - geoObjectStart, geoObjectStart);

				dev.GeometryShader = null;
			}
#if false
			if (Config.ShowInfectionPoints) {

				dotsConstData.View		= vm;
				dotsConstData.Proj		= proj;
				dotsConstData.TexWHST	= new Vector4(infectionDot.Width, infectionDot.Height, 64.0f, 0.03f);
				dotsBuf.SetData(dotsConstData);

				dev.VertexShaderConstants[1]	= dotsBuf;
				dev.GeometryShaderConstants[1]	= dotsBuf;

				dev.BlendState			= BlendState.AlphaBlend;
				dev.DepthStencilState	= DepthStencilState.None;

				shader.SetVertexShader((int)(GlobeFlags.DRAW_LINES /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int)GlobeFlags.DRAW_DOTS);
				shader.SetGeometryShader((int)(GlobeFlags.DRAW_DOTS /*| GlobeFlags.DOTS_WORLDSPACE*/));

				dev.PixelShaderSamplers[0]	= SamplerState.LinearClamp;

	//			infectionDot.SetPS(0);
				dev.PixelShaderResources[0]	= infectionDot; // ??? matbe

//!				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
//!				dev.SetupVertexInput( VertInLayout, dotsVB, null );
//!				dev.Draw(Primitive.PointList, geoObjectStart - infectionStart, infectionStart); // causes driver crash

				dev.GeometryShader = null;
			}
#endif
#if false
			// Draw people
			if (Config.ShowPeople) {
				dotsConstData.View		= vm;
				dotsConstData.Proj		= proj;
				dotsConstData.TexWHST	= new Vector4(socioClasses.Width, socioClasses.Height, 64.0f, 0.025f);
				dotsBuf.SetData(dotsConstData);

	//			Game.GraphicsDevice.SetVSConstant(1, dotsBuf);
				dev.VertexShaderConstants[1]	= dotsBuf;
	//			Game.GraphicsDevice.SetGSConstant(1, dotsBuf);
				dev.GeometryShaderConstants[1]	= dotsBuf;

	//			dev.SetBlendState(BlendState.AlphaBlend);
	//			Game.GraphicsDevice.SetDepthStencilState(DepthStencilState.None);
				dev.BlendState			= BlendState.AlphaBlend;
				dev.DepthStencilState	= DepthStencilState.None;

	//			dev.SetPSSamplerState(0, SamplerState.LinearClamp);
				dev.PixelShaderSamplers[0]	= SamplerState.LinearClamp;

				shader.SetVertexShader((int)(GlobeFlags.DRAW_LINES /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int) GlobeFlags.DRAW_DOTS);
				shader.SetGeometryShader((int)(GlobeFlags.DRAW_DOTS /*| GlobeFlags.DOTS_WORLDSPACE*/));


				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, dotsVB, null );
	//			socioClasses.SetPS(0);
				dev.PixelShaderResources[0]	= socioClasses; // ??? maybe

	//!			dev.Draw(Primitive.PointList, infectionStart - 1, 0); // causes driver crash

				dev.GeometryShader = null;
			}
#endif

#if false
			// Draw heatmap
			if (Config.ShowHeatMap && heatVB != null) {
				shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int)GlobeFlags.DRAW_POLY | (int)GlobeFlags.DRAW_HEAT);

	//			constBuffer.Data.Temp = new Vector4((float)Config.MaxHeatMapLevel, Config.HeatMapTransparency, HeatMapDim, 0);
				cbData.Temp	= new Vector4((float)Config.MaxHeatMapLevel, Config.HeatMapTransparency, HeatMapDim, 0);
				constBuffer.SetData(cbData);
	//			constBuffer.UpdateCBuffer();
	//			constBuffer.SetCBufferVS(0);

				dev.VertexShaderConstants[0] = constBuffer;
	//			constBuffer.SetCBufferPS(0);
				dev.PixelShaderConstants[0]	= constBuffer;

	//			dev.SetBlendState(BlendState.AlphaBlend);
	//			Game.GraphicsDevice.SetDepthStencilState(DepthStencilState.None);

				dev.BlendState			= BlendState.AlphaBlend;
				dev.DepthStencilState	= DepthStencilState.None;
				dev.PixelShaderSamplers[0]	= SamplerState.LinearClamp;
				dev.PixelShaderSamplers[1]	= SamplerState.AnisotropicClamp;

	//			dev.SetPSSamplerState(1, SamplerState.AnisotropicClamp);
						
	//			heatMap.SetPS(0); // ??? dynamic texture?
	//			heatMapPalette.SetPS(1);

				dev.PixelShaderResources[0]	= heatMap; // ??? maybe
				dev.PixelShaderResources[1]	= heatMapPalette; // ??? maybe

				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, heatVB, heatIB );
				dev.DrawIndexed(Primitive.TriangleList, heatIB.Capacity, 0, 0);

				//UpdateHeatMapData();
			}
#endif
#if false
			// Draw infect map
			if (Config.ShowInfectHeatMap && heatVB != null) {
				shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int)GlobeFlags.DRAW_POLY | (int)GlobeFlags.DRAW_HEAT);

				cbData.Temp		= new Vector4((float)Config.MaxInfectLevel, 0.0f, HeatMapDim, 0);
				constBuffer.SetData(cbData);
		//		constBuffer.SetCBufferVS(0);
				dev.VertexShaderConstants[0] = constBuffer;
		//		constBuffer.SetCBufferPS(0);
				dev.PixelShaderConstants[0]	= constBuffer;

				dev.BlendState			= BlendState.AlphaBlend;
				dev.DepthStencilState	= DepthStencilState.None;

				dev.PixelShaderSamplers[0]	= SamplerState.LinearClamp;
				dev.PixelShaderSamplers[1]	= SamplerState.AnisotropicClamp;

		//		infectMap.SetPS(0); // ??? dynamic texture?
		//		heatMapPalette.SetPS(1);

				dev.PixelShaderResources[0]	= infectMap; // ??? maybe
				dev.PixelShaderResources[1]	= heatMapPalette; // ??? maybe

				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, heatVB, heatIB );
				dev.DrawIndexed(Primitive.TriangleList, heatIB.Capacity, 0, 0);

				//UpdateHeatMapData();
			}
#endif
#if false	
			//Draw atmosphere
			if (Config.ShowAtmosphere && atmosVB != null) {
				shader.SetVertexShader((int)(GlobeFlags.DRAW_POLY /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader((int)GlobeFlags.DRAW_ATMOSPHERE);

				cbData.Temp	= new Vector4(atmosTime, Config.ArrowsScale, 0.0f, 0);
				constBuffer.SetData(cbData);
	//			constBuffer.SetCBufferVS(0);
				dev.VertexShaderConstants[0] = constBuffer;
	//			constBuffer.SetCBufferPS(0);
				dev.PixelShaderConstants[0]	= constBuffer;

				dev.BlendState			= BlendState.AlphaBlend;
				dev.DepthStencilState	= DepthStencilState.None;

	//			atmosTexture.SetPS(0);  // ??? dynamic texture?

	//			heatMapPalette.SetPS(1);
				dev.PixelShaderResources[1]	= heatMapPalette; // ??? maybe

	//			atmosNextTexture.SetPS(2);  // ??? dynamic texture?
				dev.PixelShaderResources[2]	= atmosNextTexture; // ??? maybe

	//			arrowTex.SetPS(3);
				dev.PixelShaderResources[3]	= arrowTex; // ??? maybe

				dev.PixelShaderSamplers[0]	= SamplerState.LinearClamp;
				dev.PixelShaderSamplers[1]	= SamplerState.AnisotropicClamp;

				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, atmosVB, atmosIB );
				dev.DrawIndexed(Primitive.TriangleList, atmosIB.Capacity, 0, 0);
			}

#endif
			//Draw airlines
			if(airLinesVB != null && Config.ShowAirLines) {
				shader.SetVertexShader((int)(GlobeFlags.DRAW_LINES /*| GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER*/));
				shader.SetPixelShader(		(int)GlobeFlags.DRAW_ARCS					);
				shader.SetGeometryShader(	(int)GlobeFlags.DRAW_ARCS					);

				dev.BlendState			= BlendState.Additive;
				dev.DepthStencilState	= DepthStencilState.Readonly;

				cbData.Temp	= new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				constBuffer.SetData(cbData);

				dotsConstData.View = vm;
				dotsConstData.Proj = proj;
				dotsBuf.SetData(dotsConstData);

		//		dotsBuf.SetCBufferVS(1);
				dev.VertexShaderConstants[1]	= dotsBuf;

		//		dotsBuf.SetCBufferGS(1);
				dev.GeometryShaderConstants[1]	= dotsBuf;

		//		constBuffer.SetCBufferGS(0);
				dev.GeometryShaderConstants[0]	= constBuffer;

				dev.RasterizerState	= RasterizerState.CullNone;

				var VertInLayout = new VertexInputLayout( dev, typeof(GeoVert) );
				dev.SetupVertexInput( VertInLayout, airLinesVB, null );
				dev.Draw(Primitive.LineList, airLinesVB.Capacity, 0);

				//Game.GraphicsDevice.DeviceContext.GeometryShader.Set(null);
				dev.GeometryShader = null;
			}

			DrawRailroadPoly();

			DrawTrain();

			DrawCharts();

			DrawDebugLines();

			//Game.GetService<DebugStrings>().Add("Cam pos     : " + Game.GetService<Camera>().CameraMatrix.TranslationVector, Color.Red);
			//Game.GetService<DebugStrings>().Add("Radius      : " + Config.earthRadius, Color.Red);
			//Game.GetService<DebugStrings>().Add("Level       : " + CurrentLevel, Color.Green);
			//Game.GetService<DebugStrings>().Add("CamDistance : " + Config.CameraDistance, Color.Green);
			//Game.GetService<DebugStrings>().Add("Pitch : " + Pitch, Color.Green);
			//Game.GetService<DebugStrings>().Add("Width : " + Game.GraphicsDevice.Viewport.Width, Color.Green);
			Game.GetService<DebugStrings>().Add("Height : " + (Config.CameraDistance - Config.earthRadius) * 1000.0, Color.Green);
			//
			//
			//Game.GetService<DebugStrings>().Add("FPS : " + gameTime.Fps);
			
	//		beamTexture		= Game.Content.Load<Texture2D>(@"UI\beam.tga");

			Color labelHeaderColor =  new Color(16, 125, 215, 255); //new Color( 99, 132, 181, 255);
			Color labelBgColor =  new Color(0, 0, 0, 92);

			

			if (trainShcedule != null) {
				var sb = Game.GetService<SpriteBatch>();

				sb.Begin(null, null, null, null, Matrix.OrthoOffCenterLH(vport.X, vport.X + vport.Width, vport.Y  + vport.Height, vport.Y, -9999, 9999));

				foreach (var line in trainShcedule) {
					double x, y, z;
					SphericalToCartesian(DMathUtil.DegreesToRadians(line.LonLat.X), DMathUtil.DegreesToRadians(line.LonLat.Y), Config.earthRadius, out x, out y, out z);

					var pos = new DVector3(x, y, z);

					if((pos - cameraPosition).Length() > 1000.0) continue;

					var p	= DVector3.Project(pos, vport.X, vport.Y, vport.Width, vport.Height, (float)camNear, (float)camFar, vvvM * projMatrix);

					int labelFrameWidth = 0;
					int cityLabelWidth = 300;
					int cityLabelXOffset = 40;
					int cityLabelYOffset = 20;
					int cityLabelTextOffset = 12;
					
					if (labelLightFont.MeasureString(line.City.ToUpper()).Width > cityLabelWidth) { cityLabelWidth = labelLightFont.MeasureString(line.City.ToUpper()).Width + cityLabelTextOffset*2;}
					
					int cityLabelBgHeight = labelLightFont.AverageCapitalLetterHeight + cityLabelTextOffset*2;
					int labelGlobalHeight = cityLabelBgHeight + labelFont.AverageCapitalLetterHeight*2 + cityLabelTextOffset*3 + 4;

					float cityLabelStartPosX = (float)p.X + cityLabelXOffset;
					float cityLabelStartPosY = (float)p.Y + cityLabelYOffset;

					float infectedPos = cityLabelStartPosY + cityLabelBgHeight + labelFont.AverageCapitalLetterHeight + cityLabelTextOffset + 2;
					float healedPos = infectedPos + labelFont.AverageCapitalLetterHeight + cityLabelTextOffset;

					float infectedXPos = cityLabelStartPosX + cityLabelWidth - labelFont.MeasureString(line.Sick.ToString()).Width - cityLabelTextOffset;
					float healedXPos = cityLabelStartPosX + cityLabelWidth - labelFont.MeasureString(line.Recovered.ToString()).Width - cityLabelTextOffset;

					sb.Draw(sb.TextureWhite, new Rectangle((int)cityLabelStartPosX - labelFrameWidth,(int)cityLabelStartPosY - labelFrameWidth, cityLabelWidth + labelFrameWidth*2, labelGlobalHeight + labelFrameWidth*2), labelBgColor);

		//			sb.DrawBeam(beamTexture, new Vector2((float)p.X, (float)p.Y), new Vector2(cityLabelStartPosX, cityLabelStartPosY), labelHeaderColor, labelHeaderColor, 12);

					sb.Draw(sb.TextureWhite, new Rectangle((int)cityLabelStartPosX,(int)cityLabelStartPosY, cityLabelWidth, cityLabelBgHeight), labelHeaderColor);
					
					labelLightFont.DrawString(sb, line.City.ToUpper(), (int)cityLabelStartPosX + cityLabelTextOffset, cityLabelStartPosY + labelLightFont.AverageCapitalLetterHeight + cityLabelTextOffset, Color.WhiteSmoke);
					labelFont.DrawString(sb, "Больных на текущий момент: ", (int)cityLabelStartPosX + cityLabelTextOffset, (int)infectedPos, Color.WhiteSmoke);
					labelFont.DrawString(sb, line.Sick.ToString(), (int)infectedXPos, (int)infectedPos, Color.OrangeRed);
					labelFont.DrawString(sb, "Выздоровевших за сутки: ", (int)cityLabelStartPosX + cityLabelTextOffset, (int)healedPos, Color.WhiteSmoke);
					labelFont.DrawString(sb, line.Recovered.ToString(), (int)healedXPos, (int)healedPos, Color.GreenYellow);
				}
				

				sb.End();
			}
		}

		Viewport vport;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="ds"></param>
		/// <param name="target"></param>
		public void DrawInRenderTarget(GameTime gameTime, DepthStencil2D ds, RenderTarget2D target, Viewport viewport)
		{
			UpdateProjectionMatrix(viewport.Width, viewport.Height);

			//Game.GraphicsDevice.ResetStates();
			Game.GraphicsDevice.SetTargets(ds.Surface, target.Surface);
			Game.GraphicsDevice.SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);

			vport = viewport;

			DrawInternal(gameTime);
			//Game.GraphicsDevice.RestoreBackbuffer();

			var sb = Game.GetService<SpriteBatch>();
			sb.Begin(blendState, SamplerState.LinearClamp, RasterizerState.CullNone, DepthStencilState.None);
			sb.Draw(sb.TextureWhite, new Rectangle(0, 0, target.Width, target.Height), new Color(1.0f, 1.0f, 1.0f, 1.0f));
			sb.End();
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Dispose()
		{
			foreach (var tile in tilesToRender) {
				tile.Value.Dispose();
			}
			foreach (var tile in tilesFree) {
				tile.Value.Dispose();
			}

			ClearAllMunicipalDivisions();

			frame.Dispose();
			shader.Dispose();
			constBuffer.Dispose();
			dotsBuf.Dispose();
			dotsVB.Dispose();

			socioClasses.Dispose();
			geoObjects.Dispose();
			infectionDot.Dispose();

	//		heatVB.Dispose();
	//		heatIB.Dispose();

	//		heatMapPalette.Dispose();
			paletteMunDiv0.Dispose();
			paletteMunDiv1.Dispose();

			if (linesVB != null)	linesVB.Dispose();
			if (airLinesVB != null) airLinesVB.Dispose();
			if (railRoadsVB != null)	railRoadsVB.Dispose();
			if (roadsVB != null)	roadsVB.Dispose();
	//		if (heatMap != null)	heatMap.Dispose();
	//		if (infectMap != null) infectMap.Dispose();
			if (arrowTex != null) arrowTex.Dispose();
			if (atmosVB != null)	atmosVB.Dispose();
			if (atmosIB  != null)	atmosIB.Dispose();
			if (atmosTexture != null)		atmosTexture.Dispose();
			if (atmosNextTexture != null)	atmosNextTexture.Dispose();
			if (contourBuildingsVB != null) contourBuildingsVB.Dispose();
			railRoadsTex.Dispose();

			blendState.Dispose();
			if (vBuf != null) vBuf.Dispose();
			if (iBuf != null) iBuf.Dispose();
			if (eTex != null) eTex.Dispose();

			base.Dispose();
		}



		/// <summary>
		/// 
		/// </summary>
		DVector2 GetCameraLonLat()
		{
			var nearPoint	= new DVector3((cameraPosition.X / Config.CameraDistance), (cameraPosition.Y / Config.CameraDistance), (cameraPosition.Z / Config.CameraDistance));
			var farPoint	= new DVector3(0, 0, 0);

			DVector3[] res;
			DVector2 ret = DVector2.Zero;

			if (LineIntersection(nearPoint, farPoint, 1.0, out res)) {
				if(res.Length > 0) {
					CartesianToSpherical(res[0], out ret.X, out ret.Y);
				}
			}

			return ret;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="lonLat"></param>
		/// <returns></returns>
		bool ScreenToSpherical(float x, float y, out DVector2 lonLat)
		{
			var w = viewportWidth;
			var h = viewportHeight;

			var nearPoint	= new DVector3(x, y, frustumZNear);
			var farPoint	= new DVector3(x, y, frustumZFar);


			var vm	= DMatrix.LookAtRH(cameraPosition, DVector3.Zero, DVector3.UnitY);
			var mVP = vm * projMatrix;

			var	near	= DVector3.Unproject(nearPoint,	0, 0, w, h, frustumZNear, frustumZFar, mVP);
			var	far		= DVector3.Unproject(farPoint,	0, 0, w, h, frustumZNear, frustumZFar, mVP);

			lonLat = DVector2.Zero;

			DVector3[] res;

			if (LineIntersection(near, far, Config.earthRadius, out res)) {
				CartesianToSpherical(res[0], out lonLat.X, out lonLat.Y);
				return true;
			}

			return false;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="lineOrigin"></param>
		/// <param name="lineEnd"></param>
		/// <param name="radius"></param>
		/// <param name="intersectionPoints"></param>
		/// <returns></returns>
		public bool LineIntersection(DVector3 lineOrigin, DVector3 lineEnd, double radius, out DVector3[] intersectionPoints)
		{
			intersectionPoints = new DVector3[0];

			double		sphereRadius = radius;
			DVector3	sphereCenter = DVector3.Zero;

			var lineDir = new DVector3(lineEnd.X - lineOrigin.X, lineEnd.Y - lineOrigin.Y, lineEnd.Z - lineOrigin.Z);
			lineDir.Normalize();

			DVector3 w = lineOrigin - sphereCenter;
			
			
			double a = DVector3.Dot(lineDir, lineDir); // 1.0f;
			double b = 2 * DVector3.Dot(lineDir, w);
			double c = DVector3.Dot(w, w) - sphereRadius*sphereRadius;
			
			
			double d = b*b - 4.0f*a*c;
			
			if (d < 0) return false;
			
			if (d == 0.0) {
			
				double x1 = (-b - Math.Sqrt(d))/(2.0*a);
			
				intersectionPoints		= new DVector3[1];
				intersectionPoints[0]	= lineOrigin + lineDir*x1;
			
				return true;
			}
			
			if (d > 0.0f) {
				double sqrt = Math.Sqrt(d);
			
				double x1 = (-b - sqrt) / (2.0 * a);
				double x2 = (-b + sqrt) / (2.0 * a);
			
				intersectionPoints = new DVector3[2];
				intersectionPoints[0] = lineOrigin + lineDir * x1;
				intersectionPoints[1] = lineOrigin + lineDir * x2;
			
				return true;  
			}

			return false;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="cart"></param>
		/// <param name="lonLat"></param>
		/// <param name="radius"></param>
		public void CartesianToSpherical(DVector3 cart, out double lon, out double lat)
		{
			var radius = cart.Length();

			if (radius == 0.0) {
				lon = 0;
				lat = 0;
				return;
			}

			lon = Math.Atan2(cart.X, cart.Z);
			lat = Math.Asin(cart.Y / radius);
		}


		public DVector2 CartesianToSpherical(DVector3 cart)
		{
			double lon, lat;
			CartesianToSpherical(cart, out lon, out lat);
			return new DVector2(lon, lat);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="lon"></param>
		/// <param name="lat"></param>
		/// <param name="radius"></param>
		/// <param name="cart"></param>
		public void SphericalToCartesian(double lon, double lat, float radius, out Vector3 cart)
		{
			cart = Vector3.Zero;
			
			double x, y, z;
			SphericalToCartesian(lon, lat, radius, out x, out y, out z);

			cart.X = (float)x;
			cart.Y = (float)y;
			cart.Z = (float)z;
		}



		public void SphericalToCartesian(double lon, double lat, double radius, out double x, out double y, out double z)
		{
			z = (radius * Math.Cos(lat) * Math.Cos(lon));
			x = (radius * Math.Cos(lat) * Math.Sin(lon));
			y = (radius * Math.Sin(lat));
		}


		public DVector3 SphericalToCartesian(DVector2 lonLat, double radius)
		{
			double x, y, z;
			SphericalToCartesian(lonLat.X, lonLat.Y, radius, out x, out y, out z);
			return new DVector3(x, y, z);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="lonLatP0"></param>
		/// lonLat should be in radians
		/// <param name="lonLatP1"></param>
		/// <returns></returns>
		public double DistanceBetweenTwoPoints(DVector2 lonLatP0, DVector2 lonLatP1)
		{
			var phi0 = lonLatP0.Y;
			var phi1 = lonLatP1.Y;
			var deltaPhi = phi1 - phi0;
			var deltaLam = lonLatP1.X - lonLatP0.X;

			var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
					Math.Cos(phi0) * Math.Cos(phi1) *
					Math.Sin(deltaLam / 2) * Math.Sin(deltaLam / 2);
			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			var d = Config.earthRadius * c;

			return d;
		}


		void UpdateProjectionMatrix(float width, float height)
		{
			viewportWidth	= width;
			viewportHeight	= height;

			double aspect = viewportWidth/viewportHeight;

			double nearHeight	= camNear * Math.Tan(DMathUtil.DegreesToRadians(camFov / 2));
			double nearWidth	= nearHeight * aspect;
			//float offset = separation / planeDist * near / 2;

			frustumWidth	= nearWidth;
			frustumHeight	= nearHeight;
			frustumZNear	= camNear;
			frustumZFar		= camFar;

			projMatrix = DMatrix.PerspectiveOffCenterRH(-nearWidth / 2, nearWidth / 2, -nearHeight / 2, nearHeight / 2, frustumZNear, frustumZFar);
		}
	}
}
