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
using Fusion.Core.Content;
using Fusion.Core.Mathematics;
using SharpDX.Direct3D;
using Native.Dds;
using Native.Wic;
using Fusion.Core;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {
	public class TextureCube : ShaderResource {

		D3D.Texture2D	texCube;
		ColorFormat		format;
		int				mipCount;

		ShaderResource	linearResource;
		ShaderResource	srgbResource;

		[ContentLoader(typeof(TextureCube))]
		public class Loader : ContentLoader {

			public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
			{
				bool srgb = assetPath.ToLowerInvariant().Contains("|srgb");
				return new TextureCube( game.GraphicsDevice, stream, srgb );
			}
		}
		


		/// <summary>
		/// Creates texture
		/// </summary>
		/// <param name="device"></param>
		public TextureCube ( GraphicsDevice device, int size, ColorFormat format, bool mips, bool srgb = false ) : base( device )
		{
			this.Width		=	size;
			this.Depth		=	1;
			this.Height		=	size;
			this.format		=	format;
			this.mipCount	=	mips ? ShaderResource.CalculateMipLevels(Width,Height) : 1;

			//Log.Warning("CHECK ARRAY SIZE!");

			var texDesc = new Texture2DDescription();
			texDesc.ArraySize		=	6;
			texDesc.BindFlags		=	BindFlags.ShaderResource;
			texDesc.CpuAccessFlags	=	CpuAccessFlags.None;
			texDesc.Format			=	MakeTypeless( Converter.Convert( format ) );
			texDesc.Height			=	Height;
			texDesc.MipLevels		=	mipCount;
			texDesc.OptionFlags		=	ResourceOptionFlags.TextureCube;
			texDesc.SampleDescription.Count	=	1;
			texDesc.SampleDescription.Quality	=	0;
			texDesc.Usage			=	ResourceUsage.Default;
			texDesc.Width			=	Width;

			texCube	=	new D3D.Texture2D( device.Device, texDesc );
			SRV		=	new ShaderResourceView( device.Device, texCube );

		}
					


		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public TextureCube ( GraphicsDevice device, Stream stream, bool forceSRgb ) : base( device )
		{
			CreateFromFile( stream.ReadAllBytes(), stream is FileStream ? (stream as FileStream).Name : "stream" , forceSRgb);
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public TextureCube ( GraphicsDevice device, byte[] fileInMemory, bool forceSRgb ) : base( device )
		{
			CreateFromFile( fileInMemory, "in memory", forceSRgb);
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

			texCube	=	new D3D.Texture2D( resource );
			SRV		=	new D3D.ShaderResourceView( resourceView );

			Width		=	texCube.Description.Width;
			Height		=	texCube.Description.Height;
			Depth		=	1;
			mipCount	=	texCube.Description.MipLevels;
			format		=	Converter.Convert( texCube.Description.Format );
		}

		

		/// <summary>
		/// Disposes
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
		/// Sets cube texture data, specifying a cubemap face.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="face"></param>
		/// <param name="data"></param>
		public void SetData<T> (CubeFace face, T[] data) where T : struct
		{
            SetData(face, 0, null, data, 0, data.Length);
		}



		/// <summary>
		/// Sets cube texture data, specifying a cubemap face, start index, and number of elements.
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
		/// Sets cube texture data, specifying a cubemap face, mipmap level, source rectangle, start index, and number of elements.
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
