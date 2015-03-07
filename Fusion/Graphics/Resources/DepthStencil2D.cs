using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Specialized;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Fusion.Mathematics;


namespace Fusion.Graphics {
	
	public class DepthStencil2D : ShaderResource {

		/// <summary>
		/// Samples count
		/// </summary>
		public int			SampleCount { get; private set; }

		/// <summary>
		/// Render target format
		/// </summary>
		public DepthFormat	Format { get; private set; }


		D3D.Texture2D		tex2D;
		DepthStencilSurface	surface;
			

		
		/// <summary>
		/// Creates depth stencil texture, view and shader resource with format D24S8
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public DepthStencil2D ( GraphicsDevice device, DepthFormat format, int width, int height, int samples = 1 ) : base( device )
		{
			Width		=	width;
			Height		=	height;
			Depth		=	1;
			Format		=	format;
			SampleCount	=	samples;

			var bindFlags	=	BindFlags.DepthStencil;

			
			if (device.GraphicsProfile==GraphicsProfile.HiDef) {
				bindFlags	|=	BindFlags.ShaderResource;

			} else if (device.GraphicsProfile==GraphicsProfile.Reach) {
				if (samples==1) {
					bindFlags	|=	BindFlags.ShaderResource;
				}
			}


			var	texDesc	=	new Texture2DDescription();
				texDesc.Width				=	width;
				texDesc.Height				=	height;
				texDesc.ArraySize			=	1;
				texDesc.BindFlags			=	bindFlags;
				texDesc.CpuAccessFlags		=	CpuAccessFlags.None;
				texDesc.Format				=	Converter.ConvertToTex( format );
				texDesc.MipLevels			=	1;
				texDesc.OptionFlags			=	ResourceOptionFlags.None;
				texDesc.SampleDescription	=	new DXGI.SampleDescription(samples, 0);
				texDesc.Usage				=	ResourceUsage.Default;

			var dsvDesc	=	new DepthStencilViewDescription();
				dsvDesc.Dimension			=	samples > 1 ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D;
				dsvDesc.Format				=	Converter.ConvertToDSV( format );
				dsvDesc.Flags				=	DepthStencilViewFlags.None;

			var srvDesc	=	new ShaderResourceViewDescription();
				srvDesc.Dimension			=	samples > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D;
				srvDesc.Format				=	Converter.ConvertToSRV( format );
				srvDesc.Texture2D.MostDetailedMip	=	0;
				srvDesc.Texture2D.MipLevels			=	1;

			tex2D		=	new D3D.Texture2D		( device.Device, texDesc );

			var dsv		=	new DepthStencilView	( device.Device, tex2D,	dsvDesc );

			if (bindFlags.HasFlag( BindFlags.ShaderResource)) {
				SRV		=	new ShaderResourceView	( device.Device, tex2D,	srvDesc );
			}

			surface		=	new DepthStencilSurface	( dsv, format, width, height, samples );
		}



		/// <summary>
		/// Gets depth stencil surface.
		/// </summary>
		public DepthStencilSurface Surface {
			get {
				return GetSurface();
			}
		}



		/// <summary>
		/// Gets depth stencil surface.
		/// </summary>
		/// <returns></returns>
		public DepthStencilSurface GetSurface()
		{
			return surface;
		}



		/// <summary>
		/// Disposes
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref tex2D );
				SafeDispose( ref SRV );
				SafeDispose( ref surface );
			}
			base.Dispose( disposing );
		}

	


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="level"></param>
		/// <param name="rect"></param>
		/// <param name="data"></param>
		/// <param name="startIndex"></param>
		/// <param name="elementCount"></param>
		public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
			throw new NotSupportedException("Reading depth values from CPU is not supported");

			#if false
            if (data == null || data.Length == 0) {
                throw new ArgumentException("data cannot be null");
			}
            
			if (data.Length < startIndex + elementCount) {
                throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");
			}

            // Create a temp staging resource for copying the data.
            // 
            // TODO: We should probably be pooling these staging resources
            // and not creating a new one each time.
            //
            var desc = new SharpDX.Direct3D11.Texture2DDescription();
				desc.Width						= Width;
				desc.Height						= Height;
				desc.MipLevels					= 1;
				desc.ArraySize					= 1;
				desc.Format						= Converter.ConvertToTex(Format);
				desc.BindFlags					= D3D.BindFlags.None;
				desc.CpuAccessFlags				= D3D.CpuAccessFlags.Read;
				desc.SampleDescription.Count	= 1;
				desc.SampleDescription.Quality	= 0;
				desc.Usage						= D3D.ResourceUsage.Staging;
				desc.OptionFlags				= D3D.ResourceOptionFlags.None;

		    
			var d3dContext = device.DeviceContext;

            using (var stagingTex = new D3D.Texture2D(device.Device, desc)) {
                lock (d3dContext)
                {
                    // Copy the data from the GPU to the staging texture.
                    int elementsInRow;
                    int rows;
                    if (rect.HasValue)
                    {
                        elementsInRow = rect.Value.Width;
                        rows = rect.Value.Height;
                        d3dContext.CopySubresourceRegion( tex2D, level, new D3D.ResourceRegion(rect.Value.Left, rect.Value.Top, 0, rect.Value.Right, rect.Value.Bottom, 1), stagingTex, 0, 0, 0, 0);
                    }
                    else
                    {
                        elementsInRow = Width;
                        rows = Height;
                        d3dContext.CopySubresourceRegion( tex2D, level, null, stagingTex, 0, 0, 0, 0);
                    }

                    // Copy the data to the array.
                    DataStream stream;
                    var databox = d3dContext.MapSubresource(stagingTex, 0, D3D.MapMode.Read, D3D.MapFlags.None, out stream);

                    // Some drivers may add pitch to rows.
                    // We need to copy each row separatly and skip trailing zeros.
                    var currentIndex = startIndex;
                    var elementSize = SharpDX.Utilities.SizeOf<T>();
                    for (var row = 0; row < rows; row++)
                    {
                        stream.ReadRange(data, currentIndex, elementsInRow);
                        stream.Seek(databox.RowPitch - (elementSize * elementsInRow), SeekOrigin.Current);
                        currentIndex += elementsInRow;
                    }
                    stream.Dispose();
                }
			}
			#endif
        }



		/// <summary>
		/// 
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
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void GetData<T> (T[] data) where T : struct
		{
			this.GetData(0, null, data, 0, data.Length);
		}
	}
}
