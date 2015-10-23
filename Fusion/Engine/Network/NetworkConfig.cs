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

namespace Fusion.Engine.Network {

	public class NetworkConfig {

		public int Port { get; set; }
		public int MaxClients { get; set; }


		public NetworkConfig ()
		{
			Port		=	28100;
			MaxClients	=	8;
		}

	}
}
