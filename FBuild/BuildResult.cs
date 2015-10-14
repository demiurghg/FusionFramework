using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using FBuild.Processors;
using Fusion.Core.IniParser;
using Fusion.Core.IniParser.Model;
using Fusion;

namespace FBuild {

	class BuildResult {

		public int Total	;
		public int Ignored	;
		public int Succeded ;
		public int Skipped	;
		public int Failed	;
		public int UpToDate	;


		public BuildResult ()
		{
			Total		=	0;
			Ignored		=	0;
			Succeded	=	0;
			Skipped		=	0;
			Failed		=	0;
		}

	}
}
