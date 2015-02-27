using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using SharpDX.DXGI;
using SharpDX.Windows;
using System.Runtime.InteropServices;
using Fusion.Mathematics;


namespace Fusion.Graphics {

	/// <summary>
	/// 
	/// </summary>
	public class VertexOutputLayout : DisposableBase {

		readonly GraphicsDevice	device;

		StreamOutputElement[] outputElements;
		int rasterizedStream;
		int[] buferredStrides;

		Dictionary<GeometryShader, D3D11.GeometryShader> shaders;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="elements"></param>
		/// <param name="rasterizedStream">Stream to send to rasterizer stage. Negative value means no stream to send.</param>
		public VertexOutputLayout ( GraphicsDevice device, VertexOutputElement[] elements, int rasterizedStream = -1 )
		{
			this.device				=	device;
			this.rasterizedStream	=	(rasterizedStream < 0) ? -1 : rasterizedStream;

			shaders	=	new Dictionary<GeometryShader,D3D11.GeometryShader>();

			outputElements	=	elements
				.Select( e => new StreamOutputElement( e.Stream, e.SemanticName, e.SemanticIndex, e.StartComponent, e.ComponentCount, e.OutputSlot ) )
				.ToArray();


			//			
			//	calculate buferred strides :
			//
			int maxBuffers = outputElements.Max( oe => oe.OutputSlot )+1;
			buferredStrides = new int[ maxBuffers ];

			for (int i=0; i<maxBuffers; i++) {
				buferredStrides[i] = outputElements	
									.Where( oe1 => oe1.OutputSlot==i )
									.Sum( oe2 => oe2.ComponentCount	) * 4;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		internal D3D11.GeometryShader GetStreamOutputGeometryShader ( GeometryShader gs )
		{
			D3D11.GeometryShader sogs;

			if (!shaders.TryGetValue( gs, out sogs )) {
				
				sogs = new D3D11.GeometryShader( device.Device, Misc.HexStringToByte( gs.Bytecode ), outputElements, buferredStrides, rasterizedStream );

				shaders.Add( gs, sogs );
			}

			return sogs;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				foreach (var entry in shaders) {	
					entry.Value.Dispose();
				}
			}
			base.Dispose( disposing );
		}
	}
}
