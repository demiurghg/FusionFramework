using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Fusion;
using Fusion.Core.Shell;
using Fusion.Pipeline;


namespace FBuild {
	class FBuild {

		/// <summary>
		/// Run with following arguments:
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		static int Main ( string[] args )
		{
			Thread.CurrentThread.CurrentCulture	=	System.Globalization.CultureInfo.InvariantCulture;
			Trace.Listeners.Add( new StdTraceListener() );

			var options = new BuildOptions();
			var parser = new CommandLineParser( options );


			try {
				if ( !parser.ParseCommandLine( args ) ) {
					return 1;
				}

				var force		=	options.ForceRebuild;
				var inputDir	=	options.InputDirectory;
				var outputDir	=	options.OutputDirectory;
				var tempDir		=	options.TempDirectory;
				var items		=	options.Items.Any() ? options.Items : null;	  
				var inputFile	=	Path.Combine(inputDir, ".content");

				//
				//	Check arguments 
				//
				if ( inputDir==null ) {
					throw new Exception("Input directory is not specified (/in:)");
				}
				if ( outputDir==null ) {
					throw new Exception("Output directory is not specified (/out:)");
				}
				if ( tempDir==null ) {
					throw new Exception("Temporary directory is not specified (/temp:)");
				}

				if ( !Directory.Exists(inputDir) ) {
					throw new Exception("Input directory does not exist");
				}
				if ( !File.Exists(inputFile) ) {
					throw new Exception("File '.content' not found");
				}
				
				Log.Message("FBuild Tool");
				Log.Message("...input  : {0}", Path.GetFullPath(inputDir) );
				Log.Message("...output : {0}", Path.GetFullPath(outputDir) );
				Log.Message("...temp   : {0}", Path.GetFullPath(tempDir) );
				Log.Message("...tools  : {0}", Environment.GetEnvironmentVariable("FUSION_BIN"));
				///contentProject.Build( force, sourceDir, tempDir, outputDir, items );
				///
				//var contentParser = new ContentParser( inputFile, inputDir );




				///contentProject.SaveToFile( options.ProjectFile );


			} catch ( Exception ex ) {
				parser.ShowError( ex.Message );
				return 1;
			}

			return 0;
		}
	}
}
