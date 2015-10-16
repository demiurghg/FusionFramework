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
		/// <param name="sourceFolder"></param>
		/// <param name="targetFolder"></param>
		/// <param name="force"></param>
		public BuildResult Build ( BuildOptions options, IniData iniData )
		{
			BuildResult result	=	new BuildResult();
	
			context				=	new BuildContext( options, iniData );

			//
			//	gather files on source folder 
			//
			var files		=	GatherAssetFiles();
			result.Total	=	files.Count;

			//
			//	ignore some of them :
			//	
			if ( iniData.Sections.ContainsSection("Ignore") ) {

				var ignorePatterns =	iniData.Sections["Ignore"]
								.Select( element => element.KeyName )
								.ToArray();

				result.Ignored = files.RemoveAll( file => {

					foreach ( var ignore in ignorePatterns ) {
						if ( Wildcard.Match( file.KeyPath, ignore, false ) ) {
							return true;
						}
					}
					return false;
				});
			}


			
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

				if (!processors.ContainsKey(section.SectionName)) {
					Log.Warning("Asset processor '{0}' not found. Files will be skipped.", section.SectionName );
					continue;
				}

				var proc = processors[section.SectionName];

				Log.Message("[{0}]", proc.Name );

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

			// Apply attribute :
			var parser =	new CommandLineParser( processor );
			parser.Configuration.OptionLeadingChar = '/';
			parser.ParseCommandLine( args );

			assetFile.BuildArgs	=	args;

			//	Is up-to-date:
			//	Write time and 
			string status	=	"...";
			bool upToDate	=	false;

			if ( assetFile.IsUpToDate && !context.Options.ForceRebuild ) {
				if ( assetFile.IsParametersEqual() ) {
					status = "UTD";
					buildResult.UpToDate ++;
					assetFile.IsProcessed = true;
					upToDate = true;
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

			
			if (upToDate) {
				return;
			}
	
			try {

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
		List<AssetFile> GatherAssetFiles ()
		{
			var list = new List<AssetFile>();

			foreach ( var contentDir in context.ContentDirectories ) {

				var files = Directory	
							.EnumerateFiles( contentDir, "*", SearchOption.AllDirectories )
							.Select( path => new AssetFile( path, contentDir, context ) )
							.Where( file => file.KeyPath != ".content" )
							.ToList();

				list.AddRange( files );
			}

			return list.DistinctBy( file => file.KeyPath ).ToList();
		}

	}
}
