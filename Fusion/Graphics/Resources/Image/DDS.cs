using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX;
using System.IO;

namespace Fusion.Graphics
{
	public sealed partial class Image {

		const uint DDSD_CAPS		= 0x00000001;
		const uint DDSD_HEIGHT		= 0x00000002;
		const uint DDSD_WIDTH		= 0x00000004;
		const uint DDSD_PIXELFORMAT	= 0x00001000;
		const uint DDSD_MipLevels	= 0x00020000;
		const uint DDSD_DEPTH		= 0x00800000;
		const uint DDPF_FOURCC		= 0x00000004;

		const uint DDSCAPS_MIPMAP	= 0x00400000;
		const uint DDSCAPS_COMPLEX	= 0x00000008; 
		const uint DDSCAPS_TEXTURE	= 0x00001000; 

		const uint DDSCAPS2_VOLUME				= 0x00200000; 
		const uint DDSCAPS2_CUBEMAP				= 0x00000200; 
		const uint DDSCAPS2_CUBEMAP_POSITIVEX	= 0x00000400;
		const uint DDSCAPS2_CUBEMAP_NEGATIVEX	= 0x00000800;
		const uint DDSCAPS2_CUBEMAP_POSITIVEY	= 0x00001000;
		const uint DDSCAPS2_CUBEMAP_NEGATIVEY	= 0x00002000;
		const uint DDSCAPS2_CUBEMAP_POSITIVEZ	= 0x00004000;
		const uint DDSCAPS2_CUBEMAP_NEGATIVEZ	= 0x00008000;
		const uint DDSCAPS2_CUBEMAP_ALL_FACES	= ( DDSCAPS2_CUBEMAP_POSITIVEX | DDSCAPS2_CUBEMAP_NEGATIVEX | DDSCAPS2_CUBEMAP_POSITIVEY | DDSCAPS2_CUBEMAP_NEGATIVEY | DDSCAPS2_CUBEMAP_POSITIVEZ | DDSCAPS2_CUBEMAP_NEGATIVEZ );

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct DDS_PixelFormat {
			public uint dwSize;
			public uint dwFlags;
			public uint dwFourCC;
			public uint dwRGBBitCount;
			public uint dwRBitMask;
			public uint dwGBitMask;
			public uint dwBBitMask;
			public uint dwABitMask; 
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		unsafe struct DDS_Header {
			public uint				dwMagic;
			public uint				dwSize;
			public uint				dwFlags;
			public uint				dwHeight;
			public uint				dwWidth;
			public uint				dwPitchOrLinearSize;
			public uint				dwDepth; 
			public uint				dwMipLevels;
			public fixed uint		dwReserved1[11];
			public DDS_PixelFormat	ddspf;
			public uint				dwCaps;
			public uint				dwCaps2;
			public uint				dwCaps3;
			public uint				dwCaps4;
			public uint				dwReserved2;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct DDS_Header_DXT10 {
			public Format				dxgiFormat;
			public ResourceDimension	resourceDimension;
			public ResourceOptionFlags	miscFlag;
			public uint					arraySize;
			public uint					miscFlags2;
		};



		public unsafe bool LoadDDS( byte[] data )
		{
			var dataOffset = SharpDX.Utilities.SizeOf<DDS_Header>();

			fixed( byte* pData = data ) {
				var pHeader = (DDS_Header*)pData;
				if( pHeader->dwMagic != MChar4( 'D', 'D', 'S',' ' ) ) {
					return false;
				}

				var arraySize = 1;
				var fmt = Format.Unknown;

				if( pHeader->ddspf.dwFourCC == MChar4( 'D', 'X', '1', '0' ) ) {
					var pHeaderDX10 = (DDS_Header_DXT10*)( pData + dataOffset );
					fmt = pHeaderDX10->dxgiFormat;
					arraySize = (int)pHeaderDX10->arraySize;
					dataOffset += SharpDX.Utilities.SizeOf<DDS_Header_DXT10>();
				} else {
					switch( pHeader->ddspf.dwFourCC ) {
						case  21: fmt = Format.B8G8R8A8_UNorm;		break; // D3DFMT_A8R8G8B8	
						case  31: fmt = Format.R10G10B10A2_UNorm;	break; // D3DFMT_A2B10G10R10
						case  32: fmt = Format.R8G8B8A8_UNorm;		break; // D3DFMT_A8B8G8R8
						case  34: fmt = Format.R16G16_UNorm;		break; // D3DFMT_G16R16
						case  36: fmt = Format.R16G16B16A16_UNorm;	break; // D3DFMT_A16B16G16R16
						case  50: fmt = Format.R8_UNorm;			break; // D3DFMT_L8
						case  60: fmt = Format.R8G8_SNorm;			break; // D3DFMT_V8U8
						case  63: fmt = Format.R8G8B8A8_SNorm;		break; // D3DFMT_Q8W8V8U8
						case  64: fmt = Format.R16G16_SNorm;		break; // D3DFMT_V16U16
						case  80: fmt = Format.D16_UNorm;			break; // D3DFMT_D16
						case  81: fmt = Format.R16_UNorm;			break; // D3DFMT_L16
						case  82: fmt = Format.D32_Float;			break; // D3DFMT_D32F_LOCKABLE
						case 110: fmt = Format.R16G16B16A16_SNorm;	break; // D3DFMT_Q16W16V16U16
						case 111: fmt = Format.R16_Float;			break; // D3DFMT_R16F 
						case 112: fmt = Format.R16G16_Float;		break; // D3DFMT_G16R16F
						case 113: fmt = Format.R16G16B16A16_Float;	break; // D3DFMT_A16B16G16R16F
						case 114: fmt = Format.R32_Float;			break; // D3DFMT_R32F
						case 115: fmt = Format.R32G32_Float;		break; // D3DFMT_G32R32F 
						case 116: fmt = Format.R32G32B32A32_Float;	break; // D3DFMT_A32B32G32R32F
						default: {
							if( pHeader->ddspf.dwFourCC == MChar4( 'D' ,'X' ,'T' ,'1' ) ) {
								fmt = Format.BC1_UNorm; 
							} else if( pHeader->ddspf.dwFourCC == MChar4( 'D' ,'X' ,'T' ,'3' ) ) {
								fmt = Format.BC2_UNorm;
							} else if( pHeader->ddspf.dwFourCC == MChar4( 'D' ,'X' ,'T' ,'5' ) ) {
								fmt = Format.BC3_UNorm;
							} else if( pHeader->ddspf.dwFourCC == MChar4( 'B' ,'C' ,'4' ,'U' ) ) {
								fmt = Format.BC4_UNorm;
							} else if( pHeader->ddspf.dwFourCC == MChar4( 'A' ,'T' ,'I' ,'1' ) ) {
								fmt = Format.BC4_UNorm;
							} else if( pHeader->ddspf.dwFourCC == MChar4( 'A' ,'T' ,'I' ,'2' ) ) {
								fmt = Format.BC5_UNorm;
							} else if( pHeader->ddspf.dwFourCC == MChar4( 'B' ,'C' ,'4' ,'S' ) ) {
								fmt = Format.BC4_SNorm;
							} else if( pHeader->ddspf.dwFourCC == MChar4( 'B' ,'C' ,'5' ,'S' ) ) {
								fmt = Format.BC5_SNorm;
							} else {
								switch( pHeader->ddspf.dwRGBBitCount ) {
									case 8: {
										fmt = Format.R8_UNorm; 
									} break;
									case 16: {
										switch( pHeader->ddspf.dwRBitMask ) {
											case 0xFFFF:	fmt = Format.R16_UNorm; break;
											case 0xFF:		fmt = Format.R8G8_UNorm; break;		
										}
									} break;
									case 32: {
										switch( pHeader->ddspf.dwRBitMask ) {
											case 0xFF:		fmt = Format.R8G8B8A8_UNorm; break;
											case 0xFF0000:	fmt = Format.B8G8R8A8_UNorm; break;
											case 0xFFFF:	fmt = Format.R16G16_UNorm; break;
											case 0x3FF:		fmt = Format.R10G10B10A2_UNorm; break;
										}
									} break;
								}
							}
						} break;
					}
				}

				if( !IsFormatValid( fmt ) ) {
					return false;
				}
				
				Format		= fmt;
				width		= (int)pHeader->dwWidth;
				height		= (int)pHeader->dwHeight;
				depth		= ( ( pHeader->dwCaps2 & DDSCAPS2_CUBEMAP ) != 0 ) ? 0 : ( ( ( pHeader->dwCaps2 & DDSCAPS2_VOLUME ) != 0 ) ? (int)pHeader->dwDepth : 1 );
				MipLevels	= ( ( pHeader->dwFlags & DDSD_MipLevels ) != 0 ) ? (int)pHeader->dwMipLevels : 1;
				ArraySize	= arraySize;
			}

			Marshal.Copy( data, dataOffset, AllocateData(), GetSizeInBytes() );

			return true;
		}



		public unsafe bool SaveDDS( string file )
		{
			if( data == IntPtr.Zero ) {
				return false;
			}

			var sizeOfHeader = SharpDX.Utilities.SizeOf<DDS_Header>();
			var sizeOfHeaderDXT10 = SharpDX.Utilities.SizeOf<DDS_Header_DXT10>();
			var sizeOfData = GetSizeInBytes();

			var buffer = new byte[sizeOfHeader + sizeOfHeaderDXT10 + sizeOfData];

			fixed( byte* ptr = buffer ) {
				var pHeader = (DDS_Header*)ptr;
				pHeader->dwMagic = MChar4( 'D', 'D', 'S', ' ' );
				pHeader->dwSize = 124;
				pHeader->dwFlags = DDSD_CAPS | DDSD_PIXELFORMAT | DDSD_WIDTH | DDSD_HEIGHT | ( Is3D() ? DDSD_DEPTH : 0 ) | ( ( MipLevels > 1 ) ? DDSD_MipLevels : 0 );
				pHeader->dwHeight = (uint)height;
				pHeader->dwWidth = (uint)width;
				pHeader->dwDepth = Is3D() ? (uint)depth : 0;
				pHeader->dwMipLevels = ( MipLevels > 1) ? (uint)MipLevels : 0;
				pHeader->dwCaps = DDSCAPS_TEXTURE | ( ( ( depth != 1 ) || ( MipLevels > 1 ) ) ? DDSCAPS_COMPLEX : 0 ) | ( ( MipLevels > 1 ) ? DDSCAPS_MIPMAP : 0 );
				pHeader->dwCaps2 = Is3D() ? DDSCAPS2_VOLUME : ( IsCube() ? DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_ALL_FACES : 0 );
				pHeader->ddspf.dwSize = 32;  
				pHeader->ddspf.dwFlags = DDPF_FOURCC;
				pHeader->ddspf.dwFourCC = MChar4( 'D', 'X', '1', '0' );

				var pHeaderDXT10 = (DDS_Header_DXT10*)( ptr + sizeOfHeader );
				pHeaderDXT10->resourceDimension = Is1D() ? ResourceDimension.Texture1D : ( Is3D() ? ResourceDimension.Texture3D : ResourceDimension.Texture2D );
				pHeaderDXT10->miscFlag = IsCube() ? ResourceOptionFlags.TextureCube : 0;
				pHeaderDXT10->arraySize = (uint)ArraySize;
				pHeaderDXT10->dxgiFormat = Format;
			}
			  
			Marshal.Copy( data, buffer, sizeOfHeader + sizeOfHeaderDXT10, sizeOfData );

			if( File.Exists( file ) ) {
				File.Delete( file );
			}

			using( var stream = File.OpenWrite( file ) ) {
				stream.Write( buffer, 0, buffer.Length );
			}
			
			return true;
		}



		static uint MChar4( char a, char b, char c, char d )
		{
			return (uint)( a | ( b << 8 ) | ( c << 16 ) | ( d << 24 ) );
		}
	}
}
