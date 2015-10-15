using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Shell;

namespace Fusion.Build.Processors {

	[AssetProcessor("Textures", "Converts TGA, PNG and JPEG images to DDS. DDS files will be bypassed.")]
	public class TextureProcessor : AssetProcessor {

		public enum TextureCompression {
			RGB	,  	BC1	,	BC1N,  	BC1A,
			BC2	,  	BC3	,	BC3N,  	BC4	,
			BC5	,		
		}
			
		[CommandLineParser.Name("nomips", "do not build mip levels")]
		public bool NoMips { get; set; }

		[CommandLineParser.Name("fast", "perform fast compression")]
		public bool Fast { get; set; }

		[CommandLineParser.Name("tonormal", "build normalmap")]
		public bool ToNormal { get; set; }

		[CommandLineParser.Name("color",  "texture contains color data")]
		public bool Color { get; set; }

		[CommandLineParser.Name("alpha", "texture contains alpha data")]
		public bool Alpha { get; set; }

		[CommandLineParser.Name("normal", "texture contains normalmap")]
		public bool Normal { get; set; }

		[CommandLineParser.Name("compression", "compression mode")]
		public TextureCompression Compression { get; set; }

		
		/// <summary>
		/// 
		/// </summary>
		public TextureProcessor ()
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceStream"></param>
		/// <param name="targetStream"></param>
		public override void Process ( AssetFile assetFile, BuildContext context )
		{
			var src	=	assetFile.FullSourcePath;
			var dst	=	context.GetTempFileName( assetFile.KeyPath, ".dds" );

			RunNVCompress( context, src, dst, NoMips, Fast, ToNormal, Color, Alpha, Normal, Compression );

			using ( var target = assetFile.OpenTargetStream() ) {
				context.CopyFileTo( dst, target );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		/// <param name="noMips"></param>
		/// <param name="fast"></param>
		/// <param name="toNormal"></param>
		/// <param name="color"></param>
		/// <param name="alpha"></param>
		/// <param name="normal"></param>
		/// <param name="compression"></param>
		internal static void RunNVCompress( BuildContext buildContext, string src, string dst, bool noMips, bool fast, bool toNormal, bool color, bool alpha, bool normal, TextureCompression compression )
		{
			string commandLine = "";

			if ( noMips		) 	commandLine	+=	" -nomips"	;
			if ( fast		) 	commandLine	+=	" -fast"	;
			if ( toNormal	) 	commandLine	+=	" -tonormal";
			if ( color		) 	commandLine	+=	" -color"	;
			if ( alpha		) 	commandLine	+=	" -alpha"	;
			if ( normal		) 	commandLine	+=	" -normal"	;

			commandLine += ( " -" + compression.ToString().ToLower() );
			commandLine += ( " \"" + src + "\"" );
			commandLine += ( " \"" + dst + "\"" );

			buildContext.RunTool( @"nvcompress.exe", commandLine );//*/
		}

	}
}
