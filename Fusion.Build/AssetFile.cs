using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Fusion;
using Fusion.Core.Content;

namespace Fusion.Build {

	public class AssetFile {
			
		string fullPath;
		string keyPath;
		string outputDir;
		bool processed;

		/// <summary>
		/// Logical relativee path to file to be built.
		/// </summary>
		public string KeyPath { 
			get { 
				return keyPath; 
			} 
		}

		/// <summary>
		/// Full actual path to asset file
		/// </summary>
		public string FullSourcePath { 
			get { 
				return fullPath; 
			} 
		}

		/// <summary>
		/// Is file have been proceessed.
		/// Could be assigned to TRUE only.
		/// </summary>
		public bool IsProcessed { 
			get { return processed; }
			set {
				if (value==false) {
					throw new BuildException("BuildFile.IsProcessed may be assigned to True only");
				}
				processed = value;
			}
		}



		/// <summary>
		/// Gets target file name.
		/// </summary>
		public string TargetName {
			get {
				return ContentUtils.GetHashedFileName( KeyPath, ".asset" );
			}
		}



		/// <summary>
		/// Full target file path
		/// </summary>
		public string FullTargetPath {
			get {
				return Path.Combine( outputDir, TargetName );
			}
		}


		byte[] argHash;

		public string[] BuildArgs {
			set {
				argHash = ContentUtils.CalclulateMD5HashBytes( string.Join(" ", value) );
			}
		}


		/// <summary>
		/// Is asset up-to-date.
		/// </summary>
		public bool IsUpToDate {
			get {
				if ( !File.Exists( FullTargetPath ) ) {
					return false;
				}

				var targetTime	=	File.GetLastWriteTime( FullTargetPath );
				var sourceTime	=	File.GetLastWriteTime( FullSourcePath );

				if (targetTime < sourceTime) {
					return false;
				}

				return true;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public bool IsParametersEqual ()
		{
			var newParamHash = argHash;
			var len = newParamHash.Length;
			var oldParamHash = new byte[ len ];

			using ( var stream = OpenTargetStreamRead() ) {
				stream.Read( oldParamHash, 0, len );
			}

			return Enumerable.SequenceEqual( newParamHash, oldParamHash );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="fullPath">Full path to this asset file</param>
		/// <param name="contentDir">Directory where file had been found.</param>
		/// <param name="context">Build context</param>
		public AssetFile ( string fullPath, string contentDir, BuildContext context )
		{
			this.outputDir		=	context.Options.FullOutputDirectory;
			this.fullPath		=	fullPath;
			this.keyPath		=	ContentUtils.BackslashesToSlashes( ContentUtils.MakeRelativePath( contentDir + "\\", fullPath ) );

			this.processed		=	false;
		}



		/// <summary>
		/// Opens source stream file
		/// </summary>
		/// <returns></returns>
		public Stream OpenSourceStream ()
		{	
			return File.OpenRead( FullSourcePath );
		}



		/// <summary>
		/// Opens target file stream
		/// </summary>
		/// <returns></returns>
		public Stream OpenTargetStream ()
		{
			var fileStream	=	File.Open( FullTargetPath, FileMode.Create, FileAccess.Write );
			var zipStream	=	new DeflateStream( fileStream, CompressionMode.Compress, false );

			var newParamHash = argHash;
			var len = newParamHash.Length;

			zipStream.Write( newParamHash, 0, len );

			return zipStream;
		}



		/// <summary>
		/// Opens target file stream
		/// </summary>
		/// <returns></returns>
		Stream OpenTargetStreamRead ()
		{
			var fileStream	=	File.Open( FullTargetPath, FileMode.Open, FileAccess.Read );
			var zipStream	=	new DeflateStream( fileStream, CompressionMode.Decompress, false );
			return zipStream;
		}
	}
}
