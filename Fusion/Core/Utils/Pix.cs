using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Fusion {
	#if true

	[System.Security.SuppressUnmanagedCodeSecurity]
	internal static class SafeNativeMethods {
		[DllImport("d3d9.dll",
			EntryPoint = "D3DPERF_BeginEvent",
			CharSet = CharSet.Unicode,
			CallingConvention = CallingConvention.Winapi)]
		internal static extern int _BeginEvent(uint col, string wszName);

		[DllImport("d3d9.dll",
			EntryPoint = "D3DPERF_EndEvent",
			CallingConvention = CallingConvention.Winapi)]
		internal static extern int _EndEvent();

		[DllImport("d3d9.dll",
			EntryPoint = "D3DPERF_SetMarker",
			CharSet = CharSet.Unicode,
			CallingConvention = CallingConvention.Winapi)]
		internal static extern void SetMarker(uint col, string wszName);
	}

	public sealed class PixEvent : IDisposable {
		public PixEvent (string eventName = "...") {

			StackTrace	st = new StackTrace();

			StackFrame sf = st.GetFrame(1);

			string clsName = new string( sf.GetMethod().DeclaringType.Name.Where(ch=>char.IsUpper(ch)).ToArray() );
			SafeNativeMethods._BeginEvent( 0, clsName + "." + sf.GetMethod().Name + " - " + eventName );
		}

		public void Dispose () {
			SafeNativeMethods._EndEvent();
			//GC.SuppressFinalize(this);
		}
	}
	#else
	public sealed class PixEvent : IDisposable {
		
		public PixEvent (string eventName = "...") 
		{
		}

		public void Dispose () 
		{
			GC.SuppressFinalize(this);
		}
	}
	#endif
}
