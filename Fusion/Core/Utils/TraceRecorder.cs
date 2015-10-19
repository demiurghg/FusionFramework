using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Fusion.Core.Utils {		 
	
	/// <summary>
	/// Trace recorder
	/// </summary>
	public class TraceRecorder : TraceListener {

		public class Line {
			public readonly TraceEventType EventType;
			public readonly string Message;

			public Line ( TraceEventType eventType, string message ) 
			{
				EventType	=	eventType;
				Message		=	message;
			}
		}


		public static event EventHandler	TraceRecorded;


		/// <summary>
		/// Max recorded line count
		/// </summary>
		public static int MaxLineCount {
			get;
			set;
		}
		
		static List<Line> lines = new List<Line>();



		/// <summary>
		/// Recorded lines.
		/// </summary>
		public static IEnumerable<Line> Lines { get { return lines; } }
		


		void NotifyTraceRecord ()
		{
			if (TraceRecorded!=null) {
				TraceRecorded(null, EventArgs.Empty);
			}
		}



		public override void Fail ( string message )
		{
			base.Fail( message );
			NotifyTraceRecord();
		}


		public override void Fail ( string message, string detailMessage )
		{
			base.Fail( message, detailMessage );
			NotifyTraceRecord();
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id )
		{
			lines.Add( new Line( eventType, "" ) );
			NotifyTraceRecord();
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args )
		{
			lines.Add( new Line( eventType, string.Format( format, args ) ) );
			NotifyTraceRecord();
		}


		public override void TraceEvent ( TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message )
		{
			lines.Add( new Line( eventType, message ) );
			NotifyTraceRecord();
		}



		public override void Write ( string message )
		{
			lines.Add( new Line( TraceEventType.Information, message ) );
			NotifyTraceRecord();
		}

		public override void WriteLine ( string message )
		{
			lines.Add( new Line( TraceEventType.Information, message ) );
			NotifyTraceRecord();
		}
	}
}
