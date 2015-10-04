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
using D3D = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Fusion.Drivers.Graphics {


	public class RenderTargetCube : ShaderResource {

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


		D3D.Texture2D			texCube;
		RenderTargetSurface[,]	surfaces;
			


		/// <summary>
		/// Creates render target
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public RenderTargetCube ( GraphicsDevice device, ColorFormat format, int size, string debugName = "" ) : base ( device )
		{
			Create( format, size, 1, false, debugName );
		}



		/// <summary>
		/// Creates render target
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public RenderTargetCube ( GraphicsDevice device, ColorFormat format, int size, int samples, string debugName = "" ) : base ( device )
		{
			Create( format, size, samples, false, debugName );
		}



		/// <summary>
		/// Creates render target
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="format"></param>
		public RenderTargetCube ( GraphicsDevice device, ColorFormat format, int size, bool mips, string debugName = "" ) : base ( device )
		{
			Create( format, size, 1, mips, debugName );
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
		void Create ( ColorFormat format, int size, int samples, bool mips, string debugName )
		{
			bool msaa	=	samples > 1;

			CheckSamplesCount( samples );

			if (mips && samples>1) {
				throw new ArgumentException("Render target should be multisampler either mipmapped");
			}

			SampleCount		=	samples;

			Format		=	format;
			SampleCount	=	samples;
			Width		=	size;
			Height		=	size;
			Depth		=	1;
			MipCount	=	mips ? ShaderResource.CalculateMipLevels( Width, Height ) : 1;

			var	texDesc	=	new Texture2DDescription();
				texDesc.Width				=	Width;
				texDesc.Height				=	Height;
				texDesc.ArraySize			=	6;
				texDesc.BindFlags			=	BindFlags.RenderTarget | BindFlags.ShaderResource;
				texDesc.CpuAccessFlags		=	CpuAccessFlags.None;
				texDesc.Format				=	Converter.Convert( format );
				texDesc.MipLevels			=	mips ? MipCount : 1;
				texDesc.OptionFlags			=	ResourceOptionFlags.TextureCube | (mips ? ResourceOptionFlags.GenerateMipMaps : ResourceOptionFlags.None);
				texDesc.SampleDescription	=	new DXGI.SampleDescription(samples, 0);
				texDesc.Usage				=	ResourceUsage.Default;


			texCube	=	new D3D.Texture2D( device.Device, texDesc );
			SRV		=	new ShaderResourceView( device.Device, texCube );




			//
			//	Create surfaces :
			//
			surfaces	=	new RenderTargetSurface[ MipCount, 6 ];

			for ( int mip=0; mip<MipCount; mip++ ) { 

				int width	=	GetMipSize( Width,  mip );
				int height	=	GetMipSize( Height, mip );

				for ( int face=0; face<6; face++) {

					var rtvDesc = new RenderTargetViewDescription();
						rtvDesc.Texture2DArray.MipSlice			=	mip;
						rtvDesc.Texture2DArray.FirstArraySlice	=	face;
						rtvDesc.Texture2DArray.ArraySize		=	1;
						rtvDesc.Dimension						=	msaa ? RenderTargetViewDimension.Texture2DMultisampledArray : RenderTargetViewDimension.Texture2DArray;
						rtvDesc.Format							=	Converter.Convert( format );

					var rtv	=	new RenderTargetView( device.Device, texCube, rtvDesc );

					int subResId	=	Resource.CalculateSubResourceIndex( mip, face, MipCount );

					surfaces[mip,face]	=	new RenderTargetSurface( rtv, null, texCube, subResId, format, Width, Height, samples );
				}
			}
		}



		public RenderTargetSurface FacePosX { get {	return GetSurface( 0, CubeFace.FacePosX );	} }
		public RenderTargetSurface FaceNegX { get {	return GetSurface( 0, CubeFace.FaceNegX );	} }
		public RenderTargetSurface FacePosY { get {	return GetSurface( 0, CubeFace.FacePosY );	} }
		public RenderTargetSurface FaceNegY { get {	return GetSurface( 0, CubeFace.FaceNegY );	} }
		public RenderTargetSurface FacePosZ { get {	return GetSurface( 0, CubeFace.FacePosZ );	} }
		public RenderTargetSurface FaceNegZ { get {	return GetSurface( 0, CubeFace.FaceNegZ );	} }



		/// <summary>
		/// Gets render target surface for given mip level.
		/// </summary>
		/// <param name="mipLevel"></param>
		/// <returns></returns>
		public RenderTargetSurface GetSurface ( int mipLevel, CubeFace face )
		{
			return surfaces[ mipLevel, (int)face ];
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
					
					for (int mip=0; mip<surfaces.GetLength(0); mip++) {
						for (int face=0; face<6; face++) {
							var surf = surfaces[mip,face];
							SafeDispose( ref surf );
						}
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
