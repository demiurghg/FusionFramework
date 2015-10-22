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
using Fusion.Core;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {
	public class Texture3D : ShaderResource {

		D3D.Texture3D	tex3D;
		ColorFormat		format;
		int				mipCount;

		[ContentLoader(typeof(Texture3D))]
		public class Loader : ContentLoader {

			public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
			{
				bool srgb = assetPath.ToLowerInvariant().Contains("|srgb");
				return new Texture3D( game.GraphicsDevice, stream, srgb );
			}
		}
		


		/// <summary>
		/// Creates texture
		/// </summary>
		/// <param name="device"></param>
		public Texture3D ( GraphicsDevice device, int width, int height, int depth, ColorFormat format, bool mips, bool srgb = false ) : base( device )
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
			SRV		=	new D3D.ShaderResourceView( device.Device, tex3D );
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public Texture3D ( GraphicsDevice device, Stream stream, bool forceSRgb = false ) : base( device )
		{
			CreateFromFile( stream.ReadAllBytes(), stream is FileStream ? (stream as FileStream).Name : "stream", forceSRgb );
		}



		/// <summary>
		/// Creates texture from file
		/// </summary>
		/// <param name="device"></param>
		/// <param name="path"></param>
		public Texture3D ( GraphicsDevice device, byte[] fileInMemory, bool forceSRgb = false ) : base( device )
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

			tex3D	=	new D3D.Texture3D( resource );
			SRV		=	new D3D.ShaderResourceView( resourceView );

			Width		=	tex3D.Description.Width;
			Height		=	tex3D.Description.Height;
			Depth		=	tex3D.Description.Depth;
			mipCount	=	tex3D.Description.MipLevels;
			format		=	Converter.Convert( tex3D.Description.Format );
		}

		

		/// <summary>
		/// Disposes
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref tex3D );
				SafeDispose( ref SRV );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Sets 3D texture data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
        public void SetData<T>(T[] data) where T : struct
		{
			SetData<T>(data, 0, data.Length);
		}
		


		/// <summary>
		/// Sets 3D texture data, specifying a start index and number of elements.
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
		/// Sets 3D texture data, specifying a mipmap level, source box, start index, and number of elements.
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

				lock (device.DeviceContext) {
					device.DeviceContext.UpdateSubresource( box, tex3D, subresourceIndex, region );
				}

			} finally {
				dataHandle.Free ();
			}

		}


	}
}
