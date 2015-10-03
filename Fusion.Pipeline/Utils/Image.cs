using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Fusion.Core.Mathematics;


namespace Fusion.Pipeline.Utils {
	internal partial class Image {

		public int	Width	{ get; protected set; }
		public int	Height	{ get; protected set; }

		public Color4[]	RawImageData { get; protected set; }

		public object Tag { get; set; }
		public string Name { get; set; }
		

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		/// <param name="fillColor">Color to fill image</param>
		public Image ( int width, int height )
		{
			RawImageData	=	new Color4[width*height];

			Width	=	width;
			Height	=	height;

			if (Width<=0) {
				throw new ArgumentOutOfRangeException("Image width must be > 0");
			}

			if (Height<=0) {
				throw new ArgumentOutOfRangeException("Image height must be > 0");
			}

			for (int i=0; i<RawImageData.Length; i++) {
				RawImageData[i]	=	Color4.Black;
			}
		}



		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="width">Image width</param>
		/// <param name="height">Image height</param>
		/// <param name="fillColor">Color to fill image</param>
		public Image ( int width, int height, Color4 fillColor )
		{
			RawImageData	=	new Color4[width*height];

			Width	=	width;
			Height	=	height;

			if (Width<=0) {
				throw new ArgumentOutOfRangeException("Image width must be > 0");
			}

			if (Height<=0) {
				throw new ArgumentOutOfRangeException("Image height must be > 0");
			}

			for (int i=0; i<RawImageData.Length; i++) {
				RawImageData[i]	=	fillColor;
			}
		}



		/// <summary>
		/// Returns address of pixel with given coordinates and adressing mode
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <param name="wrap"></param>
		/// <returns></returns>
		public int Address ( int x, int y, bool wrap = true )
		{
			if (wrap) {
				x =	Wrap( x, Width );
				y =	Wrap( y, Height );
			} else {
				x	=	Clamp( x, 0, Width - 1 );
				y	=	Clamp( y, 0, Height - 1 );
			}
			return x + y * Width;
		}


