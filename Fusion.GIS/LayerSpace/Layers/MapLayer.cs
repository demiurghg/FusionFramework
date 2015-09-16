using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Fusion.Mathematics;
using Fusion.GIS.DataSystem.MapSources;
using Fusion.GIS.DataSystem.MapSources.OpenStreetMaps;
using Fusion.GIS.DataSystem.MapSources.Projections;
using Fusion.GIS.DataSystem.MapSources.YandexMaps;
using Fusion.GIS.MapSources;
using Fusion.Input;
using Fusion.Graphics;
/*using SharpDX.DXGI;
using SharpDX.Direct3D;
/*using SharpDX.Direct3D11;
using D3DTex2D = SharpDX.Direct3D11.Texture2D;
using BlendState = Fusion.Graphics.BlendState;
using Buffer	= SharpDX.Direct3D11.Buffer;
using DepthStencilState = Fusion.Graphics.DepthStencilState;
using MapFlags	= SharpDX.Direct3D11.MapFlags;
using RasterizerState = Fusion.Graphics.RasterizerState;
using SamplerState = Fusion.Graphics.SamplerState;*/

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class MapLayer : BaseLayer
	{
		public BaseMapSource CurrentMapSource;

		protected Vector2 CenterPositionScreen		{ set; get; }
		protected Vector2 CenterPositionMercator	{ set; get; }
		protected Vector2 CenterPositionWorld		{ set; get; }

		Matrix Transform;

		int oldMouseScroll = 0;

		public Vector2	Offset;
		public float	Zoom	= 1280;
		public float	Level	= 1;

		public int TilesCountOnLowLevel = 0;
		public const int MAX_TILES = 15;


		SpriteBatch sb;

		public delegate void ProjectionChangedDelegate	( object sender );
		public event ProjectionChangedDelegate OnProjectionChanged;


		public enum Places
		{
			VasilIsland
		}


		public MapLayer(Game game, LayerServiceConfig config) : base(game, config)
		{
			Transform = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Vector3.One, Vector3.Zero, Quaternion.Identity, Vector3.Zero);

			sb = Game.GetService<SpriteBatch>();

			RegisterMapSources();
			CurrentMapSource = MapSources[0];

			//Game.InputDevice.MouseMove += InputDeviceOnMouseMove;

			InitShaderAndBuffer();
		}

		void InputDeviceOnMouseMove(object sender, Vector2 position)
		{
			if (Game.InputDevice.IsKeyDown(Keys.LeftButton)) {
				Transform.TranslationVector += new Vector3(Game.InputDevice.GlobalMouseOffset - oldMousePos, 0.0f);
			}
			oldMousePos = Game.InputDevice.GlobalMouseOffset;
		}

		Vector2 oldMousePos = Vector2.Zero;

		public void GoToPlace(Places place)
		{
			switch (place) {
				case Places.VasilIsland: {
						Transform = Matrix.Scaling(2695866.0f/1280.0f);
						Transform = Transform*Matrix.Translation(-1573813.0f, -783398.6f, 0.0f);
						break;
					}
			}
		}


		public Vector2 ScreenCoordsToMercator(Vector2 pos)
		{
			return (pos - Offset)/Zoom;
		}

		public Vector2 ScreenCoordsToWorld(Vector2 pos)
		{
			var m = ScreenCoordsToMercator(pos);

			Vector2 ret;

			//if (CurrentMapSource is YandexMap) {
			//	ret = MercatorProjectionYandex.Instance.FromPixelToLngLat((double)m.X * 256.0, (double)m.Y * 256.0, 0);
			//} else {
				ret = GeoHelper.TileToWorldPos(m.X, m.Y, 0);
			//}

			return ret;
		}


		public override void Update(GameTime gameTime)
		{
			var rs = Game.GraphicsDevice;

			Vector3		scale, translation;
			Quaternion	rotation;

			Transform.Decompose(out scale, out rotation, out translation);

			Offset	= new Vector2(translation.X, translation.Y);
			Zoom	= scale.X * 1280;

			//rs.DebugStrings.Add("Offset :" + Offset);
			//rs.DebugStrings.Add("Zoom :" + Zoom);


			var oldProj = CurrentMapSource.Projection;

			CurrentMapSource.Update(gameTime);

			CurrentMapSource = MapSources[(int)Config.MapSource];

			if (!oldProj.Equals(CurrentMapSource.Projection)) {
				if (OnProjectionChanged != null) {
					OnProjectionChanged.Invoke(this);
				}
			}


			var ms	= Game.InputDevice.GlobalMouseOffset;

			int wheelDelta = Game.InputDevice.TotalMouseScroll - oldMouseScroll;
			if (wheelDelta > 0) {
				ZoomMap(new Vector2(ms.X, ms.Y), 1.1f);
			} else if (wheelDelta < 0) {
				ZoomMap(new Vector2(ms.X, ms.Y), 0.9f);
			}


			oldMouseScroll = Game.InputDevice.TotalMouseScroll;

			int vw = rs.DisplayBounds.Width;
			int vh = rs.DisplayBounds.Height;
			CenterPositionScreen	= new Vector2(vw / 2, vh / 2);
			CenterPositionMercator	= (CenterPositionScreen - Offset) / Zoom;

			if (CurrentMapSource is YandexMap) {
				//CenterPositionWorld = MercatorProjectionYandex.Instance.TileToWorldPos((double)CenterPositionMercator.X * 256.0, (double)CenterPositionMercator.Y * 256.0, 0);
			} else {
				CenterPositionWorld = GeoHelper.TileToWorldPos(CenterPositionMercator.X, CenterPositionMercator.Y, 0);
			}

			DragToCenterPositionWorld();


			float currentLevel = Zoom / CurrentMapSource.TileSize;
			Level = (float)Math.Log(currentLevel, 2);
			base.Update(gameTime);
		}


		void DragToCenterPositionWorld()
		{
			Vector2 newMercatorPos;
			//if (CurrentMapSource is YandexMap) {
				//newMercatorPos = MercatorProjectionYandex.Instance.FromLatLngToPixel(CenterPositionWorld.Y, CenterPositionWorld.X, 0) / 256.0f;
			//} else {
				newMercatorPos = GeoHelper.WorldToTilePos(CenterPositionWorld);
			//}

			var diff = newMercatorPos - CenterPositionMercator;
			if (diff != Vector2.Zero) {
				Vector3		scale, translation;
				Quaternion	rotation;

				Transform.TranslationVector = Transform.TranslationVector - new Vector3(diff * Zoom, 0);

				Transform.Decompose(out scale, out rotation, out translation);

				Offset	= new Vector2(translation.X, translation.Y);
				Zoom	= scale.X * 1280;
			}
		}

		void ZoomMap(Vector2 screenPoint, float zoomFactor)
		{
			Transform *= Matrix.Translation(new Vector3(-screenPoint.X, -screenPoint.Y, 0)) * Matrix.Scaling(zoomFactor) * Matrix.Translation(new Vector3(screenPoint.X, screenPoint.Y, 0));
		}


		public override void Draw(GameTime gameTime)
		{
			foreach (var ct in CurrentMapSource.RamCache) {
				ct.Value.LruIndex--;
				//if (ct.Value.LruIndex<0) ct.Value.LruIndex = 0;
			}


			//var cam = Game.GetService<Camera>();
			//var rs	= Game.GraphicsDevice;
			//var dr	= rs.DebugRender;
			//dr.DrawBasis(Matrix.Translation(10.0f, 10.0f, 10.0f), 10.0f);


			/*sb.Begin(BlendState.AlphaBlend, SamplerState.LinearClamp, RasterizerState.CullCCW, DepthStencilState.Default, cam.ProjOrthoMatrix);

			DrawTileRecursive(0, 0, 0);

			sb.End();*/

			base.Draw(gameTime);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="m"></param>
		/// <param name="n"></param>
		/// <param name="level"></param>
		void DrawTileRecursive(int m, int n, int level)
		{
			var rs = Game.GraphicsDevice;
			//
			int vw = rs.DisplayBounds.Width;
			int vh = rs.DisplayBounds.Height;


			if (level > CurrentMapSource.MaxZoom) {
				return;
			}

			int numTiles = 1 << level;

			float opacity = ((Zoom / (numTiles * CurrentMapSource.TileSize)) - 0.5f) * 4;
			if (level <= 2) opacity = 1;

			int ox = (int)Offset.X;
			int oy = (int)Offset.Y;
			int x0 = (int)((m + 0) * Zoom / numTiles) + ox;
			int y0 = (int)((n + 0) * Zoom / numTiles) + oy;
			int x1 = (int)((m + 1) * Zoom / numTiles) + ox;
			int y1 = (int)((n + 1) * Zoom / numTiles) + oy;

			if (x0 > vw) return;
			if (y0 > vh) return;
			if (x1 < 0) return;
			if (y1 < 0) return;

			if ((x1 - x0) < 2 * CurrentMapSource.TileSize / 3 && level >= 2) return;
			if ((y1 - y0) < 2 * CurrentMapSource.TileSize / 3 && level >= 2) return;

			//Vector3 tint = cfg.MapTint.ToVector3();
		//	Color color = new Color(1.0f, 1.0f, 1.0f, opacity);
			Color color = new Color(0.5f, 0.5f, 0.5f, opacity);

			if (level >= 1) {
				var rec = new Rectangle(x0, y0, x1 - x0, y1 - y0);
				sb.Draw(CurrentMapSource.GetTile(m, n, level).Tile, rec.X, rec.Y, rec.Width, rec.Height, color);
			}

			DrawTileRecursive(m * 2 + 0, n * 2 + 0, level + 1);
			DrawTileRecursive(m * 2 + 1, n * 2 + 0, level + 1);
			DrawTileRecursive(m * 2 + 1, n * 2 + 1, level + 1);
			DrawTileRecursive(m * 2 + 0, n * 2 + 1, level + 1);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void Draw2(GameTime gameTime)
		{
			var rs = Game.GraphicsDevice;
			var cam = Game.GetService<Camera>();
			//var dr = rs.DebugRender;
			//dr.DrawBasis(Matrix.Translation(1.0f, 1.0f, 1.0f), 100.0f);

			//int w = rs.Viewport.Width;
			//int h = rs.Viewport.Height;
			//dr.DrawBox(new BoundingBox(new Vector3(0,0,0), new Vector3(w, 1000, h) ), Color.Green);


			//dr.DrawPoint(new Vector3(w / 2, 0, h / 2), 2.0f, Color.Red);

			var numTiles = 1 << (int)Level;
			int x = (int) (numTiles*CenterPositionMercator.X);
			int y = (int) (numTiles*CenterPositionMercator.Y);


			int ox = (int)Offset.X;
			int oy = (int)Offset.Y;
			int x0 = (int)((x + 0) * Zoom / numTiles) + ox;
			int y0 = (int)((y + 0) * Zoom / numTiles) + oy;
			int x1 = (int)((x + 1) * Zoom / numTiles) + ox;
			int y1 = (int)((y + 1) * Zoom / numTiles) + oy;

			var rec = new Rectangle(x0, y0, x1 - x0, y1 - y0);
			float t = (float)rec.Width/CurrentMapSource.TileSize;
			var tlPos = new Vector3(x0, 0, y0);

			#warning
			/*sb.Begin(SpriteBlend.AlphaBlend, SamplerState.AnisotropicClamp, RasterizerState.CullCCW, DepthStencilState.Default, cam.GetViewMatrix(StereoEye.Mono)*cam.GetProjectionMatrix(StereoEye.Mono) );
			int tileWidth = rec.Width;

			for (int row = -TilesCountOnLowLevel; row <= TilesCountOnLowLevel; row++) {
				for (int col = -TilesCountOnLowLevel; col <= TilesCountOnLowLevel; col++) {
					
					var lT = tlPos	+ new Vector3( tileWidth*col, 0, tileWidth*row );
					var rB = lT		+ new Vector3( tileWidth, 0, tileWidth );

					sb.Draw(CurrentMapSource.GetTile(x + col, y + row, (int)Level).Tile, lT, rB, Color.White);
				}
				
			}

			sb.End();*/
		}




		struct Vertex
		{
			[Vertex("POSITION")]	public Vector4 Position;
			[Vertex("TEXCOORD", 0)]	public Vector4 TexCoord;
		}	

		[StructLayout(LayoutKind.Explicit)]
		public struct ConstData 
		{
			[FieldOffset(0)]	public	Matrix 	ViewProj	;
			[FieldOffset(64)]	public	Vector4 Zoom		;
			[FieldOffset(80)]	public	Vector4 Offset		;
			[FieldOffset(96)]	public	Vector4	TesAmount	;
			[FieldOffset(112)]	public	Vector4 Padding		;
		}

		VertexBuffer vertexBuffer;

		Ubershader				shader;
		ConstBufferGeneric<ConstData>	constBuffer;


		[Flags]
		public enum MapFlags : int
		{
			MAP_DRAW = 0x0001,
			MAP_ELEV = 0x0002,
		}

		Vertex[]	vertices;
		int			vertexCount;
		int			MaxVertices;

		public float TesselationFactor = 12.0f;

		struct Quad
		{
			public int StartIndex;
			public Texture2D SRV0;
			public Texture2D SRV1;
			public Texture2D SRV2;
			public Texture2D SRV3;
		}

		List<Quad> quads = new List<Quad>(); 

		void InitShaderAndBuffer()
		{
			var rs = Game.GraphicsDevice;

			MaxVertices = (MAX_TILES*2+1)*(MAX_TILES*2+1)*4;
			vertices	= new Vertex[MaxVertices];
			vertexCount = 0;

			vertexBuffer = new VertexBuffer(Game.GraphicsDevice, typeof(Vertex), MaxVertices);


			constBuffer = new ConstBufferGeneric<ConstData>(Game.GraphicsDevice);
			//shader		= Game.GraphicsDevice.CreateUberShader(@"Map.hlsl", typeof(MapFlags));
		}

#if false
		public void Draw3(GameTime gameTime)
		{
			var rs	= Game.GraphicsDevice;
			var cam = Game.GetService<Camera>();
			//var dr	= rs.DebugRender;
			//dr.DrawBasis(Matrix.Translation(1.0f, 1.0f, 1.0f), 100.0f);


			//int w = rs.Viewport.Width;
			//int h = rs.Viewport.Height;
			//dr.DrawBox(new BoundingBox(new Vector3(0, 0, 0), new Vector3(w, 1000, h)), Color.Green);
			//dr.DrawPoint(new Vector3(w / 2, 0, h / 2), 2.0f, Color.Red);

			var numTiles	= 1 << (int)Level;
			int x			= (int)(numTiles * CenterPositionMercator.X);
			int y			= (int)(numTiles * CenterPositionMercator.Y);


			vertexCount = 0;

			var srvs = new List<Texture2D>();
			for (int row = -TilesCountOnLowLevel; row <= TilesCountOnLowLevel; row++) {
				for (int col = -TilesCountOnLowLevel; col <= TilesCountOnLowLevel; col++) {
					if (x + col < 0 || x + col > numTiles - 1 || y + row < 0 || y + row > numTiles - 1) continue;

					AddQuad(x + col, y + row, numTiles);
					srvs.Add(CurrentMapSource.GetTile(x + col, y + row, (int)Level).Tile);
				}
			}

			vertexBuffer.SetData(vertices, 0, vertexCount);

			constBuffer.Data.ViewProj	= cam.GetViewMatrix(StereoEye.Mono) * cam.GetProjectionMatrix(StereoEye.Mono);
			constBuffer.Data.Zoom		= new Vector4(Zoom);
			constBuffer.Data.Offset		= new Vector4(Offset, 0.0f, 0.0f);
			constBuffer.Data.Padding	= new Vector4();
			constBuffer.Data.TesAmount	= new Vector4(TesselationFactor);
			constBuffer.UpdateCBuffer();


			string signature;
			shader.SetVertexShader(	(int)MapFlags.MAP_DRAW);
			shader.SetHullShader(	(int)MapFlags.MAP_DRAW	);
			shader.SetDomainShader(	(int)MapFlags.MAP_DRAW);
			shader.SetPixelShader(	(int)MapFlags.MAP_DRAW	);
			constBuffer.SetCBufferVS(0);
			constBuffer.SetCBufferHS(0);
			constBuffer.SetCBufferDS(0);
			constBuffer.SetCBufferPS(0);


			rs.SetPSSamplerState(0, SamplerState.LinearClamp);
			rs.SetDSSamplerState(0, SamplerState.LinearClamp);


			rs.SetupVertexInput(vertexBuffer, null);

			rs.SetBlendState(BlendState.Opaque);
			rs.SetDepthStencilState(DepthStencilState.Default);

			rs.SetRasterizerState(Game.InputDevice.IsKeyDown(Keys.NumPad1) ? RasterizerState.Wireframe : RasterizerState.CullCW);


			for (int i = 0; i < srvs.Count; i++) {
				srvs[i].SetPS(0);
				quads[i].SRV0.SetDS(1); quads[i].SRV1.SetDS(2); quads[i].SRV2.SetDS(3); quads[i].SRV3.SetDS(4);
				quads[i].SRV0.SetPS(1); quads[i].SRV1.SetPS(2); quads[i].SRV2.SetPS(3); quads[i].SRV3.SetPS(4);

				rs.Draw(4, 4 * i);

				
				//rs.DeviceContext.DomainShader.SetShaderResources(1, new ShaderResourceView[] { null, null, null, null });
				//rs.DeviceContext.PixelShader.SetShaderResources(1, new ShaderResourceView[] { null, null, null, null });
			}

			quads.Clear();
			srvs.Clear();

			//rs.DeviceContext.VertexShader.SetConstantBuffer(0, null);
			//rs.DeviceContext.HullShader.SetConstantBuffer(0, null);
			//rs.DeviceContext.DomainShader.SetConstantBuffer(0, null);
			//rs.DeviceContext.PixelShader.SetConstantBuffer(0, null);
			//
			//rs.DeviceContext.PixelShader.SetSampler(0, null);
			//rs.DeviceContext.DomainShader.SetSampler(0, null);
			//rs.DeviceContext.PixelShader.SetShaderResource(0, null);
			//
			//rs.DeviceContext.VertexShader.Set(null);
			//rs.DeviceContext.HullShader.Set(null);
			//rs.DeviceContext.DomainShader.Set(null);
			//rs.DeviceContext.PixelShader.Set(null);
		}
#endif

		void AddQuad(int x, int y, int numTiles)
		{
			var worldPosLT = GeoHelper.TileToWorldPos((float)x			/ numTiles, (float)y		/ numTiles, 0);
			var worldPosRT = GeoHelper.TileToWorldPos((float)(x + 1)	/ numTiles, (float)y		/ numTiles, 0);
			var worldPosRB = GeoHelper.TileToWorldPos((float)(x + 1)	/ numTiles, (float)(y + 1)	/ numTiles, 0);
			var worldPosLB = GeoHelper.TileToWorldPos((float)x			/ numTiles, (float)(y + 1)	/ numTiles, 0);

			//var el = Game.GetService<LayerService>().ElevationLayer;

			Texture2D ltSRV = null;//el.GetElevationTile((int)worldPosLT.X, (int)worldPosLT.Y);
			Texture2D rtSRV = null;//el.GetElevationTile((int)worldPosRT.X, (int)worldPosRT.Y);
			Texture2D rbSRV = null;//el.GetElevationTile((int)worldPosRB.X, (int)worldPosRB.Y);
			Texture2D lbSRV = null;// el.GetElevationTile((int)worldPosLB.X, (int)worldPosLB.Y);

			quads.Add(new Quad {
				StartIndex	= vertexCount,
				SRV0		= ltSRV,
				SRV1		= rtSRV,
				SRV2		= rbSRV,
				SRV3		= lbSRV,
			});

			//Console.WriteLine(worldPosLT + " " + worldPosRB);

			vertices[vertexCount++] = new Vertex {
				Position = new Vector4((float)x / numTiles, (float)y / numTiles, worldPosLT.X, worldPosLT.Y),
				TexCoord = new Vector4(0.0f, 0.0f, 0.0f, 0.0f),
			};
			vertices[vertexCount++] = new Vertex {
				Position = new Vector4((float)(x + 1) / numTiles, (float)y / numTiles, worldPosLT.X, worldPosLT.Y),
				TexCoord = new Vector4(1.0f, 0.0f, 1.0f, 0.0f),
			};
			vertices[vertexCount++] = new Vertex {
				Position = new Vector4((float)(x + 1) / numTiles, (float)(y + 1) / numTiles, worldPosLT.X, worldPosLT.Y),
				TexCoord = new Vector4(1.0f, 1.0f, 2.0f, 0.0f),
			};
			vertices[vertexCount++] = new Vertex {
				Position = new Vector4((float)x / numTiles, (float)(y + 1) / numTiles, worldPosLT.X, worldPosLT.Y),
				TexCoord = new Vector4(0.0f, 1.0f, 3.0f, 0.0f),
			};
		}



		public override void Dispose()
		{
			foreach (var mapSource in MapSources) {
				mapSource.Dispose();
			}

			if (BaseMapSource.EmptyTile != null) {
				BaseMapSource.EmptyTile.Dispose();
			}

			//shader.Dispose();
			constBuffer.Dispose();
			vertexBuffer.Dispose();

			base.Dispose();
		}

	}
}
