using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Drivers.Graphics;
using Fusion.Core.Mathematics;
using Fusion.Content;


namespace Fusion.Pipeline.AssetTypes {
	
	[Asset("Content", "BMFC Sprite Font", "*.bmfc")]
	public class BMFontSpriteFontAsset : Asset {

		public string SourceFile { get; set; }

		public override string[] Dependencies
		{
			get { return new[]{ SourceFile }; }
		}



		public BMFontSpriteFontAsset ( string path ) : base(path)
		{
		}



		/// <summary>
		/// Builds asset
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Build ( BuildContext buildContext )
		{														   
			string tempFileName		= buildContext.GetTempFileName( AssetPath, ".fnt" );
			string resolvedPath		= buildContext.Resolve( SourceFile );	

			//	Launch 'bmfont.com' with temporary output file :
			buildContext.RunTool( @"bmfont.com",  string.Format("-c \"{0}\" -o \"{1}\"", resolvedPath, tempFileName ) );


			//	load temporary output :
			SpriteFont.FontFile font;
			using ( var stream = File.OpenRead( tempFileName ) ) {
				font = SpriteFont.FontLoader.Load( stream );
			}


			//	perform some checks :
			if (font.Common.Pages!=1) {
				throw new ContentException("Only one page of font image is supported");
			}


			//	patch font description and add children (e.g. "secondary") content :
			using ( var stream = buildContext.OpenTargetStream( this ) ) {

				using ( var sw = new BinaryWriter( stream ) ) {

					var xml = SpriteFont.FontLoader.SaveToString( font );
					sw.Write( xml );

					//	write pages :
					foreach (var p in font.Pages) {

						var pageFile	=	Path.Combine( Path.GetDirectoryName( tempFileName ), p.File );

						if ( Path.GetExtension( pageFile ).ToLower() == ".dds" ) {

							buildContext.CopyTo( pageFile, sw );

						} else {

							ImageFileTextureAsset.RunNVCompress( buildContext, pageFile, pageFile + ".dds", true, false, false, true, true, false, ImageFileTextureAsset.TextureCompression.RGB );

							buildContext.CopyTo( pageFile + ".dds", sw );

						}
					}
				}
			}
		}
	} 
}