		/// <summary>
		/// Samples image at given coordinates with wraping addressing mode
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <returns></returns>
		public Color4 Sample ( int x, int y, bool wrap = true)
		{
			return RawImageData[ Address( x, y, wrap ) ];
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="img"></param>
		public void Copy ( int offsetX, int offsetY, Image img )
		{
			for (int x=0; x<img.Width; x++) {
				for (int y=0; y<img.Height; y++) {
					Write( offsetX + x, offsetY + y, img.Sample( x, y ) );
				}
			}
		}



		/// <summary>
		/// Sample with filtering
		/// </summary>
		/// <param name="x">value within range 0..1</param>
		/// <param name="y">value within range 0..1</param>
		/// <param name="wrap"></param>
		/// <returns></returns>
		public Color4 Sample ( float x, float y, bool wrap = true, bool useCosineInterpolation = false )
		{
			var	tx	=	Frac( x * Width );
			var	ty	=	Frac( y * Height );
			int	x0	=	Wrap( (int)(x * Width)		, Width );
			int	x1	=	Wrap( (int)(x * Width + 1)	, Width );
			int	y0	=	Wrap( (int)(y * Height)		, Height );
			int	y1	=	Wrap( (int)(y * Height + 1) , Height );
			
			//   xy
			var v00	=	Sample( x0, y0, wrap );
			var v01	=	Sample( x0, y1, wrap );
			var v10	=	Sample( x1, y0, wrap );
			var v11	=	Sample( x1, y1, wrap );

			if (useCosineInterpolation) {
				var v0x	=	CosLerp( v00, v01, ty );
				var v1x	=	CosLerp( v10, v11, ty );
				return		CosLerp( v0x, v1x, tx );
			} else {
				var v0x	=	Color4.Lerp( v00, v01, ty );
				var v1x	=	Color4.Lerp( v10, v11, ty );
				return		Color4.Lerp( v0x, v1x, tx );
			}
		}



		/// <summary>
		/// Writes pixel to image
		/// </summary>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <param name="color"></param>
		/// <param name="wrap"></param>
		public void Write ( int u, int v, Color4 value, bool wrap = true )
		{
			RawImageData[ Address( u, v, wrap ) ] = value;
		}


		
		/// <summary>
		/// Does perpixel processing with given function
		/// </summary>
		/// <param name="procFunc"></param>
		public void PerpixelProcessing ( Func<Color4, Color4> procFunc, float amount = 1 )
		{
			for (int i=0; i<RawImageData.Length; i++) {
				RawImageData[i] = Color4.Lerp( RawImageData[i], procFunc( RawImageData[i] ), amount );
			}
		}

		
		/// <summary>
		/// Does perpixel processing with given function
		/// </summary>
		/// <param name="procFunc"></param>
		public void PerpixelProcessing ( Func<int, int, Color4, Color4> procFunc )
		{
			for (int x=0; x<Width; x++) 
			for (int y=0; y<Height; y++)  
				Write(x,y, procFunc( x, y, Sample( x,y ) ) );
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Simple Math :
		 * 
		-----------------------------------------------------------------------------------------*/

		public static int Clamp ( int x, int min, int max ) 
		{
			if (x < min) return min;
			if (x > max) return max;
			return x;
		}



		public static int Wrap ( int x, int wrapSize ) 
		{
			if ( x<0 ) {
				x = x % wrapSize + wrapSize;
			}
			return	x % wrapSize;
		}



		public static float Frac ( float x )
		{
			return x < 0 ? x%1+1 : x%1;
		}



		public static float Lerp ( float a, float b, float x ) 
		{
			return a*(1-x) + b*x;
		}



		public static float CosLerp ( float a, float b, float x)
		{
			float ft = x * 3.1415927f;
			float f  = (1 - (float)Math.Cos(ft)) * 0.5f;
			return  a*(1-f) + b*f;
  		}



		public static Color4 CosLerp ( Color4 a, Color4 b, float x )
		{
			Color4 v = new Color4(0,0,0,0);
			v.Red   = CosLerp( a.Red   , b.Red   , x );
			v.Green = CosLerp( a.Green , b.Green , x );
			v.Blue  = CosLerp( a.Blue  , b.Blue  , x );
			v.Alpha = CosLerp( a.Alpha , b.Alpha , x );
			return v;
		}


		public static float Luminance ( Color4 color ) 
		{
			return Color4.Dot( new Color4(0.3f, 0.5f, 0.2f, 0.0f), color );
		}


		public static Color4 Desaturate ( Color4 color ) 
		{
			float lum = Luminance( color );
			return new Color4( lum, lum, lum, color.Alpha );
		}


		public static float QuantizeValue ( float value, int levels )
		{
			value = MathUtil.Clamp( value, 0, 1 + float.Epsilon );
			return (float)Math.Floor( value * (levels + 1) ) / levels;
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	More Math :
		 * 
		-----------------------------------------------------------------------------------------*/

		public void Normalize ()
		{
			float max   = RawImageData.Max( v => Luminance( v ) );
			float min   = RawImageData.Min( v => Luminance( v ) );

			if ( !MathUtil.WithinEpsilon(max, 0, float.Epsilon ) ) {
				PerpixelProcessing( v => (v - min) / (max - min) );
			}
		}


		public void Saturate ( float saturation )
		{
			PerpixelProcessing( v => Color4.Lerp( Desaturate(v), v, saturation ) );
		}


		public void Quantize ( int numLevels, float amount = 1 )
		{
			//Saturate( 0 );

			PerpixelProcessing( v => { 
				var v1 = new Color4();
				v1.Red   = QuantizeValue( v.Red   , numLevels );
				v1.Green = QuantizeValue( v.Green , numLevels );
				v1.Blue  = QuantizeValue( v.Blue  , numLevels );
				v1.Alpha = QuantizeValue( v.Alpha , numLevels );
				return Color4.Lerp( v, v1, amount );
			} );
		}


		public void Gamma ( float gamma )
		{
			PerpixelProcessing( v => { 
				var v1 = new Color4();
				v1.Red    = (float)Math.Pow( v.Red   , gamma );
				v1.Green  = (float)Math.Pow( v.Green , gamma );
				v1.Blue   = (float)Math.Pow( v.Blue  , gamma );
				v1.Alpha  = (float)Math.Pow( v.Alpha , gamma );
				return v1;
			} );
		}
	}

}
