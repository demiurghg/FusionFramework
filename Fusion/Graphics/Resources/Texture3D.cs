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
	public class Texture3D : ShaderResource {

		D3D.Texture3D	tex3D;
		ColorFormat		format;
		int				mipCount;

		ShaderResource	linearResource;
		ShaderResource	srgbResource;

		[ContentLoader(typeof(Texture3D))]
		public class Loader : ContentLoader {

			public override object Load ( Game game, Stream stream, Type requestedType, string assetPath )
			{
				return new Texture3D( game.GraphicsDevice, stream );
			}
		}
		


		/// <summary>
		/// Creates texture
		/// </summary>
		/// <param name="device"></param>
		public Texture3D ( GraphicsDevice device, int width, int height, int depth, ColorFormat format, bool mips ) : base( device )
		{
			this.Width		=	width;
			this.Height		=	height;
			this.Depth		=	depth;
			this.format		=	format;
			this.mipCount	=	mips ? ShaderResource.CalculateMipLevels(Width,Height,Depth) : 1;

			var texDesc = new Texture3DDescription();
			texDesc.BindFlags		=	BindFlags.ShaderResource;
			texDesc.CpuAccessFlags	=	CpuAccessFlags.None;
			texDesc.Format			=	Converter.Convert( format );
			texDesc.Height			=	Height;
			texDesc.MipLevels		=	mipCount;
			texDesc.OptionFlags		=	ResourceOptionFlags.None;
			texDesc.Usage			=	ResourceUsage.Default;
			texDesc.Width			=	Width;
			texDesc.Depth			=	Depth;

			tex3D	=	new D3D.Texture3D( device.Device, texDesc );


			var descLinear = new ShaderResourceViewDescription();
			descLinear.Format		=	Converter.Convert( format );
			descLinear.Dimension	=	ShaderResourceViewDimension.Texture3D;
			descLinear.Texture3D.MipLevels			=	mipCount;
			descLinear.Texture3D.MostDetailedMip	=	0;


			var descSRGB = new ShaderResourceViewDescription();
			descSRGB.Format		=	MakeSRgb( Converter.Convert( format ) );
			descSRGB.Dimension	=	ShaderResourceViewDimension.Texture3D;
			descLinear.Texture3D.MipLevels		=	mipCount;
			descLinear.Texture3D.MostDetailedMip	=	0;

			linearResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex3D, descLinear ), Width, Height, Depth );
			srgbResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex3D, descSRGB )  , Width, Height, Depth );

			SRV			=	linearResource.SRV;
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public Texture3D ( GraphicsDevice device, Stream stream ) : base( device )
		{
			CreateFromFile( stream.ReadAllBytes(), stream is FileStream ? (stream as FileStream).Name : "stream" );
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public Texture3D ( GraphicsDevice device, byte[] fileInMemory ) : base( device )
		{
			CreateFromFile( fileInMemory, "in memory");
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

			if (ii.ResourceDimension!=ResourceDimension.Texture3D) {
				throw new GraphicsException("File {0} does not contain three-dimensional texture", name);
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
			ili.Format			=	MakeTypeless( ii.Format );
			ili.Filter			=	FilterFlags.None;//(FilterFlags)ImageLoadInformation.FileDefaultValue;
			ili.MipFilter		=	FilterFlags.None;//(FilterFlags)ImageLoadInformation.FileDefaultValue;
			ili.PSrcInfo		=	new System.IntPtr(0);

			Width			=	ii.Width;
			Height			=	ii.Height;
			Depth			=	ii.Depth;
			mipCount		=	ii.MipLevels;

			tex3D			=	D3D.Texture3D.FromMemory( device.Device, fileInMemory, ili ).QueryInterface<D3D.Texture3D>();


			var descLinear = new ShaderResourceViewDescription();
			descLinear.Format		=	Converter.Convert( format );
			descLinear.Dimension	=	ShaderResourceViewDimension.Texture3D;
			descLinear.Texture3D.MipLevels			=	mipCount;
			descLinear.Texture3D.MostDetailedMip	=	0;


			var descSRGB = new ShaderResourceViewDescription();
			descSRGB.Format		=	MakeSRgb( Converter.Convert( format ) );
			descSRGB.Dimension	=	ShaderResourceViewDimension.Texture3D;
			descLinear.Texture3D.MipLevels		=	mipCount;
			descLinear.Texture3D.MostDetailedMip	=	0;

			linearResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex3D, descLinear ), Width, Height, Depth );
			srgbResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, tex3D, descSRGB )  , Width, Height, Depth );

			SRV			=	linearResource.SRV;
		}

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref tex3D );
				SafeDispose( ref SRV );
				SafeDispose( ref srgbResource );
				SafeDispose( ref linearResource );
			}
			base.Dispose( disposing );
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
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
        public void SetData<T>(T[] data) where T : struct
		{
			SetData<T>(data, 0, data.Length);
		}
		


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		public void SetData<T> (T[] data, int startIndex, int elementCount) where T : struct
		{
			SetData<T>(0, 0, 0, Width, Height, 0, Depth, data, startIndex, elementCount);
		}
		

		
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="level"></param>
		/// <param name="left"></param>
		/// <param name="top"></param>
		/// <param name="right"></param>
		/// <param name="bottom"></param>
		/// <param name="front"></param>
		/// <param name="back"></param>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		public void SetData<T>( int level,
		    int left, int top, int right, int bottom, int front, int back,
		    T[] data, int startIndex, int elementCount ) where T: struct
		{
			if (data == null) { 
				throw new ArgumentNullException("data");
			}

			var elementSizeInByte	=	Marshal.SizeOf(typeof(T));
			var dataHandle			=	GCHandle.Alloc(data, GCHandleType.Pinned);
			var dataPtr				=	(IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
            
			try {
				int width	=	right - left;
				int height	=	bottom - top;
				int depth	=	back - front;

				int rowPitch	=	width * Converter.SizeOf( format );
				int slicePitch	=	rowPitch * height; // For 3D texture: Size of 2D image.
				var box			=	new DataBox(dataPtr, rowPitch, slicePitch);

				int subresourceIndex = level;

				var region		= new ResourceRegion(left, top, front, right, bottom, back);

				var d3dContext	= device.DeviceContext;
            
				lock (d3dContext) {
					d3dContext.UpdateSubresource( box, tex3D, subresourceIndex, region );
				}

			} finally {
				dataHandle.Free ();
			}

		}


	}
}
