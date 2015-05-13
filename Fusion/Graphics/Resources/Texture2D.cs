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
using FusionDDS;


namespace Fusion.Graphics {
	public class Texture2D : ShaderResource {

		D3D.Texture2D	tex2D;
		ColorFormat		format;
		int				mipCount;

		[ContentLoader(typeof(Texture2D))]
		public class Loader : ContentLoader {

			public override object Load ( Game game, Stream stream, Type requestedType, string assetPath )
			{
				return new Texture2D( game.GraphicsDevice, stream, false );
			}
		}
		


		/// <summary>
		/// Creates texture
		/// </summary>
		/// <param name="device"></param>
		public Texture2D ( GraphicsDevice device, int width, int height, ColorFormat format, bool mips, bool srgb = false ) : base( device )
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
			texDesc.Format			=	srgb ? MakeSRgb( Converter.Convert( format ) ) : Converter.Convert( format );
			texDesc.Height			=	Height;
			texDesc.MipLevels		=	mipCount;
			texDesc.OptionFlags		=	ResourceOptionFlags.None;
			texDesc.SampleDescription.Count	=	1;
			texDesc.SampleDescription.Quality	=	0;
			texDesc.Usage			=	ResourceUsage.Default;
			texDesc.Width			=	Width;

			//var descLinear = new ShaderResourceViewDescription();
			//descLinear.Format		=	srgb ? MakeSRgb( Converter.Convert( format ) ) : Converter.Convert( format );
			//descLinear.Dimension	=	ShaderResourceViewDimension.Texture2D;
			//descLinear.Texture2D.MipLevels = mipCount;
			//descLinear.Texture2D.MostDetailedMip = 0;

			tex2D	=	new D3D.Texture2D( device.Device, texDesc );
			SRV		=	new ShaderResourceView( device.Device, tex2D/*, srvDesc*/ );

			/*var descSRGB = new ShaderResourceViewDescription();
			descSRGB.Format		=	MakeSRgb( Converter.Convert( format ) );
			descSRGB.Dimension	=	ShaderResourceViewDimension.Texture2D;
			descSRGB.Texture2D.MipLevels = mipCount;
			descSRGB.Texture2D.MostDetailedMip = 0;


			tex2D		=	new D3D.Texture2D( device.Device, texDesc );

			linearResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex2D, descLinear ), Width, Height, 1 );
			srgbResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex2D, descSRGB )  , Width, Height, 1 );

			SRV			=	linearResource.SRV;	*/
			//SRV		=	new ShaderResourceView( device.Device, tex2D/*, srvDesc*/ );
		}



		/// <summary>
		/// Returns SRgb version of the current resource.
		/// </summary>
		public ShaderResource SRgb {
			get {
				return this;
			}
		}


		/// <summary>
		/// Returns linear version of the current resource.
		/// </summary>
		public ShaderResource Linear {
			get {
				return this;
			}
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public Texture2D ( GraphicsDevice device, Stream stream, bool forceSRgb = false ) : base( device )
		{
			CreateFromFile( stream.ReadAllBytes(), stream is FileStream ? (stream as FileStream).Name : "stream", forceSRgb );
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public Texture2D ( GraphicsDevice device, byte[] fileInMemory, bool forceSRgb = false ) : base( device )
		{
			CreateFromFile( fileInMemory, "file in memory", forceSRgb );
		}



		/// <summary>
		/// Create texture inplace with new parameters. 
		/// Old texture will be completely discarded
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		/// <param name="mips"></param>
		void CreateFromFile ( byte[] fileInMemory, string name, bool forceSRgb )
		{
			IntPtr	resource		=	new IntPtr(0);
			IntPtr	resourceView	=	new IntPtr(0);

			var r = DdsLoader.CreateTextureFromMemory( device.Device.NativePointer, fileInMemory, forceSRgb, ref resource, ref resourceView );

			if (!r) {	
				throw new GraphicsException( "Failed to load texture: " + name );
			}

			tex2D	=	new D3D.Texture2D( resource );
			SRV		=	new D3D.ShaderResourceView( resourceView );

			Width		=	tex2D.Description.Width;
			Height		=	tex2D.Description.Height;
			Depth		=	1;
			mipCount	=	tex2D.Description.MipLevels;
			format		=	Converter.Convert( tex2D.Description.Format );

			/*var tex2DDesc	=	tex2D.Description;
			tex*/

			/*var pii	=	ImageInformation.FromMemory( fileInMemory );

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

			SRV			=	linearResource.SRV;	  */
		}



		/// <summary>
		/// Disposes
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref tex2D );
				SafeDispose( ref SRV );
				//SafeDispose( ref srgbResource );
				//SafeDispose( ref linearResource );
			}
			base.Dispose( disposing );
		}


		
		/// <summary>
		/// Sets 2D texture data, specifying a start index, and number of elements.
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
		/// Sets 2D texture data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void SetData<T>(T[] data) where T : struct
        {
			this.SetData(0, null, data, 0, data.Length);
        }
		


		/// <summary>
		/// Sets 2D texture data, specifying a mipmap level, source rectangle, start index, and number of elements.
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
					if (format == ColorFormat.Dxt1 ||
						format == ColorFormat.Dxt3 ||
						format == ColorFormat.Dxt5 ) {
						w = (w + 3) & ~3;
						h = (h + 3) & ~3;
					}
				}

				var box = new SharpDX.DataBox(dataPtr, w * Converter.SizeOf(format), 0);

				var region		= new SharpDX.Direct3D11.ResourceRegion();
				region.Top		= y;
				region.Front	= 0;
				region.Back		= 1;
				region.Bottom	= y + h;
				region.Left		= x;
				region.Right	= x + w;

				lock (device.DeviceContext) {
					device.DeviceContext.UpdateSubresource(box, tex2D, level, region);
				}
				
			} finally {
				dataHandle.Free();
			}
		}


	}
}
