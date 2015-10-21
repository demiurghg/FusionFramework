using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Drivers.Graphics;
using Fusion.Core.Content;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Represents any kind of texture that could be loaded from content.
	/// </summary>
	public class DiscTexture : Texture {

		[ContentLoader(typeof(DiscTexture))]
		public class Loader : ContentLoader {

			public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
			{
				bool srgb = assetPath.ToLowerInvariant().Contains("|srgb");
				return new DiscTexture( game.GraphicsEngine, new Texture2D( game.GraphicsDevice, stream, srgb ) );
			}
		}


		Texture2D texture;


		/// <summary>
		///
		/// </summary>
		/// <param name="ge"></param>
		/// <param name="texture"></param>
		internal DiscTexture ( GraphicsEngine ge, Texture2D texture )
		{
			this.texture	=	texture;
			this.Width		=	texture.Width;
			this.Height		=	texture.Height;
			this.Srv		=	texture;
		}



		/// <summary>
		///	Disposes DiscTexture 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref texture );
			}

			base.Dispose( disposing );
		}
	}
}
