﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Fusion.Content {

	/// <summary>
	/// Loads string from text file.
	/// </summary>
	[ContentLoader(typeof(string))]
	class StringLoader : ContentLoader {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		/// <param name="stream"></param>
		/// <param name="requestedType"></param>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		public override object Load ( Game game, Stream stream, Type requestedType, string assetPath )
		{
			var bytes = stream.ReadAllBytes();

			if (assetPath.ToLowerInvariant().Contains("|default")) {
				return Encoding.Default.GetString( bytes );
			}

			if (assetPath.ToLowerInvariant().Contains("|utf8")) {
				return Encoding.UTF8.GetString( bytes );
			}

			if (assetPath.ToLowerInvariant().Contains("|utf7")) {
				return Encoding.UTF7.GetString( bytes );
			}

			if (assetPath.ToLowerInvariant().Contains("|utf32")) {
				return Encoding.UTF32.GetString( bytes );
			}

			if (assetPath.ToLowerInvariant().Contains("|ascii")) {
				return Encoding.ASCII.GetString( bytes );
			}

			return Encoding.Default.GetString( bytes );
		}
	}
}
