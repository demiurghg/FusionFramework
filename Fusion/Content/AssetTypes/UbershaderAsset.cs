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



		class UsdbEntry {

			public string Defines;
			public byte[] PSBytecode;
			public byte[] VSBytecode;
			public byte[] GSBytecode;
			public byte[] HSBytecode;
			public byte[] DSBytecode;
			public byte[] CSBytecode;

			public UsdbEntry ( string defines, byte[] ps, byte[] vs, byte[] gs, byte[] hs, byte[] ds, byte[] cs ) 
			{
				this.Defines	=	defines;
				this.PSBytecode	=	ps;
				this.VSBytecode	=	vs;
				this.GSBytecode	=	gs;
				this.HSBytecode	=	hs;
				this.DSBytecode	=	ds;
				this.CSBytecode	=	cs;
			}
		}

	

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		public override string[] Dependencies
		{
			get { return new[]{SourceFile}; }
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Build ( BuildContext buildContext )
		{
			//
			//	Get combinations :
			//
			var combDecl	=	File.ReadAllLines( buildContext.Resolve( SourceFile ) )
									.Where( line0 => line0.Trim().StartsWith("$ubershader") )
									.ToList();

			var defineList = new List<string>();

			foreach ( var comb in combDecl ) {
				var ue = new UbershaderEnumerator( comb.Trim(), "$ubershader" );
				defineList.AddRange( ue.DefineList );
			}


			//
			//	Start listing builder :
			//	
			ListingPath	=	buildContext.GetTempFileName( AssetPath, ".html", true );
			var htmlBuilder = new StringBuilder();

			htmlBuilder.AppendFormat("<pre>");
			htmlBuilder.AppendLine("<b>Ubershader assembly listing</b>");
			htmlBuilder.AppendLine("");
			htmlBuilder.AppendLine("<b>Source:</b> <i>" + AssetPath + "</i>" );
			htmlBuilder.AppendLine("");
			htmlBuilder.AppendLine("<b>Declarations:</b>");

			foreach ( var comb in combDecl ) {
				htmlBuilder.AppendLine("  <i>" + comb + "</i>");
			}
			htmlBuilder.AppendLine("");


			var usdb = new List<UsdbEntry>();

			//
			//	Build all :
			//
			foreach ( var defines in defineList ) {

				var id		=	defineList.IndexOf( defines );

				var psbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".PS.dxbc", true );
				var vsbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".VS.dxbc", true );
				var gsbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".GS.dxbc", true );
				var hsbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".HS.dxbc", true );
				var dsbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".DS.dxbc", true );
				var csbc	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".CS.dxbc", true );

				var pshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".PS.html", true );
				var vshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".VS.html", true );
				var gshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".GS.html", true );
				var hshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".HS.html", true );
				var dshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".DS.html", true );
				var cshtm	=	buildContext.GetTempFileName(AssetPath, "." + id.ToString("D4") + ".CS.html", true );

				var ps = RunFxc( buildContext, SourceFile, "ps_5_0", PSEntryPoint, defines, psbc, pshtm );
				var vs = RunFxc( buildContext, SourceFile, "vs_5_0", VSEntryPoint, defines, vsbc, vshtm );
				var gs = RunFxc( buildContext, SourceFile, "gs_5_0", GSEntryPoint, defines, gsbc, gshtm );
				var hs = RunFxc( buildContext, SourceFile, "hs_5_0", HSEntryPoint, defines, hsbc, hshtm );
				var ds = RunFxc( buildContext, SourceFile, "ds_5_0", DSEntryPoint, defines, dsbc, dshtm );
				var cs = RunFxc( buildContext, SourceFile, "cs_5_0", CSEntryPoint, defines, csbc, cshtm );
				

				htmlBuilder.AppendFormat( (ps==null) ? ".. " : "<a href=\"{0}\">ps</a> ", Path.GetFileName(pshtm) );
				htmlBuilder.AppendFormat( (vs==null) ? ".. " : "<a href=\"{0}\">vs</a> ", Path.GetFileName(vshtm) );
				htmlBuilder.AppendFormat( (gs==null) ? ".. " : "<a href=\"{0}\">gs</a> ", Path.GetFileName(gshtm) );
				htmlBuilder.AppendFormat( (hs==null) ? ".. " : "<a href=\"{0}\">hs</a> ", Path.GetFileName(hshtm) );
				htmlBuilder.AppendFormat( (ds==null) ? ".. " : "<a href=\"{0}\">ds</a> ", Path.GetFileName(dshtm) );
				htmlBuilder.AppendFormat( (cs==null) ? ".. " : "<a href=\"{0}\">cs</a> ", Path.GetFileName(cshtm) );

				htmlBuilder.Append( "[" + defines + "]<br>" );

				usdb.Add( new UsdbEntry( defines, ps, vs, gs, hs, ds, cs ) );
			}

			File.WriteAllText( buildContext.GetTempFileName(AssetPath, ".html", true), htmlBuilder.ToString() );


			//
			//	Write ubershader :
			//
			using ( var fs = buildContext.TargetStream( this ) ) {

				using ( var bw = new BinaryWriter( fs ) ) {

					bw.Write( new[]{'U','S','D','B'});

					bw.Write( usdb.Count );

					foreach ( var entry in usdb ) {

						bw.Write( entry.Defines );

						bw.Write( new[]{'P','S','B','C'});
						bw.Write( entry.PSBytecode.Length );
						bw.Write( entry.PSBytecode );

						bw.Write( new[]{'V','S','B','C'});
						bw.Write( entry.VSBytecode.Length );
						bw.Write( entry.VSBytecode );

						bw.Write( new[]{'G','S','B','C'});
						bw.Write( entry.GSBytecode.Length );
						bw.Write( entry.GSBytecode );

						bw.Write( new[]{'H','S','B','C'});
						bw.Write( entry.HSBytecode.Length );
						bw.Write( entry.HSBytecode );

						bw.Write( new[]{'D','S','B','C'});
						bw.Write( entry.DSBytecode.Length );
						bw.Write( entry.DSBytecode );

						bw.Write( new[]{'C','S','B','C'});
						bw.Write( entry.CSBytecode.Length );
						bw.Write( entry.CSBytecode );
					}
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFile"></param>
		/// <param name="profile"></param>
		/// <param name="entryPoint"></param>
		/// <param name="defines"></param>
		/// <returns></returns>
		byte[] RunFxc ( BuildContext buildContext, string sourceFile, string profile, string entryPoint, string defines, string output, string listing )
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("/Cc" + " ");
			sb.Append("/T" + profile + " ");
			sb.Append("/E" + entryPoint + " ");
			sb.Append("/Fo\"" + output + "\" ");
			sb.Append("/Fc\"" + listing + "\" ");
			sb.Append("/nologo" + " ");
			sb.Append("/O" + OptimizationLevel.ToString() + " ");

			if ( DisableOptimization)	sb.Append("/Od ");
			if ( PreferFlowControl)	sb.Append("/Gfp ");
			if ( AvoidFlowControl)	sb.Append("/Gfa ");

			if ( MatrixPacking==ShaderMatrixPacking.ColumnMajor )	sb.Append("/Zpc ");
			if ( MatrixPacking==ShaderMatrixPacking.RowMajor )	sb.Append("/Zpr ");

			foreach ( var def in defines.Split(new[]{' ','\t'}, StringSplitOptions.RemoveEmptyEntries) ) {
				sb.AppendFormat("/D{0}=1 ", def);
			}

			sb.Append("\"" + buildContext.Resolve( sourceFile ) + "\"");

			try {
				
				buildContext.RunTool("fxc.exe", sb.ToString());

			} catch ( ToolException tx ) {
				///	entry point not fount - that is ok.
				if (tx.Message.Contains("error X3501")) {
					return new byte[0];
				}

				throw;
			}

			return File.ReadAllBytes( output );
		}


	#if false
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
	#endif
	}
}
