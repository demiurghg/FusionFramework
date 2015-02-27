using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace Fusion.Graphics
{
	public sealed partial class Image : DisposableBase {

		public const int AllMipMapChain = int.MaxValue;

		int width;
		int height;
		int depth;

		IntPtr data;

		public int		MipLevels	{ get; private set; }
		public int		ArraySize	{ get; private set; }
		public Format	Format		{ get; private set; }
		
		public int Width( int mipLevel = 0 )
		{
			Debug.Assert( ( mipLevel >= 0 ) && ( mipLevel < MipLevels ) );
			return ( mipLevel > 0 ) ? Math.Max( 1, width >> mipLevel ) : width;
		}



		public int Height( int mipLevel = 0 )
		{
			Debug.Assert( ( mipLevel >= 0 ) && ( mipLevel < MipLevels ) );
			return ( mipLevel > 0 ) ? Math.Max( 1, height >> mipLevel ) : height;
		}



		public int Depth( int mipLevel = 0 )
		{
			Debug.Assert( ( mipLevel >= 0 ) && ( mipLevel < MipLevels ) );
			return IsCube() ? 0 : Math.Max( 1, depth >> mipLevel );
		}
		


		public IntPtr Data( int mipLevel = 0, int arraySlice = 0 )
		{
			Debug.Assert( ( mipLevel >= 0 ) && ( mipLevel < MipLevels ) && ( arraySlice >= 0 ) && ( arraySlice < ( IsCube() ? 6 : 1 ) * ArraySize ) );

			var depth = Math.Max( 1, this.depth );

			var mipOffset	= ( mipLevel == 0 ) ? 0 : GetSizeInBytes( Format, width, height, depth, mipLevel );
			var sliceOffset	= ( arraySlice == 0 ) ? 0 : GetSizeInBytes( Format, width, height, depth, MipLevels ) * arraySlice;

			return data + mipOffset + sliceOffset;
		}



		public Image( Format fmt = Format.R8_UNorm, int width = 1, int height = 1, int depth = 1, int mipLevels = 1, int arraySize = 1 )
		{
			this.width = width;
			this.height = height;
			this.depth = depth;
			
			Format = fmt;
			MipLevels = ( mipLevels == AllMipMapChain ) ? GetMipCount( width, height, depth ) : mipLevels;
			ArraySize = arraySize;

			Debug.Assert( IsFormatValid( fmt ) );
			Debug.Assert( ( width > 0 ) && ( height > 0 ) && ( depth >= 0 ) && ( arraySize > 0 ) && ( ( arraySize == 1 ) || ( depth <= 1 ) ) );
			Debug.Assert( ( MipLevels > 0 ) && ( MipLevels <= GetMipCount( width, height, depth ) ) );
			Debug.Assert( !IsCube() || ( width == height ) );
			Debug.Assert( !IsFormatCompressed( fmt ) || ( ( ( width & 0x3 ) == 0 ) && ( ( height & 0x3 ) == 0 ) ) );
		}



		public Image( Image img )
		{
			width		= img.width;
			height		= img.height;
			depth		= img.depth;
			MipLevels	= img.MipLevels;
			ArraySize	= img.ArraySize;
			Format		= img.Format;
		}



		public int GetSliceSizeInBytes( int mipLevel = AllMipMapChain )
		{
			Debug.Assert( ( mipLevel == AllMipMapChain ) || ( ( mipLevel >= 0 ) && ( mipLevel < MipLevels ) ) );
			if( mipLevel == AllMipMapChain ) {
				return GetSizeInBytes( Format, width, height, Math.Max( 1, depth ), MipLevels );
			}
			return GetSizeInBytes( Format, Width( mipLevel ), Height( mipLevel ), Math.Max( 1, Depth( mipLevel ) ), 1 );
		}
		


		public int GetSizeInBytes( int mipLevel = AllMipMapChain )
		{
			return ( IsCube() ? 6 : 1 ) * ArraySize * GetSliceSizeInBytes( mipLevel );
		}



		public bool Is1D() 
		{
			return ( height == 1 ) && ( depth == 1 );
		}



		public bool Is2D() 
		{
			return ( height > 1 ) && ( depth == 1 );
		}



		public bool Is3D() 
		{
			return ( depth > 1 );
		}



		public bool IsCube() 
		{
			return ( depth == 0 );
		}



		public bool IsArray() 
		{
			return ( ArraySize > 1 );
		}



		public void FreeData()
		{
			if( data == IntPtr.Zero ) {
				return;
			}
			Marshal.FreeHGlobal( data );
			data = IntPtr.Zero;
		}



		public IntPtr AllocateData()
		{
			FreeData();
			data = Marshal.AllocHGlobal( GetSizeInBytes() );
			return data;
		}



		// Override the Object.Equals(object o) method:
		public override bool Equals( object obj ) 
		{
			try {
				return ( this == (Image)obj );
			}
			catch {
				return false;
			}
		}



		// Override the Object.GetHashCode() method:
		public override int GetHashCode() 
		{
			return Format.GetHashCode() ^ width.GetHashCode() ^ height.GetHashCode() ^ depth.GetHashCode() ^ MipLevels.GetHashCode() ^ ArraySize.GetHashCode();
		}



		public bool Load( string file )
		{
			if( !File.Exists( file ) ) {
				return false;
			}

			if( Path.GetExtension( file ).ToLower() == ".dds" ) {
				return LoadDDS( File.ReadAllBytes( file ) );
			}

            var imgfactory = new ImagingFactory();
			var decoder = new BitmapDecoder( imgfactory, file, DecodeOptions.CacheOnDemand );

			BitmapSource bitmap = decoder.GetFrame(0);

			var dstPF = bitmap.PixelFormat;

			if( dstPF == PixelFormat.Format24bppBGR ) {
				dstPF = PixelFormat.Format32bppRGBA;
			} else if ( dstPF == PixelFormat.Format32bppBGRA ) {
				dstPF = PixelFormat.Format32bppRGBA;
			} else if ( dstPF == PixelFormat.Format48bppRGB ) {
				dstPF = PixelFormat.Format64bppRGBA;
			}

			if( dstPF != bitmap.PixelFormat ) {
				var formatConverter = new FormatConverter( imgfactory );
				formatConverter.Initialize( bitmap, dstPF, BitmapDitherType.None, null, 0.0, BitmapPaletteType.Custom );
				bitmap = formatConverter;
			}

			var fmt = Format.Unknown;

			if( dstPF == PixelFormat.Format8bppGray ) {
				fmt = Format.R8_UNorm;
			} else if( dstPF == PixelFormat.Format32bppRGBA ) {
				fmt = Format.R8G8B8A8_UNorm;
			} else if( dstPF == PixelFormat.Format16bppGray ) {
				fmt = Format.R16_UNorm;
			} else if( dstPF == PixelFormat.Format64bppRGBA ) {
				fmt = Format.R16G16B16A16_UNorm;
			}

			if( fmt == Format.Unknown ) {
				return false;
			}

			var sizeOfNewData = GetSizeInBytes( fmt, bitmap.Size.Width, bitmap.Size.Height, 1, 1 );
			var newData = Marshal.AllocHGlobal( sizeOfNewData );

			try {
				bitmap.CopyPixels( bitmap.Size.Width * ( (int)FormatHelper.SizeOfInBits( fmt ) >> 3 ), newData, sizeOfNewData );
			}
			catch {
				Marshal.FreeHGlobal( newData );
				newData = IntPtr.Zero;
			}

			if( newData == IntPtr.Zero ) {
				return false;
			}

			FreeData();

			Format = fmt;
			width = bitmap.Size.Width;
			height = bitmap.Size.Height;
			depth = 1;
			MipLevels = 1;
			ArraySize = 1;
			data = newData;

			return true;
		}



		protected override void Dispose( bool disposing )
		{
			FreeData();
			base.Dispose( disposing );
		}
	}
}
