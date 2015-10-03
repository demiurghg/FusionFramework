using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;

namespace Fusion.Core.Shell
{
	[Serializable]
	public class CommandLineParserException : Exception {

		public CommandLineParserException ()
		{
		}
		
		public CommandLineParserException ( string message ) : base( message )
		{
		}

		public CommandLineParserException ( string format, params object[] args ) : base( string.Format(format, args) )
		{
			
		}

		public CommandLineParserException ( string message, Exception inner ) : base( message, inner )
		{
		}
	}
}
