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
	public class VertexBuffer : GraphicsResource {
		
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
		public VertexBufferOptions Options  { get; private set; }


		/// <summary>
		/// Internal buffer
		/// </summary>
		internal	D3D11.Buffer	Buffer { get { return vertexBuffer; } }


		D3D11.Buffer			vertexBuffer;



		/// <summary>
		/// Creates an instance of this object.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="capacity"></param>
		public VertexBuffer ( GraphicsDevice device, Type vertexType, int capacity, VertexBufferOptions options = VertexBufferOptions.Default ) : base(device)
		{
			//Log.Message("Creation: Vertex Buffer");

			this.Capacity	=	capacity;
			this.Options	=	options;

			Stride		=	Marshal.SizeOf( vertexType );

			BufferDescription	desc = new BufferDescription();

			if (options==VertexBufferOptions.Default) {

				desc.BindFlags				=	BindFlags.VertexBuffer;
				desc.CpuAccessFlags			=	CpuAccessFlags.None;
				desc.OptionFlags			=	ResourceOptionFlags.None;
				desc.SizeInBytes			=	Capacity * Stride;
				desc.StructureByteStride	=	0;
				desc.Usage					=	ResourceUsage.Default;

			} else if (options==VertexBufferOptions.VertexOutput) {

				if ((Stride/4)*4!=Stride) {
					throw new GraphicsException("Stride for vertex buffer with enabled vertex output must be multiple of 4.");
				}

				desc.BindFlags				=	BindFlags.VertexBuffer | BindFlags.StreamOutput;
				desc.CpuAccessFlags			=	CpuAccessFlags.None;
				desc.OptionFlags			=	ResourceOptionFlags.None;
				desc.SizeInBytes			=	Capacity * Stride;
				desc.StructureByteStride	=	0;
				desc.Usage					=	ResourceUsage.Default;

			} else if (options==VertexBufferOptions.Dynamic) {

				desc.BindFlags				=	BindFlags.VertexBuffer;
				desc.CpuAccessFlags			=	CpuAccessFlags.Write;
				desc.OptionFlags			=	ResourceOptionFlags.None;
				desc.SizeInBytes			=	Capacity * Stride;
				desc.StructureByteStride	=	0;
				desc.Usage					=	ResourceUsage.Dynamic;

			} else {
				throw new ArgumentException("options");
			}

			lock (device.DeviceContext) {
				vertexBuffer				=	new D3D11.Buffer( device.Device, desc );
			}
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
			if (Options==VertexBufferOptions.VertexOutput) {
				throw new GraphicsException("Vertex buffer created with enabled vertex output can not be written.");
			}

			lock (device.DeviceContext) {

				if (Options==VertexBufferOptions.Dynamic) {

					var dataBox = device.DeviceContext.MapSubresource( vertexBuffer, 0, MapMode.WriteDiscard, D3D11.MapFlags.None );

					SharpDX.Utilities.Write( dataBox.DataPointer, data, offset, count );

					device.DeviceContext.UnmapSubresource( vertexBuffer, 0 );
				} 
				else if (Options==VertexBufferOptions.Default) {

					var bufferDesc = new BufferDescription {
							BindFlags			= BindFlags.None,
							Usage				= ResourceUsage.Staging,
							CpuAccessFlags		= CpuAccessFlags.Write | CpuAccessFlags.Read,
							OptionFlags			= ResourceOptionFlags.None,
							SizeInBytes			= Capacity * Stride,
						};

					var bufferStaging		= new D3D11.Buffer(device.Device, bufferDesc);

					var dataBox = device.DeviceContext.MapSubresource( bufferStaging, 0, MapMode.Write, D3D11.MapFlags.None );

					SharpDX.Utilities.Write( dataBox.DataPointer, data, offset, count );

					device.DeviceContext.UnmapSubresource( bufferStaging, 0 );

					device.DeviceContext.CopyResource( bufferStaging, vertexBuffer );

					SafeDispose( ref bufferStaging );
				}
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
