using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using Fusion.Core;

namespace Fusion.Drivers.Graphics {
	public class DepthStencilSurface : DisposableBase {

		public int			Width			{ get; private set; }
		public int			Height			{ get; private set; }
		public DepthFormat	Format			{ get; private set; }
		public int			SampleCount		{ get; private set; }

		internal	DepthStencilView	DSV	=	null;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="dsv"></param>
		internal DepthStencilSurface ( DepthStencilView dsv, DepthFormat format, int width, int height, int sampleCount )
		{
			Width			=	width;
			Height			=	height;
			Format			=	format;
			SampleCount		=	sampleCount;
			DSV				=	dsv;
		}



		/// <summary>
		/// Immediately releases the unmanaged resources used by this object.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref DSV );
			}

			base.Dispose( disposing );
		}
	}
}
