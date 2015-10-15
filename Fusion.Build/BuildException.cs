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
	public class BuildException : System.Exception {

		public BuildException ()
		{
		}
		
		public BuildException ( string message ) : base( message )
		{
		}

		public BuildException( string message, Exception inner ) : base( message, inner )
		{
		}
	}
}
