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


namespace Fusion.Graphics {
	public class VertexBuffer : DisposableBase {
		
		/// <summary>
		/// Vertex stride
		/// </summary>
		public int Stride { get; private set; }

		
		/// <summary>
		/// Capacity of this vertex buffer
		/// </summary>
		public int Capacity  { get; private set; }


		/// <summary>
		/// Capacity of this vertex buffer
		/// </summary>
		public bool IsVertexOutputEnabled  { get; private set; }


		/// <summary>
		/// Internal buffer
		/// </summary>
		internal	D3D11.Buffer	Buffer { get { return vertexBuffer; } }


		readonly GraphicsDevice device;
		D3D11.Buffer			vertexBuffer;



		/// <summary>
		/// Creates an instance of this object.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="capacity"></param>
		public VertexBuffer ( GraphicsDevice device, Type vertexType, int capacity, bool enableVertexOutput = false )
		{
			this.device			=	device;
			this.Capacity		=	capacity;
			this.IsVertexOutputEnabled	=	enableVertexOutput;

			Stride		=	Marshal.SizeOf( vertexType );

			if (enableVertexOutput) {
				if ((Stride/4)*4!=Stride) {
					throw new GraphicsException("Stride for vertex buffer with enabled vertex output must be multiple of 4.");
				}
			}

			BufferDescription	desc = new BufferDescription();

			if (enableVertexOutput) {
				desc.BindFlags	=	BindFlags.VertexBuffer | BindFlags.StreamOutput;
			} else {
				desc.BindFlags	=	BindFlags.VertexBuffer;
			}

			desc.CpuAccessFlags			=	enableVertexOutput ? CpuAccessFlags.None : CpuAccessFlags.Write;
			desc.OptionFlags			=	ResourceOptionFlags.None;
			desc.SizeInBytes			=	Capacity * Stride;
			desc.StructureByteStride	=	0;
			desc.Usage					=	enableVertexOutput ? ResourceUsage.Default : ResourceUsage.Dynamic;

			vertexBuffer				=	new D3D11.Buffer( device.Device, desc );
		}



		/// <summary>
		/// Creates vertex buffer
		/// </summary>
		/// <typeparam name="TVertex"></typeparam>
		/// <param name="device"></param>
		/// <param name="vertexData"></param>
		/// <returns></returns>
		public static VertexBuffer Create<TVertex> ( GraphicsDevice device, TVertex[] vertexData ) where TVertex: struct
		{
			var vb = new VertexBuffer( device, typeof(TVertex), vertexData.Length );
			vb.SetData( vertexData );
			return vb;
		}



		/// <summary>
		/// Immediately releases the unmanaged resources used by this object. 
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if (disposing) {
				vertexBuffer.Dispose();
			}
		 	base.Dispose(disposing);
		}



		/// <summary>
		/// Sets the vertex buffer data.
		/// </summary>
		/// <param name="data"></param>
		public void SetData<T> ( T[] data, int offset, int count ) where T: struct
		{
			if (IsVertexOutputEnabled) {
				throw new GraphicsException("Vertex buffer created with enabled vertex output can not be written.");
			}

			lock (device.DeviceContext) {
				var dataBox = device.DeviceContext.MapSubresource( vertexBuffer, 0, MapMode.WriteDiscard, D3D11.MapFlags.None );

				SharpDX.Utilities.Write( dataBox.DataPointer, data, offset, count );

				device.DeviceContext.UnmapSubresource( vertexBuffer, 0 );
			}
		}



		/// <summary>
		/// Sets the vertex buffer data.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void SetData<T>( T[] data ) where T: struct
		{
			SetData<T>( data, 0, data.Length );
		}
	}
}
