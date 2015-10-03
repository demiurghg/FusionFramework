using System;


namespace Fusion {

	[Serializable]
	public class GameException : System.Exception {

		public GameException ()
		{
		}
		
		public GameException ( string message ) : base( message )
		{
		}

		public GameException( string message, Exception inner ) : base( message, inner )
		{
		}
	}
}
