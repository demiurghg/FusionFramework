using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Fusion.Core;
using Fusion.Core.Configuration;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {

	public sealed class SpriteBatch : GameService {

		const int MaxQuads		=	1024;
		const int MaxVertices	=	MaxQuads * 4;
		const int MaxIndices	=	MaxQuads * 6;

		SpriteBatch sb { get { return this; } }

		public Texture2D	TextureWhite	{ get; private set; }
		public Texture2D	TextureBlack	{ get; private set; }
		public Texture2D	TextureRed		{ get; private set; }
		public Texture2D	TextureGreen	{ get; private set; }
		public Texture2D	TextureBlue		{ get; private set; }
		public Texture2D	TextureFont		{ get { return fontTexture; } }


		public Color ColorMultiplier = Color.White;


		public class Config {

			public Config() {
				SuppressRendering	=	false;
			}
			
			public bool SuppressRendering { get; set; }
		}



		[Config]
		public Config	Cfg { get; set; }

		public Rectangle	Clip { get; set; }
		
		VertexBuffer		vertexBuffer;
		IndexBuffer			indexBuffer;
		Ubershader			shader;
		GraphicsDevice		device;

		enum DrawFlags : int {
			NONE		=	0,
		}


		class Batch {
			public Batch ( ShaderResource texture, int start, int num ) {
				this.texture	=	texture;
				this.start		=	start;
				this.num		=	num;
			}
			public ShaderResource	texture;
			public int start;
			public int num;
		}


		int				vertexPointer	=	0;
		SpriteVertex[]	vertices		=	new	SpriteVertex[MaxVertices];
		List<Batch>		batches			=	new	List<Batch>();

		PipelineState		pipelineState;
		SamplerState		samplerState;
		DepthStencilState	depthStencilState;
		Texture2D			fontTexture;

		[StructLayout(LayoutKind.Explicit)]
		struct ConstData {
			[FieldOffset( 0)]	public Matrix	Transform;
			[FieldOffset(64)]	public Vector4	ClipRectangle;
		}

		ConstData		constData;
		ConstantBuffer	constBuffer;

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		public SpriteBatch ( GameEngine game ) : base( game )
		{
			Cfg				=	new Config();
			this.device		=	GameEngine.GraphicsDevice;
		}


		Dictionary<SpriteBlend, PipelineState>	pipelineStates = new Dictionary<SpriteBlend,PipelineState>();


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			vertexBuffer	=	new VertexBuffer( GameEngine.GraphicsDevice, typeof(SpriteVertex), MaxVertices, VertexBufferOptions.Dynamic );
			indexBuffer		=	new IndexBuffer( GameEngine.GraphicsDevice, MaxIndices );

			var indices = new int[MaxIndices];

			for (int i=0; i<MaxQuads; i++) {
				indices[ i*6 + 0 ] = i * 4 + 0;
				indices[ i*6 + 1 ] = i * 4 + 1;
				indices[ i*6 + 2 ] = i * 4 + 2;

				indices[ i*6 + 3 ] = i * 4 + 0;
				indices[ i*6 + 4 ] = i * 4 + 2;
				indices[ i*6 + 5 ] = i * 4 + 3;
			}

			indexBuffer.SetData( indices );


			batches.Capacity	=	128;

			LoadContent();

			vertices		=	new SpriteVertex[MaxVertices];
			constBuffer		=	new ConstantBuffer(GameEngine.GraphicsDevice, typeof(ConstData));

			TextureWhite	=	new Texture2D( GameEngine.GraphicsDevice, 8, 8, ColorFormat.Rgba8, false, false );
			TextureBlack	=	new Texture2D( GameEngine.GraphicsDevice, 8, 8, ColorFormat.Rgba8, false, false );
			TextureRed		=	new Texture2D( GameEngine.GraphicsDevice, 8, 8, ColorFormat.Rgba8, false, false );
			TextureGreen	=	new Texture2D( GameEngine.GraphicsDevice, 8, 8, ColorFormat.Rgba8, false, false );
			TextureBlue		=	new Texture2D( GameEngine.GraphicsDevice, 8, 8, ColorFormat.Rgba8, false, false );

			var range = Enumerable.Range(0, 8*8);
			TextureWhite	.SetData( range.Select( i => Color.White ).ToArray() ); 
			TextureBlack	.SetData( range.Select( i => Color.Black ).ToArray() );  
			TextureRed		.SetData( range.Select( i => Color.Red	 ).ToArray() );  
			TextureGreen	.SetData( range.Select( i => Color.Green ).ToArray() );  
			TextureBlue		.SetData( range.Select( i => Color.Blue  ).ToArray() );  

			GameEngine.Reloading	+= (s,e) => LoadContent();
		}



		void LoadContent ()
		{
			fontTexture		=	device.GameEngine.Content.Load<Texture2D>( @"debugFont.tga" );

 			shader			=	GameEngine.Content.Load<Ubershader>(@"spriteBatch.hlsl");

			DisposePSO();

			foreach (SpriteBlend blend in Enum.GetValues(typeof(SpriteBlend))) {

				var ps = new PipelineState( GameEngine.GraphicsDevice );

				ps.RasterizerState		=	RasterizerState.CullNone;
				ps.DepthStencilState	=	DepthStencilState.None;

				if (blend==SpriteBlend.Opaque			) ps.BlendState	=	BlendState.Opaque;
				if (blend==SpriteBlend.AlphaBlend		) ps.BlendState	=	BlendState.AlphaBlend;
				if (blend==SpriteBlend.AlphaBlendPremul	) ps.BlendState	=	BlendState.AlphaBlendPremul;
				if (blend==SpriteBlend.Additive			) ps.BlendState	=	BlendState.Additive;
				if (blend==SpriteBlend.Screen			) ps.BlendState	=	BlendState.Screen;
				if (blend==SpriteBlend.Multiply			) ps.BlendState	=	BlendState.Multiply;
				if (blend==SpriteBlend.NegMultiply		) ps.BlendState	=	BlendState.NegMultiply;
				if (blend==SpriteBlend.ClearAlpha		) ps.BlendState =	BlendState.ClearAlpha;

				ps.VertexInputElements	=	VertexInputElement.FromStructure( typeof(SpriteVertex) );

				ps.PixelShader	=	shader.GetPixelShader("");
				ps.VertexShader	=	shader.GetVertexShader("");
				ps.Primitive	=	Primitive.TriangleList;

				pipelineStates.Add( blend, ps );
								
			}

		}



		void DisposePSO()
		{
			foreach (var ps in pipelineStates) {
				ps.Value.Dispose();
			}
			pipelineStates.Clear();
		}


		/// <summary>
		/// 
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				DisposePSO();
				
				TextureWhite	.Dispose();
				TextureBlack	.Dispose();
				TextureRed		.Dispose();
				TextureGreen	.Dispose();
				TextureBlue		.Dispose();

				vertexBuffer.Dispose();
				indexBuffer.Dispose();
				shader.Dispose();
				constBuffer.Dispose();
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="spriteBlend"></param>
		public void Begin ( SpriteBlend spriteBlend = SpriteBlend.AlphaBlend, SamplerState ss = null, DepthStencilState dss = null, Matrix? transform = null, Rectangle? clip = null )
		{
			Begin( pipelineStates[ spriteBlend ], ss, dss, transform, clip );
		}



		/// <summary>
		/// 
		/// </summary>
		public void Begin ( PipelineState ps, SamplerState ss = null, DepthStencilState dss = null, Matrix? transform = null, Rectangle? clip = null )
		{
			if (ps==null) {
				throw new ArgumentNullException("ps");
			}

			pipelineState		=	ps;
			samplerState		=	ss	?? SamplerState.LinearWrap;
			depthStencilState	=	dss	?? DepthStencilState.Readonly;

			int	w	=	device.DisplayBounds.Width;
			int h	=	device.DisplayBounds.Height;
			var ofs	=	0.0f;

			Matrix sbTransform;
			if (transform.HasValue) {
				sbTransform = transform.Value;
			} else {
				sbTransform = Matrix.OrthoOffCenterRH(ofs, w + ofs, h + ofs, ofs, -9999, 9999);
			}

			Vector4 clipRect  = new Vector4(0,0,w,h);

			if ( clip.HasValue ) {
				clipRect.X = clip.Value.X;
				clipRect.Y = clip.Value.Y;
				clipRect.Z = clip.Value.Width;
				clipRect.W = clip.Value.Height;
			}

			device.PipelineState			=	pipelineState;
			device.PixelShaderSamplers[0]	=	samplerState;

			constData.Transform		=	sbTransform;
			constData.ClipRectangle	=	clipRect;
			constBuffer.SetData( constData );
		}


		public void Restart ( SpriteBlend spriteBlend = SpriteBlend.AlphaBlend, SamplerState ss = null, DepthStencilState dss = null, Matrix? transform = null, Rectangle? clip = null )
		{
			Restart( pipelineStates[ spriteBlend ], ss, dss, transform, clip );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="bs"></param>
		/// <param name="ss"></param>
		/// <param name="rs"></param>
		/// <param name="dss"></param>
		/// <param name="transform"></param>
		/// <param name="clip"></param>
		public void Restart ( PipelineState ps, SamplerState ss = null, DepthStencilState dss = null, Matrix? transform = null, Rectangle? clip = null )
		{
			End();
			Begin( ps, ss, dss, transform, clip );
		} 


		/// <summary>
		/// 
		/// </summary>
		public void End ()
		{
			FlushQueue();
		}



		/// <summary>
		/// 
		/// </summary>
		void FlushQueue ()
		{
			if (vertexPointer==0) {
				return;
			}

			if (Cfg.SuppressRendering) {	
				batches.Clear();
			}


			vertexBuffer.SetData( vertices, 0, vertexPointer );

			device.VertexShaderConstants[0]	=	constBuffer ;
			device.PixelShaderConstants[0]	=	constBuffer ;


			foreach ( var batch in batches ) {

				device.PixelShaderResources[0]	= batch.texture;

				device.SetupVertexInput( vertexBuffer, indexBuffer );

				device.DrawIndexed( batch.num, batch.start, 0 );
			}

			vertexPointer = 0;

			batches.Clear();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		void PushQuad ( ShaderResource texture, SpriteVertex v0, SpriteVertex v1, SpriteVertex v2, SpriteVertex v3 )
		{
			if (vertexPointer>MaxVertices-4) {
				FlushQueue();
			}

			/*Marshal.StructureToPtr( v0, mappedVertexBuffer.DataPointer, false );	mappedVertexBuffer.DataPointer +=	vertexSize;	   vertexCount ++;
			Marshal.StructureToPtr( v1, mappedVertexBuffer.DataPointer, false );	mappedVertexBuffer.DataPointer +=	vertexSize;	   vertexCount ++;
			Marshal.StructureToPtr( v2, mappedVertexBuffer.DataPointer, false );	mappedVertexBuffer.DataPointer +=	vertexSize;	   vertexCount ++;

			Marshal.StructureToPtr( v0, mappedVertexBuffer.DataPointer, false ); 	mappedVertexBuffer.DataPointer +=	vertexSize;	   vertexCount ++;
			Marshal.StructureToPtr( v2, mappedVertexBuffer.DataPointer, false ); 	mappedVertexBuffer.DataPointer +=	vertexSize;	   vertexCount ++;
			Marshal.StructureToPtr( v3, mappedVertexBuffer.DataPointer, false ); 	mappedVertexBuffer.DataPointer +=	vertexSize;	   vertexCount ++;*/

			int num = vertexPointer;

			vertices[ vertexPointer ] = v0;		vertexPointer++;	
			vertices[ vertexPointer ] = v1;		vertexPointer++;
			vertices[ vertexPointer ] = v2;		vertexPointer++;
			vertices[ vertexPointer ] = v3;		vertexPointer++;


			if (!batches.Any()) {
				batches.Add( new Batch( texture, num/4*6, 6 ) );
			} else {
				var last = batches.Last();

				if (last.texture==texture) {
					last.num += 6;
				} else {
					batches.Add( new Batch( texture, num/4*6, 6 ) );
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		public void DrawQuad ( ShaderResource texture, SpriteVertex v0, SpriteVertex v1, SpriteVertex v2, SpriteVertex v3 )
		{
			PushQuad( texture, v0, v1, v2, v3 );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Drawing functions :
		 * 
		-----------------------------------------------------------------------------------------*/

		public void Draw ( ShaderResource srv, float x, float y, float w, float h, Color color, float u=0, float v=0, float tw=1, float th=1 )
		{
			var c = color * ColorMultiplier;
			PushQuad( srv,
					 new SpriteVertex( x + 0, y + 0, 0, c, u + 0 ,  v + 0  ),
					 new SpriteVertex( x + w, y + 0, 0, c, u + tw,  v + 0  ),
					 new SpriteVertex( x + w, y + h, 0, c, u + tw,  v + th ),
					 new SpriteVertex( x + 0, y + h, 0, c, u + 0 ,  v + th ) );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="srv"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="angle"></param>
		/// <param name="color"></param>
        public void DrawSprite(ShaderResource srv, float x, float y, float width, float height, float angle, Color color)
		{
			float c		=	(float)Math.Cos( angle );
			float s		=	(float)Math.Sin( angle );
			float nszx	=	-width / 2;
			float nszy	=	-height / 2;
			float pszx	=	width / 2;
			float pszy	=	height / 2;



            var p0 = new Vector3( pszx * c + pszy * s + x, - pszx * s + pszy * c + y, 0 );
            var p1 = new Vector3( pszx * c + nszy * s + x, - pszx * s + nszy * c + y, 0 );
            var p2 = new Vector3( nszx * c + nszy * s + x, - nszx * s + nszy * c + y, 0 );
            var p3 = new Vector3( nszx * c + pszy * s + x, - nszx * s + pszy * c + y, 0 );
            
            PushQuad(srv,
                new SpriteVertex(p0, color, new Vector2(1, 1)),
                new SpriteVertex(p1, color, new Vector2(1, 0)),
                new SpriteVertex(p2, color, new Vector2(0, 0)),
                new SpriteVertex(p3, color, new Vector2(0, 1)));
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="srv"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="size"></param>
		/// <param name="angle"></param>
		/// <param name="color"></param>
        public void DrawSprite(ShaderResource srv, float x, float y, float size, float angle, Color color)
        {
			DrawSprite( srv, x, y, size, size, angle, color );
        }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="srv"></param>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="color"></param>
		/// <param name="width"></param>
		public void DrawBeam ( ShaderResource srv, Vector2 p0, Vector2 p1, Color color0, Color color1, float width, float scale=1, float offset=0 )
		{
			if (p1 == p0) {
				return;
			}

			var origin	=	new Vector3( p0, 0 );
			var basisX	=	new Vector3( p1 - p0, 0 );
			var basisY	=	new Vector3( -basisX.Y, basisX.X, 0 );

			basisY.Normalize();
			basisY *= (width / 2);

			float u0	=	0 * scale - offset;
			float u1	=	1 * scale - offset;

            PushQuad(srv,
                new SpriteVertex( origin - basisY 			, color0, new Vector2(u0, 0)),
                new SpriteVertex( origin - basisY + basisX	, color1, new Vector2(u1, 0)),
                new SpriteVertex( origin + basisY + basisX	, color1, new Vector2(u1, 1)),
                new SpriteVertex( origin + basisY 			, color0, new Vector2(u0, 1)) 
				);
		}



		/// <summary>
		/// 
		/// </summary>
		public void Draw( ShaderResource srv, Vector3 leftTopCorner, Vector3 rightBottomCorner, Color color, float u = 0, float v = 0, float tw = 1, float th = 1)
		{
			PushQuad(srv,
					 new SpriteVertex(leftTopCorner.X,		leftTopCorner.Y,	leftTopCorner.Z, color, u + 0, v + 0),
					 new SpriteVertex(rightBottomCorner.X,	leftTopCorner.Y,	leftTopCorner.Z, color, u + tw, v + 0),
					 new SpriteVertex(rightBottomCorner.X,	leftTopCorner.Y,	rightBottomCorner.Z, color, u + tw, v + th),
					 new SpriteVertex(leftTopCorner.X,		leftTopCorner.Y,	rightBottomCorner.Z, color, u + 0, v + th));
		}
		


		public void Draw ( ShaderResource texture, Rectangle dstRect, Rectangle srcRect, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			sb.Draw( texture, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, color, srcRect.X/w, srcRect.Y/h, srcRect.Width/w, srcRect.Height/h );
		}



		public void Draw ( ShaderResource texture, Rectangle dstRect, Rectangle srcRect, int offsetX, int offsetY, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			sb.Draw( texture, dstRect.X + offsetY, dstRect.Y + offsetY, dstRect.Width, dstRect.Height, color, srcRect.X/w, srcRect.Y/h, srcRect.Width/w, srcRect.Height/h );
		}



		public void DrawDebugString ( int x, int y, string text, Color color )
		{
			int len = text.Length;
			var duv = 1.0f / 16.0f;

			for (int i=0; i<len; i++) {
				int ch = text[i];

				var u  = (ch%16)/16.0f;
				var v  = (ch/16)/16.0f;

				sb.Draw( fontTexture, x, y, 8, 8, color, u, v, duv, duv );
				x += 8;
			}
		}



		public void Draw ( ShaderResource texture, RectangleF dstRect, RectangleF srcRect, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			sb.Draw( texture, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, color, srcRect.X/w, srcRect.Y/h, srcRect.Width/w, srcRect.Height/h );
		}



		public void Draw ( ShaderResource texture, Rectangle dstRect, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			sb.Draw( texture, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, color );
		}



		public void Draw ( ShaderResource texture, RectangleF dstRect, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			sb.Draw( texture, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, color );
		}



		/*[Obsolete("Use DrawSprite or DrawBeam")]
		public void DrawCentered ( ShaderResource texture, int centerX, int centerY, int sizeX, int sizeY, Color color )
		{
			int w = sizeX;
			int h = sizeY;
			sb.Draw( texture, centerX - w/2, centerY - h/2, w, h, color );
		}


		[Obsolete("Use DrawSprite or DrawBeam")]
		public void DrawCentered ( ShaderResource texture, float centerX, float centerY, float sizeX, float sizeY, Color color )
		{
			float w = sizeX;
			float h = sizeY;
			sb.Draw( texture, centerX - w/2, centerY - h/2, w, h, color );
		}


		[Obsolete("Use DrawSprite or DrawBeam")]
		public void DrawCentered ( ShaderResource texture, Vector2 center, float sizeX, float sizeY, Color color )
		{
			float w = sizeX;
			float h = sizeY;
			sb.Draw( texture, center.X - w/2, center.Y - h/2, w, h, color );
		} */
	}
}
