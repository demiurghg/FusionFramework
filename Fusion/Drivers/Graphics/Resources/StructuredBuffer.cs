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
using Buffer = SharpDX.Direct3D11.Buffer;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using System.Runtime.InteropServices;


namespace Fusion.Drivers.Graphics {

	public class StructuredBuffer : ShaderResource {

		/// <summary>
		/// Structure stride in bytes
		/// </summary>
		public int StructureStride		{ get; private set; }

		/// <summary>
		/// Total amount of structures in buffer
		/// </summary>
		public int StructureCapacity  { get; private set; }


		internal UnorderedAccessView	UAV				{ get { return uav; } }
		internal Buffer					BufferGPU		{ get { return bufferGpu; } }
		internal Buffer					BufferStaging	{ get { return bufferStaging; } }
			
		UnorderedAccessView	uav;
		Buffer				bufferGpu;
		Buffer				bufferStaging;
		Buffer				bufferCount;


			
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="elementCount"></param>
		/// <param name="elementType"></param>
		public StructuredBuffer ( GraphicsDevice device, int structureStride, int structureCount, StructuredBufferFlags flags) : base(device)
		{
			Create( device, structureStride, structureCount, flags );
		}


			
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="elementCount"></param>
		/// <param name="elementType"></param>
		public StructuredBuffer ( GraphicsDevice device, Type structureType, int structureCount, StructuredBufferFlags flags ) : base(device)
		{
			Create( device, Marshal.SizeOf(structureType), structureCount, flags );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="structureStride"></param>
		/// <param name="structureCount"></param>
		void Create ( GraphicsDevice device, int structureStride, int structureCount, StructuredBufferFlags flags )
		{
			StructureCapacity	=	structureCount;
			StructureStride		=	structureStride;

			Width		=	structureCount;
			Height		=	0;
			Depth		=	0;

			//	create staging buffer :
			var bufferDesc = new BufferDescription {
					BindFlags			= BindFlags.None,
					Usage				= ResourceUsage.Staging,
					CpuAccessFlags		= CpuAccessFlags.Write | CpuAccessFlags.Read,
					OptionFlags			= ResourceOptionFlags.BufferStructured,
					SizeInBytes			= structureCount * structureStride,
					StructureByteStride = structureStride
				};

			bufferStaging		= new Buffer(device.Device, bufferDesc);



			//	create count buffer :
			bufferDesc = new BufferDescription {
					BindFlags			= BindFlags.None,
					Usage				= ResourceUsage.Staging,
					CpuAccessFlags		= CpuAccessFlags.Write | CpuAccessFlags.Read,
					OptionFlags			= ResourceOptionFlags.None,
					SizeInBytes			= 4,
				};

			bufferCount		= new Buffer(device.Device, bufferDesc);


			//	create GPU buffer :
			bufferDesc = new BufferDescription {
					BindFlags			= BindFlags.UnorderedAccess | BindFlags.ShaderResource,
					Usage				= ResourceUsage.Default,
					CpuAccessFlags		= CpuAccessFlags.None,
					OptionFlags			= ResourceOptionFlags.BufferStructured,
					SizeInBytes			= structureCount * structureStride,
					StructureByteStride = structureStride
				};

			bufferGpu		= new Buffer(device.Device, bufferDesc);


			var uavFlag = UnorderedAccessViewBufferFlags.None;

			if ( flags==StructuredBufferFlags.None	  ) { uavFlag = UnorderedAccessViewBufferFlags.None;	 } 
			if ( flags==StructuredBufferFlags.Append  ) { uavFlag = UnorderedAccessViewBufferFlags.Append;	 } 
			if ( flags==StructuredBufferFlags.Counter ) { uavFlag = UnorderedAccessViewBufferFlags.Counter; } 

			//	create UAV :
			var uavDesc = new UnorderedAccessViewDescription {
					Format		= DXGI.Format.Unknown,
					Dimension	= UnorderedAccessViewDimension.Buffer,
					Buffer		= new UnorderedAccessViewDescription.BufferResource { 
						ElementCount = structureCount, 
						FirstElement = 0, 
						Flags = uavFlag
					}
				};

			uav	= new UnorderedAccessView(device.Device, BufferGPU, uavDesc);


			//	create SRV :
			var srvDesc = new ShaderResourceViewDescription {
					Format		= DXGI.Format.Unknown,
					Buffer		= {ElementCount = structureCount, FirstElement = 0 },
					Dimension	= ShaderResourceViewDimension.Buffer
				};

			SRV	=	new ShaderResourceView( device.Device, BufferGPU, srvDesc );
		}



		/// <summary>
		/// Disposes
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref SRV );
				SafeDispose( ref uav );
				SafeDispose( ref bufferGpu );
				SafeDispose( ref bufferStaging );
				SafeDispose( ref bufferCount );
			} 
			
