using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Shell;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Lidgren.Network;

namespace Fusion.Engine.Network {

	public class NetworkEngine : GameModule {

		[Config]
		public NetworkConfig Config { get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameEngine"></param>
		internal NetworkEngine ( GameEngine gameEngine ) : base( gameEngine )
		{
			Config	=	new NetworkConfig();
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
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
