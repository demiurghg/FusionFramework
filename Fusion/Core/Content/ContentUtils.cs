using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Fusion.Core.Mathematics;
using Fusion.Core.Test;
using System.Runtime.InteropServices;


namespace Fusion.Content {
	public static class ContentUtils {


		/// <summary>
		/// Computes MD5 hash for given input string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string CalculateMD5Hash( string input, bool upperCase=true )
		{
			// step 1, calculate MD5 hash from input
			MD5 md5 = System.Security.Cryptography.MD5.Create();
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);
			string format = upperCase ? "X2" : "x2";
 
			// step 2, convert byte array to hex string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++) {
				sb.Append(hash[i].ToString(format));
			}
			return sb.ToString();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string BackslashesToSlashes ( string input ) 
		{
			var words	=	input.Split(new[]{'\\', '/'}, StringSplitOptions.RemoveEmptyEntries);
			var path	 =	string.Join( "/", words );

			return path;
		}


		[Test]
		static void TestBackslashesToSlashes()
		{
			var a = @"C:/Test/text.txt";
			var b = BackslashesToSlashes(@"\/C:////Test\\text.txt/");
			Tester.AreEqual( a, b, "{0} != {1}", a, b );

			/*var c = @"./text.txt";
			var d = BackslashesToSlashes(@"\text.txt/");
			Tester.AreEqual( c, d, "{0} != {1}", c, d );*/
		}



		[Test]
		static void TestHashEquality ()
		{
			var a = GetHashedFileName(@"test/text.jpg", ".hash");
			var b = GetHashedFileName(@"\//Test\\\text.txt", ".hash");
			Tester.AreEqual( a, b, "{0} != {1}", a, b );
		}


		/// <summary>
		/// Get full path without extension
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		static string GetPathWithoutExtension ( string path )
		{
			return Path.Combine( Path.GetDirectoryName( path ), Path.GetFileNameWithoutExtension( path ) );
		}// */



		/// <summary>
		/// Generates almost unique file name for given string and extension
		/// </summary>
		/// <param name="assetPath"></param>
		/// <param name="ext"></param>
		/// <returns></returns>
		public static string GetHashedFileName ( string assetPath, string ext )
		{
			return CalculateMD5Hash( GetPathWithoutExtension( BackslashesToSlashes(assetPath).ToLower()) ) + ext;
		}



		/// <summary>
		/// Checks whether cached file is out of date
		/// </summary>
		/// <param name="cachedFileName"></param>
		/// <param name="dependencies"></param>
		/// <returns></returns>
		static public bool IsCachedFileUpToDate ( string cachedFileName, string resolvedFileName, IEnumerable<string> dependencies = null )
		{
			//	cached file does not exist
			//  is needs to be updated :
			if ( !File.Exists( cachedFileName ) ) {
				return false;
			}

			var cachedTime	=	File.GetLastWriteTime( cachedFileName );

			//	if file exist - check creation time, otherwice pass it by
			if ( File.Exists( resolvedFileName ) ) {
				var srcTime = File.GetLastWriteTime( resolvedFileName );

				if ( cachedTime < srcTime ) {
					return false;
				}
			} else {
				Log.Warning("Source file {0} does not exist", resolvedFileName );
			}

			//	check all dependencies in the same way :
			if ( dependencies!=null ) {

				foreach ( var depName in dependencies ) {

					//	skip if dependecy file does not exits
					if ( File.Exists( depName ) ) {

						var depTime	=	File.GetLastWriteTime( depName );

						if ( cachedTime < depTime ) {
							return false;
						}
					}

				}
			}

			return true;
		} 



		/// <summary>
		/// 
		/// </summary>
		/// <param name="cachedFileName"></param>
		/// <param name="srcFileName"></param>
		/// <param name="dependencyListFileName"></param>
		/// <returns></returns>
		static public bool IsCachedFileUpToDate ( string cachedFileName, string srcFileName, string dependencyListFileName )
		{
			var dependencies = File.Exists( dependencyListFileName ) ? File.ReadAllLines( dependencyListFileName ) : null;

			return IsCachedFileUpToDate( cachedFileName, srcFileName, dependencies );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public static bool WildcardPathMatch( string text, string pattern, bool caseSensitive )
		{
			var regexPattern = "^" + Regex.Escape( pattern ).
									 Replace( "\\*", ".*" ).
									 Replace( "\\?", "." ) + "$";

			var regex = new Regex( regexPattern, caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase );

			return regex.IsMatch( text );

		}



		/// <summary>
		/// Class for path comaprison
		/// </summary>
		public class PathComparer : IComparer<string> {

			[DllImport("shlwapi.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
			static extern int StrCmpLogicalW(String x, String y);

			public int Compare(string x, string y) {

				var pathX = x.Split( new[]{'/', '\\'}, StringSplitOptions.RemoveEmptyEntries );
				var pathY = y.Split( new[]{'/', '\\'}, StringSplitOptions.RemoveEmptyEntries );

				var min	  = Math.Min( pathX.Length, pathY.Length );

				
				for ( int i = 0; i<min; i++ ) {

					if ( i == min-1 ) {
						
						var lenD = pathY.Length - pathX.Length;
						var extD = StrCmpLogicalW( Path.GetExtension(pathX[i]), Path.GetExtension(pathY[i]) );
						
						if (lenD!=0) {
							return lenD;
						} else if (extD!=0) {
							return extD;
						} else {
							return StrCmpLogicalW( pathX[i], pathY[i] );
						}
					}

					int cmp = StrCmpLogicalW( pathX[i], pathY[i] );

					if (cmp!=0) {
						return cmp;
					}
				}

				return 0;
			}

		}

	}
}
