using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Specialized;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using System.Runtime.InteropServices;


namespace Fusion.Drivers.Graphics {
	public enum VertexBufferOptions {

		/// <summary>
		/// Fastest vertex buffer, slow update.
		/// </summary>
		Default,

		/// <summary>
		/// Enabled vertex output.
		/// </summary>
		VertexOutput,

		/// <summary>
		/// Fast vertex update.
		/// </summary>
		Dynamic,
		
	}
}
