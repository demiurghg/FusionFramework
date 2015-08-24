using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Fusion.Mathematics;


namespace Fusion.Pipeline {
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
			return input.Replace('\\', '/');
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
			return CalculateMD5Hash( GetPathWithoutExtension(assetPath).ToLower() ) + ext;
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
	}
}
