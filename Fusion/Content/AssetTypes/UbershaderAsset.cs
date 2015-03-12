using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using SharpDX;
using Fusion.Graphics;
using System.Threading;
using Fusion.Mathematics;


namespace Fusion.Content {

	[Asset("Content", "HLSL Ubershader", "*.hlsl")]
	public partial class UbershaderAsset : Asset {
			
		public enum ShaderMatrixPacking {
			RowMajor,
			ColumnMajor,
		}


		[Category("Shader Compiler Parameters")]
		[Description("Hardware profile")]
		public GraphicsProfile	GraphicsProfile { get; set; }
			
		[Category("Shader Compiler Parameters")]
		[Description("/Od - disable optimizations")]
		public bool				DisableOptimization { get; set; }
			
		[Category("Shader Compiler Parameters")]
		[Description("/O - optimization level 0..3.  1 is default")]
		public int				OptimizationLevel { get { return optimizationLevel; } set { optimizationLevel = MathUtil.Clamp( value, 0, 3 ); } }
		int optimizationLevel = 1;

		[Category("Shader Compiler Parameters")]
		[Description("/Gfa - avoid flow control constructs")]
		public bool				AvoidFlowControl { get; set; }

		[Category("Shader Compiler Parameters")]
		[Description("/Gfp - prefer flow control constructs")]
		public bool				PreferFlowControl { get; set; }
			
		[Category("Shader Compiler Parameters")]
		[Description("/Zpr /Zpc - pack matrices in row-major or in column-major order")]
		public ShaderMatrixPacking	MatrixPacking { get; set; }
			
		[Category("Shader Compiler Parameters")]
		public string			PSEntryPoint	{ get; set; }
			
		[Category("Shader Compiler Parameters")]
		public string			VSEntryPoint	{ get; set; }
			
		[Category("Shader Compiler Parameters")]
		public string			GSEntryPoint	{ get; set; }
			
		[Category("Shader Compiler Parameters")]
		public string			DSEntryPoint	{ get; set; }
			
		[Category("Shader Compiler Parameters")]
		public string			HSEntryPoint	{ get; set; }
			
		[Category("Shader Compiler Parameters")]
		public string			CSEntryPoint	{ get; set; }

		[Category("Shader Compiler Parameters")]
		public string			SourceFile	{ get; set; }

		[Category("Shader Compiler Parameters")]
		public bool				ShowPemutations	{ get; set; }

		[ReadOnly(true)]
		[Browsable(false)]
		public string			ListingPath		{ get; set; }


		public UbershaderAsset ()
		{
			GraphicsProfile	=	GraphicsProfile.HiDef;
			MatrixPacking	=	ShaderMatrixPacking.RowMajor;
			PSEntryPoint	=	"PixelShader";
			VSEntryPoint	=	"VertexShader";
			GSEntryPoint	=	"GeometryShader";
			DSEntryPoint	=	"DomainShader";
			HSEntryPoint	=	"HullShader";
			CSEntryPoint	=	"ComputeShader";
		}

			
		[Command("ShowListing", 2)]
		public void ShowListing ()
		{
			if (!string.IsNullOrEmpty(ListingPath)) {
				System.Diagnostics.Process.Start( ListingPath );
			}
		}


		#if false
			Expression	::=		'$pixel'|'$vertex'|'$geometry'|'$compute'|'$domain'|'$hull' ' ' Combination
			Combination	::=		Sequence (' '+ Sequence)*
			Sequence	::=		Exclusion ('..' Exclusion)*
			Exclusion	::=		Factor ('|' Factor)*
			Factor		::=		[-] Definition | Factor
			Definition	::=		IdentChar+
		#endif


		enum Target {
			None,
			PixelShader,
			VertexShader,
			GeometryShader,
			DomainShader,
			HullShader,
			ComputeShader,
		}


		class UsdbEntry {
			public UsdbEntry ( Target target, string defines, byte[] bytecode, string listingPath, string binPath ) {
				Target		= target;
				Defines		= defines;
				Bytecode	= bytecode;
				ListingPath	= listingPath;
				BinPath		= binPath;
			}

			public Target Target;
			public string Defines;
			public byte[] Bytecode;

			public string ListingPath;
			public string BinPath;
		}

	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		public override string[] Dependencies
		{
			get { return new[]{SourceFile}; }
		}



