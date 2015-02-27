using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;

namespace Fusion.Graphics
{
	public sealed partial class Image {

		static readonly bool[] floatFormats			= new bool[256]; 
		static readonly bool[] sIntFormats			= new bool[256]; 
		static readonly bool[] uIntFormats			= new bool[256]; 
		static readonly bool[] depthFormats			= new bool[256]; 
		static readonly bool[] packedFormats		= new bool[256]; 
		static readonly bool[] normalizedFormats	= new bool[256]; 
		static readonly bool[] renderableFormats	= new bool[256]; 
		
		public static bool operator != ( Image img1, Image img2 )
		{
			return !( img1 == img2 );
		}



		public static bool operator == ( Image img1, Image img2 )
		{
			// If both are null, or both are same instance, return true.
			if( object.ReferenceEquals( img1, img2 ) ) {
				return true;
			}

			// If one is null, but not both, return false.
			if( ( (object)img1 == null ) || ( (object)img2 == null ) ) {
				return false;
			}

			return 
			( img1.Format		== img2.Format ) && 
			( img1.width		== img2.width ) && 
			( img1.height		== img2.height ) && 
			( img1.depth		== img2.depth ) &&
			( img1.MipLevels	== img2.MipLevels ) &&
			( img1.ArraySize	== img2.ArraySize );
		}



		public static bool IsFormatSRGB( Format fmt )
		{
			return FormatHelper.IsSRgb( fmt );
		}



		public static bool IsFormatCompressed( Format fmt )
		{
			Debug.Assert( !FormatHelper.IsTypeless( fmt ) );
			return FormatHelper.IsCompressed( fmt );
		}



		public static bool IsFormatFloat( Format fmt )
        {
            return floatFormats[(int)fmt];
        } 



		public static bool IsFormatSInt( Format fmt )
        {
            return sIntFormats[(int)fmt];
        } 



		public static bool IsFormatUInt( Format fmt )
        {
            return uIntFormats[(int)fmt];
        } 



		public static bool IsFormatDepth( Format fmt )
        {
            return depthFormats[(int)fmt];
        } 



		public static bool IsFormatPacked( Format fmt )
        {
            return packedFormats[(int)fmt];
        } 



		public static bool IsFormatNormalized( Format fmt )
        {
            return normalizedFormats[(int)fmt];
        } 



		public static bool IsFormatRenderable( Format fmt )
        {
            return renderableFormats[(int)fmt];
        } 



		public static bool IsFormatValid( Format fmt )
		{
			return IsFormatFloat(fmt) || IsFormatSInt(fmt) || IsFormatUInt(fmt) || IsFormatNormalized(fmt) || IsFormatPacked(fmt) || IsFormatCompressed(fmt) || IsFormatDepth(fmt);
		}



		public static int GetMipCount( int width, int height = 1, int depth = 1 ) 
		{
			Debug.Assert( ( width > 0 ) || ( height > 0 ) );
			int mipCount = 0;
			for( var maxDimension = Math.Max( Math.Max( width, height ), depth ); ( maxDimension > 0 ); maxDimension >>= 1, ++mipCount );
			return mipCount;
		}



		public static int GetPixelCount( int width, int height, int depth, int mipCount ) 
		{
			Debug.Assert( ( width > 0 ) && ( height > 0 ) && ( mipCount > 0 ) && ( mipCount <= GetMipCount( width, height, depth ) ) );

			bool bCube = ( depth == 0 );

			int pixelCount = 0;
			for( var i = 0; i < mipCount; ++i, width >>= 1, height >>= 1, depth >>= 1 ) {
				width	= Math.Max( 1, width );
				height	= Math.Max( 1, height );
				depth	= Math.Max( 1, depth );

				pixelCount += width * height * depth;
			}
			return bCube ? 6 * pixelCount : pixelCount;
		}



		public static int GetBlockCount( int width, int height, int depth, int mipCount ) {
			Debug.Assert( ( width > 0 ) && ( height > 0 ) && ( mipCount > 0 ) && ( mipCount <= GetMipCount( width, height, depth ) ) );

			bool bCube = ( depth == 0 );

			int blockCount = 0;
			for( var i = 0; i < mipCount; ++i, width >>= 1, height >>= 1, depth >>= 1 ) {
				width	= Math.Max( 1, width );
				height	= Math.Max( 1, height );
				depth	= Math.Max( 1, depth );

				blockCount += ( ( width + 3 ) >> 2 ) * ( ( height + 3 ) >> 2 ) * depth;
			}
			return bCube ? 6 * blockCount : blockCount;
		}



		public static int GetElementSizeInBytes( Format fmt ) 
		{
			return  IsFormatCompressed( fmt ) ? ( (int)FormatHelper.SizeOfInBits( fmt ) << 1 ) : ( (int)FormatHelper.SizeOfInBits( fmt ) >> 3 );
		}



