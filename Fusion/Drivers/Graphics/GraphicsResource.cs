using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using Native.NvApi;
using Device = SharpDX.Direct3D11.Device;
using System.IO;
using Fusion.Core;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Graphics.Display;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {

	public class GraphicsResource : DisposableBase {

		/// <summary>
		/// Gets the GraphicsDevice associated with this GraphicsResource.
		/// </summary>
		public GraphicsDevice GraphicsDevice {
			get {
				return device;
			}
		}				


		protected readonly GraphicsDevice device;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		public GraphicsResource ( GraphicsDevice device )
		{
			this.device	=	device;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			base.Dispose( disposing );
		}
	}
}
