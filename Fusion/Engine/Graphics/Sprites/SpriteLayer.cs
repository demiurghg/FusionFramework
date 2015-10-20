using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {
	public class SpriteLayer : DisposableBase {

		readonly GraphicsEngine ge;

		/// <summary>
		/// Is current layer is visible. Default: True
		/// </summary>
		public bool Visible { get; set; }
		
		/// <summary>
		/// Layer's transform. Default: Indentity.
		/// </summary>
		public Matrix Transform { get; set; }

		/// <summary>
		/// Layer's master color. Default: White.
		/// Color of each sprite will be multiplied by this value.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// Layer blend mode. Default: AlphaBlend
		/// </summary>
		public SpriteBlendMode	BlendMode { get; set; }

		/// <summary>
		/// Layer filter mode. Default: LinearClamp
		/// </summary>
		public SpriteFilterMode	FilterMode { get; set; }

		/// <summary>
		/// Sprite stereo mode.	Default: All
		/// </summary>
		public SpriteStereoMode StereoMode { get; set; }

		/// <summary>
		/// Draw order
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		/// Sprite layer name. For debugging purposes only.
		/// </summary>
		public string Name { get; set; }


		/// <summary>
		/// Layer's child layers.
		/// </summary>
		public ICollection<SpriteLayer> Layers { get { return layers; } }
		List<SpriteLayer> layers = new List<SpriteLayer>();


		VertexBuffer	vertexBuffer;
		int				capacity	=	0;

		const int		VertexPerSprite = 6;


		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="ge"></param>
		/// <param name="capacity">Number of sprites</param>
		public SpriteLayer ( GraphicsEngine ge, int capacity )
		{
			this.ge		=	ge;

			Order		=	0;

			Visible		=	true;
			Transform	=	Matrix.Identity;
			Color		=	Color.White;
			BlendMode	=	SpriteBlendMode.AlphaBlend;
			FilterMode	=	SpriteFilterMode.LinearClamp;
			StereoMode	=	SpriteStereoMode.All;

			ReallocGpuBuffers( capacity );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				SafeDispose( ref vertexBuffer );
			}
 			base.Dispose(disposing);
		}
		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="capacity"></param>
		void ReallocGpuBuffers ( int newCapacity )
		{
			if (capacity>=newCapacity) {
				return;
			}

			this.capacity	=	newCapacity;

			vertices.Capacity	=	capacity * VertexPerSprite;

			SafeDispose( ref vertexBuffer );

			vertexBuffer	=	new VertexBuffer( ge.Device, typeof(SpriteVertex), capacity * VertexPerSprite );
		}



		/// <summary>
		/// 
		/// </summary>
		internal void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			if (spriteCount==0) {
				return;
			}

			if (dirty) {

				//	realloc buffers in necessary
				if ( vertices.Count > capacity * VertexPerSprite ) {

					var newCapacity	=	MathUtil.IntDivRoundUp( vertices.Count / VertexPerSprite, 64 ) * 64;
					
					ReallocGpuBuffers( newCapacity );
				}

				vertexBuffer.SetData( vertices.ToArray() );

				dirty	=	false;
			}


			ge.Device.SetupVertexInput( vertexBuffer, null );


			foreach ( var group in groups ) {
				ge.Device.PixelShaderResources[0] = group.Texture.Srv;
				ge.Device.Draw( group.SpriteCount * VertexPerSprite, group.StartSprite * VertexPerSprite );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="pivot"></param>
		/// <param name="angle"></param>
		public void SetTransform ( Vector2 offset, Vector2 pivot, float angle )
		{
			Transform	=	Matrix.Translation( -pivot.X, -pivot.Y, 0 )
						*	Matrix.RotationZ( angle )
						*	Matrix.Translation( pivot.X, pivot.Y, 0 )
						*	Matrix.Translation( offset.X, offset.Y, 0 )
						;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="pivot"></param>
		/// <param name="angle"></param>
		public void SetTransform ( float x, float y )
		{
			Transform	=	Matrix.Translation( x, y, 0 );
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Drawing functions :
		 * 
		-----------------------------------------------------------------------------------------*/

		class Group {
			public Texture	Texture;
			public int		StartSprite;
			public int		SpriteCount;
		}

		List<SpriteVertex>	vertices	=	new List<SpriteVertex>();
		List<Group>			groups		=	new List<Group>();
		int					spriteCount	=	0;

		bool dirty	=	true;


		/// <summary>
		/// Clears all sprites.
		/// </summary>
		public void Clear ()
		{
			dirty		=	true;
			spriteCount	=	0;
			vertices.Clear();
			groups.Clear();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		void PushQuad ( Texture texture, SpriteVertex v0, SpriteVertex v1, SpriteVertex v2, SpriteVertex v3 )
		{
			if ( groups.Count == 0 || groups[groups.Count-1].Texture != texture ) {
				groups.Add( new Group { Texture = texture, StartSprite = spriteCount } );
			}

			groups[groups.Count-1].SpriteCount++;
			spriteCount++;

			vertices.Add( v0 );
			vertices.Add( v1 );
			vertices.Add( v2 );
			
			vertices.Add( v0 );
			vertices.Add( v2 );
			vertices.Add( v3 );

			dirty	=	true;
		}
			


		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="v3"></param>
		internal void DrawQuad ( Texture texture, SpriteVertex v0, SpriteVertex v1, SpriteVertex v2, SpriteVertex v3 )
		{
			PushQuad( texture, v0, v1, v2, v3 );
		}


		public void Draw ( Texture srv, float x, float y, float w, float h, Color color, float u=0, float v=0, float tw=1, float th=1 )
		{
			var c = color;
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
        public void DrawSprite(Texture srv, float x, float y, float width, float height, float angle, Color color)
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
        public void DrawSprite( Texture srv, float x, float y, float size, float angle, Color color )
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
		public void DrawBeam ( Texture srv, Vector2 p0, Vector2 p1, Color color0, Color color1, float width, float scale=1, float offset=0 )
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
		public void Draw( Texture srv, Vector3 leftTopCorner, Vector3 rightBottomCorner, Color color, float u = 0, float v = 0, float tw = 1, float th = 1)
		{
			PushQuad(srv,
					 new SpriteVertex(leftTopCorner.X,		leftTopCorner.Y,	leftTopCorner.Z, color, u + 0, v + 0),
					 new SpriteVertex(rightBottomCorner.X,	leftTopCorner.Y,	leftTopCorner.Z, color, u + tw, v + 0),
					 new SpriteVertex(rightBottomCorner.X,	leftTopCorner.Y,	rightBottomCorner.Z, color, u + tw, v + th),
					 new SpriteVertex(leftTopCorner.X,		leftTopCorner.Y,	rightBottomCorner.Z, color, u + 0, v + th));
		}
		


		public void Draw ( Texture texture, Rectangle dstRect, Rectangle srcRect, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			Draw( texture, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, color, srcRect.X/w, srcRect.Y/h, srcRect.Width/w, srcRect.Height/h );
		}



		public void Draw ( Texture texture, Rectangle dstRect, Rectangle srcRect, int offsetX, int offsetY, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			Draw( texture, dstRect.X + offsetY, dstRect.Y + offsetY, dstRect.Width, dstRect.Height, color, srcRect.X/w, srcRect.Y/h, srcRect.Width/w, srcRect.Height/h );
		}



		public void DrawDebugString ( Texture fontTexture, int x, int y, string text, Color color )
		{
			int len = text.Length;
			var duv = 1.0f / 16.0f;

			for (int i=0; i<len; i++) {
				int ch = text[i];

				var u  = (ch%16)/16.0f;
				var v  = (ch/16)/16.0f;

				Draw( fontTexture, x, y, 8, 8, color, u, v, duv, duv );
				x += 8;
			}
		}



		public void Draw ( Texture texture, RectangleF dstRect, RectangleF srcRect, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			Draw( texture, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, color, srcRect.X/w, srcRect.Y/h, srcRect.Width/w, srcRect.Height/h );
		}



		public void Draw ( Texture texture, Rectangle dstRect, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			Draw( texture, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, color );
		}



		public void Draw ( Texture texture, RectangleF dstRect, Color color )
		{
			float w = texture.Width;
			float h = texture.Height;

			Draw( texture, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height, color );
		}

	}
}
