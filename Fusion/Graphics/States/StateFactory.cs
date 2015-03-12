using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Graphics.States {

	/// <summary>
	/// 
	/// </summary>
	public class StateFactory : DisposableBase {

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="ubershader"></param>
		public StateFactory ( GraphicsDevice device, Ubershader ubershader )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				
			}

			base.Dispose( disposing );
		}
	}
}
