using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using System.Xml;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;
using System.Net;



namespace Fusion.Build {

	[Serializable]
	public class ToolException : BuildException {

		public ToolException ()
		{
		}
		
		public ToolException ( string message ) : base( message )
		{
		}

		public ToolException( string message, Exception inner ) : base( message, inner )
		{
		}
	}
}
