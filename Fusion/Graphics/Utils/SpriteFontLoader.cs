using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using SharpDX;
using Fusion.Graphics;
using System.Diagnostics;
using System.ComponentModel;
using Fusion.Content;


namespace Fusion.Graphics {
	[ContentLoader(typeof(SpriteFont))]
	public class SpriteFontLoader : ContentLoader {

		public override object Load ( Game game, Stream stream, Type requestedType, string assetPath )
		{
			return new SpriteFont( game.GraphicsDevice, stream );
		}
	}
}
