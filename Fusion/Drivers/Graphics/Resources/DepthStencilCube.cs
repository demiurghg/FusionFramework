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


namespace Fusion.Drivers.Graphics {


	public class DepthStencilCube : ShaderResource {

		
		/// <summary>
		/// Sample count
		/// </summary>
		public int	SampleCount { get; private set; }

		/// <summary>
		/// Render target format
		/// </summary>
		public DepthFormat	Format { get; private set; }


		D3D.Texture2D			texCube;
		DepthStencilSurface[]	surfaces;
			


		/// <summary>
		/// Creates render target
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public DepthStencilCube ( GraphicsDevice device, DepthFormat format, int size, int samples, string debugName = "" ) : base ( device )
		{
			bool msaa	=	samples > 1;

			CheckSamplesCount( samples );

			SampleCount	=	samples;

			Format		=	format;
			SampleCount	=	samples;
			Width		=	size;
			Height		=	size;
			Depth		=	1;

			var	texDesc	=	new Texture2DDescription();
				texDesc.Width				=	Width;
				texDesc.Height				=	Height;
				texDesc.ArraySize			=	6;
				texDesc.BindFlags			=	BindFlags.RenderTarget | BindFlags.ShaderResource;
				texDesc.CpuAccessFlags		=	CpuAccessFlags.None;
				texDesc.Format				=	Converter.ConvertToTex( format );
				texDesc.MipLevels			=	1;
				texDesc.OptionFlags			=	ResourceOptionFlags.TextureCube;
				texDesc.SampleDescription	=	new DXGI.SampleDescription(samples, 0);
				texDesc.Usage				=	ResourceUsage.Default;


			texCube	=	new D3D.Texture2D( device.Device, texDesc );


			var srvDesc	=	new ShaderResourceViewDescription();
				srvDesc.Dimension			=	samples > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D;
				srvDesc.Format				=	Converter.ConvertToSRV( format );
				srvDesc.Texture2D.MostDetailedMip	=	0;
				srvDesc.Texture2D.MipLevels			=	1;

			SRV		=	new ShaderResourceView( device.Device, texCube );




			//
			//	Create surfaces :
			//
			surfaces	=	new DepthStencilSurface[ 6 ];

			for ( int face=0; face<6; face++) {

				var rtvDesc = new DepthStencilViewDescription();
					rtvDesc.Texture2DArray.MipSlice			=	0;
					rtvDesc.Texture2DArray.FirstArraySlice	=	face;
					rtvDesc.Texture2DArray.ArraySize		=	1;
					rtvDesc.Dimension						=	msaa ? DepthStencilViewDimension.Texture2DMultisampledArray : DepthStencilViewDimension.Texture2DArray;
					rtvDesc.Format							=	Converter.ConvertToDSV( format );

				var dsv	=	new DepthStencilView( device.Device, texCube, rtvDesc );

				int subResId	=	Resource.CalculateSubResourceIndex( 0, face, 1 );

				surfaces[face]	=	new DepthStencilSurface( dsv, format, Width, Height, SampleCount );
			}
		}



		public DepthStencilSurface FacePosX { get {	return GetSurface( CubeFace.FacePosX );	} }
		public DepthStencilSurface FaceNegX { get {	return GetSurface( CubeFace.FaceNegX );	} }
		public DepthStencilSurface FacePosY { get {	return GetSurface( CubeFace.FacePosY );	} }
		public DepthStencilSurface FaceNegY { get {	return GetSurface( CubeFace.FaceNegY );	} }
		public DepthStencilSurface FacePosZ { get {	return GetSurface( CubeFace.FacePosZ );	} }
		public DepthStencilSurface FaceNegZ { get {	return GetSurface( CubeFace.FaceNegZ );	} }



		/// <summary>
		/// Gets render target surface for given mip level.
		/// </summary>
		/// <param name="mipLevel"></param>
		/// <returns></returns>
		public DepthStencilSurface GetSurface ( CubeFace face )
		{
			return surfaces[ (int)face ];
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
					for (int face=0; face<6; face++) {
						var surf = surfaces[face];
						SafeDispose( ref surf );
					}
					surfaces = null;
				}

				SafeDispose( ref SRV );
				SafeDispose( ref texCube );
			}

			base.Dispose(disposing);
		}
	}
}

