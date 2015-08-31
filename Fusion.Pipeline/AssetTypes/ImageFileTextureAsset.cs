using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using Fusion.Mathematics;


namespace Fusion.Pipeline.AssetTypes {

	[Asset("Content", "Image File Texture", "*.tga;*.png;*.jpg")]
	public class ImageFileTextureAsset : Asset, IFileDerivable {

		public enum TextureCompression {
			RGB	,  	BC1	,	BC1N,  	BC1A,
			BC2	,  	BC3	,	BC3N,  	BC4	,
			BC5	,		
		}
			

		public override string ToString ()
		{
			var sb = new StringBuilder();
			sb.Append( Compression.ToString() );
			if (NoMips)		sb.Append(" NoMips");
			if (Fast)		sb.Append(" Fast");
			if (ToNormal)	sb.Append(" ToNormal");
			if (Color)		sb.Append(" Color");
			if (Alpha)		sb.Append(" Alpha");
			if (Normal)		sb.Append(" Normal");
			return sb.ToString();
		}

		[Category("Texture Parameters")]
		[Description("Disable mipmap generation.")]
		public bool			NoMips		{ get; set; }

		[Category("Texture Parameters")]
		[Description("Fast compression.")]
		public bool			Fast		{ get; set; }

		[Category("Texture Parameters")]
		[Description("Convert input to normal map.")]
		public bool			ToNormal	{ get; set; }

		[Category("Texture Parameters")]
		[Description("The input image is a color map (default).")]
		public bool			Color		{ get; set; }

		[Category("Texture Parameters")]
		[Description("The input image has an alpha channel used for transparency.")]
		public bool			Alpha		{ get; set; }

		[Category("Texture Parameters")]
		[Description("The input image is a normal map.")]
		public bool			Normal		{ get; set; }

		[Category("Texture Parameters")]
		[Description(@"Compression format")]
		public TextureCompression	Compression	{ get; set; }

		[Category("Texture Parameters")]
		[Description(@"Compression format")]
		public string SourceFile { get; set; }


		/// <summary>
		/// 
		/// </summary>
		public ImageFileTextureAsset()
		{
			Color			=	true;
			Compression		=	TextureCompression.RGB;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFile"></param>
		public ImageFileTextureAsset( string keyPath, string sourceFile )
		{
			AssetPath	=	keyPath;
			Color		=	true;
			Compression	=	TextureCompression.RGB;
			SourceFile	=	sourceFile;
		}



		public override string[] Dependencies
		{
			get { return new[]{ SourceFile }; }
		}


		/// <summary>
		/// Inits asset from file
		/// </summary>
		/// <param name="path"></param>
		public void InitFromFile( string path )
		{
			AssetPath	=	path;
			SourceFile	=	path;	
		}



		public override void Build ( BuildContext buildContext )
		{
			var src	=	buildContext.Resolve( SourceFile );
			var dst	=	buildContext.GetTempFileName( Hash, ".dds" );

			RunNVCompress( buildContext, src, dst, NoMips, Fast, ToNormal, Color, Alpha, Normal, Compression );

			using ( var target = buildContext.OpenTargetStream( this ) ) {
				buildContext.CopyTo( dst, target );
			}
		}



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
