using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Test {

	[Serializable]
	public class TestException : System.Exception {

		public TestException ()
		{
		}
		
		public TestException ( string message ) : base( message )
		{
		}

		public TestException( string message, Exception inner ) : base( message, inner )
		{
		}
	}
}
