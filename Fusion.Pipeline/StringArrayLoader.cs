using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Engine.Common;

namespace Fusion.Pipeline {

	/// <summary>
	/// Loads string from text file.
	/// </summary>
	[ContentLoader(typeof(string[]))]
	class StringArrayLoader : ContentLoader {

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
				return Encoding.Default.GetString( bytes ).Split(new[]{"\r\n","\n"}, StringSplitOptions.None );
			}

			if (assetPath.ToLowerInvariant().Contains("|utf8")) {
				return Encoding.UTF8.GetString( bytes ).Split(new[]{"\r\n","\n"}, StringSplitOptions.None );
			}

			if (assetPath.ToLowerInvariant().Contains("|utf7")) {
				return Encoding.UTF7.GetString( bytes ).Split(new[]{"\r\n","\n"}, StringSplitOptions.None );
			}

			if (assetPath.ToLowerInvariant().Contains("|utf32")) {
				return Encoding.UTF32.GetString( bytes ).Split(new[]{"\r\n","\n"}, StringSplitOptions.None );
			}

			if (assetPath.ToLowerInvariant().Contains("|ascii")) {
				return Encoding.ASCII.GetString( bytes ).Split(new[]{"\r\n","\n"}, StringSplitOptions.None );
			}

			return Encoding.Default.GetString( bytes ).Split(new[]{"\r\n","\n"}, StringSplitOptions.None );
		}
	}
}
