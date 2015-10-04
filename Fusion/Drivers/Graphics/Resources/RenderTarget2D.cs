#define DIRECTX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Specialized;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {

	public class RenderTarget2D : ShaderResource {

		/// <summary>
		/// Samples count
		/// </summary>
		public int	SampleCount { get; private set; }

		/// <summary>
		/// Mipmap levels count
		/// </summary>
		public int	MipCount { get; private set; }

		/// <summary>
		/// Render target format
		/// </summary>
		public ColorFormat	Format { get; private set; }


		D3D.Texture2D			tex2D;
		RenderTargetSurface[]	surfaces;
			


		/// <summary>
		/// Creates render target
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public RenderTarget2D ( GraphicsDevice device, ColorFormat format, int width, int height, bool enableRWBuffer = false ) : base ( device )
		{
			Create( format, width, height, 1, false, enableRWBuffer );
		}



		/// <summary>
		/// Internal constructor to create RT for backbuffer.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="backbufColor"></param>
		internal RenderTarget2D ( GraphicsDevice device, D3D.Texture2D backbufColor ) : base( device )
		{
			if (backbufColor.Description.Format!=DXGI.Format.R8G8B8A8_UNorm) {
				Log.Warning("R8G8B8A8_UNorm");
			}

			Width			=	backbufColor.Description.Width;
			Height			=	backbufColor.Description.Height;
			Format			=	ColorFormat.Rgba8;
			MipCount		=	1;
			SampleCount		=	backbufColor.Description.SampleDescription.Count;
			SRV				=	null;

			tex2D			=	backbufColor;
			surfaces		=	new RenderTargetSurface[1];
			surfaces[0]		=	new RenderTargetSurface( new RenderTargetView( device.Device, backbufColor ), null, tex2D, 0, Format, Width, Height, SampleCount );
		} 



		/// <summary>
		/// Creates render target
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public RenderTarget2D ( GraphicsDevice device, ColorFormat format, int width, int height, int samples ) : base ( device )
		{
			Create( format, width, height, samples, false, false );
		}



		/// <summary>
		/// Creates render target
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public RenderTarget2D ( GraphicsDevice device, ColorFormat format, int width, int height, bool mips, bool enableRWBuffer ) : base ( device )
		{
			Create( format, width, height, 1, mips, enableRWBuffer );
		}



		/// <summary>
		/// Creates render target
		/// </summary>
		/// <param name="?"></param>
		/// <param name="format"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="samples"></param>
		/// <param name="mips"></param>
		/// <param name="debugName"></param>
		void Create ( ColorFormat format, int width, int height, int samples, bool mips, bool enableRWBuffer )
		{
			bool msaa	=	samples > 1;

			CheckSamplesCount( samples );

			if (mips && samples>1) {
				throw new ArgumentException("Render target should be multisampler either mipmapped");
			}

			SampleCount		=	samples;

			Format		=	format;
			SampleCount	=	samples;
			Width		=	width;
			Height		=	height;
			Depth		=	1;
			MipCount	=	mips ? ShaderResource.CalculateMipLevels( width, height ) : 1;

			var	texDesc	=	new Texture2DDescription();
				texDesc.Width				=	width;
				texDesc.Height				=	height;
				texDesc.ArraySize			=	1;
				texDesc.BindFlags			=	BindFlags.RenderTarget | BindFlags.ShaderResource;
				texDesc.CpuAccessFlags		=	CpuAccessFlags.None;
				texDesc.Format				=	Converter.Convert( format );
				texDesc.MipLevels			=	mips ? MipCount : 1;
				texDesc.OptionFlags			=	mips ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None;
				texDesc.SampleDescription	=	new DXGI.SampleDescription(samples, 0);
				texDesc.Usage				=	ResourceUsage.Default;

			if (enableRWBuffer) {
				texDesc.BindFlags |= BindFlags.UnorderedAccess;
			}


			tex2D	=	new D3D.Texture2D( device.Device, texDesc );
			SRV		=	new ShaderResourceView( device.Device, tex2D );



			//
			//	Create surfaces :
			//
			surfaces	=	new RenderTargetSurface[ MipCount ];

			for ( int i=0; i<MipCount; i++ ) { 

				width	=	GetMipSize( Width,  i );
				height	=	GetMipSize( Height, i );

				var rtvDesc = new RenderTargetViewDescription();
					rtvDesc.Texture2D.MipSlice	=	i;
					rtvDesc.Dimension			=	msaa ? RenderTargetViewDimension.Texture2DMultisampled : RenderTargetViewDimension.Texture2D;
					rtvDesc.Format				=	Converter.Convert( format );

				var rtv	=	new RenderTargetView( device.Device, tex2D, rtvDesc );

				UnorderedAccessView uav = null;

				if (enableRWBuffer) {
					var uavDesc = new UnorderedAccessViewDescription();
					uavDesc.Buffer.ElementCount	=	width * height;
					uavDesc.Buffer.FirstElement	=	0;
					uavDesc.Buffer.Flags		=	UnorderedAccessViewBufferFlags.None;
					uavDesc.Dimension			=	UnorderedAccessViewDimension.Texture2D;
					uavDesc.Format				=	Converter.Convert( format );
					uavDesc.Texture2D.MipSlice	=	i;

					uav	=	new UnorderedAccessView( device.Device, tex2D, uavDesc );
				}

				surfaces[i]	=	new RenderTargetSurface( rtv, uav, tex2D, i, format, width, height, samples );
			}
		}



		/// <summary>
		/// Gets top mipmap level's surface.
		/// Equivalent for GetSurface(0)
		/// </summary>
		public RenderTargetSurface Surface {
			get {
				return GetSurface( 0 );
			}
		}



		/// <summary>
		/// Gets render target surface for given mip level.
		/// </summary>
		/// <param name="mipLevel"></param>
		/// <returns></returns>
		public RenderTargetSurface GetSurface ( int mipLevel )
		{
			return surfaces[ mipLevel ];
		}



		/// <summary>
		/// Builds mipmap chain.
		/// </summary>
		public void BuildMipmaps ()
		{
			device.DeviceContext.GenerateMips( SRV );
		}



		/// <summary>
		/// Sets viewport for given render target
		/// </summary>
		public void SetViewport ()
		{
			device.DeviceContext.Rasterizer.SetViewport( 0,0, Width, Height, 0, 1 );
		}



		/// <summary>
		/// Disposes
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				if (surfaces!=null) {
					for (int i=0; i<surfaces.Length; i++) {
						var surf = surfaces[i];
						SafeDispose( ref surf );
					}
					surfaces = null;
				}

				SafeDispose( ref SRV );
				SafeDispose( ref tex2D );
			}

			base.Dispose(disposing);
		}



		/// <summary>
		/// Gets a copy of 2D texture data, specifying a mipmap level, source rectangle, start index, and number of elements.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="level"></param>
		/// <param name="rect"></param>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null || data.Length == 0) {
                throw new ArgumentException("data cannot be null");
			}
            
			if (data.Length < startIndex + elementCount) {
                throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");
			}

			if (rect.HasValue) {
				throw new NotImplementedException("Set 'rect' parameter to null.");
			}


			var mipWidth	=	MathUtil.Clamp( Width >> level , 1, Width );
			var mipHeight	=	MathUtil.Clamp( Width >> level , 1, Height );


            // Create a temp staging resource for copying the data.
            // 
            // TODO: We should probably be pooling these staging resources
            // and not creating a new one each time.
            //
            var desc = new SharpDX.Direct3D11.Texture2DDescription();
				desc.Width						= mipWidth;
				desc.Height						= mipHeight;
				desc.MipLevels					= 1;
				desc.ArraySize					= 1;
				desc.Format						= Converter.Convert(Format);
				desc.BindFlags					= D3D.BindFlags.None;
				desc.CpuAccessFlags				= D3D.CpuAccessFlags.Read;
				desc.SampleDescription.Count	= 1;
				desc.SampleDescription.Quality	= 0;
				desc.Usage						= D3D.ResourceUsage.Staging;
				desc.OptionFlags				= D3D.ResourceOptionFlags.None;

		    
			var d3dContext = device.DeviceContext;

            using (var stagingTex = new D3D.Texture2D(device.Device, desc)) {
                
				lock (d3dContext) {
					//
                    // Copy the data from the GPU to the staging texture.
					//
                    int elementsInRow;
                    int rows;
                    
					if (rect.HasValue) {
                        
						elementsInRow = rect.Value.Width;
                        rows = rect.Value.Height;

						var region = new D3D.ResourceRegion( rect.Value.Left, rect.Value.Top, 0, rect.Value.Right, rect.Value.Bottom, 1 );

                        d3dContext.CopySubresourceRegion( tex2D, level, region, stagingTex, 0, 0, 0, 0);

                    } else {
                        
						elementsInRow = mipWidth;
                        rows = mipHeight;

                        d3dContext.CopySubresourceRegion( tex2D, level, null, stagingTex, 0, 0, 0, 0);

                    }


                    // Copy the data to the array :
                    DataStream stream;
                    var databox = d3dContext.MapSubresource(stagingTex, 0, D3D.MapMode.Read, D3D.MapFlags.None, out stream);

                    // Some drivers may add pitch to rows.
                    // We need to copy each row separatly and skip trailing zeros.
                    var currentIndex	=	startIndex;
                    var elementSize		=	Marshal.SizeOf(typeof(T));
                    
					for (var row = 0; row < rows; row++) {

                        stream.ReadRange(data, currentIndex, elementsInRow);
                        stream.Seek(databox.RowPitch - (elementSize * elementsInRow), SeekOrigin.Current);
                        currentIndex += elementsInRow;

                    }
                    stream.Dispose();
                }
			}
        }



		/// <summary>
		/// Gets a copy of 2D texture data, specifying a start index and number of elements.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
		{
			this.GetData(0, null, data, startIndex, elementCount);
		}
		


		/// <summary>
		/// Gets a copy of 2D texture data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void GetData<T> (T[] data) where T : struct
		{
			this.GetData(0, null, data, 0, data.Length);
		}



		/// <summary>
		/// Saves rendertarget to file.
		/// Output image format will be get automatically from extension.
		/// BMP, DDS, GIF, JPG, PNG, TIFF, WMP are supported.
		/// </summary>
		/// <param name="path"></param>
		public void SaveToFile ( string path )
		{
			lock ( device.DeviceContext ) {
				if (SampleCount>1) {
												
					using( var temp = new RenderTarget2D( this.device, this.Format, this.Width, this.Height, false, false ) ) {
						this.device.Resolve( this, temp );
						temp.SaveToFile( path );
					}
				
				} else {
					var ext = Path.GetExtension( path ).ToLower();

					var iff = ImageFileFormat.Jpg;

					if ( ext == ".bmp"  ) iff = ImageFileFormat.Bmp;
					if ( ext == ".dds"  ) iff = ImageFileFormat.Dds;
					if ( ext == ".gif"  ) iff = ImageFileFormat.Gif;
					if ( ext == ".jpg"  ) iff = ImageFileFormat.Jpg;
					if ( ext == ".png"  ) iff = ImageFileFormat.Png;
					if ( ext == ".tiff" ) iff = ImageFileFormat.Tiff;
					if ( ext == ".wmp"  ) iff = ImageFileFormat.Wmp;

					//
					D3D.Texture2D.ToFile( device.DeviceContext, tex2D, iff, path );
				}
			}
		}
	}
}
