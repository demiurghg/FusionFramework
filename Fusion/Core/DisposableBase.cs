using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core {
	public class DisposableBase : IDisposable {
		
		/// <summary>
		/// Flag: Has Dispose already been called?
		/// </summary>
		public bool IsDisposed { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		public void Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose ( bool disposing )
		{
			if (IsDisposed) {
				return;
			}

			if (disposing) {
				//	dispose managed stuff
			}

			//	dispose unmanaged stuff

			//	mark as disposed
			IsDisposed	=	true;
		}



		~DisposableBase()
		{
			Dispose( false );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public static void SafeDispose<T> ( ref T obj ) where T: IDisposable
		{
			if ( obj != null ) {
				obj.Dispose();
				obj = default(T);
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		public static void SafeDispose<T> ( ref T[] objArray ) where T: IDisposable
		{
			if ( objArray==null ) {
				return;
			}

			foreach ( var obj in objArray ) {
				if (obj!=null) {
					obj.Dispose();
				}
			}

			objArray	=	null;
		}
	}
}
