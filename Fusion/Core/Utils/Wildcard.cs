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
	[Serializable]
	public class Wildcard : Regex {
		/// <summary>
		/// Initializes a wildcard with the given search pattern.
		/// </summary>
		/// <param name="pattern">The wildcard pattern to match.</param>
		public Wildcard ( string pattern )
			: base( WildcardToRegex( pattern ) )
		{
		}


		protected Wildcard( SerializationInfo info, StreamingContext context ) : base(info, context)
		{
			throw new NotImplementedException();
		} 


		/// <summary>
		/// Initializes a wildcard with the given search pattern and options.
		/// </summary>
		/// <param name="pattern">The wildcard pattern to match.</param>
		/// <param name="options">A combination of one or more
        /// <see cref="System.Text.RegularExpressions.RegexOptions"/>.</param>
		public Wildcard ( string pattern, RegexOptions options )
			: base( WildcardToRegex( pattern ), options )
		{
		}

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
		public static bool IsMatch ( string pattern, string value, bool caseSensitive = true )
		{
			Wildcard wildcard;
			
			if (!caseSensitive) {
				wildcard = new Wildcard( pattern, RegexOptions.IgnoreCase );
			} else {
				wildcard = new Wildcard( pattern );
			}

			return wildcard.IsMatch( value );
		}
	}
}
