using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using FBuild.Processors;
using Fusion.Core.IniParser;
using Fusion.Core.IniParser.Model;
using Fusion;
using Fusion.Core.Shell;

namespace FBuild {
	class Builder {

		Dictionary<string, AssetProcessorBinding> processors;


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
			BuildResult result = new BuildResult();

			//
			//	gather files on source folder 
			//
			var files		=	GatherAssetFiles( options.FullInputDirectory, options.FullOutputDirectory );
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

				if (section.SectionName=="Ignore") {
					continue;
				}

				if (!processors.ContainsKey(section.SectionName)) {
					Log.Warning("Asset processor '{0}' not found. Files will be skipped.", section.SectionName );
					continue;
				}

				var proc = processors[section.SectionName];


				var maskArgs = section.Keys
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
			var parser =	new CommandLineParser( processor );
			parser.ParseCommandLine( args );

			if (assetFile.IsUpToDate) {
				buildResult.UpToDate ++;
				return;
			} 

			try {

				Log.Message("{0}", assetFile.KeyPath );
				processor.Process( assetFile );
				buildResult.Succeded ++;

			} catch ( Exception e ) {
				Log.Error( e.Message );
				buildResult.Failed ++;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFolder"></param>
		/// <returns></returns>
		List<AssetFile> GatherAssetFiles ( string sourceFolder, string targetFolder )
		{
			var files = Directory	
						.EnumerateFiles( Path.GetFullPath(sourceFolder), "*", SearchOption.AllDirectories )
						.Select( path => new AssetFile( path, sourceFolder, targetFolder ) )
						.Where( file => file.KeyPath != ".content" )
						.ToList();

			return files;
		}

	}
}
