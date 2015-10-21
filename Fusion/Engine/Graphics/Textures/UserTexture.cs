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
	public class UserTexture : Texture {

		Texture2D texture;


		/// <summary>
		///
		/// </summary>
		/// <param name="ge"></param>
		/// <param name="texture"></param>
		public UserTexture ( GraphicsEngine ge, Stream stream, bool forceSRgb  )
		{
			this.texture	=	new Texture2D( ge.Device, stream, forceSRgb );
			this.Width		=	texture.Width;
			this.Height		=	texture.Height;
			this.Srv		=	texture;
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="ge"></param>
		/// <param name="texture"></param>
		public UserTexture ( GraphicsEngine ge, byte[] data, bool forceSRgb )
		{
			this.texture	=	new Texture2D( ge.Device, data, forceSRgb );
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
