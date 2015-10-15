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
using Fusion.Core.IniParser;
using FBuild.Processors;


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

				if ( options.Help ) {
					PrintHelp( parser );
					return 0;
				}

				options.CheckOptionsAndMakeDirs();

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

            Log.Message("Asset Processors:");

            Log.Message("");

			var bindings = AssetProcessorBinding.GatherAssetProcessors();

			foreach ( var bind in bindings ) {
				Log.Message( "  {0} - {1}", bind.Name, bind.Type.Name );
									 
				var proc = bind.CreateAssetProcessor();
				var prs  = new CommandLineParser( proc, bind.Name );

                foreach (string optional in prs.OptionalUsageHelp) {
                    Log.Message("    {0}", optional);
                }

	            Log.Message("");
			}

		}
	}
}
