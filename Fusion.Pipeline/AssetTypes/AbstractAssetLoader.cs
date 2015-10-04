using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Pipeline {

	[ContentLoader(typeof(AbstractAsset))]
	public class AbstractAssetLoader : ContentLoader {

		public override object Load( Game game, Stream stream, Type requestedType, string assetPath )
		{
 			return Misc.LoadObjectFromXml( requestedType, stream, new Type[0] );
		}

	}
}
