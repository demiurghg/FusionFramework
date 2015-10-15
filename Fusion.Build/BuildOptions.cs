using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Core.Shell;
using System.Diagnostics;

namespace Fusion.Build {

	public class BuildOptions {

		/// <summary>
		/// Input directory
		/// </summary>
		[CommandLineParser.Required]
		[CommandLineParser.Name("inputDir", "input directory")]
		public string InputDirectory { get; set; }
			
		/// <summary>
		/// Output directory
		/// </summary>
		[CommandLineParser.Required]
		[CommandLineParser.Name("outputDir", "output directory")]
		public string OutputDirectory { get; set; }
			
		/// <summary>
		/// Temporary directory
		/// </summary>
		[CommandLineParser.Name("temp", "temporary directory")]
		public string TempDirectory { get; set; }
			
		/// <summary>
		/// Force rebuild
		/// </summary>
		[CommandLineParser.Name("force", "force rebuild")]
		public bool ForceRebuild { get; set; }
			
		/// <summary>
		/// Force rebuild
		/// </summary>
		[CommandLineParser.Name("help", "print detailed help")]
		public bool Help { get; set; }
			

		/// <summary>
		/// Full input directory
		/// </summary>
		[CommandLineParser.Ignore]
		public string FullInputDirectory { 
			get {
				return Path.GetFullPath( InputDirectory );
			}
		}

		/// <summary>
		/// Full output directory
		/// </summary>
		[CommandLineParser.Ignore]
		public string FullOutputDirectory { 
			get {
				return Path.GetFullPath( OutputDirectory );
			}
		}

		/// <summary>
		/// Full temp directory
		/// </summary>
		[CommandLineParser.Ignore]
		public string FullTempDirectory { 
			get {
				return Path.GetFullPath( TempDirectory );
			}
		}


		[CommandLineParser.Ignore]
		public string ContentIniFile {
			get {
				return Path.Combine( FullInputDirectory, ".content" );
			}
		}


		/// <summary>
		/// Ctor
		/// </summary>
		public BuildOptions ()
		{
		}



		/// <summary>
		/// Checks options
		/// </summary>
		public void CheckOptionsAndMakeDirs ()
		{
			if ( InputDirectory==null ) {
				throw new BuildException("Input directory is not specified (/in:)");
			}
			if ( OutputDirectory==null ) {
				throw new BuildException("Output directory is not specified (/out:)");
			}
			if ( TempDirectory==null ) {
				TempDirectory = Path.GetTempPath();
			} else {
				if ( !Directory.Exists(FullTempDirectory) ) {
					var dir = Directory.CreateDirectory( FullTempDirectory );
					dir.Attributes = FileAttributes.Hidden;
				}
			}

			if ( !Directory.Exists(FullOutputDirectory) ) {
				var dir = Directory.CreateDirectory( FullOutputDirectory );
			}

			if ( !Directory.Exists(FullInputDirectory) ) {
				throw new BuildException("Input directory does not exist");
			}

			if ( !File.Exists(ContentIniFile) ) {
				throw new BuildException("File '.content' not found");
			}
				

		}
	}


}
