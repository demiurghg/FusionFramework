using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Forms = System.Windows.Forms;

namespace Fusion {
	public static class ConsoleExt {

		static class NativeMethods {
			[DllImport("kernel32.dll", ExactSpelling = true)]
			public static extern IntPtr GetConsoleWindow();
			[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
			public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
			[DllImport("user32.dll")]
			public static extern Boolean SetForegroundWindow(IntPtr hWnd);

			[DllImport("kernel32.dll")]
			public static extern Boolean AllocConsole();
			[DllImport("kernel32.dll")]
			public static extern Boolean FreeConsole();
		}


		/// <summary>
		/// Shows console and makes it foreground
		/// </summary>
		public static void ShowConsole ()
		{
			NativeMethods.AllocConsole();
			BringToFront();
		}

			  
		/// <summary>
		/// Moves console window to specified position
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public static void Move( int x, int y ) {
			IntPtr	hCon	=	NativeMethods.GetConsoleWindow();
			NativeMethods.SetWindowPos( hCon, 0, x, y, 9999,9999, 0x0001);
		}


		/// <summary>
		/// Brings console window to front
		/// </summary>
		public static void BringToFront ()
		{
			IntPtr	hCon	=	NativeMethods.GetConsoleWindow();
			NativeMethods.SetForegroundWindow(hCon);
		}


		/// <summary>
		/// ???
		/// </summary>
		private static void RedirtectOutputToDebugWindow () 
		{
			if (Debugger.IsAttached) {
				Console.SetOut(new DebugWriter());   
			}
		}


		/// <summary>
		/// ???
		/// </summary>
		class DebugWriter : TextWriter
		{        
			public override void WriteLine(string value)
			{
				Debug.WriteLine(value);
				base.WriteLine(value);
			}

			public override void Write(string value)
			{
				Debug.Write(value);
				base.Write(value);
			}

			public override Encoding Encoding
			{
				get { return Encoding.Unicode; }
			}
		}
	}

}
