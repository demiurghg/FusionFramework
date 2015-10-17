using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using SharpDX;
using Fusion.Drivers.Graphics;
using System.Diagnostics;
using System.ComponentModel;
using Fusion.Core.Content;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {
	[ContentLoader(typeof(SpriteFont))]
	public class SpriteFontLoader : ContentLoader {

		public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
		{
			return new SpriteFont( game.GraphicsDevice, stream );
		}
	}
}