			base.Dispose(disposing);
		}



		/// <summary>
		/// Sets structured buffer data
		/// </summary>
		public void SetData<T> ( T[] data, int startIndex, int elementCount ) where T: struct
		{
			if (data==null) {
				throw new ArgumentNullException("data");
			}

			if (data.Length < startIndex + elementCount) {
				throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " elements have been requested."); 
			}

			int inputBytes	=	elementCount * Marshal.SizeOf(typeof(T));
			int bufferBytes =	StructureCapacity * StructureStride;

			if ( inputBytes > bufferBytes ) {
				throw new ArgumentException("Output data (" + inputBytes.ToString() + " bytes) exceeded buffer size (" + bufferBytes.ToString() + " bytes)"); 
			}


			//
			//	Write data
			//
			lock (device.DeviceContext ) {
				var db = device.DeviceContext.MapSubresource( bufferStaging, 0, MapMode.Write, D3D11.MapFlags.None );

				SharpDX.Utilities.Write( db.DataPointer, data, startIndex, elementCount );

				device.DeviceContext.UnmapSubresource( bufferStaging, 0 );

				device.DeviceContext.CopyResource( bufferStaging, bufferGpu );
			}
		}



		/// <summary>
		/// Sets structured buffer data
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void SetData<T>( T[] data ) where T: struct 
		{
			SetData<T>( data, 0, data.Length );
		}



		/// <summary>
		/// Gets structured buffer data
		/// </summary>
		public void GetData<T> ( T[] data, int startIndex, int elementCount ) where T: struct
		{
			if (data==null) {
				throw new ArgumentNullException("data");
			}

			if (data.Length < startIndex + elementCount) {
				throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " elements have been requested."); 
			}

			int inputBytes	=	elementCount * Marshal.SizeOf(typeof(T));
			int bufferBytes =	StructureCapacity * StructureStride;
			if ( inputBytes > bufferBytes ) {
				throw new ArgumentException("Input data (" + inputBytes.ToString() + " bytes) exceeded buffer size (" + bufferBytes.ToString() + " bytes)"); 
			}


			//
			//	Read data
			//	
			lock (device.DeviceContext) {
				device.DeviceContext.CopyResource( bufferGpu, bufferStaging );

				var db = device.DeviceContext.MapSubresource( bufferStaging, 0, MapMode.Read, D3D11.MapFlags.None );

				SharpDX.Utilities.Read( db.DataPointer, data, 0, data.Length );

				device.DeviceContext.UnmapSubresource( bufferStaging, 0 );
			}
		}



		/// <summary>
		/// Gets structured buffer data
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		public void GetData<T> ( T[] data ) where T: struct 
		{
			GetData<T>( data, 0, data.Length );
		}



		/// <summary>
		/// Copies structure count to constant buffer with given byte offset
		/// </summary>
		/// <param name="constantBuffer"></param>
		/// <param name="offset"></param>
		public void CopyStructureCount ( ConstantBuffer constantBuffer, int dstByteOffset )
		{
			device.DeviceContext.CopyStructureCount( constantBuffer.buffer, dstByteOffset, UAV );
		}



		/// <summary>
		/// Gets structure count.
		/// <remarks>
		/// This method can cause significant performance hit.
		/// </remarks>
		/// </summary>
		/// <returns></returns>
		public int GetStructureCount ()
		{
			int count = 1;

			device.DeviceContext.CopyStructureCount( bufferCount, 0, UAV );

			var db = device.DeviceContext.MapSubresource( bufferCount, 0, MapMode.Read, D3D11.MapFlags.None );

			SharpDX.Utilities.Read( db.DataPointer, ref count );

			device.DeviceContext.UnmapSubresource( bufferCount, 0 );

			return count;
		}
	}
}
