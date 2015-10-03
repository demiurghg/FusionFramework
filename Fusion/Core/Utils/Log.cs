using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;
using CC = System.ConsoleColor;
using Fusion.Core.Mathematics;


namespace Fusion {
	public static class Log {

		/// <summary>
		/// Verbocoty level
		/// false - Only Initializing, Loading, Disposing and Processing.
		/// true - Additional info, tools output etc.
		/// </summary>
		static public bool Verbocity	=	false;



		/// <summary>
		/// Trace.TraceInformation
		/// </summary>
		/// <param name="message"></param>
		public static void Message ( string message )
		{
			Trace.TraceInformation( message );
		}



		/// <summary>
		/// Trace.TraceInformation
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Message ( string format, params object[] args )
		{
			Trace.TraceInformation( format, args );
		}



		/// <summary>
		///	Trace.TraceWarning
		/// </summary>
		/// <param name="message"></param>
		public static void Warning ( string message )
		{
			Trace.TraceWarning( message );
		}



		/// <summary>
		/// Trace.TraceWarning
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Warning ( string format, params object[] args )
		{
			Trace.TraceWarning( format, args );
		}



		/// <summary>
		/// Trace.TraceError
		/// </summary>
		/// <param name="message"></param>
		public static void Error ( string message )
		{
			Trace.TraceError( message );
		}



		/// <summary>
		/// Trace.TraceError
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Error ( string format, params object[] args )
		{
			Trace.TraceError( format, args );
		}



		/// <summary>
		/// Debug.Print
		/// </summary>
		/// <param name="message"></param>
		public static void Debug ( string message )
		{	
			System.Diagnostics.Debug.Print( message );
		}



		/// <summary>
		/// Debug.Print
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Debug ( string format, params object[] args )
		{	
			System.Diagnostics.Debug.Print( format, args );
		}



		[Obsolete("Use Log.Message()", true)]
		public static void Information ( string format, params object[] args ) {}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		public static void Dump ( byte[] array )
		{
			Trace.WriteLine( "---------------------------------------------------------------------");
			Trace.WriteLine( string.Format("Dump: {0} bytes ({0:X8})", array.Length) );

			for (int i=0; i<MathUtil.IntDivRoundUp( array.Length, 16 ); i++) {

				int count	=	Math.Min(16, array.Length - i * 16);

				string hex	= "";
				string txt  = "";
				
				for (int j=0; j<count; j++) {
					
					var b  = array[i*16+j];
					var ch = (char)b;
					hex += b.ToString("x2");

					if (char.IsControl(ch)) {
						txt += ".";
					} else {
						txt += ch;
					}

					if (j==3||j==7||j==11) {
						hex += "  ";
					} else {
						hex += " ";
					}
				}

				Trace.WriteLine( string.Format("{0,-51}| {1}", hex, txt) );
			}

			Trace.WriteLine( "---------------------------------------------------------------------");
		}
	}
}
