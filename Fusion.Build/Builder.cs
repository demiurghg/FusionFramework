using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Build.Processors;
using Fusion.Core.IniParser;
using Fusion.Core.IniParser.Model;
using Fusion;
using Fusion.Core.Shell;
using Fusion.Core.Content;

namespace Fusion.Build {
	public class Builder {

		Dictionary<string, AssetProcessorBinding> processors;

		BuildContext	context;


		public int Total { get; private set; }
		public int Ignored { get; private set; }
		public int Succeded { get; private set; }
		public int Skipped { get; private set; }
		public int Failed { get; private set; }


		/// <summary>
		/// Initialize builder with given set of processors.
		/// Key is a name of processor.
		/// Value is a processor.
		/// </summary>
		/// <param name="processors"></param>
		public Builder ( IEnumerable<AssetProcessorBinding> processors )
		{
			this.processors	=	processors.ToDictionary( p => p.Name );
		}






		/// <summary>
		/// 
		/// </summary>
		/// <param name="inputDirectory"></param>
		/// <param name="outputDirectory"></param>
		/// <param name="temporaryDirectory"></param>
		/// <param name="force"></param>
		public static void Build ( string inputDirectory, string outputDirectory, string temporaryDirectory, bool force )
		{
			var options = new BuildOptions();
			options.InputDirectory	=	inputDirectory;
			options.OutputDirectory	=	outputDirectory;
			options.TempDirectory	=	temporaryDirectory;
			options.ForceRebuild	=	force;

			Build( options );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		public static void Build ( BuildOptions options )
		{
			Log.Message("");
			Log.Message("-------- Build started : {0} --------", options.InputDirectory );

			options.CheckOptionsAndMakeDirs();

			Log.Message("Reading '.content'...");
			//
			//	Parse INI file :
			//
			var ip = new StreamIniDataParser();
			ip.Parser.Configuration.AllowDuplicateSections	=	true;
			ip.Parser.Configuration.AllowDuplicateKeys		=	true;
			ip.Parser.Configuration.CommentString			=	"#";
			ip.Parser.Configuration.OverrideDuplicateKeys	=	true;
			ip.Parser.Configuration.KeyValueAssigmentChar	=	'=';
			ip.Parser.Configuration.AllowKeysWithoutValues	=	true;

			var iniData = ip.ReadData( new StreamReader( options.ContentIniFile ) );


			//
			//	Setup builder :
			//	
			var bindings = AssetProcessorBinding.GatherAssetProcessors();

			Log.Message("Asset processors:");
			foreach ( var bind in bindings ) {
				Log.Message("  {0,-20} - {1}", bind.Name, bind.Type.Name );
			}
			Log.Message("");

			var builder = new Builder( bindings );

			var result  = builder.Build( options, iniData );

			Log.Message("-------- {5} total, {0} succeeded, {1} failed, {2} up-to-date, {3} ignored, {4} skipped --------", 
				result.Succeded,
				result.Failed,
				result.UpToDate,
				result.Ignored,
				result.Skipped,
				result.Total );

			Log.Message("");

			if (result.Failed>0) {
				throw new BuildException("Build errors");
			}
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFolder"></param>
		/// <param name="targetFolder"></param>
		/// <param name="force"></param>
		public BuildResult Build ( BuildOptions options, IniData iniData )
		{
			BuildResult result	=	new BuildResult();
	
			context				=	new BuildContext( options, iniData );
			var ignorePatterns	=	new string[0];

			if ( iniData.Sections.ContainsSection("Ignore") ) {
				ignorePatterns	=	iniData.Sections["Ignore"]
									.Select( element => element.KeyName )
									.Select( key => ContentUtils.BackslashesToSlashes( key ) )
									.ToArray();
			}


			//
			//	gather files on source folder 
			//
			Log.Message("Gathering files...");
			var files		=	GatherAssetFiles( ignorePatterns, ref result );
			Log.Message("");


			//
			//	Check hash collisions :
			//
			var collisions	=	files
								.GroupBy( file0 => file0.TargetName )
								.Where( fileGroup1 => fileGroup1.Count() > 1 )
								.Select( fileGroup2 => fileGroup2.ToArray() )
								.Distinct()
								.ToArray();


			//
			//	ignore some of them :
			//	


			if (collisions.Any()) {
				Log.Error("Hash collisions detected:");
				foreach ( var collision in collisions ) {
					Log.Error( "  {0} - {1}", collision[0].KeyPath, collision[1].KeyPath );
				}
				throw new BuildException("Hash collisions detected");
			}


			result.Total	=	files.Count;

			//
			//	Build everything :
			//
			foreach ( var section in iniData.Sections ) {

				//	'Ingore' is a special section.
				if (section.SectionName=="Ignore") {
					continue;
				}
				if (section.SectionName=="ContentDirectories") {
					continue;
				}
				if (section.SectionName=="BinaryDirectories") {
					continue;
				}

				Log.Message("-------- {0} --------", section.SectionName );

				if (!processors.ContainsKey(section.SectionName)) {
					Log.Warning("Asset processor '{0}' not found. Files will be skipped.", section.SectionName );
					Log.Message("");
					continue;
				}

				var proc = processors[section.SectionName];

				var maskArgs = section.Keys
					.Reverse()
					.Select( key => new {
						Mask = key.KeyName.Split(' ', '\t').FirstOrDefault(), 
						Args = CommandLineParser.SplitCommandLine( key.KeyName ).Skip(1).ToArray()
					 })
					.ToList();


				foreach ( var file in files ) {

					foreach ( var maskArg in maskArgs ) {
						
						if ( Wildcard.Match( file.KeyPath, maskArg.Mask, true ) ) {

							var processor = proc.CreateAssetProcessor();

							BuildAsset( processor, maskArg.Args, file, ref result );

							break;
						}

					}
				}

				Log.Message("");
			}

			return result;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="processor"></param>
		/// <param name="fileName"></param>
		void BuildAsset ( AssetProcessor processor, string[] args, AssetFile assetFile, ref BuildResult buildResult )
		{					
			if (assetFile.IsProcessed) {
				Log.Warning("{0} : already proccessed. Skipped.", assetFile.KeyPath );
				return;
			}

			try {
				assetFile.BuildArgs	=	args;

				//	Is up-to-date:
				//	Write time and 
				string status	=	"...";

				if ( assetFile.IsUpToDate && !context.Options.ForceRebuild ) {
					if ( assetFile.IsParametersEqual() ) {
						status = "UTD";
						buildResult.UpToDate ++;
						assetFile.IsProcessed = true;
						return;
					} else {
						status = "HSH";
					}
				} 																   


				var keyPath = assetFile.KeyPath;

				if (keyPath.Length > 40) {
					keyPath = "..." + keyPath.Substring( keyPath.Length - 40 + 3 );
				}

				Log.Message("{0,-40} {1,-5} {2}  {3}", keyPath, Path.GetExtension(keyPath), status, string.Join(" ", args) );

				// Apply attribute :
				var parser =	new CommandLineParser( processor );
				parser.Configuration.OptionLeadingChar = '/';
				parser.Configuration.ThrowExceptionOnShowError = true;

				parser.ParseCommandLine( args );

				//
				//	Build :
				//
				processor.Process( assetFile, context );

				assetFile.IsProcessed = true;

				buildResult.Succeded ++;

			} catch ( Exception e ) {
				Log.Error( "{0} : {1}", assetFile.KeyPath, e.Message );
				buildResult.Failed ++;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFolder"></param>
		/// <returns></returns>
		List<AssetFile> GatherAssetFiles ( string[] ignorePatterns, ref BuildResult result )
		{
			var list = new List<AssetFile>();

			foreach ( var contentDir in context.ContentDirectories ) {

				var files = Directory	
							.EnumerateFiles( contentDir, "*", SearchOption.AllDirectories )
							.Select( path => new AssetFile( path, contentDir, context ) )
							.Where( file => file.KeyPath != ".content" )
							.ToList();

				int ignored = files.RemoveAll( file => {

						foreach ( var patten in ignorePatterns ) {
							if (Wildcard.Match( file.KeyPath, patten )) {
								return true;
							}
						}
						return false;
					} );

				result.Ignored += ignored;

				list.AddRange( files );
			}


			return list.DistinctBy( file => file.KeyPath ).ToList();
		}

	}
}
