using System;
using System.Diagnostics;
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
using System.Runtime.InteropServices;


namespace Fusion.Graphics {

	//[Obsolete]
	/*
	public class GenericBuffer : ShaderResource {
		int		byteStride;
		int		capacity;
			
		UnorderedAccessView	uav;
		D3D11.Buffer		bufferGpu;
		D3D11.Buffer		bufferStaging;



		public void SetCS_UAV ( int register ) { device.DeviceContext.ComputeShader.SetUnorderedAccessView( register, uav ); }


			
		public GenericBuffer( GraphicsDevice rs ) : base( rs )
		{
		}



		void Intialize<T,T2>( T type, T2[] data, int capacity, CpuAccessFlags  flagsCPU ) where T2 : struct
		{
			Debug.Assert( capacity > 0 );

			var isStructureBuffer = typeof( T ) != typeof( DXGI.Format );

			byteStride	=	isStructureBuffer ? Marshal.SizeOf( (System.Type)(object)type ) : (int)DXGI.FormatHelper.SizeOfInBytes( ( DXGI.Format)(object)type );
			this.capacity		=	capacity;
			Width	=	capacity;
			Height	=	0;
			Depth	=	0;

			//	create GPU buffer :
			var bufferDesc = new BufferDescription {
				BindFlags			= ( isStructureBuffer ? BindFlags.UnorderedAccess : 0 ) | BindFlags.ShaderResource,
				Usage				= ResourceUsage.Default,
				CpuAccessFlags		= CpuAccessFlags.None,
				OptionFlags			= isStructureBuffer ? ResourceOptionFlags.BufferStructured : 0,
				SizeInBytes			= capacity * byteStride,
				StructureByteStride = isStructureBuffer ? byteStride : 0
			};

			bufferGpu	=	 ( data == null ) ? new D3D11.Buffer( device.Device, bufferDesc ) : D3D11.Buffer.Create( device.Device, data, bufferDesc );

			if( flagsCPU != CpuAccessFlags.None ) {
				//	create staging buffer :
				bufferDesc = new BufferDescription {
					BindFlags			= BindFlags.None,
					Usage				= ResourceUsage.Staging,
					CpuAccessFlags		= flagsCPU,
					OptionFlags			= isStructureBuffer ? ResourceOptionFlags.BufferStructured : 0,
					SizeInBytes			= capacity * byteStride,
					StructureByteStride = isStructureBuffer ? byteStride : 0
				};
				bufferStaging	= new D3D11.Buffer( device.Device, bufferDesc );
			}

			if( isStructureBuffer ) {
				//	create UAV :
				var uavDesc = new UnorderedAccessViewDescription {
					Format		= DXGI.Format.Unknown,
					Dimension	= UnorderedAccessViewDimension.Buffer,
					Buffer		= new UnorderedAccessViewDescription.BufferResource { 
						ElementCount = capacity, 
						FirstElement = 0, 
						//Flags = UnorderedAccessViewBufferFlags.Append 
					}
				};
				uav	= new UnorderedAccessView(device.Device, bufferGpu, uavDesc);
			}

			//	create SRV :
			var srvDesc = new ShaderResourceViewDescription {
				Format		= isStructureBuffer ? DXGI.Format.Unknown : ( DXGI.Format)(object)type,
				Buffer		= { ElementCount = capacity },
				Dimension	= ShaderResourceViewDimension.Buffer
			};

			SRV	=	new ShaderResourceView( device.Device, bufferGpu, srvDesc );
		}



		internal static GenericBuffer Create<T>( GraphicsDevice rs, T type, int capacity, CpuAccessFlags flagsCPU )
		{
			var buffer = new GenericBuffer( rs );
			buffer.Intialize( type, (int[])null, capacity, flagsCPU );
			return buffer;
		}



		internal static GenericBuffer Create<T,T2>( GraphicsDevice rs, T type, T2[] data, CpuAccessFlags flagsCPU ) where T2 : struct
		{
			var buffer = new GenericBuffer( rs );
			buffer.Intialize( type, data, data.Length, flagsCPU );
			return buffer;
		}



		/// <summary>
		/// 
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SharpDX.Utilities.Dispose( ref uav );
				SharpDX.Utilities.Dispose( ref bufferGpu );
				SharpDX.Utilities.Dispose( ref bufferStaging );
			} 
			
			base.Dispose(disposing);
		}



		public void WriteData<T>( T[] src, int srcOffset = 0, int dstOffset = 0,  int elementCount = 0 ) where T : struct
		{
			elementCount = ( elementCount == 0 ) ? capacity : elementCount;

			Debug.Assert( ( bufferStaging != null ) && ( ( bufferStaging.Description.CpuAccessFlags & CpuAccessFlags.Write ) > 0 ) );
			Debug.Assert( byteStride == Marshal.SizeOf( typeof( T ) ) );
			Debug.Assert( ( srcOffset >= 0 ) && ( dstOffset >= 0 ) );
			Debug.Assert( ( srcOffset + elementCount ) <= src.Length );
			Debug.Assert( ( dstOffset + elementCount ) <= capacity );

			var db = device.DeviceContext.MapSubresource( bufferStaging, 0, MapMode.Write, D3D11.MapFlags.None );

			SharpDX.Utilities.Write( db.DataPointer, src, srcOffset, elementCount );

			device.DeviceContext.UnmapSubresource( bufferStaging, 0 );

			if( capacity == elementCount ) {
				device.DeviceContext.CopyResource( bufferStaging, bufferGpu );
			} else {
				device.DeviceContext.CopySubresourceRegion( bufferStaging, 0, new ResourceRegion( 0, 0, 0, elementCount * byteStride, 1, 1 ), bufferGpu, 0, dstOffset * byteStride );
			}
		}



		public void ReadData<T>( T[] dst, int dstOffset = 0, int srcOffset = 0, int elementCount = 0 ) where T : struct
		{
			elementCount = (elementCount == 0) ? capacity : elementCount;

			Debug.Assert( ( bufferStaging != null ) && ( ( bufferStaging.Description.CpuAccessFlags & CpuAccessFlags.Read ) > 0 ) );
			Debug.Assert( byteStride == Marshal.SizeOf( typeof( T ) ) );
			Debug.Assert( ( srcOffset >= 0 ) && ( dstOffset >= 0 ) );
			Debug.Assert( ( dstOffset + elementCount ) <= dst.Length );
			Debug.Assert( ( srcOffset + elementCount ) <= capacity );

			if( capacity == elementCount ) {
				device.DeviceContext.CopyResource(  bufferGpu, bufferStaging );
			} else {
				device.DeviceContext.CopySubresourceRegion( bufferGpu, 0, new ResourceRegion( srcOffset * byteStride, 0, 0, ( srcOffset + elementCount ) * byteStride, 1, 1 ), bufferStaging, 0 );
			}

			var db = device.DeviceContext.MapSubresource( bufferStaging, 0, MapMode.Read, D3D11.MapFlags.None );

			SharpDX.Utilities.Read( db.DataPointer, dst, dstOffset, elementCount );

			device.DeviceContext.UnmapSubresource( bufferStaging, 0 );

		}
	}*/

}
