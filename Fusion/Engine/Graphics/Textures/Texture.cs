using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {
	public class Texture : DisposableBase {

		public int Width;
		public int Height;

		internal ShaderResource Srv = null;
		
	}
}
