using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Fusion {
	public class ColoredTraceListener : TraceListener {

		public override void Fail ( string message )
		{
			base.Fail( message );
		}


		public override void Fail ( string message, string detailMessage )
		{
			base.Fail( message, detailMessage );
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id )
		{
			Colorize( eventType );
			Console.Write("[{0:HH:mm:ss}] {1}> : ", eventCache.DateTime, eventCache.ThreadId );
			Console.WriteLine();
			Console.ResetColor();
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args )
		{
			Colorize( eventType );
			Console.Write("[{0:HH:mm:ss}] {1}> : ", eventCache.DateTime, eventCache.ThreadId );
			Console.WriteLine( format, args );

			if (eventType.HasFlag(TraceEventType.Error) || eventType.HasFlag(TraceEventType.Warning)) {
				Console.Error.WriteLine( format, args );
			}

			Console.ResetColor();
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message )
		{
			Colorize( eventType );
			Console.Write("[{0:HH:mm:ss}] {1}> : ", eventCache.DateTime, eventCache.ThreadId );
			Console.WriteLine( message );

			if (eventType.HasFlag(TraceEventType.Error) || eventType.HasFlag(TraceEventType.Warning)) {
				Console.Error.WriteLine( message );
			}

			Console.ResetColor();
		}



		void Colorize ( TraceEventType eventType )
		{
			if (eventType.HasFlag( TraceEventType.Critical )) {
				Console.ForegroundColor	=	ConsoleColor.White;
				Console.BackgroundColor	=	ConsoleColor.Red;
			} else
			if (eventType.HasFlag( TraceEventType.Error )) {
				Console.ForegroundColor	=	ConsoleColor.Red;
				Console.BackgroundColor	=	ConsoleColor.Black;
			} else
			if (eventType.HasFlag( TraceEventType.Warning )) {
				Console.ForegroundColor	=	ConsoleColor.Yellow;
				Console.BackgroundColor	=	ConsoleColor.Black;
			} else
			if (eventType.HasFlag( TraceEventType.Information )) {
				Console.ForegroundColor	=	ConsoleColor.White;
				Console.BackgroundColor	=	ConsoleColor.Black;
			} else
			if (eventType.HasFlag( TraceEventType.Verbose )) {
				Console.ForegroundColor	=	ConsoleColor.Gray;
				Console.BackgroundColor	=	ConsoleColor.Blue;
			} else {
				Console.ForegroundColor	=	ConsoleColor.White;
				Console.BackgroundColor	=	ConsoleColor.Black;
			}
		}


		public override void Write ( string message )
		{
			Console.Write( message );
		}

		public override void WriteLine ( string message )
		{
			Console.WriteLine( message );
		}

	}
}
