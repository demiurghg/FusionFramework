using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Fusion.Core.Utils {

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

			GetWriter(eventType).Write("[{0:HH:mm:ss}] {1}> : ", eventCache.DateTime, eventCache.ThreadId );
			GetWriter(eventType).WriteLine();

			Console.ResetColor();
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args )
		{
			Colorize( eventType );
			
			GetWriter(eventType).Write("[{0:HH:mm:ss}] {1}> : ", eventCache.DateTime, eventCache.ThreadId );
			GetWriter(eventType).WriteLine( format, args );

			Console.ResetColor();
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message )
		{
			Colorize( eventType );
			
			GetWriter(eventType).Write("[{0:HH:mm:ss}] {1}> : ", eventCache.DateTime, eventCache.ThreadId );
			GetWriter(eventType).WriteLine( message );

			Console.ResetColor();
		}



		TextWriter GetWriter ( TraceEventType eventType )
		{
			if (eventType.HasFlag(TraceEventType.Error) || eventType.HasFlag(TraceEventType.Warning)) {
				return Console.Error;
			} else {
				return Console.Out;
			}
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
