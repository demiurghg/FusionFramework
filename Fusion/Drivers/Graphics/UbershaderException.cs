using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Specialized;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;
using Fusion.Core.Content;



namespace Fusion.Drivers.Graphics {

	[Serializable]
	public class UbershaderException : GraphicsException {

		/// <summary>
		/// Bit flags thats causes ubershader exception
		/// </summary>
		public int Combination { get; private set; }

		/// <summary>
		/// Array of string defines
		/// </summary>
		public string[] Defines { get; private set; }

		/// <summary>
		/// All defines as single string
		/// </summary>
		public string AllDefines { 
			get {
				return "[" + string.Join(" ", Defines) + "]";
			}
		}



		public UbershaderException ()
		{
		}
		

		public UbershaderException ( string message ) : base( message )
		{
		}


		public UbershaderException ( string format, params object[] args ) : base( string.Format(format, args) )
		{
			
		}


		public UbershaderException ( string message, Exception inner ) : base( message, inner )
		{
		}


		public UbershaderException ( string message, int combination, Type combinerEnum ) : base( message )
		{
			this.Combination	=	combination;
			
			List<string> defs = new List<string>();

            for (int i=0; i<32; i++) {
				int bit = 1<<i;
				
				if ( (bit & combination) != 0 ) {
					defs.Add( Enum.GetName( combinerEnum, bit ) );					
				}
            }

			Defines	=	defs.ToArray();
		}



		public void Report ()
		{
			Log.Warning("Message : {0}", Message );
			Log.Warning("Combination : 0x{0:X}", Combination );
			Log.Warning("Defines :");
			foreach (var def in Defines ) {
				Log.Warning("   {0}", def);
			}
			Log.Warning("");
		}

	}
}