		public static int GetElementCount( Format fmt, int width, int height, int depth, int mipCount ) 
		{
			return IsFormatCompressed( fmt ) ? GetBlockCount( width, height, depth, mipCount ) : GetPixelCount( width, height, depth, mipCount );
		}



		public static int GetSizeInBytes( Format fmt, int width, int height, int depth, int mipCount ) {
			return GetElementCount( fmt, width, height, depth, mipCount ) * GetElementSizeInBytes( fmt );
		}



		private static void InitDefaults( IEnumerable<Format> formats, bool[] outputArray )
        {
            foreach( var fmt in formats ) {
                outputArray[(int)fmt] = true;
			}
        } 

		static Image()
		{
			
			InitDefaults( new[]
			{ 
				Format.R8_SInt, 
				Format.R8_UInt, 
				Format.R8_UNorm,
				Format.R16_Float,
				Format.R16_SInt,
				Format.R16_UInt,
				Format.R16_UNorm,
				Format.R8G8_SInt,
				Format.R8G8_UInt,
				Format.R8G8_UNorm,
				Format.R10G10B10A2_UInt,
				Format.R10G10B10A2_UNorm,
				Format.R11G11B10_Float,
				Format.R16G16_Float,
				Format.R16G16_SInt,
				Format.R16G16_UInt,
				Format.R16G16_UNorm,
				Format.R32_Float,
				Format.R32_SInt,
				Format.R32_UInt,
				Format.R8G8B8A8_SInt,
				Format.R8G8B8A8_UInt,
				Format.R8G8B8A8_UNorm,
				Format.R8G8B8A8_UNorm_SRgb,
				Format.R16G16B16A16_Float,
				Format.R16G16B16A16_SInt,
				Format.R16G16B16A16_UInt,
				Format.R16G16B16A16_UNorm,
				Format.R32G32_Float,
				Format.R32G32_SInt,
				Format.R32G32_UInt,
				Format.R32G32B32_Float,
				Format.R32G32B32_SInt,
				Format.R32G32B32_UInt,
				Format.R32G32B32A32_Float,
				Format.R32G32B32A32_SInt,
				Format.R32G32B32A32_UInt,
			}, renderableFormats ); 
 
			InitDefaults( new[]
			{ 
				Format.R16_Float,
				Format.R16G16_Float,
				Format.R32_Float,
				Format.R16G16B16A16_Float,
				Format.R32G32_Float,
				Format.R32G32B32_Float,
				Format.R32G32B32A32_Float,
			}, floatFormats ); 

			InitDefaults( new[]
			{ 
				Format.R8_SInt, 
				Format.R16_SInt,
				Format.R8G8_SInt,
				Format.R16G16_SInt,
				Format.R32_SInt,
				Format.R8G8B8A8_SInt,
				Format.R16G16B16A16_SInt,
				Format.R32G32_SInt,
				Format.R32G32B32_SInt,
				Format.R32G32B32A32_SInt,
			}, sIntFormats ); 

			InitDefaults( new[]
			{ 
				Format.R8_UInt, 
				Format.R16_UInt,
				Format.R8G8_UInt,
				Format.R16G16_UInt,
				Format.R32_UInt,
				Format.R8G8B8A8_UInt,
				Format.R16G16B16A16_UInt,
				Format.R32G32_UInt,
				Format.R32G32B32_UInt,
				Format.R32G32B32A32_UInt,
			}, uIntFormats ); 
			
			InitDefaults( new[]
			{ 
				Format.D16_UNorm,
				Format.D24_UNorm_S8_UInt,
				Format.D32_Float,
				Format.D32_Float_S8X24_UInt,
			}, depthFormats ); 
			
			InitDefaults( new[]
			{ 
				Format.R10G10B10A2_UInt,
				Format.R10G10B10A2_UNorm,
				Format.R11G11B10_Float,
				Format.R9G9B9E5_Sharedexp,
			}, packedFormats ); 

			InitDefaults( new[]
			{ 
				Format.R8_SNorm, 
				Format.R8_UNorm,
				Format.R16_SNorm,
				Format.R16_UNorm,
				Format.R8G8_SNorm,
				Format.R8G8_UNorm,
				Format.R16G16_SNorm,
				Format.R16G16_UNorm,
				Format.R8G8B8A8_SNorm,
				Format.R8G8B8A8_UNorm,
				Format.R8G8B8A8_UNorm_SRgb,
				Format.B8G8R8A8_UNorm,
				Format.B8G8R8A8_UNorm_SRgb,
				Format.R16G16B16A16_SNorm,
				Format.R16G16B16A16_UNorm,
			}, normalizedFormats ); 
		}
	}
}
