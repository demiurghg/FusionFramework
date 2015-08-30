using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using Fusion.Mathematics;
using Fusion.Content;


namespace Fusion.Pipeline {

	/// <summary>
	/// 
	/// </summary>
	public class BuildContext {

		/// <summary>
		/// Fusion binary folder
		/// </summary>
		string FusionBinary { 
			get { 
				return Environment.GetEnvironmentVariable("FUSION_BIN"); 
			} 
		}

		/// <summary>
		/// Fusion content folders
		/// </summary>
		string FusionContent { 
			get { 
				return Environment.GetEnvironmentVariable("FUSION_CONTENT"); 
			} 
		}


		/// <summary>
		/// Fusion content directories
		/// </summary>
		ICollection<string> FusionContentDirs {
			get {
				return new[]{ FusionContent };//FusionContent.Split( new[]{';'}, StringSplitOptions.RemoveEmptyEntries );
			}
		}



		/// <summary>
		/// Full path to content folder
		/// </summary>
		public string ContentDirectory {
			get {
				return Path.GetFullPath( contentFolder );
			}
		}

		string contentFolder;
		string targetFolder;
		List<Asset> assetCollection;

		


		/// <summary>
		/// Creates instance of BuildContext
		/// </summary>
		/// <param name="contentFolder"></param>
		internal BuildContext ( string contentFolder, string targetFolder, ContentProject contentProject )
		{
			this.contentFolder		=	contentFolder;
			this.targetFolder		=	targetFolder;
			this.assetCollection	=	new List<Asset>();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="asset"></param>
		public T AddAsset<T>( string keyPath ) where T: Asset
		{	
			var existingAsset = assetCollection.FirstOrDefault( a => a.AssetPath == keyPath );

			if (existingAsset==null) {

				var asset = Activator.CreateInstance<T>();
				asset.AssetPath = keyPath;
				assetCollection.Add( asset );

				Log.Message("...added: {0}", asset.AssetPath );

				return asset;

			} else {
				if ( typeof(T) == existingAsset.GetType() ) {
					//	that is ok.
					return (T)existingAsset;
				} else {
					throw new ContentException(string.Format("Attempt to add asset of different type: {0}", keyPath ));
				}
			}
		}



		/// <summary>
		/// Generates 
		/// </summary>
		/// <param name="key">unique key string value</param>
		/// <param name="ext">Desired extension with leading dot</param>
		/// <param name="resolve">Whether returned path is resolved relative to current directory</param>
		/// <returns></returns>
		public string GetTempFileName ( string key, string ext, bool resolve )
		{
			var di = Directory.CreateDirectory( Path.Combine( contentFolder, "Temp") );
			di.Attributes = FileAttributes.Directory|FileAttributes.Hidden;

			var fileName	=	ContentUtils.CalculateMD5Hash( key.ToLower(), true );

			if (resolve) {
				return Path.Combine( contentFolder, "Temp", fileName ) + ext;
			} else {
				return Path.Combine( "Temp", fileName ) + ext;
			}
		}



		/// <summary>
		/// Returns full target asset's path
		/// </summary>
		/// <param name="targetFileName"></param>
		/// <returns></returns>
		private string TargetPath ( Asset asset )
		{
			return Path.Combine( targetFolder, asset.Hash ) + ".asset";
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceFile"></param>
		/// <param name="targetStream"></param>
		public void CopyTo( string sourceFile, Stream targetStream )
		{
			using ( var source = File.OpenRead( Resolve( sourceFile ) ) ) { 
				source.CopyTo( targetStream );
			}
		}



		/// <summary>
		/// Opens and returns stream for given asset.
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		public Stream OpenTargetStream ( Asset asset )
		{
			var targetPath	= TargetPath( asset );			

			var fileStream	=	File.Open( targetPath, FileMode.Create, FileAccess.Write );
			var zipStream	=	new DeflateStream( fileStream, CompressionMode.Compress, false );
			return zipStream;
		}



		/// <summary>
		/// Resolves source file name.
		/// This function search first in content folder, then in $FUSION_CONTENT folder.
		///		- existing paths in content directory (by /content)
		///		- existing paths in %FUSION_CONTENT%
		/// </summary>
		/// <param name="sourceFile"></param>
		/// <returns></returns>
		public string Resolve ( string path )
		{
			if (path==null) {
				throw new ArgumentNullException("path");
			}

			if ( Path.IsPathRooted( path ) ) {
				//Log.Warning("Rooted path: {0}", path);
				return path;
			}

			//
			//	make search list :
			//
			var searchDirs	=	new List<string>();

			searchDirs.Add( contentFolder );
			searchDirs.AddRange( FusionContentDirs );

			foreach ( var dir in searchDirs ) {
				//Log.Message("...{0}", dir );
				var fullPath = Path.GetFullPath( Path.Combine( dir, path ) );
				if ( File.Exists( fullPath ) ) {
					return fullPath;
				}
			}

			throw new ContentException(string.Format("Path '{0}' not resolved", path));
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="patterns"></param>
		/// <returns></returns>
		public string[] ExpandAndResolveSearchPatterns ( string[] patterns )
		{
			List<string> fileNames = new List<string>();

			foreach ( var pattern in patterns ) {

				// skip empty patterns :
				if ( string.IsNullOrWhiteSpace(pattern) ) {
					Log.Warning("Empty pattern detected. Skipped.");
					continue;
				}

				//	pattern contains '*' or '?':
				if ( pattern.Contains('*') || pattern.Contains('?') ) {

					fileNames.AddRange( Directory.GetFiles( contentFolder, pattern, SearchOption.TopDirectoryOnly ) );

				} else {
					fileNames.Add( Resolve( pattern ) );
				}
			}

			return fileNames.ToArray();
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="src"></param>
		/// <param name="depList"></param>
		/// <param name="dst"></param>
		/// <param name="forced"></param>
		/// <returns></returns>
		public bool IsOutOfDate ( Asset asset, bool forced )
		{
			var targetFile		=	TargetPath( asset );
			var dependencies	=	ExpandAndResolveSearchPatterns( asset.Dependencies ).ToList();

			Log.Debug("Resolved dependencies:");
			foreach ( var dep in dependencies ) {
				Log.Debug("  {0}", dep);
			}

			//	build forced - always out of date :
			if (forced) {
				return true;
			}

			//	target file does not exist :
			if (!File.Exists(targetFile)) {
				return true;
			}


			if (asset!=null && !asset.IgnoreToolChanges) {
				dependencies.Add( asset.GetType().Assembly.Location );
			}// */

			var dstTime	=	File.GetLastWriteTime( targetFile );

			//	one of the deps is exist and write time > dst write time :
			foreach ( var file in dependencies ) {
				if ( File.Exists(file) && File.GetLastWriteTime(file) > dstTime ) {
					return true;
				}
			}

			return false;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="exePath"></param>
		/// <returns></returns>
		string ResolveToolPath ( string exePath )
		{
			if (File.Exists( exePath )) {
				return exePath;
			}

			var path = Path.Combine( FusionBinary, exePath );
			
			if (File.Exists(path)) {
				return path;
			}

			throw new ToolException( string.Format( "Path to '{0}' is not resolved", exePath ) );
		}


		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetFile"></param>
		public void CompressFile ( string targetFile )
		{
			using (FileStream originalFileStream = File.OpenRead(targetFile)) {
	
				using (FileStream compressedFileStream = File.Create(targetFile + ".cmp")) {

					using (DeflateStream compressionStream = new DeflateStream(compressedFileStream, CompressionMode.Compress)) {
						originalFileStream.CopyTo(compressionStream);
					}
				}
			}
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
			psi.FileName				=	ResolveToolPath( exePath );
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
