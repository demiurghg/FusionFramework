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
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {

	[ContentLoader(typeof(Ubershader))]
	public class UbershaderLoader : ContentLoader {

		public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
		{
			return new Ubershader( game.GraphicsDevice, stream );
		}
	}
}
