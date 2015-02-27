using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Diagnostics;
using Fusion.Mathematics;
using DXGI = SharpDX.DXGI;


namespace Fusion.Graphics {
	public class ShaderResource : DisposableBase {

		/// <summary>
		/// Gets the GraphicsDevice associated with this GraphicsResource.
		/// </summary>
		protected		GraphicsDevice		device { get; private set; }

		/// <summary>
		/// Internal shader resource view
		/// </summary>
		internal		ShaderResourceView	SRV;

		/// <summary>
		/// Gets the width of this resource in pixels 
		/// (for textures, render targets and depth stencil buffers) 
		/// or size in elements for buffers.
		/// </summary>
		public int		Width		{ get; protected set; }

		/// <summary>
		/// Gets the height of this resource in pixels.
		/// For buffers this values is always 1.
		/// </summary>
		public int		Height		{ get; protected set; }

		/// <summary>
		/// Gets the depth of this resource in pixels.
		/// For 2D-textures, CUBE-textures and buffers this values is always 1.
		/// </summary>
		public int		Depth		{ get; protected set; }

		/// <summary>
		/// Gets width and height and reciprocal (X,Y) width and height of this resource (Z,W) as Vector4.
		/// This property is useful for shader constant.
		/// </summary>
		public Vector4	SizeRcpSize	{ get { return new Vector4( Width, Height, 1.0f/Width, 1.0f/Height ); } }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		protected ShaderResource( GraphicsDevice device )
		{
			this.device = device;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		internal protected ShaderResource( GraphicsDevice device, ShaderResourceView srv, int w, int h, int d )
		{
			this.device = device;
			this.SRV	= srv;
			this.Width	= w;
			this.Height	= h;
			this.Depth	= d;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (SRV!=null) {
					SRV.Dispose();
				}
			}
			base.Dispose(disposing);
		}


		/// <summary>
		/// Makes SRgb format from UNorm or Typless.
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		internal protected DXGI.Format MakeSRgb ( DXGI.Format format )
		{
			if (format==DXGI.Format.B8G8R8A8_UNorm) return DXGI.Format.B8G8R8A8_UNorm_SRgb;
			if (format==DXGI.Format.R8G8B8A8_UNorm) return DXGI.Format.R8G8B8A8_UNorm_SRgb;
			if (format==DXGI.Format.B8G8R8X8_UNorm) return DXGI.Format.B8G8R8X8_UNorm_SRgb;
			if (format==DXGI.Format.BC1_UNorm) return DXGI.Format.BC1_UNorm_SRgb;
			if (format==DXGI.Format.BC2_UNorm) return DXGI.Format.BC2_UNorm_SRgb;
			if (format==DXGI.Format.BC3_UNorm) return DXGI.Format.BC3_UNorm_SRgb;
			if (format==DXGI.Format.BC7_UNorm) return DXGI.Format.BC7_UNorm_SRgb;
			if (format==DXGI.Format.B8G8R8A8_Typeless) return DXGI.Format.B8G8R8A8_UNorm_SRgb;
			if (format==DXGI.Format.R8G8B8A8_Typeless) return DXGI.Format.R8G8B8A8_UNorm_SRgb;
			if (format==DXGI.Format.B8G8R8X8_Typeless) return DXGI.Format.B8G8R8X8_UNorm_SRgb;
			if (format==DXGI.Format.BC1_Typeless) return DXGI.Format.BC1_UNorm_SRgb;
			if (format==DXGI.Format.BC2_Typeless) return DXGI.Format.BC2_UNorm_SRgb;
			if (format==DXGI.Format.BC3_Typeless) return DXGI.Format.BC3_UNorm_SRgb;
			if (format==DXGI.Format.BC7_Typeless) return DXGI.Format.BC7_UNorm_SRgb;
			return format;
		}


		/// <summary>
		/// Makes Typless format from UNorm or SRgb
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		internal protected DXGI.Format MakeTypeless ( DXGI.Format format )
		{
			if (format==DXGI.Format.B8G8R8A8_UNorm) return DXGI.Format.B8G8R8A8_Typeless;
			if (format==DXGI.Format.R8G8B8A8_UNorm) return DXGI.Format.R8G8B8A8_Typeless;
			if (format==DXGI.Format.B8G8R8X8_UNorm) return DXGI.Format.B8G8R8X8_Typeless;
			if (format==DXGI.Format.BC1_UNorm) return DXGI.Format.BC1_Typeless;
			if (format==DXGI.Format.BC2_UNorm) return DXGI.Format.BC2_Typeless;
			if (format==DXGI.Format.BC3_UNorm) return DXGI.Format.BC3_Typeless;
			if (format==DXGI.Format.BC7_UNorm) return DXGI.Format.BC7_Typeless;
			if (format==DXGI.Format.B8G8R8A8_UNorm_SRgb) return DXGI.Format.B8G8R8A8_Typeless;
			if (format==DXGI.Format.R8G8B8A8_UNorm_SRgb) return DXGI.Format.R8G8B8A8_Typeless;
			if (format==DXGI.Format.B8G8R8X8_UNorm_SRgb) return DXGI.Format.B8G8R8X8_Typeless;
			if (format==DXGI.Format.BC1_UNorm_SRgb) return DXGI.Format.BC1_Typeless;
			if (format==DXGI.Format.BC2_UNorm_SRgb) return DXGI.Format.BC2_Typeless;
			if (format==DXGI.Format.BC3_UNorm_SRgb) return DXGI.Format.BC3_Typeless;
			if (format==DXGI.Format.BC7_UNorm_SRgb) return DXGI.Format.BC7_Typeless;
			return format;
		}



		/// <summary>
		/// Calculates total number of mip levels
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
        public static int CalculateMipLevels(int width, int height = 0, int depth = 0)
        {
            int levels = 1;
            int size = Math.Max(Math.Max(width, height), depth);
            while (size > 1)
            {
                size = size / 2;
                levels++;
            }
            return levels;
        }



		/// <summary>
		/// Gets the mip size of resource of giveb size at given mip level.
		/// </summary>
		/// <param name="size">Original size</param>
		/// <param name="mipLevel">Mip level</param>
		/// <returns></returns>
		public static int GetMipSize ( int size, int mipLevel )
		{
			return Math.Max(1, size>>mipLevel);
		}



		/// <summary>
		/// Computes shader resource pitch
		/// </summary>
		/// <param name="format"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		public static int GetPitch( ColorFormat format, int width )
		{
			Debug.Assert(width > 0, "The width is negative!");

			int pitch;

			switch (format) {
				case ColorFormat.Dxt1:
				case ColorFormat.Dxt3:
				case ColorFormat.Dxt5:
					Debug.Assert(MathUtil.IsPowerOfTwo(width), "This format must be power of two!");
					pitch = ((width + 3) / 4) * Converter.SizeOf( format );
					break;

				default:
					pitch = width * Converter.SizeOf( format );
					break;
			};

			return pitch;
		}

	}
}
