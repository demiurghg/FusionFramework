using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using Fusion.Core;

namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// Represenst single rendering surface for render targets.
	/// 
	/// Never dispose RenderTargetSurface. 
	/// I always will be disposed by owning object.
	/// </summary>
	public class RenderTargetSurface : DisposableBase {

		public int			Width			{ get; private set; }
		public int			Height			{ get; private set; }
		public ColorFormat	Format			{ get; private set; }
		public int			SampleCount		{ get; private set; }

		internal	UnorderedAccessView	UAV =	null;
		internal	RenderTargetView	RTV	=	null;
		internal	Resource			Resource = null;
		internal	int					Subresource = 0;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="rtv"></param>
		internal RenderTargetSurface ( RenderTargetView rtv, UnorderedAccessView uav, Resource resource, int subresource, ColorFormat format, int width, int height, int sampleCount )
		{
			Width			=	width;
			Height			=	height;
			Format			=	format;
			SampleCount		=	sampleCount;
			RTV				=	rtv;
			UAV				=	uav;
			Resource		=	resource;
			Subresource		=	subresource;
		}



		/// <summary>
		/// Disposes
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref RTV );
				SafeDispose( ref UAV );
			}

			base.Dispose( disposing );
		}
	}
}
