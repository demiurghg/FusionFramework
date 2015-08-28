using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Fusion;
using Fusion.Shell;
using Fusion.Pipeline;


namespace FBuild {
	class FBuild {

		static int Main ( string[] args )
		{
			Thread.CurrentThread.CurrentCulture	=	System.Globalization.CultureInfo.InvariantCulture;
			Trace.Listeners.Add( new StdTraceListener() );

			var options = new BuildOptions();
			var parser = new CommandLineParser( options, true );


			try {
				parser.ParseCommandLine( args );

				var assetCollection = AssetCollection.Load( options.ProjectFile );

				var force		=	options.ForceRebuild;
				var sourceDir	=	Path.GetDirectoryName( Path.GetFullPath( options.ProjectFile ) );
				var outputDir	=	options.OutputDirectory;
				var message		=	"";
				var items		=	options.Items.Any() ? options.Items : null;	  

				assetCollection.Build( force, sourceDir, outputDir,	out message, items );

			} catch ( Exception ex ) {

				Log.Error( ex.Message );
				return 1;
			}

			return 0;
		}
	}
}
