using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Fusion {
	/// <summary>
	/// Represents a wildcard running on the
	/// <see cref="System.Text.RegularExpressions"/> engine.
	/// </summary>
	public static class Wildcard {

		/// <summary>
		/// Converts a wildcard to a regex.
		/// </summary>
		/// <param name="pattern">The wildcard pattern to convert.</param>
		/// <returns>A regex equivalent of the given wildcard.</returns>
		public static string WildcardToRegex ( string pattern )
		{
			return "^" + Regex.Escape( pattern ).
			 Replace( "\\*", ".*" ).
			 Replace( "\\?", "." ) + "$";
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="pattern"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool Match ( string value, string pattern, bool ignoreCase = false )
		{
			Regex regex = new Regex( WildcardToRegex( pattern ), ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None ); 

			return regex.IsMatch( value );
		}
	}
}
