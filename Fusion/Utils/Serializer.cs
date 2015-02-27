using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpDX;
using System.Runtime.InteropServices;


namespace Fusion {

	internal sealed class Serializer<T> : IDisposable where T : struct {

		byte[] buffer;
		GCHandle handle;
		DataStream dataStream;
		int elementCount;



		public T Read() 
		{
			return dataStream.Read<T>();
		}



		public T[] ReadRange() 
		{
			return dataStream.ReadRange<T>( elementCount );
		}



		public Serializer( byte[] buffer ) 
		{
			this.buffer = buffer;
			elementCount = buffer.Length / Marshal.SizeOf( typeof( T ) );
			handle = GCHandle.Alloc( buffer, GCHandleType.Pinned );
			dataStream = new DataStream( handle.AddrOfPinnedObject(), buffer.Length, true, false );
		}



		public void Dispose()
		{
			dataStream.Dispose();
			handle.Free();
		}
	}


	public static class SerializerExtensions {

		static void Write<T>( BinaryWriter writer, object src, int elementCount )
		{
			var size = elementCount * Marshal.SizeOf( typeof( T ) );
			var buffer = new byte[ size ];
			var handle = GCHandle.Alloc( src, GCHandleType.Pinned );
			var ds = new DataStream( handle.AddrOfPinnedObject(), size, true, false );
			ds.ReadRange( buffer, 0, size );
			writer.Write( buffer );
			ds.Dispose();
			handle.Free();
		}



		public static void Write<T>( this BinaryWriter writer, T structure ) where T : struct
		{
			Write<T>( writer, structure, 1 );
		}



		public static void Write<T>( this BinaryWriter writer, T[] array ) where T : struct
		{
			Write<T>( writer, array, array.Length );
		}



		public static T[] Read<T> ( this BinaryReader reader, int count ) where T : struct
		{
			var buffer			= reader.ReadBytes( count * Marshal.SizeOf(typeof(T)) );
			var elementCount	= count;
			var handle			= GCHandle.Alloc( buffer, GCHandleType.Pinned );
			var dataStream		= new DataStream( handle.AddrOfPinnedObject(), buffer.Length, true, false );
			
			var range		= dataStream.ReadRange<T>( elementCount );

			dataStream.Dispose();
			handle.Free();

			return range;
		}



		public static T Read<T> ( this BinaryReader reader ) where T : struct
		{
			var size	=	Marshal.SizeOf( typeof( T ) );
			var bytes	=	reader.ReadBytes( size ); 

			var handle	=	GCHandle.Alloc( bytes, GCHandleType.Pinned );

			T structure	=	(T)Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof(T) );

			handle.Free();
			
			return structure;
		}
	}
}
