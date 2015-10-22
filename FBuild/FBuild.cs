using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Fusion;
using Fusion.Core;
using Fusion.Core.Shell;
using Fusion.Core.Content;
using Fusion.Core.IniParser;
using Fusion.Core.Utils;
using Fusion.Build;
using Fusion.Build.Processors;


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

				if ( args.Any( a => a=="/help" )) {
					PrintHelp( parser );
					return 0;
				}

				if ( !parser.ParseCommandLine( args ) ) {
					return 1;
				}

				Builder.Build( options );


			} catch ( Exception ex ) {
				parser.ShowError( ex.Message );
				return 1;
			}

			return 0;
		}



		/// <summary>
		/// 
		/// </summary>
		static void PrintHelp ( CommandLineParser parser )
		{
			var name = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);

            Log.Message("Usage: {0} {1}", name, string.Join(" ", parser.RequiredUsageHelp));

            if ( parser.OptionalUsageHelp.Any() ) {
                Log.Message("");
                foreach (string optional in parser.OptionalUsageHelp) {
                    Log.Message("    {0}", optional);
                }
            }

	        Log.Message("");
	        Log.Message("");

            Log.Message("Asset Processors:");

            Log.Message("");

			var bindings = AssetProcessorBinding.GatherAssetProcessors();

			foreach ( var bind in bindings ) {
				Log.Message( "  {0} - {1}", bind.Name, bind.Type.Name );
				Log.Message( "    {0}", bind.Type.GetCustomAttribute<AssetProcessorAttribute>().Description );
	            Log.Message("");
									 
				var proc = bind.CreateAssetProcessor();
				var prs  = new CommandLineParser( proc, bind.Name );

				if (prs.OptionalUsageHelp.Any()) {
					foreach (string optional in prs.OptionalUsageHelp) {
						Log.Message("    {0}", optional);
					}
				} else {
					Log.Message("    <no options>");
				}

	            Log.Message("");
	            Log.Message("");
			}

		}
	}
}
