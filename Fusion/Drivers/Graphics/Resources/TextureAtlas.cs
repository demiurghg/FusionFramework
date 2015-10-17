#define DIRECTX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Fusion.Core.Content;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics {

	[ContentLoader(typeof(TextureAtlas))]
	public class TextureAtlasLoader : ContentLoader {

		public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
		{
			bool srgb = assetPath.ToLowerInvariant().Contains("|srgb");
			return new TextureAtlas( game.GraphicsDevice, stream, srgb );
		}
	}
		


	/// <summary>
	/// 
	/// </summary>
	public class TextureAtlas : DisposableBase {

		private	Texture2D	texture;

		struct Element {
			public string Name;
			public int X;
			public int Y;
			public int Width;
			public int Height;
		}

		List<Element> elements = new List<Element>();
		Dictionary<string,Element> dictionary;


		/// <summary>
		/// Atlas texture.
		/// </summary>
		public Texture2D Texture { 
			get { 
				return texture; 
			} 
		}



		/// <summary>
		/// Creates texture atlas from stream.
		/// </summary>
		/// <param name="device"></param>
		public TextureAtlas ( GraphicsDevice device, Stream stream, bool useSRgb = false )
		{
			using ( var br = new BinaryReader(stream) ) {
			
				if (!br.CheckMagic("ATLS")) {
					throw new IOException("Bad texture atlas file");
				}
				
				int count = br.ReadInt32();
				
				for ( int i=0; i<count; i++ ) {
					var element = new Element();
					element.Name	=	br.ReadString();
					element.X		=	br.ReadInt32();
					element.Y		=	br.ReadInt32();
					element.Width	=	br.ReadInt32();
					element.Height	=	br.ReadInt32();

					elements.Add( element );
				}				

				int ddsFileLength	=	br.ReadInt32();
				
				var ddsImageBytes	=	br.ReadBytes( ddsFileLength );

				texture	=	new Texture2D( device, ddsImageBytes, useSRgb );
			}


			dictionary	=	elements.ToDictionary( e => e.Name );
		}

					

		/// <summary>
		/// Gets names of all subimages. 
		/// </summary>
		public string[] SubImageNames {
			get {
				return elements.Select( e => e.Name ).ToArray();
			}
		}
		


		/// <summary>
		/// Gets subimage rectangle in this atlas.
		/// </summary>
		/// <param name="name">Subimage name. Case sensitive. Without extension.</param>
		/// <returns>Rectangle</returns>
		public Rectangle GetSubImageRectangle ( string name )
		{
			Element e;
			var r = dictionary.TryGetValue( name, out e );

			if (!r) {
				throw new InvalidOperationException(string.Format("Texture atlas does not contain subimage '{0}'", name));
			}

			return new Rectangle( e.X, e.Y, e.Width, e.Height );
		}



		/// <summary>
		/// Disposes texture atlas.
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
