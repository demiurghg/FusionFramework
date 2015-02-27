using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;
using CC = System.ConsoleColor;
using Fusion.Mathematics;


namespace Fusion {
	public static class Log {

		static object lockObject = new object();

		public struct LogMessage {
			public string		Message;
			public MessageType	Type;	
		}

		static List<LogMessage> logLines = new List<LogMessage>();


		/// <summary>
		/// Message type
		/// </summary>
		public enum MessageType {
			Message,
			Debug,
			Information,
			Warning,
			Error,
			Dump,
		}



		/// <summary>
		/// Verbocoty level
		/// false - Only Initializing, Loading, Disposing and Processing.
		/// true - Additional info, tools output etc.
		/// </summary>
		static public bool Verbocity	=	false;



		/// <summary>
		/// Message callback
		/// </summary>
		/// <param name="messageType"></param>
		/// <param name="message"></param>
		public delegate void MessageCallback ( MessageType messageType, string message );



		/// <summary>
		/// Returns list of lines
		/// </summary>
		public static ICollection<LogMessage> LogLines {
			get {
				return logLines;
			}
		}


		static List<MessageCallback>	messageCallbacks = new List<MessageCallback>();



		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public static void AddMessageCallback ( MessageCallback callback, MessageCallback callbackHistory )
		{
			lock (lockObject) {
				messageCallbacks.Add( callback );

				foreach ( var line in logLines ) {
					callbackHistory( line.Type, line.Message );
				}
			}
		}


	
		/// <summary>
		/// Write line
		/// </summary>
		/// <param name="frmt"></param>
		/// <param name="args"></param>
		private static void WriteLine ( MessageType type, string frmt, params object[] args )
		{
			if ( frmt==null ) {
				return;
			}

			lock (lockObject) {
				
				if (type==MessageType.Message) {
					Console.ForegroundColor	=	ConsoleColor.White;
					Console.BackgroundColor	=	ConsoleColor.Black;
				}
				if (type==MessageType.Debug) {
					Console.ForegroundColor	=	ConsoleColor.DarkGray;
					Console.BackgroundColor	=	ConsoleColor.Black;
				}
				if (type==MessageType.Information) {
					Console.ForegroundColor	=	ConsoleColor.White;
					Console.BackgroundColor	=	ConsoleColor.Blue;
				}
				if (type==MessageType.Warning) {
					Console.ForegroundColor	=	ConsoleColor.Yellow;
					Console.BackgroundColor	=	ConsoleColor.Black;
				}
				if (type==MessageType.Error) {
					Console.ForegroundColor	=	ConsoleColor.Red;
					Console.BackgroundColor	=	ConsoleColor.Black;
				}
				if (type==MessageType.Dump) {
					Console.ForegroundColor	=	ConsoleColor.DarkCyan;
					Console.BackgroundColor	=	ConsoleColor.Black;
				}


				var date = string.Format( "[{0:HH:mm:ss}] ", DateTime.Now );
				var message = string.Format( frmt, args );
				var lines = message.Split(new[]{"\r\n", "\n"}, StringSplitOptions.None );


				foreach ( var line in lines ) {
					logLines.Add( new LogMessage(){ Message = line, Type = type } );
					Console.WriteLine( date + line );
				}

				//System.Diagnostics.Debug.WriteLine( date + frmt, args );

				Console.ResetColor();

				foreach ( var cb in messageCallbacks ) {
					cb( type, message );
				}
			}
		}



		/// <summary>
		/// Simple message that gives developer common information
		/// about program execution
		/// </summary>
		/// <param name="frmt"></param>
		/// <param name="args"></param>
		public static void Message ( string frmt, params object[] args )
		{	
			WriteLine( MessageType.Message, frmt, args );
		}



		/// <summary>
		/// Information message that can be noticed by developer/user.
		/// Will be highlighted in neautral manner
		/// </summary>
		/// <param name="frmt"></param>
		/// <param name="args"></param>
		public static void Information ( string frmt, params object[] args )
		{	
			WriteLine( MessageType.Information, frmt, args );
		}



		/// <summary>
		/// Empty message
		/// </summary>
		public static void Message ()
		{	
			Message("");
		}



		/// <summary>
		/// Debug message that gives user/developer more deeper information about execution flow.
		/// But usually uneccessary.
		/// </summary>
		/// <param name="frmt"></param>
		/// <param name="args"></param>
		public static void Debug ( string frmt, params object[] args )
		{	
			if (Verbocity) {
				WriteLine( MessageType.Debug, frmt, args );
			}
		}



		/// <summary>
		/// Print message about something that goes wrong 
		/// but nothing else broke and program continues its application.
		/// </summary>
		/// <param name="frmt"></param>
		/// <param name="args"></param>
		public static void Warning ( string frmt, params object[] args )
		{	
			if (frmt==null) {
				return;
			}

			WriteLine( MessageType.Warning, frmt, args );
		}



		/// <summary>
		/// Pring message abount something that goes wrong and can interrupt normal execution.
		/// </summary>
		/// <param name="frmt"></param>
		/// <param name="args"></param>
		public static void Error ( string frmt, params object[] args )
		{	
			if (frmt==null) {
				return;
			}

			WriteLine( MessageType.Error, frmt, args );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		public static void Dump ( byte[] array )
		{

			WriteLine( MessageType.Dump, "---------------------------------------------------------------------");
			WriteLine( MessageType.Dump, "Dump: {0} bytes ({0:X8})", array.Length );

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

				WriteLine( MessageType.Dump, "{0,-51}| {1}", hex, txt );
			}

			WriteLine( MessageType.Dump, "---------------------------------------------------------------------");
		}
	}
}
