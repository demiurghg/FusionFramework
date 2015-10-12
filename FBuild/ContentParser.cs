using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;

namespace FBuild {
	class ContentParser {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="inputDirectory"></param>
		public ContentParser ( string fileName, string inputDirectory )
		{
			var lines = File.ReadAllLines(fileName)
						.Select( line1 => line1.Trim(' ', '\t') )
						.Where( line2 => !line2.StartsWith("#") )
						.Where( line3 => !string.IsNullOrWhiteSpace(line3) )
						.ToArray();

			foreach ( var line in lines ) {
				Log.Message("{0}" , line );
			}

		}
		
	}
}