		public override void Build ( BuildContext buildContext )
		{
			var combDecl	=	File.ReadAllLines( buildContext.Resolve( SourceFile ) )
									.Where( line0 => line0.Trim().StartsWith("$") )
									.ToList();

			Target target;

			List<UsdbEntry> db	=	new List<UsdbEntry>();

			ListingPath	=	buildContext.GetTempFileName( AssetPath, ".html", true );
			StringBuilder htmlBuilder = new StringBuilder();

			htmlBuilder.AppendFormat("<pre>");
			htmlBuilder.AppendLine("<b>Ubershader assembly listing</b>");
			htmlBuilder.AppendLine("");
			htmlBuilder.AppendLine("<b>Source:</b> <i>" + AssetPath + "</i>" );
			htmlBuilder.AppendLine("");
			htmlBuilder.AppendLine("");

			int counter = 0;

			foreach ( var decl in combDecl ) {

				if (ShowPemutations) {
					Log.Message("{0}", decl );
				}
				var definesSet = Parse( decl.Trim(), out target );

				htmlBuilder.AppendLine("<b>Declaration:</b> <i>" + decl.Replace('\t', ' ').Replace("  ", " ").Replace("  ", " ") + "</i>" );

				foreach ( var defines in definesSet ) {
					counter++;
				
					var asmPath		= buildContext.GetTempFileName( AssetPath, "." + counter.ToString("D4") + ".html", true );
					var binPath		= buildContext.GetTempFileName( AssetPath, "." + counter.ToString("D4") + ".dxbc", true );
					var usdbEntry	= new UsdbEntry( target, defines, null, asmPath, binPath );

					db.Add( usdbEntry );

					htmlBuilder.AppendFormat("<a href=\"{0}\">{1}: [{2}]</a><br>", Path.GetFileName(asmPath), target, defines );
				}

				htmlBuilder.AppendLine("");
			}

			htmlBuilder.AppendFormat("</pre>");

			File.WriteAllText( ListingPath, htmlBuilder.ToString());


			//
			//	Perform parallel compilation :
			//
			Parallel.ForEach( db, (usdbEntry, state) => {

				if (ShowPemutations) {
					Log.Message(" {1,3}> [{0}]...", usdbEntry.Defines, Thread.CurrentThread.ManagedThreadId );
				}

				usdbEntry.Bytecode	=	RunFxc( buildContext, this, usdbEntry.Target, usdbEntry.Defines, usdbEntry.ListingPath, usdbEntry.BinPath );

				if (ShowPemutations) {
					Log.Message(" {1,3}> ...Done.", usdbEntry.Defines, Thread.CurrentThread.ManagedThreadId );
				}
			});




			//
			//	Write ubershader database :
			//
			using ( var fs = buildContext.TargetStream( this ) ) {

				using ( var bw = new BinaryWriter( fs ) ) {

					bw.Write('U');
					bw.Write('S');
					bw.Write('D');
					bw.Write('B');

					bw.Write( db.Count );

					foreach ( var entry in db ) {

						bw.Write( entry.Target.ToString() );
						bw.Write( entry.Defines );
						bw.Write( entry.Bytecode.Length );
						bw.Write( entry.Bytecode );
					}
				}
			}
		}


		
		/// <summary>
		/// Runs fxc.exe
		/// </summary>
		/// <param name="item"></param>
		/// <param name="target"></param>
		/// <param name="defList"></param>
		byte[] RunFxc ( BuildContext buildContext, UbershaderAsset asset, Target target, string defList, string listingPath, string binPath )
		{
			StringBuilder sb = new StringBuilder();

			string entryPoint;
			string profile;
			string systemDefine;
			string output			=	binPath;
			string listing			=	listingPath;
			string listingHex		=	Path.ChangeExtension(binPath, ".hex");
			string shaderVersion	=	HardwareProfileChecker.GetShaderVersion( asset.GraphicsProfile );
			//string listing	=	Path.ChangeExtension( item.ResolvedPath, ".html" );

			TargetToProfile ( target, asset, shaderVersion, out profile, out entryPoint, out systemDefine );

			defList += " " + systemDefine;

			sb.Append("/Cc" + " ");
			sb.Append("/T" + profile + " ");
			sb.Append("/E" + entryPoint + " ");
			sb.Append("/Fo\"" + output + "\" ");
			sb.Append("/Fc\"" + listing + "\" ");
			sb.Append("/nologo" + " ");
			sb.Append("/O" + asset.OptimizationLevel.ToString() + " ");

			if (asset.DisableOptimization) sb.Append("/Od ");
			if (asset.PreferFlowControl) sb.Append("/Gfp ");
			if (asset.AvoidFlowControl) sb.Append("/Gfa ");

			if ( asset.MatrixPacking==ShaderMatrixPacking.ColumnMajor ) sb.Append("/Zpc ");
			if ( asset.MatrixPacking==ShaderMatrixPacking.RowMajor )	 sb.Append("/Zpr ");

			foreach ( var def in defList.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries) ) {
				sb.AppendFormat("/D{0}=1 ", def);
			}

			//	add source file :
			sb.Append("\"" + buildContext.Resolve( SourceFile ) + "\"");


			buildContext.RunTool("fxc.exe", sb.ToString());

			var bytecode = File.ReadAllBytes( output );


			StringBuilder sb2 = new StringBuilder();

			int counter = 0;
			var str = new byte[16];
			for (int i=0; i<bytecode.Length; i++) {
				str[counter] = bytecode[i];
				counter ++;

				sb2.Append( bytecode[i].ToString("X2") );
				sb2.Append(" ");

				if (counter==4 || counter==12) {
					sb2.Append(" ");
				}

				if (counter==8) {
					sb2.Append(" ");
				}
				
				if (counter==16) {

					var s = new string( str.Select( b => char.IsControl( (char)b ) ? '.':(char)b ).ToArray() );

					sb2.Append( "| " + s + "\r\n" );

					str = new byte[]{0,0,0,0,0,0,0,0, 0,0,0,0,0,0,0,0};
					counter = 0;
				}
			}

			File.WriteAllText( listingHex, sb2.ToString() );

			//listingText	 = File.ReadAllText( listing );
			//File.Delete( output );
			//File.Delete( listing );

			return bytecode;
		}
	}
}
