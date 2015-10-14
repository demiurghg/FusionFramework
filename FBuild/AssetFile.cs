using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Content;

namespace FBuild {

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
		/// <param name="path"></param>
		public AssetFile ( string fullPath, string fullInputDirPath, string fullOutputDirPath )
		{
			this.fullPath	=	fullPath;
			this.keyPath	=	ContentUtils.BackslashesToSlashes( ContentUtils.MakeRelativePath( fullInputDirPath + "\\", fullPath ) );
			this.outputDir	=	fullOutputDirPath;

			this.processed	=	false;
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
			return File.OpenWrite( FullTargetPath );
		}
	}
}
