using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using FBuild.Processors;
using Fusion.Core.IniParser;
using Fusion.Core.IniParser.Model;
using Fusion;
using Fusion.Content;

namespace FBuild {

	public class BuildContext {

		/// <summary>
		/// 
		/// </summary>
		public BuildOptions Options {
			get; private set;
		}


		IEnumerable<string> toolPaths;



		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		public BuildContext ( BuildOptions options )
		{
			this.Options	=	options;

			Log.Message("Source directory:");
			Log.Message("  {0}", options.FullInputDirectory );
			Log.Message("");

			
			Log.Message("Target directory:");
			Log.Message("  {0}", options.FullOutputDirectory );
			Log.Message("");

			
			Log.Message("Temp directory:");
			Log.Message("  {0}", options.FullTempDirectory );
			Log.Message("");


			Log.Message("Tool paths:");
			toolPaths		=	GetToolPaths();

			foreach ( var toolPath in toolPaths ) {
				Log.Message("  {0}", toolPath );
			}

			Log.Message("");
			 
		}

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="dirs"></param>
		/// <returns></returns>
		string ResolvePath ( string path, IEnumerable<string> dirs )
		{
			if (path==null) {
				throw new ArgumentNullException("path");
			}

			if ( Path.IsPathRooted( path ) ) {
				if (File.Exists( path )) {
					return path;
				} else {
					throw new BuildException(string.Format("Path '{0}' not resolved", path));
				}
			}

			//
			//	make search list :
			//
			foreach ( var dir in dirs ) {
				//Log.Message("...{0}", dir );
				var fullPath = Path.GetFullPath( Path.Combine( dir, path ) );
				if ( File.Exists( fullPath ) ) {
					return fullPath;
				}
			}

			throw new BuildException(string.Format("Path '{0}' not resolved", path));
		}



		/// <summary>
		/// Generates temporary file name for given key with given extension. 
		/// </summary>
		/// <param name="key">unique key string value</param>
		/// <param name="ext">Desired extension with leading dot</param>
		/// <returns>Full path for generated file name.</returns>
		public string GetTempFileName ( string key, string ext )
		{
			var fileName	=	ContentUtils.CalculateMD5Hash( key.ToLower(), true );

			return Path.Combine( Options.FullTempDirectory, fileName ) + ext;
		}



		/// <summary>
		/// Copies file file to stream.
		/// </summary>
		/// <param name="fullSourceFileName"></param>
		/// <param name="targetStream"></param>
		public void CopyFileTo( string fullSourceFileName, Stream targetStream )
		{
			using ( var source = File.OpenRead( fullSourceFileName ) ) { 
				source.CopyTo( targetStream );
			}
		}



		/// <summary>
		/// Copies file file to stream.
		/// </summary>
		/// <param name="fullSourceFileName">Resolved source file name</param>
		/// <param name="targetStream"></param>
		public void CopyFileTo( string fullSourceFileName, BinaryWriter writer )
		{
			var data = File.ReadAllBytes( fullSourceFileName ); 
			writer.Write( data );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IEnumerable<string> GetToolPaths ()
		{
			var fusionBin	=	Environment.GetEnvironmentVariable("FUSION_BIN");

			if (fusionBin==null) {
				Log.Warning("FUSION_BIN environment variable is not set.");
			}

			return new[]{ fusionBin };
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="exePath"></param>
		/// <param name="commandLine"></param>
		public void RunTool ( string exePath, string commandLine )
		{
			Log.Debug("...exec: {0} {1}", exePath, commandLine );

			ProcessStartInfo psi = new ProcessStartInfo();
			psi.RedirectStandardInput	=	true;
			psi.RedirectStandardOutput	=	true;
			psi.RedirectStandardError	=	true;
			psi.FileName				=	ResolvePath( exePath, toolPaths );
			psi.Arguments				=	commandLine;
			psi.UseShellExecute			=	false;
			psi.CreateNoWindow			=	true;

			int exitCode = 0;

			string stdout;
			string stderr;


			using ( Process proc = Process.Start( psi ) ) {
				stdout = proc.StandardOutput.ReadToEnd().Trim(new[]{'\r', '\n'});
				stderr = proc.StandardError.ReadToEnd().Trim(new[]{'\r', '\n'});
				proc.WaitForExit();
				exitCode = proc.ExitCode;
			}

			Log.Debug( "{0}", stdout );
				
			if ( exitCode != 0 ) {
				throw new ToolException( string.Format("Failed to launch tool:\r\n{0} {1}\r\n{2}", exePath, commandLine, stderr ) );
			} else {
				if (!string.IsNullOrWhiteSpace(stderr)) {				
					Log.Warning( "{0}", stderr.Trim(new[]{'\r', '\n'}) );
				}
			}
		}
	}
}
