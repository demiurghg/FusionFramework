using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Shell;
using System.Diagnostics;

namespace FBuild {

	public class BuildOptions {

		[CommandLineParser.Name("in", "Input directory")]
		public string InputDirectory { get; set; }
			
		[CommandLineParser.Name("out", "Output directory")]
		public string OutputDirectory { get; set; }
			
		[CommandLineParser.Name("temp", "Temporary directory")]
		public string TempDirectory { get; set; }
			
		[CommandLineParser.Name("force", "Force rebuild")]
		public bool ForceRebuild { get; set; }
			
		[CommandLineParser.Name("item", "Rebuild item")]
		public List<string> Items { get; set; }


		public BuildOptions ()
		{
			Items	=	new List<string>();
		}
	}


}
