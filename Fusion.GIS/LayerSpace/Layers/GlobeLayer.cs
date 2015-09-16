using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Mathematics;
using Fusion.UserInterface;
using Fusion.GIS.DataSystem.MapSources.Projections;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;
using Fusion.Input;


namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer : BaseLayer
	{
		public struct GeoVert
		{
			[Vertex("Position")]
			public Vector3 Position;
			[Vertex("Color")]
			public Color Color;
			[Vertex("TEXCOORD", 0)]
			public Vector4 Tex;
			[Vertex("TEXCOORD", 1)]
			public double Lon;
			[Vertex("TEXCOORD", 2)]
			public double Lat;
		}


		public struct CartVert
		{
			[Vertex("Color")]
			public Color Color;
			[Vertex("TEXCOORD", 0)]
			public Vector4 Data;
			[Vertex("TEXCOORD", 1)]
			public double X;
			[Vertex("TEXCOORD", 2)]
			public double Y;
			[Vertex("TEXCOORD", 3)]
			public double Z;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct ConstData
		{
			[FieldOffset(0)]
			public Matrix ViewProj;
			[FieldOffset(64)]
			public double ViewPositionX;
			[FieldOffset(72)]
			public double ViewPositionY;
			[FieldOffset(80)]
			public double ViewPositionZ;
			[FieldOffset(96)]
			public Vector4 Temp;
			[FieldOffset(112)]
			public Vector4 ViewDir;
		}

		Ubershader shader;
		ConstBufferGeneric<ConstData> constBuffer;


		[Flags]
		enum GlobeFlags : int
		{
			NONE					= 0x0000,
			SHOW_FRAMES				= 0x0001,
			DRAW_COLOR				= 0x0002,
			DRAW_TEXTURED			= 0x0004,
			DRAW_PALETTE			= 0x0008,
			DRAW_POLY				= 0x0010,
			DRAW_DOTS				= 0x0020,
			DRAW_HEAT				= 0x0040,
			DRAW_LINES				= 0x0080,
			DRAW_ARCS				= 0x0100,
			DRAW_SEGMENTED_LINES	= 0x0200,
			DRAW_ATMOSPHERE			= 0x0400,
			DRAW_SIMPLE_BUILDINGS	= 0x0800,

			USE_GEOCOORDS	= 0x1000,
			USE_CARTCOORDS	= 0x2000,

			VERTEX_SHADER	= 0x4000,
			DRAW_CHARTS		= 0x8000,

			DOTS_SCREENSPACE	= 0x00010000,
			DOTS_WORLDSPACE		= 0x00020000,

			DRAW_VERTEX_LINES	= 0x00040000,
			DRAW_VERTEX_DOTS	= 0x00080000,
			DRAW_VERTEX_POLY	= 0x00100000,
		}


		BlendState blendState = new BlendState
		{
			WriteMask = ColorChannels.Alpha,
			SrcColor = Blend.SrcAlpha,
			DstColor = Blend.InvSrcAlpha,
			SrcAlpha = Blend.One,
			DstAlpha = Blend.Zero,
		};


		Texture2D frame;


		public enum Places
		{
			SaintPetersburg_VO,
			Vladivostok,
		}

		public int MaxDotsToDraw = 0;

		double pitch = 0.0f;
		double maxPitch = 0;
		double minPitch = 0;

		double Yaw { set; get; }
		double Pitch
		{
			set
			{
				pitch = value;
				if (pitch > maxPitch) pitch = maxPitch;
				if (pitch < minPitch) pitch = minPitch;
			}
			get { return pitch; }
		}

		DQuaternion Rotation { get { return DQuaternion.RotationAxis(DVector3.UnitY, Yaw) * DQuaternion.RotationAxis(DVector3.UnitX, Pitch); } }





		// Camera stuff
		public static DMatrix viewMatrix;
		public static DMatrix projMatrix;
		Matrix	viewMatrixFloat;
		Matrix	projMatrixFloat;
		
		DVector3 cameraPosition;
		float viewportWidth = 1280, viewportHeight = 720;
		double camNear = 10, camFar = 90000.0;
		double camFov = 20;

		double frustumWidth;
		double frustumHeight;
		double frustumZNear;
		double frustumZFar;



		void EnumFunc(GlobeFlags flags, PipelineState ps)
		{
			ps.VertexInputElements = flags.HasFlag(GlobeFlags.USE_CARTCOORDS) ? VertexInputElement.FromStructure<CartVert>() : VertexInputElement.FromStructure<GeoVert>();
		}


		class MyMiniFactory
		{
			Game game;
			Ubershader shader;


			public struct Tuple<F, S, T, A, B> : IEquatable<Tuple<F, S, T, A, B>>
			{
				public readonly F First;
				public readonly S Second;
				public readonly T Third;
				public readonly A Four;
				public readonly B Five;

				public Tuple(F first, S second, T third, A four, B five)
				{
					First	= first;
					Second	= second;
					Third	= third;
					Four	= four;
					Five	= five;
				}

				public override bool Equals(object obj)
				{
					if (obj == null)
						return false;
					if (this.GetType() != obj.GetType())
						return false;
					return AreEqual(this, (Tuple<F, S, T, A, B>)obj);
				}

				public bool Equals(Tuple<F, S, T, A, B> other)
				{
					return AreEqual(this, other);
				}

				private static bool AreEqual(Tuple<F, S, T, A, B> a,
				   Tuple<F, S, T, A, B> b)
				{
					if (!a.First.Equals(b.First))
						return false;
					if (!a.Second.Equals(b.Second))
						return false;
					if (!a.Third.Equals(b.Third))
						return false;
					if (!a.Four.Equals(b.Four))
						return false;
					if (!a.Five.Equals(b.Five))
						return false;
					return true;
				}

				public static bool operator ==(Tuple<F, S, T, A, B> a, Tuple<F, S, T, A, B> b)
				{
					return AreEqual(a, b);
				}

				public static bool operator !=(Tuple<F, S, T, A, B> a, Tuple<F, S, T, A, B> b)
				{
					return !AreEqual(a, b);
				}

				public override int GetHashCode()
				{
					return First.GetHashCode() ^ Second.GetHashCode() ^ Third.GetHashCode() ^ Four.GetHashCode() ^ Five.GetHashCode();
				}
			}

			public static class Tuple
			{
				public static Tuple<F, S, T, A, B> CreateNew<F, S, T, A, B>(F first, S second, T third, A four, B five)
				{
					return new Tuple<F, S, T, A, B>(first, second, third, four, five);
				}
			}

			Dictionary<Tuple<Primitive, BlendState, DepthStencilState, RasterizerState, bool>, StateFactory> factories = new Dictionary<Tuple<Primitive, BlendState, DepthStencilState, RasterizerState, bool>, StateFactory>(); 

			public MyMiniFactory(Game Game, Ubershader Shader)
			{
				game	= Game;
				shader	= Shader;
			}

			public PipelineState ProducePipelineState(GlobeFlags flags, Primitive primitive, BlendState blendState, RasterizerState rasterizerState, DepthStencilState depthStencilState)
			{
				var t = Tuple.CreateNew(primitive, blendState, depthStencilState, rasterizerState, flags.HasFlag(GlobeFlags.USE_CARTCOORDS));

				if (factories.ContainsKey(t)) {
					return factories[t][(int) flags];
				}
				
				var f = new StateFactory(shader, typeof (GlobeFlags), primitive,
										flags.HasFlag(GlobeFlags.USE_CARTCOORDS)
											? VertexInputElement.FromStructure<CartVert>()
											: VertexInputElement.FromStructure<GeoVert>(), blendState, rasterizerState, depthStencilState);
				factories.Add(t, f);

				return f[(int) flags];
			}


			public void Dispose()
			{
				foreach (var value in factories.Values) {
					value.Dispose();
				}
			}
		}


		MyMiniFactory myMiniFactory;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public GlobeLayer(Game game, LayerServiceConfig config) : base(game, config)
		{
			shader		= Game.Content.Load<Ubershader>(@"Globe.hlsl");
			constBuffer = new ConstBufferGeneric<ConstData>(Game.GraphicsDevice);

			frame			= Game.Content.Load<Texture2D>("redframe.tga");
			railRoadsTex	= Game.Content.Load<Texture2D>("Urban/Railroads.tga");

			UpdateProjectionMatrix(Game.GraphicsDevice.DisplayBounds.Width, Game.GraphicsDevice.DisplayBounds.Height);

			maxPitch = DMathUtil.DegreesToRadians(87.5);
			minPitch = DMathUtil.DegreesToRadians(-87.5);

			GoToPlace(Places.SaintPetersburg_VO);

			InitDots();

			InitAirLines();

			Game.GetService<LayerService>().MapLayer.OnProjectionChanged += (s) => { updateTiles = true; };

			CreateSphere(100, 50);

			//AddDebugLine(new DVector3(10000, 0, 0), Color.Red, new DVector3(0, 0, 0), Color.Red);
			//AddDebugLine(new DVector3(0, 10000, 0), Color.Green, new DVector3(0, 0, 0), Color.Green);
			//AddDebugLine(new DVector3(0, 0, 10000), Color.Blue, new DVector3(0, 0, 0), Color.Blue);


			myMiniFactory = new MyMiniFactory(Game, shader);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="place"></param>
		public void GoToPlace(Places place)
		{
			switch (place)
			{
				case Places.SaintPetersburg_VO:
					Yaw = DMathUtil.DegreesToRadians(30.301419);
					Pitch = -DMathUtil.DegreesToRadians(59.942562);
					break;
				case Places.Vladivostok:
					Yaw = DMathUtil.DegreesToRadians(131.881642);
					Pitch = -DMathUtil.DegreesToRadians(43.111248);
					break;
			}
		}


		Vector2 previousMousePosition;
		public void InputDeviceOnMouseMove(object sender, Frame.MouseEventArgs args)
		{
			if (Game.InputDevice.IsKeyDown(Keys.LeftButton)) {
				DVector2 before, after;
				var beforeHit = ScreenToSpherical(previousMousePosition.X, previousMousePosition.Y, out before);
				var afterHit = ScreenToSpherical(args.X, args.Y, out after);

				if (beforeHit && afterHit) {
					Yaw -= after.X - before.X;
					Pitch += after.Y - before.Y;
				}
			}
			if (Game.InputDevice.IsKeyDown(Keys.RightButton)) {
				var dif = new Vector2(args.X, args.Y) - previousMousePosition;

				if (dif.Y > 0) {
					Config.CameraDistance -= (Config.CameraDistance - Config.earthRadius) * 0.04;
				}
				else if (dif.Y < 0) {
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

			var pos = new Vector2(args.X, args.Y);

			var nearPoint	= new DVector3(pos.X, pos.Y, frustumZNear);
			var farPoint	= new DVector3(pos.X, pos.Y, frustumZFar);

			var vm	= DMatrix.LookAtRH(cameraPosition, DVector3.Zero, DVector3.UnitY);
			var mVP = vm * projMatrix;

			var near	= DVector3.Unproject(nearPoint, 0, 0, w, h, frustumZNear, frustumZFar, mVP);
			var far		= DVector3.Unproject(farPoint,	0, 0, w, h, frustumZNear, frustumZFar, mVP);

			DVector3[] res;

			if (LineIntersection(near, far, Config.earthRadius, out res)) {
				if (res.Length > 0) {
					double lon, lat;

					CartesianToSpherical(res[0], out lon, out lat);

					var lonLat = new DVector2(DMathUtil.RadiansToDegrees(lon), DMathUtil.RadiansToDegrees(lat));

					Console.WriteLine(lonLat);
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update(GameTime gameTime)
		{
			// Update camera position
			cameraPosition = DVector3.Transform(new DVector3(0, 0, Config.CameraDistance), Rotation);

			DetermineTiles();

			//Game.GetService<DebugStrings>().Add("Tiles count : " + tilesToRender.Count);

			UpdateCamera();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Draw(GameTime gameTime)
		{
			UpdateProjectionMatrix(Game.GraphicsDevice.DisplayBounds.Width, Game.GraphicsDevice.DisplayBounds.Height);

			DrawInternal(gameTime);
		}


		double FreeCamYaw = Math.PI/2.0;
		double FreeCamPitch = -Math.PI / 10.0;

		void UpdateCamera()
		{
			var input = Game.InputDevice;

			if (input.IsKeyDown(Keys.LeftShift) && input.IsKeyDown(Keys.MiddleButton)) {
				FreeCamYaw		+= input.RelativeMouseOffset.X * 0.003;
				FreeCamPitch	-= input.RelativeMouseOffset.Y * 0.003;

				FreeCamPitch = DMathUtil.Clamp(FreeCamPitch, -Math.PI / 2.01, 0.0);
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

				DVector3 cameraUp = cameraPosition / cameraPosition.Length();
				DVector3 lookAtPoint = cameraUp * Config.earthRadius;

				double length = Config.CameraDistance - Config.earthRadius;

				var quat = DQuaternion.RotationAxis(DVector3.UnitY, FreeCamYaw) * DQuaternion.RotationAxis(DVector3.UnitX, FreeCamPitch);

				var qRot = DMatrix.RotationQuaternion(quat);
				var mat = DMatrix.Identity;

				var xAxis = DVector3.TransformNormal(DVector3.UnitX, DMatrix.RotationAxis(DVector3.UnitY, Yaw));
				xAxis.Normalize();

				mat.Up = cameraUp;
				mat.Right = xAxis;
				mat.Forward = DVector3.Cross(xAxis, cameraUp);
				mat.Forward.Normalize();

				var matrix = qRot * mat;

				var c = DVector3.Transform(new DVector3(0, 0, length), matrix);

				var camPoint = new DVector3(c.X, c.Y, c.Z) + lookAtPoint;

				camPos = camPoint;

				viewMatrix = DMatrix.LookAtRH(camPoint, lookAtPoint, cameraUp);
				vvvM = viewMatrix;
				viewMatrix.TranslationVector = DVector3.Zero;
			}

			viewMatrixFloat = DMatrix.ToFloatMatrix(viewMatrix);
			projMatrixFloat = DMatrix.ToFloatMatrix(projMatrix);

			var viewDir = cameraPosition / cameraPosition.Length();

			constBuffer.Data.ViewProj = viewMatrixFloat * projMatrixFloat;
			constBuffer.Data.ViewPositionX = camPos.X;
			constBuffer.Data.ViewPositionY = camPos.Y;
			constBuffer.Data.ViewPositionZ = camPos.Z;
			constBuffer.Data.Temp = new Vector4(0.0f, (float)Config.earthRadius, 0, 0);
			constBuffer.Data.ViewDir = new Vector4((float)viewDir.X, (float)viewDir.Y, (float)viewDir.Z, 0);
			constBuffer.UpdateCBuffer();

			Game.GraphicsDevice.VertexShaderConstants[0] = constBuffer;

			Game.GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearClamp;
			Game.GraphicsDevice.PixelShaderResources[1] = frame;

			if (gridVertexBuffer != null) {
				Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_POLY | GlobeFlags.DRAW_VERTEX_POLY | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_TEXTURED | (Config.ShowFrames ? GlobeFlags.SHOW_FRAMES : GlobeFlags.NONE),
					Primitive.TriangleList,
					BlendState.AlphaBlend,
					RasterizerState.CullCW,
					DepthStencilState.Readonly);

				Game.GraphicsDevice.PixelShaderResources[0] = gridTex;

				Game.GraphicsDevice.SetupVertexInput(gridVertexBuffer, gridIndexBuffer);
				dev.DrawIndexed(/*Primitive.TriangleList,*/ gridIndexBuffer.Capacity, 0, 0);
			}

			var rastState = Game.InputDevice.IsKeyDown(Keys.Tab) ? RasterizerState.Wireframe : RasterizerState.CullCW;

			Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_POLY | GlobeFlags.DRAW_VERTEX_POLY | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_TEXTURED | (Config.ShowFrames ? GlobeFlags.SHOW_FRAMES : GlobeFlags.NONE),
					Primitive.TriangleList,
					BlendState.AlphaBlend,
					rastState,
					DepthStencilState.Default);

			foreach (var globeTile in tilesToRender) {
				var tex = Game.GetService<LayerService>().MapLayer.CurrentMapSource.GetTile(globeTile.Value.X, globeTile.Value.Y, globeTile.Value.Z).Tile;
				dev.PixelShaderResources[0] = tex;

				dev.SetupVertexInput(globeTile.Value.VertexBuf, globeTile.Value.IndexBuf);
				dev.DrawIndexed(globeTile.Value.IndexBuf.Capacity, 0, 0);
			}


			dotsBuf.Data.View = viewMatrixFloat;
			dotsBuf.Data.Proj = projMatrixFloat;
			dotsBuf.UpdateCBuffer();

			//// Draw simple railroads
			//if (simpleRailroadsVB != null && Config.ShowRailroads) {
			//	Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
			//		GlobeFlags.DRAW_VERTEX_LINES | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_SEGMENTED_LINES,
			//		Primitive.LineList,
			//		BlendState.AlphaBlend,
			//		RasterizerState.CullNone,
			//		DepthStencilState.None);
			//
			//	constBuffer.Data.Temp = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
			//	constBuffer.UpdateCBuffer();
			//
			//	Game.GraphicsDevice.VertexShaderConstants[1]	= dotsBuf;
			//	Game.GraphicsDevice.GeometryShaderConstants[1]	= dotsBuf;
			//	Game.GraphicsDevice.GeometryShaderConstants[0]	= constBuffer;
			//	Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			//	Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;
			//
			//	Game.GraphicsDevice.PixelShaderResources[0] = railRoadsTex;
			//	Game.GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearWrap;
			//
			//	dev.SetupVertexInput(simpleRailroadsVB, null);
			//	dev.Draw(simpleRailroadsVB.Capacity, 0);
			//}


			//// Draw buildings
			//if (contourBuildingsVB != null && Config.ShowBuildingsContours) {
			//
			//	Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
			//		GlobeFlags.DRAW_POLY | GlobeFlags.DRAW_VERTEX_POLY | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_COLOR,
			//		Primitive.LineList,
			//		BlendState.AlphaBlend,
			//		RasterizerState.CullNone,
			//		DepthStencilState.None);
			//
			//
			//	constBuffer.Data.Temp = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
			//	constBuffer.UpdateCBuffer();
			//
			//	Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			//	Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;
			//
			//	dev.SetupVertexInput(contourBuildingsVB, null);
			//	dev.Draw(contourBuildingsVB.Capacity, 0);
			//}

			// Draw Lines
			if (linesBatch.Count != 0 && Config.ShowRoads) {
				Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_POLY | GlobeFlags.DRAW_VERTEX_POLY | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_COLOR,
					Primitive.LineList,
					BlendState.AlphaBlend,
					RasterizerState.CullNone,
					DepthStencilState.None);
			
			
				constBuffer.Data.Temp = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				constBuffer.UpdateCBuffer();
			
				Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
				Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;

				foreach (var vb in linesBatch) {
					dev.SetupVertexInput(vb, null);
					dev.Draw(vb.Capacity, 0);
				}
			}



			// Draw simple railroads
			if (linesPolyBatch.Count != 0 && Config.ShowRoads) {
				Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_VERTEX_LINES | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_LINES,
					Primitive.LineList,
					BlendState.AlphaBlend,
					RasterizerState.CullNone,
					DepthStencilState.None);
			
				constBuffer.Data.Temp = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				constBuffer.UpdateCBuffer();
			
				Game.GraphicsDevice.VertexShaderConstants[1]	= dotsBuf;
				Game.GraphicsDevice.GeometryShaderConstants[1]	= dotsBuf;
				Game.GraphicsDevice.GeometryShaderConstants[0]	= constBuffer;
				Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
				Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;
			
			
				foreach (var vb in linesPolyBatch) {
					dev.SetupVertexInput(vb, null);
					dev.Draw(vb.Capacity, 0);
				}
			}



			if (Config.ShowPOI) {
				dotsBuf.Data.TexWHST = new Vector4(geoObjects.Width, geoObjects.Height, 164.0f, 0.05f);
				dotsBuf.UpdateCBuffer();

				Game.GraphicsDevice.VertexShaderConstants[1]	= dotsBuf;
				Game.GraphicsDevice.GeometryShaderConstants[1]	= dotsBuf;
				Game.GraphicsDevice.GeometryShaderConstants[0]	= constBuffer;

				Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_VERTEX_LINES | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_DOTS | GlobeFlags.DOTS_WORLDSPACE,
					Primitive.PointList,
					BlendState.AlphaBlend,
					RasterizerState.CullNone,
					DepthStencilState.None);

				Game.GraphicsDevice.PixelShaderResources[0] = geoObjects;
				Game.GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearClamp;

				dev.SetupVertexInput(dotsVB, null);
				dev.Draw(dotsVB.Capacity - geoObjectStart, geoObjectStart);
			}

			

			// Draw people
			if (Config.ShowPeople) {
				dotsBuf.Data.TexWHST = new Vector4(socioClasses.Width, socioClasses.Height, 64.0f, 0.025f);
				dotsBuf.UpdateCBuffer();

				Game.GraphicsDevice.VertexShaderConstants[1]	= dotsBuf;
				Game.GraphicsDevice.GeometryShaderConstants[1]	= dotsBuf;
				Game.GraphicsDevice.GeometryShaderConstants[0] = constBuffer;

				Game.GraphicsDevice.PixelShaderResources[0] = socioClasses;
				Game.GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearClamp;

				Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_VERTEX_LINES | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_DOTS | GlobeFlags.DOTS_WORLDSPACE,
					Primitive.PointList,
					BlendState.AlphaBlend,
					RasterizerState.CullNone,
					DepthStencilState.None);

				dev.SetupVertexInput(dotsVB, null);
				dev.Draw( MaxDotsToDraw == 0 ? geoObjectStart - 1 : MaxDotsToDraw, 0);
			}



			//// Draw heatmap
			//if (Config.ShowHeatMap && heatVB != null) {
			//	constBuffer.Data.Temp = new Vector4((float)Config.MaxHeatMapLevel, Config.HeatMapTransparency, HeatMapDim, 0);
			//	constBuffer.UpdateCBuffer();
			//
			//	Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			//	Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;
			//
			//	Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
			//		GlobeFlags.DRAW_POLY | GlobeFlags.DRAW_VERTEX_POLY | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_HEAT,
			//		Primitive.TriangleList,
			//		BlendState.AlphaBlend,
			//		RasterizerState.CullNone,
			//		DepthStencilState.None);
			//
			//
			//	Game.GraphicsDevice.PixelShaderSamplers[0] = SamplerState.LinearClamp;
			//	Game.GraphicsDevice.PixelShaderSamplers[1] = SamplerState.AnisotropicClamp;
			//
			//	Game.GraphicsDevice.PixelShaderResources[0] = heatMap;
			//	Game.GraphicsDevice.PixelShaderResources[1] = heatMapPalette;
			//
			//	Game.GraphicsDevice.SetupVertexInput(heatVB, heatIB);
			//	Game.GraphicsDevice.DrawIndexed(heatIB.Capacity, 0, 0);
			//}

			//// Draw infection map
			//if (Config.ShowInfectHeatMap && heatVB != null) {
			//	constBuffer.Data.Temp = new Vector4((float)Config.MaxInfectLevel, 0.0f, HeatMapDim, 0);
			//	constBuffer.UpdateCBuffer();
			//
			//	Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			//	Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;
			//
			//	Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
			//		GlobeFlags.DRAW_POLY | GlobeFlags.DRAW_VERTEX_POLY | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_HEAT,
			//		Primitive.TriangleList,
			//		BlendState.AlphaBlend,
			//		RasterizerState.CullNone,
			//		DepthStencilState.None);
			//
			//	Game.GraphicsDevice.PixelShaderSamplers[0] = SamplerState.LinearClamp;
			//	Game.GraphicsDevice.PixelShaderSamplers[1] = SamplerState.AnisotropicClamp;
			//
			//	Game.GraphicsDevice.PixelShaderResources[0] = infectMap;
			//	Game.GraphicsDevice.PixelShaderResources[1] = heatMapPalette;
			//
			//	Game.GraphicsDevice.SetupVertexInput(heatVB, heatIB);
			//	Game.GraphicsDevice.DrawIndexed(/*Primitive.TriangleList,*/ heatIB.Capacity, 0, 0);
			//}


			////Draw atmosphere
			//if (Config.ShowAtmosphere && atmosVB != null) {
			//	constBuffer.Data.Temp = new Vector4(atmosTime, Config.ArrowsScale, 0.0f, 0);
			//	constBuffer.UpdateCBuffer();
			//
			//	Game.GraphicsDevice.VertexShaderConstants[0]	= constBuffer;
			//	Game.GraphicsDevice.PixelShaderConstants[0]		= constBuffer;
			//
			//
			//	Game.GraphicsDevice.PixelShaderResources[0] = atmosTexture;
			//	Game.GraphicsDevice.PixelShaderResources[1] = heatMapPalette;
			//	Game.GraphicsDevice.PixelShaderResources[2] = atmosNextTexture;
			//	Game.GraphicsDevice.PixelShaderResources[3] = arrowTex;
			//
			//
			//	Game.GraphicsDevice.PixelShaderSamplers[0] = SamplerState.LinearClamp;
			//	Game.GraphicsDevice.PixelShaderSamplers[1] = SamplerState.AnisotropicClamp;
			//
			//	Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
			//		GlobeFlags.DRAW_VERTEX_POLY | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_ATMOSPHERE,
			//		Primitive.TriangleList,
			//		BlendState.AlphaBlend,
			//		RasterizerState.CullNone,
			//		DepthStencilState.None);
			//
			//	Game.GraphicsDevice.SetupVertexInput(atmosVB, atmosIB);
			//	Game.GraphicsDevice.DrawIndexed(atmosIB.Capacity, 0, 0);
			//}


			//Draw airlines
			if (airLinesVB != null && Config.ShowAirLines) {
				constBuffer.Data.Temp = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
				constBuffer.UpdateCBuffer();

				Game.GraphicsDevice.VertexShaderConstants[1]	= dotsBuf;
				Game.GraphicsDevice.GeometryShaderConstants[0]	= constBuffer;
				Game.GraphicsDevice.GeometryShaderConstants[1]	= dotsBuf;

				Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_VERTEX_LINES | GlobeFlags.USE_GEOCOORDS | GlobeFlags.VERTEX_SHADER | GlobeFlags.DRAW_ARCS,
					Primitive.LineList,
					BlendState.Additive,
					RasterizerState.CullNone,
					DepthStencilState.Readonly);

				dev.SetupVertexInput(airLinesVB, null);
				dev.Draw(/*Primitive.LineList,*/ airLinesVB.Capacity, 0);
			}

			DrawDebugLines();

			//Game.GetService<DebugStrings>().Add("Cam pos     : " + Game.GetService<Camera>().CameraMatrix.TranslationVector, Color.Red);
			//Game.GetService<DebugStrings>().Add("Radius      : " + Config.earthRadius, Color.Red);
			//Game.GetService<DebugStrings>().Add("Level       : " + CurrentLevel, Color.Green);
			//Game.GetService<DebugStrings>().Add("CamDistance : " + Config.CameraDistance, Color.Green);
			//Game.GetService<DebugStrings>().Add("Pitch : " + Pitch, Color.Green);
			//Game.GetService<DebugStrings>().Add("Width : " + Game.GraphicsDevice.Viewport.Width, Color.Green);
			Game.GetService<DebugStrings>().Add("", Color.Green);
			//Game.GetService<DebugStrings>().Add("Height : " + (Config.CameraDistance - Config.earthRadius) * 1000.0, Color.Green);

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
			Game.GraphicsDevice.SetTargets(ds, target);
			Game.GraphicsDevice.SetViewport(viewport.X, viewport.Y, viewport.Width, viewport.Height);

			vport = viewport;

			DrawInternal(gameTime);
			//Game.GraphicsDevice.RestoreBackbuffer();

			var sb = Game.GetService<SpriteBatch>();
			sb.Begin(SpriteBlend.ClearAlpha, SamplerState.LinearClamp, DepthStencilState.None);
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

			frame.Dispose();
			shader.Dispose();
			constBuffer.Dispose();
			dotsBuf.Dispose();
			dotsVB.Dispose();

			socioClasses.Dispose();
			geoObjects.Dispose();


			if (municipalLinesVB != null)	municipalLinesVB.Dispose();
			if (airLinesVB != null)			airLinesVB.Dispose();
			if (railRoadsVB != null)		railRoadsVB.Dispose();
			if (roadsVB != null)			roadsVB.Dispose();
			if (heatMap != null)			heatMap.Dispose();
			if (infectMap != null)			infectMap.Dispose();
			if (arrowTex != null)			arrowTex.Dispose();
			if (atmosVB != null)			atmosVB.Dispose();
			if (atmosIB != null)			atmosIB.Dispose();
			if (atmosTexture != null)		atmosTexture.Dispose();
			if (atmosNextTexture != null)	atmosNextTexture.Dispose();
			if (contourBuildingsVB != null) contourBuildingsVB.Dispose();
			railRoadsTex.Dispose();

			if (gridVertexBuffer != null)	gridVertexBuffer.Dispose();
			if (gridIndexBuffer != null)	gridIndexBuffer.Dispose();
			if (gridTex != null)			gridTex.Dispose();

			base.Dispose();
		}



		/// <summary>
		/// 
		/// </summary>
		DVector2 GetCameraLonLat()
		{
			var nearPoint = new DVector3((cameraPosition.X / Config.CameraDistance), (cameraPosition.Y / Config.CameraDistance), (cameraPosition.Z / Config.CameraDistance));
			var farPoint = new DVector3(0, 0, 0);

			DVector3[] res;
			DVector2 ret = DVector2.Zero;

			if (LineIntersection(nearPoint, farPoint, 1.0, out res)) {
				if (res.Length > 0) {
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

			var nearPoint = new DVector3(x, y, frustumZNear);
			var farPoint = new DVector3(x, y, frustumZFar);


			var vm = DMatrix.LookAtRH(cameraPosition, DVector3.Zero, DVector3.UnitY);
			var mVP = vm * projMatrix;

			var near = DVector3.Unproject(nearPoint, 0, 0, w, h, frustumZNear, frustumZFar, mVP);
			var far = DVector3.Unproject(farPoint, 0, 0, w, h, frustumZNear, frustumZFar, mVP);

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
		bool LineIntersection(DVector3 lineOrigin, DVector3 lineEnd, double radius, out DVector3[] intersectionPoints)
		{
			intersectionPoints = new DVector3[0];

			double sphereRadius = radius;
			DVector3 sphereCenter = DVector3.Zero;

			var lineDir = new DVector3(lineEnd.X - lineOrigin.X, lineEnd.Y - lineOrigin.Y, lineEnd.Z - lineOrigin.Z);
			lineDir.Normalize();

			DVector3 w = lineOrigin - sphereCenter;


			double a = DVector3.Dot(lineDir, lineDir); // 1.0f;
			double b = 2 * DVector3.Dot(lineDir, w);
			double c = DVector3.Dot(w, w) - sphereRadius * sphereRadius;


			double d = b * b - 4.0f * a * c;

			if (d < 0) return false;

			if (d == 0.0)
			{

				double x1 = (-b - Math.Sqrt(d)) / (2.0 * a);

				intersectionPoints = new DVector3[1];
				intersectionPoints[0] = lineOrigin + lineDir * x1;

				return true;
			}

			if (d > 0.0f)
			{
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
		void CartesianToSpherical(DVector3 cart, out double lon, out double lat)
		{
			var radius = cart.Length();

			if (radius == 0.0)
			{
				lon = 0;
				lat = 0;
				return;
			}

			lon = Math.Atan2(cart.X, cart.Z);
			lat = Math.Asin(cart.Y / radius);
		}


		DVector2 CartesianToSpherical(DVector3 cart)
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



		void SphericalToCartesian(double lon, double lat, double radius, out double x, out double y, out double z)
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
			viewportWidth = width;
			viewportHeight = height;

			double aspect = viewportWidth / viewportHeight;

			double nearHeight = camNear * Math.Tan(DMathUtil.DegreesToRadians(camFov / 2));
			double nearWidth = nearHeight * aspect;
			//float offset = separation / planeDist * near / 2;

			frustumWidth = nearWidth;
			frustumHeight = nearHeight;
			frustumZNear = camNear;
			frustumZFar = camFar;

			projMatrix = DMatrix.PerspectiveOffCenterRH(-nearWidth / 2, nearWidth / 2, -nearHeight / 2, nearHeight / 2, frustumZNear, frustumZFar);
		}
	}
}
