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
			var parser = new CommandLineParser( options, true );


			try {
				parser.ParseCommandLine( args );

				var contentProject	=	new ContentProject( options.ProjectFile );

				var force		=	options.ForceRebuild;
				var sourceDir	=	Path.GetDirectoryName( Path.GetFullPath( options.ProjectFile ) );
				var outputDir	=	options.OutputDirectory;
				var items		=	options.Items.Any() ? options.Items : null;	  

				contentProject.Build( force, sourceDir, outputDir, items );

			} catch ( Exception ex ) {

				Log.Error( ex.Message );
				return 1;
			}

			return 0;
		}
	}
}
