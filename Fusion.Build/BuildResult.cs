using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Fusion.Build.Processors;
using Fusion.Core.IniParser;
using Fusion.Core.IniParser.Model;
using Fusion;

namespace Fusion.Build {

	public class BuildResult {

		public int Total	;
		public int Ignored	;
		public int Succeded ;
		public int Failed	;
		public int UpToDate	;

		public int Skipped {
			get {
				return Total /*- Ignored*/ - Succeded - UpToDate - Failed;
			}
		}

		public BuildResult ()
		{
			Total		=	0;
			Ignored		=	0;
			Succeded	=	0;
			Failed		=	0;
		}

	}
}
