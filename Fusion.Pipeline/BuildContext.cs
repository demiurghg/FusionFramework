using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using Fusion.Core.Mathematics;
using Fusion.Content;


namespace Fusion.Pipeline {

	/// <summary>
	/// 
	/// </summary>
	public class BuildContext {

		string contentFolder;
		string targetFolder;
		string tempFolder;
		List<Asset> assetCollection;
		ContentProject	contentProject;
		


		/// <summary>
		/// Creates instance of BuildContext
		/// </summary>
		/// <param name="contentFolder"></param>
		internal BuildContext ( string contentFolder, string tempFolder, string targetFolder, ContentProject contentProject )
		{
			this.contentFolder		=	contentFolder;
			this.targetFolder		=	targetFolder;
			this.tempFolder			=	tempFolder;
			this.contentProject		=	contentProject;
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

				var asset = (T)Activator.CreateInstance( typeof(T), new object[]{ keyPath } );
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
		/// Generates temporary file name for given key with given extension. 
		/// </summary>
		/// <param name="key">unique key string value</param>
		/// <param name="ext">Desired extension with leading dot</param>
		/// <returns>Full path for generated file name.</returns>
		public string GetTempFileName ( string key, string ext )
		{
			var fileName	=	ContentUtils.CalculateMD5Hash( key.ToLower(), true );

			return Path.Combine( tempFolder, fileName ) + ext;
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
		/// Resolves file name and copy file file to stream.
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
		/// Resolves file name and copy file file to stream.
		/// </summary>
		/// <param name="sourceFile"></param>
		/// <param name="targetStream"></param>
		public void CopyTo( string sourceFile, BinaryWriter writer )
		{
			var data = File.ReadAllBytes( Resolve( sourceFile ) ); 
			writer.Write( data );
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
			return contentProject.ResolveContentPath( path );
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


			if (asset!=null /*&& !asset.IgnoreToolChanges*/) {
				dependencies.Add( asset.GetType().Assembly.Location );
			} //*/

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
			return contentProject.ResolveBinaryPath( exePath );
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
