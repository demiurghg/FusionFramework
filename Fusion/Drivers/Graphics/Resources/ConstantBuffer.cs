//#define USE_DYNAMIC_CB

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DXGI = SharpDX.DXGI;
using SharpDX;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;
using System.Runtime.InteropServices;


namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// Wrapper for constant data and buffer
	/// </summary>
	/// <typeparam name="ConstDataT"></typeparam>
	public class ConstantBuffer : GraphicsResource {
			
		internal	D3D11.Buffer	buffer;

		int bufferSizeInBytes;

		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rs"></param>
		public ConstantBuffer( GraphicsDevice device, int sizeInBytes ) : base(device) 
		{
			Create( sizeInBytes );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="underlayingType"></param>
		public ConstantBuffer ( GraphicsDevice device, Type dataType ) : base(device)
		{
			Create( Marshal.SizeOf( dataType ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="underlayingType"></param>
		public ConstantBuffer ( GraphicsDevice device, Type dataType, int count ) : base(device)
		{
			if (count<1) {
				throw new ArgumentOutOfRangeException("count must be greater than zero");
			}
			Create( Marshal.SizeOf( dataType ) * count );
		}



		void CheckStructLayout ()
		{
			
		}



		
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="device"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static ConstantBuffer Create<T>( GraphicsDevice device, T data ) where T: struct
		{
			var cb = new ConstantBuffer( device, typeof(T) );
			cb.SetData( data );
			return cb;
		}



		
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="device"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static ConstantBuffer Create<T>( GraphicsDevice device, T[] data ) where T: struct
		{
			var cb = new ConstantBuffer( device, typeof(T), data.Length );
			cb.SetData( data );
			return cb;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sizeInBytes"></param>
		void Create ( int sizeInBytes )
		{
			int size 	=	sizeInBytes;

			int padSize	=	size % 16 == 0 ? size : (size/16 * 16) + 16;

			if (size!=padSize) {
				throw new ArgumentException(string.Format("Size of constant buffer in bytes must be padded by 16 bytes ({0} instead of {1})", padSize, size));
			}// */

			size	=	padSize;

			bufferSizeInBytes	=	size;

			#if USE_DYNAMIC_CB
				buffer = new D3D11.Buffer( device.Device, size, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0 );
			#else
				buffer = new D3D11.Buffer( device.Device, size, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0 );
			#endif
		}



		/// <summary>
		/// Disposes buffer
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref buffer );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="?"></param>
		public void SetData<T> ( T value ) where T: struct
		{
			#if USE_DYNAMIC_CB
				var db = device.DeviceContext.MapSubresource( buffer, 0, MapMode.WriteDiscard, D3D11.MapFlags.None );
				Marshal.StructureToPtr( value, db.DataPointer, false );
				device.DeviceContext.UnmapSubresource( buffer, 0 );
			#else
				lock (device.DeviceContext) {
					device.DeviceContext.UpdateSubresource( ref value, buffer );
				}
			#endif
		}

		


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="?"></param>
		public void SetData<T> ( T[] data ) where T: struct
		{
			if ( bufferSizeInBytes!=Marshal.SizeOf(typeof(T)) * data.Length ) {
				throw new ArgumentException("Size of argument data must be equal to constant buffer size");
			}

			lock (device.DeviceContext) {
				device.DeviceContext.UpdateSubresource( data, buffer );
			}
		}

	}
}
