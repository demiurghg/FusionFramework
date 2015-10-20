using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Drivers.Graphics;
using SharpDX;
using Fusion.Core;
using Fusion.Core.Configuration;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {
	#if true
	public class DebugRender : GameService
	{
		struct LineVertex {
			[Vertex("POSITION")] public Vector3 Pos;
			[Vertex("COLOR", 0)] public Vector4 Color;
		}

		[Flags]
		public enum RenderFlags : int {
			NONE = 0x0000,
		}

		[StructLayout(LayoutKind.Explicit)]
		struct ConstData {
			[FieldOffset(0)] public Matrix Transform;
		}

		VertexBuffer		vertexBuffer;
		Ubershader			effect;
		StateFactory		factory;
		ConstantBuffer		constBuffer;

		List<LineVertex>	vertexDataAccum	= new List<LineVertex>();
		List<LineVertex>	vertexDataDraw	= new List<LineVertex>();
		LineVertex[]		vertexArray = new LineVertex[vertexBufferSize];

		const int vertexBufferSize = 4096;

		/// <summary>
		/// View matrix
		/// </summary>
		public Matrix	View		{ get; set; }

		/// <summary>
		/// Projection matrix
		/// </summary>
		public Matrix	Projection	{ get; set; }


		[Config]
		public DebugRenderConfig	Config { get; set; }

		ConstData	constData;


		public RenderTargetSurface RenderTarget { get; set; }
		public DepthStencilSurface DepthStencil { get; set; }


		/// <summary>
		/// Constructor
		/// </summary>
		public DebugRender(GameEngine game) : base(game)
		{
			RenderTarget	= null;
			Config	=	new DebugRenderConfig();
		}



		/// <summary>
		/// Initialization
		/// </summary>
		public override void Initialize ()
		{
			base.Initialize();

			var dev		= GameEngine.GraphicsDevice;

			effect		= GameEngine.Content.Load<Ubershader>("debugRender.hlsl");
			factory		= new StateFactory( effect, typeof(RenderFlags), Primitive.LineList, VertexInputElement.FromStructure( typeof(LineVertex) ), BlendState.AlphaBlend, RasterizerState.CullNone );

			constData	= new ConstData();
			constBuffer = new ConstantBuffer(dev, typeof(ConstData));


			//	create vertex buffer :
			vertexBuffer		= new VertexBuffer(dev, typeof(LineVertex), vertexBufferSize, VertexBufferOptions.Dynamic );
			vertexDataAccum.Capacity = vertexBufferSize;
		}



		/// <summary>
		/// Dispose
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				factory.Dispose();
				vertexBuffer.Dispose();
				constBuffer.Dispose();
				effect.Dispose();
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Draws line between p0 and p1
		/// </summary>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="color"></param>
		public void DrawLine(Vector3 p0, Vector3 p1, Color color)
		{
			vertexDataAccum.Add(new LineVertex() { Pos = p0, Color = color.ToVector4() });
			vertexDataAccum.Add(new LineVertex() { Pos = p1, Color = color.ToVector4() });
			//DrawLine( p0, p1, color, Matrix.Identity );
		}

		/// <summary>
		/// Draws line between p0 and p1
		/// </summary>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="color"></param>
		public void DrawLine(Vector2 p0, Vector2 p1, Color color)
		{
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector3(p0, 0), Color = color.ToVector4() });
			vertexDataAccum.Add(new LineVertex() { Pos = new Vector3(p1, 0), Color = color.ToVector4() });
			//DrawLine( p0, p1, color, Matrix.Identity );
		}



		/// <summary>
		/// 
		/// </summary>
		/// 
		public override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			if (!vertexDataAccum.Any()) {
				return;
			}

			if (Config.SuppressDebugRender) {
				vertexDataAccum.Clear();
			}

			var dev = GameEngine.GraphicsDevice;


			if (RenderTarget!=null) {
				if (DepthStencil==null) {	
					throw new InvalidOperationException("Both RenderTarget and DepthStencil must be set.");
				}
				dev.SetTargets( DepthStencil, RenderTarget );
			}


			constData.Transform = View * Projection;
			constBuffer.SetData(constData);

			dev.SetupVertexInput( vertexBuffer, null );
			dev.VertexShaderConstants[0]	=	constBuffer ;
			dev.PipelineState				=	factory[0];


			int numDPs = MathUtil.IntDivUp(vertexDataAccum.Count, vertexBufferSize);

			for (int i = 0; i < numDPs; i++) {

				int numVerts = i < numDPs - 1 ? vertexBufferSize : vertexDataAccum.Count % vertexBufferSize;

				if (numVerts == 0) {
					break;
				}

				vertexDataAccum.CopyTo(i * vertexBufferSize, vertexArray, 0, numVerts);

				vertexBuffer.SetData(vertexArray, 0, numVerts);

				dev.Draw( numVerts, 0);

			}

			vertexDataAccum.Clear();
		}


		/*-----------------------------------------------------------------------------------------
		 *	Primitives :
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wireCount"></param>
		public void DrawGrid(int wireCount)
		{
			int gridsz = wireCount;
			for (int x = -gridsz; x <= gridsz; x += 1)
			{
				float dim = 0.7f;
				if (x == 0) dim = 1.0f;
				DrawLine(new Vector3(x, 0, gridsz), new Vector3(x, 0, -gridsz), Color.DarkGray * dim);
				DrawLine(new Vector3(gridsz, 0, x), new Vector3(-gridsz, 0, x), Color.DarkGray * dim);
			}
		}


		public void DrawBasis(Matrix basis, float scale)
		{
			Vector3 pos = Vector3.TransformCoordinate(Vector3.Zero, basis);
			Vector3 xaxis = Vector3.TransformNormal(Vector3.UnitX * scale, basis);
			Vector3 yaxis = Vector3.TransformNormal(Vector3.UnitY * scale, basis);
			Vector3 zaxis = Vector3.TransformNormal(Vector3.UnitZ * scale, basis);
			DrawLine(pos, pos + xaxis, Color.Red);
			DrawLine(pos, pos + yaxis, Color.Lime);
			DrawLine(pos, pos + zaxis, Color.Blue);
		}


		public void DrawVector(Vector3 origin, Vector3 dir, Color color, float scale = 1.0f)
		{
			DrawLine(origin, origin + dir * scale, color/*, Matrix.Identity*/ );
		}


		public void DrawPoint(Vector3 p, float size, Color color)
		{
			float h = size / 2;	// half size
			DrawLine(p + Vector3.UnitX * h, p - Vector3.UnitX * h, color);
			DrawLine(p + Vector3.UnitY * h, p - Vector3.UnitY * h, color);
			DrawLine(p + Vector3.UnitZ * h, p - Vector3.UnitZ * h, color);
		}


		public void DrawWaypoint(Vector3 p, float size, Color color)
		{
			float h = size / 2;	// half size
			DrawLine(p + Vector3.UnitX * h, p - Vector3.UnitX * h, color);
			DrawLine(p + Vector3.UnitZ * h, p - Vector3.UnitZ * h, color);
		}


		public void DrawRing(Vector3 origin, float radius, Color color, int numSegments = 32, float angle = 0)
		{
			int N = numSegments;
			Vector3[] points = new Vector3[N + 1];

			for (int i = 0; i <= N; i++)
			{
				points[i].X = origin.X + radius * (float)Math.Cos(Math.PI * 2 * i / N + angle);
				points[i].Y = origin.Y;
				points[i].Z = origin.Z + radius * (float)Math.Sin(Math.PI * 2 * i / N + angle);
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color);
			}
		}


		public void DrawSphere(Vector3 origin, float radius, Color color, int numSegments = 32)
		{
			int N = numSegments;
			Vector3[] points = new Vector3[N + 1];

			for (int i = 0; i <= N; i++)
			{
				points[i].X = origin.X + radius * (float)Math.Cos(Math.PI * 2 * i / N);
				points[i].Y = origin.Y;
				points[i].Z = origin.Z + radius * (float)Math.Sin(Math.PI * 2 * i / N);
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color);
			}

			for (int i = 0; i <= N; i++)
			{
				points[i].X = origin.X + radius * (float)Math.Cos(Math.PI * 2 * i / N);
				points[i].Y = origin.Y + radius * (float)Math.Sin(Math.PI * 2 * i / N);
				points[i].Z = origin.Z;
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color);
			}

			for (int i = 0; i <= N; i++)
			{
				points[i].X = origin.X;
				points[i].Y = origin.Y + radius * (float)Math.Cos(Math.PI * 2 * i / N);
				points[i].Z = origin.Z + radius * (float)Math.Sin(Math.PI * 2 * i / N);
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color);
			}
		}


		public void DrawRing(Matrix basis, float radius, Color color, float stretch = 1, int numSegments = 32)
		{
			int N = numSegments;
			Vector3[] points = new Vector3[N + 1];
			Vector3 origin = basis.TranslationVector;

			for (int i = 0; i <= N; i++)
			{
				points[i] = origin + radius * basis.Forward * (float)Math.Cos(Math.PI * 2 * i / N) * stretch
									+ radius * basis.Left * (float)Math.Sin(Math.PI * 2 * i / N);
			}

			for (int i = 0; i < N; i++)
			{
				DrawLine(points[i], points[i + 1], color);
			}
		}


		public void DrawFrustum ( BoundingFrustum frustum, Color color )
		{
			var points = frustum.GetCorners();

			DrawLine( points[0], points[1], color );
			DrawLine( points[1], points[2], color );
			DrawLine( points[2], points[3], color );
			DrawLine( points[3], points[0], color );

			DrawLine( points[4], points[5], color );
			DrawLine( points[5], points[6], color );
			DrawLine( points[6], points[7], color );
			DrawLine( points[7], points[4], color );

			DrawLine( points[0], points[4], color );
			DrawLine( points[1], points[5], color );
			DrawLine( points[2], points[6], color );
			DrawLine( points[3], points[7], color );
		}


		public void DrawBox(BoundingBox bbox, Color color)
		{
			var crnrs = bbox.GetCorners();

			var p = bbox.Maximum;
			var n = bbox.Minimum;

			DrawLine(new Vector3(p.X, p.Y, p.Z), new Vector3(n.X, p.Y, p.Z), color);
			DrawLine(new Vector3(n.X, p.Y, p.Z), new Vector3(n.X, p.Y, n.Z), color);
			DrawLine(new Vector3(n.X, p.Y, n.Z), new Vector3(p.X, p.Y, n.Z), color);
			DrawLine(new Vector3(p.X, p.Y, n.Z), new Vector3(p.X, p.Y, p.Z), color);

			DrawLine(new Vector3(p.X, n.Y, p.Z), new Vector3(n.X, n.Y, p.Z), color);
			DrawLine(new Vector3(n.X, n.Y, p.Z), new Vector3(n.X, n.Y, n.Z), color);
			DrawLine(new Vector3(n.X, n.Y, n.Z), new Vector3(p.X, n.Y, n.Z), color);
			DrawLine(new Vector3(p.X, n.Y, n.Z), new Vector3(p.X, n.Y, p.Z), color);

			DrawLine(new Vector3(p.X, p.Y, p.Z), new Vector3(p.X, n.Y, p.Z), color);
			DrawLine(new Vector3(n.X, p.Y, p.Z), new Vector3(n.X, n.Y, p.Z), color);
			DrawLine(new Vector3(n.X, p.Y, n.Z), new Vector3(n.X, n.Y, n.Z), color);
			DrawLine(new Vector3(p.X, p.Y, n.Z), new Vector3(p.X, n.Y, n.Z), color);
		}


		public void DrawBox(BoundingBox bbox, Matrix transform, Color color)
		{
			var crnrs = bbox.GetCorners();

			//Vector3.TransformCoordinate( crnrs, ref transform, crnrs );

			var p = bbox.Maximum;
			var n = bbox.Minimum;

			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, p.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, p.Y, p.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, p.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, p.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, p.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, p.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, p.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, p.Y, p.Z), transform), color);

			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, n.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, n.Y, p.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, n.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, n.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, n.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, n.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, n.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, n.Y, p.Z), transform), color);

			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, p.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, n.Y, p.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, p.Y, p.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, n.Y, p.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(n.X, p.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(n.X, n.Y, n.Z), transform), color);
			DrawLine(Vector3.TransformCoordinate(new Vector3(p.X, p.Y, n.Z), transform), Vector3.TransformCoordinate(new Vector3(p.X, n.Y, n.Z), transform), color);
		}
	}
	#endif
}
