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
	public class Texture2D : ShaderResource {

		D3D.Texture2D	tex2D;
		ColorFormat		format;
		int				mipCount;

		[ContentLoader(typeof(Texture2D))]
		public class Loader : ContentLoader {

			public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
			{
				bool srgb = assetPath.ToLowerInvariant().Contains("|srgb");
				return new Texture2D( game.GraphicsDevice, stream, srgb );
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
													 
			lock (device.DeviceContext) {
				tex2D	=	new D3D.Texture2D( device.Device, texDesc );
				SRV		=	new ShaderResourceView( device.Device, tex2D );
			}
		}
		



		/// <summary>
		/// Returns SRgb version of the current resource.
		/// </summary>
		[Obsolete]
		public ShaderResource SRgb {
			get {
				return this;
			}
		}


		/// <summary>
		/// Returns linear version of the current resource.
		/// </summary>
		[Obsolete]
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
			bool	result;

			lock (device.DeviceContext) {
				if ((char)fileInMemory[0]=='D' &&
					(char)fileInMemory[1]=='D' &&
					(char)fileInMemory[2]=='S' &&
					(char)fileInMemory[3]==' ' ) {
	
					result = DdsLoader.CreateTextureFromMemory( device.Device.NativePointer, fileInMemory, forceSRgb, ref resource, ref resourceView );
				} else {
					result = WicLoader.CreateTextureFromMemory( device.Device.NativePointer, fileInMemory, forceSRgb, ref resource, ref resourceView );
				}

				if (!result) {	
					throw new GraphicsException( "Failed to load texture: " + name );
				}

				tex2D	=	new D3D.Texture2D( resource );
				SRV		=	new D3D.ShaderResourceView( resourceView );
			}

			Width		=	tex2D.Description.Width;
			Height		=	tex2D.Description.Height;
			Depth		=	1;
			mipCount	=	tex2D.Description.MipLevels;
			format		=	Converter.Convert( tex2D.Description.Format );
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
