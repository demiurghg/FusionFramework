#define DIRECTX
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
	public class TextureCube : ShaderResource {

		D3D.Texture2D	texCube;
		ColorFormat		format;
		int				mipCount;

		ShaderResource	linearResource;
		ShaderResource	srgbResource;

		[ContentLoader(typeof(TextureCube))]
		public class Loader : ContentLoader {

			public override object Load ( Game game, Stream stream, Type requestedType, string assetPath )
			{
				return new TextureCube( game.GraphicsDevice, stream );
			}
		}
		


		/// <summary>
		/// Creates texture
		/// </summary>
		/// <param name="device"></param>
		public TextureCube ( GraphicsDevice device, int size, ColorFormat format, bool mips ) : base( device )
		{
			this.Width		=	size;
			this.Depth		=	1;
			this.Height		=	size;
			this.format		=	format;
			this.mipCount	=	mips ? ShaderResource.CalculateMipLevels(Width,Height) : 1;

			//Log.Warning("CHECK ARRAY SIZE!");

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

			texCube	=	new D3D.Texture2D( device.Device, texDesc );

			
			var descLinear = new ShaderResourceViewDescription();
			descLinear.Format		=	Converter.Convert( format );
			descLinear.Dimension	=	ShaderResourceViewDimension.TextureCube;
			descLinear.TextureCube.MipLevels		=	mipCount;
			descLinear.TextureCube.MostDetailedMip	=	0;

			var descSRGB = new ShaderResourceViewDescription();
			descSRGB.Format		=	MakeSRgb( Converter.Convert( format ) );
			descSRGB.Dimension	=	ShaderResourceViewDimension.TextureCube;
			descLinear.TextureCube.MipLevels		=	mipCount;
			descLinear.TextureCube.MostDetailedMip	=	0;

			linearResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, texCube, descLinear ), size, size, 1 );
			srgbResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, texCube, descSRGB )  , size, size, 1 );

			SRV			=	linearResource.SRV;

		}
					


		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public TextureCube ( GraphicsDevice device, Stream stream ) : base( device )
		{
			CreateFromFile( stream.ReadAllBytes(), stream is FileStream ? (stream as FileStream).Name : "stream" );
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public TextureCube ( GraphicsDevice device, byte[] fileInMemory ) : base( device )
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

			if (ii.ResourceDimension!=ResourceDimension.Texture2D || ii.ArraySize!=6) {
				throw new GraphicsException("File {0} does not contain cube texture", name);
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
			ili.Format			=	MakeTypeless(ii.Format);
			ili.Filter			=	FilterFlags.None;//(FilterFlags)ImageLoadInformation.FileDefaultValue;
			ili.MipFilter		=	FilterFlags.None;//(FilterFlags)ImageLoadInformation.FileDefaultValue;
			ili.PSrcInfo		=	new System.IntPtr(0);


			Width			=	ii.Width;
			Height			=	ii.Height;
			Depth			=	ii.Depth;
			mipCount		=	ii.MipLevels;

			texCube			=	D3D.Texture2D.FromMemory( device.Device, fileInMemory, ili ).QueryInterface<D3D.Texture2D>();


			var descLinear = new ShaderResourceViewDescription();
			descLinear.Format		=	Converter.Convert( format );
			descLinear.Dimension	=	ShaderResourceViewDimension.TextureCube;
			descLinear.TextureCube.MipLevels		=	mipCount;
			descLinear.TextureCube.MostDetailedMip	=	0;

			var descSRGB = new ShaderResourceViewDescription();
			descSRGB.Format		=	MakeSRgb( Converter.Convert( format ) );
			descSRGB.Dimension	=	ShaderResourceViewDimension.TextureCube;
			descLinear.TextureCube.MipLevels		=	mipCount;
			descLinear.TextureCube.MostDetailedMip	=	0;

			linearResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, texCube, descLinear ), Width, Height, 1 );
			srgbResource	=	new ShaderResource( device, new ShaderResourceView( device.Device, texCube, descSRGB )  , Width, Height, 1 );

			SRV			=	linearResource.SRV;

		}

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref texCube );
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
		/// <param name="face"></param>
		/// <param name="data"></param>
		public void SetData<T> (CubeFace face, T[] data) where T : struct
		{
            SetData(face, 0, null, data, 0, data.Length);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="face"></param>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
        public void SetData<T>(CubeFace face, T[] data, int startIndex, int elementCount) where T : struct
		{
            SetData(face, 0, null, data, startIndex, elementCount);
		}
		


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="face"></param>
		/// <param name="level"></param>
		/// <param name="rect"></param>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
        public void SetData<T>(CubeFace face, int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
		{
            if (data == null) {
                throw new ArgumentNullException("data");
			}

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
			var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try {
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                int xOffset, yOffset, width, height;

                if (rect.HasValue) {
                    xOffset = rect.Value.X;
                    yOffset = rect.Value.Y;
                    width = rect.Value.Width;
                    height = rect.Value.Height;
                } else {
                    xOffset = 0;
                    yOffset = 0;
                    width = Math.Max(1, this.Width >> level);
                    height = Math.Max(1, this.Height >> level);

                    // For DXT textures the width and height of each level is a multiple of 4.
                    if (format == ColorFormat.Dxt1 ||
                        format == ColorFormat.Dxt3 ||
                        format == ColorFormat.Dxt5)
                    {
                        width = (width + 3) & ~3;
                        height = (height + 3) & ~3;
                    }
                }

                var box = new DataBox(dataPtr, width * Converter.SizeOf(format), 0);

	            int subresourceIndex = (int)face * mipCount + level;

                var region = new ResourceRegion {
                    Top = yOffset,
                    Front = 0,
                    Back = 1,
                    Bottom = yOffset + height,
                    Left = xOffset,
                    Right = xOffset + width
                };

				lock (device.DeviceContext) {
					device.DeviceContext.UpdateSubresource(box, texCube, subresourceIndex, region);
				}

            } finally {
                dataHandle.Free();
            }
		}
	}
}
