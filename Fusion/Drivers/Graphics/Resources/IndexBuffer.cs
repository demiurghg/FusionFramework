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
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Fusion.Core;


namespace Fusion.Drivers.Graphics {
	public class IndexBuffer : DisposableBase {

		/// <summary>
		/// Capacity of this vertex buffer
		/// </summary>
		public int	Capacity { get { return capacity; } }


		internal	D3D11.Buffer	Buffer { get { return indexBuffer; } }

		readonly GraphicsDevice device;
		readonly int			capacity;
		D3D11.Buffer			indexBuffer;
		

		/// <summary>
		/// Creates an instance of this object.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="capacity"></param>
		public IndexBuffer ( GraphicsDevice device, int capacity )
		{
			//Log.Message("Creation: Index Buffer");

			this.device		=	device;
			this.capacity	=	capacity;

			BufferDescription	desc = new BufferDescription();

			desc.BindFlags				=	BindFlags.IndexBuffer;
			desc.CpuAccessFlags			=	CpuAccessFlags.Write;
			desc.OptionFlags			=	ResourceOptionFlags.None;
			desc.SizeInBytes			=	capacity * sizeof(int);
			desc.StructureByteStride	=	0;
			desc.Usage					=	ResourceUsage.Dynamic;

			lock (device.DeviceContext) {
				indexBuffer	=	new D3D11.Buffer( device.Device, desc );
			}
		}



		/// <summary>
		/// Creates buffer from given indices
		/// </summary>
		/// <param name="device"></param>
		/// <param name="indices"></param>
		/// <returns></returns>
		public static IndexBuffer Create ( GraphicsDevice device, int[] indices )
		{
			var ib = new IndexBuffer( device, indices.Length );
			ib.SetData( indices );
			return ib;
		}



		/// <summary>
		/// Immediately releases the unmanaged resources used by this object. 
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				indexBuffer.Dispose();
			}
			base.Dispose(disposing);
		}



		/// <summary>
		/// Copies array data to the index buffer.
		/// </summary>
		/// <param name="data"></param>
		public void SetData ( int[] data, int offset, int count )
		{
			lock ( device.DeviceContext ) {
				var dataBox = device.DeviceContext.MapSubresource( indexBuffer, 0, MapMode.WriteDiscard, D3D11.MapFlags.None );

				SharpDX.Utilities.Write( dataBox.DataPointer, data, offset, count );

				device.DeviceContext.UnmapSubresource( indexBuffer, 0 );
			}
		}



		/// <summary>
		/// Copies array data to the index buffer.
		/// </summary>
		/// <param name="data"></param>
		public void SetData ( int[] data )
		{
			SetData( data, 0, data.Length );
		}
	}
}
