using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Fusion.Content;
using Fusion.Mathematics;
using SharpDX.Direct3D;


namespace Fusion.Graphics {
	public class Texture2D : ShaderResource {

		D3D.Texture2D	tex2D;
		ColorFormat		format;
		int				mipCount;

		ShaderResource	linearResource;
		ShaderResource	srgbResource;


		[ContentLoader(typeof(Texture2D))]
		public class Loader : ContentLoader {

			public override object Load ( Game game, Stream stream, Type requestedType, string assetPath )
			{
				return new Texture2D( game.GraphicsDevice, stream );
			}
		}
		


		/// <summary>
		/// Creates texture
		/// </summary>
		/// <param name="device"></param>
		public Texture2D ( GraphicsDevice device, int width, int height, ColorFormat format, bool mips ) : base( device )
		{
			this.Width		=	width;
			this.Height		=	height;
			this.Depth		=	1;
			this.format		=	format;
			this.mipCount	=	mips ? ShaderResource.CalculateMipLevels(Width,Height) : 1;

			var texDesc = new Texture2DDescription();
			texDesc.ArraySize		=	1;
			texDesc.BindFlags		=	BindFlags.ShaderResource;
			texDesc.CpuAccessFlags	=	CpuAccessFlags.None;
			texDesc.Format			=	MakeTypeless( Converter.Convert( format ) );
			texDesc.Height			=	Height;
			texDesc.MipLevels		=	mipCount;
			texDesc.OptionFlags		=	ResourceOptionFlags.None;
			texDesc.SampleDescription.Count	=	1;
			texDesc.SampleDescription.Quality	=	0;
			texDesc.Usage			=	ResourceUsage.Default;
			texDesc.Width			=	Width;


			
			var descLinear = new ShaderResourceViewDescription();
			descLinear.Format		=	Converter.Convert( format );
			descLinear.Dimension	=	ShaderResourceViewDimension.Texture2D;
			descLinear.Texture2D.MipLevels = mipCount;
			descLinear.Texture2D.MostDetailedMip = 0;

			var descSRGB = new ShaderResourceViewDescription();
			descSRGB.Format		=	MakeSRgb( Converter.Convert( format ) );
			descSRGB.Dimension	=	ShaderResourceViewDimension.Texture2D;
			descSRGB.Texture2D.MipLevels = mipCount;
			descSRGB.Texture2D.MostDetailedMip = 0;


			tex2D		=	new D3D.Texture2D( device.Device, texDesc );

			linearResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex2D, descLinear ), Width, Height, 1 );
			srgbResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex2D, descSRGB )  , Width, Height, 1 );

			SRV			=	linearResource.SRV;
			//SRV		=	new ShaderResourceView( device.Device, tex2D/*, srvDesc*/ );
		}



		/// <summary>
		/// Returns SRgb version of the current resource.
		/// </summary>
		public ShaderResource SRgb {
			get {
				return srgbResource;
			}
		}


		/// <summary>
		/// Returns linear version of the current resource.
		/// </summary>
		public ShaderResource Linear {
			get {
				return linearResource;
			}
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public Texture2D ( GraphicsDevice device, Stream stream ) : base( device )
		{
			CreateFromFile( stream.ReadAllBytes(), stream is FileStream ? (stream as FileStream).Name : "stream" );
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public Texture2D ( GraphicsDevice device, byte[] fileInMemory ) : base( device )
		{
			CreateFromFile( fileInMemory, "file in memory" );
		}



		/// <summary>
		/// Create texture inplace with new parameters. 
		/// Old texture will be completely discarded
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		/// <param name="mips"></param>
		void CreateFromFile ( byte[] fileInMemory, string name )
		{
			var pii	=	ImageInformation.FromMemory( fileInMemory );

			if (pii==null) {
				throw new GraphicsException( "Failed to get image information from file {0}", name );
			}

			var ii	=	pii.Value;
			var ili =	new ImageLoadInformation();

			if (ii.ResourceDimension!=ResourceDimension.Texture2D || ii.ArraySize!=1) {
				throw new GraphicsException("File {0} does not contain two-dimesional texture", name);
			}


			ili.Width			=	ImageLoadInformation.FileDefaultValue;
			ili.Height			=	ImageLoadInformation.FileDefaultValue;
			ili.Depth			=	ImageLoadInformation.FileDefaultValue;
			ili.FirstMipLevel	=	0;
			ili.MipLevels		=	ii.MipLevels;// ImageLoadInformation.FileDefaultValue;
			ili.Usage			=	ResourceUsage.Default;// (ResourceUsage) ImageLoadInformation.FileDefaultValue;
			ili.BindFlags		=	(BindFlags)ImageLoadInformation.FileDefaultValue;
			ili.CpuAccessFlags	=	(CpuAccessFlags)ImageLoadInformation.FileDefaultValue;
			ili.OptionFlags		=	(ResourceOptionFlags)ImageLoadInformation.FileDefaultValue;
			ili.Format			=	MakeTypeless(ii.Format); //(DXGI.Format)ImageLoadInformation.FileDefaultValue;
			ili.Filter			=	FilterFlags.None;//(FilterFlags)ImageLoadInformation.FileDefaultValue;
			ili.MipFilter		=	FilterFlags.None;//(FilterFlags)ImageLoadInformation.FileDefaultValue;
			ili.PSrcInfo		=	new System.IntPtr(0);


			Width			=	ii.Width;
			Height			=	ii.Height;
			Depth			=	ii.Depth;
			mipCount		=	ii.MipLevels;

			
			var descLinear = new ShaderResourceViewDescription();
			descLinear.Format		=	ii.Format;
			descLinear.Dimension	=	ShaderResourceViewDimension.Texture2D;
			descLinear.Texture2D.MipLevels = ii.MipLevels;
			descLinear.Texture2D.MostDetailedMip = 0;

			var descSRGB = new ShaderResourceViewDescription();
			descSRGB.Format		=	MakeSRgb( ii.Format );
			descSRGB.Dimension	=	ShaderResourceViewDimension.Texture2D;
			descSRGB.Texture2D.MipLevels = ii.MipLevels;
			descSRGB.Texture2D.MostDetailedMip = 0;


			tex2D			=	D3D.Texture2D.FromMemory( device.Device, fileInMemory, ili ).QueryInterface<D3D.Texture2D>();

			linearResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex2D, descLinear ), Width, Height, 1 );
			srgbResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex2D, descSRGB ), Width, Height, 1 );

			SRV			=	linearResource.SRV;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref tex2D );
				SafeDispose( ref SRV );
				SafeDispose( ref srgbResource );
				SafeDispose( ref linearResource );
			}
			base.Dispose( disposing );
		}


		
	#if false
		internal static bool SaveDDS( DeviceContext context, Resource resource, string file )
		{
			Image img = null;

			switch( resource.Dimension ) {
				case ResourceDimension.Buffer: return false;
				case ResourceDimension.Texture1D: {
					var descr = resource.QueryInterface<D3D.Texture1D>().Description;
					if( ( descr.CpuAccessFlags & CpuAccessFlags.Read ) == 0 ) {
						return false;
					}
					img = new Image( descr.Format, descr.Width, 1, 1, descr.MipLevels, descr.ArraySize );
				} break;
				case ResourceDimension.Texture2D: {
					var descr = resource.QueryInterface<D3D.Texture2D>().Description;
					if( ( descr.CpuAccessFlags & CpuAccessFlags.Read ) == 0 ) {
						return false;
					}
					var isCube = ( descr.OptionFlags & ResourceOptionFlags.TextureCube ) != 0;
					img = new Image( descr.Format, descr.Width, descr.Height, isCube ?  0 : 1, descr.MipLevels, descr.ArraySize / ( isCube ? 6 : 1 ) );
				} break;
				case ResourceDimension.Texture3D: {
					var descr = resource.QueryInterface<D3D.Texture3D>().Description;
					if( ( descr.CpuAccessFlags & CpuAccessFlags.Read ) == 0 ) {
						return false;
					}
					img = new Image( descr.Format, descr.Width, descr.Height, descr.Depth, descr.MipLevels, 1 );
				} break;
			}

			img.AllocateData();

			var buffer = new byte[Image.GetSizeInBytes( img.Format, img.Width(), img.Height(), Math.Max( 1, img.Depth() ), 1 )];

			for( int arraySlice = ( img.IsCube() ? 6 : 1 ) * img.ArraySize, subResourceID = arraySlice * img.MipLevels; --arraySlice >= 0; ) {
				for( int mipLevel = img.MipLevels; --mipLevel >= 0; ) {
					var db = context.MapSubresource( resource, --subResourceID, MapMode.Read, MapFlags.None );
					var size = Image.GetSizeInBytes( img.Format, img.Width( mipLevel ), img.Height( mipLevel ), Math.Max( 1, img.Depth( mipLevel ) ), 1 );
					Marshal.Copy( db.DataPointer, buffer, 0, size );
					context.UnmapSubresource( resource, subResourceID );
					Marshal.Copy( buffer, 0, img.Data( mipLevel, arraySlice ), size );
				}
			}

			var result = img.SaveDDS( file );

			img.Dispose();

			return result;
		}
	#endif


		
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            this.SetData(0, null, data, startIndex, elementCount);
        }

		

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void SetData<T>(T[] data) where T : struct
        {
			this.SetData(0, null, data, 0, data.Length);
        }
		


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="level"></param>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		public void SetData<T>( int level, Rectangle? rect, T[] data, int startIndex, int elementCount ) where T: struct
		{
            var elementSizeInByte	=	Marshal.SizeOf(typeof(T));
            var dataHandle			=	GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try {
				var startBytes	=	startIndex * elementSizeInByte;
				var dataPtr		=	(IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);

				int x, y, w, h;
				if (rect.HasValue) {
					x = rect.Value.X;
					y = rect.Value.Y;
					w = rect.Value.Width;
					h = rect.Value.Height;
				} else {
					x = 0;
					y = 0;
					w = Math.Max(Width >> level, 1);
					h = Math.Max(Height >> level, 1);

					// For DXT textures the width and height of each level is a multiple of 4.
					// OpenGL only: The last two mip levels require the width and height to be 
					// passed as 2x2 and 1x1, but there needs to be enough data passed to occupy 
					// a 4x4 block. 
					// Ref: http://www.mentby.com/Group/mac-opengl/issue-with-dxt-mipmapped-textures.html 
					if (format == ColorFormat.Dxt1 ||
						format == ColorFormat.Dxt3 ||
						format == ColorFormat.Dxt5 ) {
						w = (w + 3) & ~3;
						h = (h + 3) & ~3;
					}
				}

				var box = new SharpDX.DataBox(dataPtr, w * Converter.SizeOf(format), 0);

				var region = new SharpDX.Direct3D11.ResourceRegion();
				region.Top = y;
				region.Front = 0;
				region.Back = 1;
				region.Bottom = y + h;
				region.Left = x;
				region.Right = x + w;

				// TODO: We need to deal with threaded contexts here!
				var d3dContext = device.DeviceContext;

				lock (d3dContext) {
					d3dContext.UpdateSubresource(box, tex2D, level, region);
				}
				
			} finally {
				dataHandle.Free();
			}
		}


	}
}
