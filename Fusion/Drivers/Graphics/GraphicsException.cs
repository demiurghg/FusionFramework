using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
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


namespace Fusion.Drivers.Graphics {

	[Serializable]
	public class GraphicsException : Exception {

		public GraphicsException ()
		{
		}
		
		public GraphicsException ( string message ) : base( message )
		{
		}

		public GraphicsException ( string format, params object[] args ) : base( string.Format(format, args) )
		{
			
		}

		public GraphicsException ( string message, Exception inner ) : base( message, inner )
		{
		}
	}
}
