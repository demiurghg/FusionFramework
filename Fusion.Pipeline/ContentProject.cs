using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Reflection;
using Fusion;
using Fusion.Content;
using Microsoft.Win32;


namespace Fusion.Pipeline {
	public class ContentProject {

		class AssetDesc {
			public string Path;
			public string Type;
			public List<KeyValuePair<string, string>> Parameters;

			public AssetDesc ( XmlNode node ) 
			{
				if (node.Attributes["Path"]==null) {
					throw new ContentException("Missing 'Path' attribute in asset node");
				}

				if (node.Attributes["Type"]==null) {
					throw new ContentException("Missing 'Type' attribute in asset node");
				}

				Path		=	node.Attributes["Path"].Value;
				Type		=	node.Attributes["Type"].Value;

				Parameters	=	node
								.ChildNodes
								.Cast<XmlNode>()
								.Select( n => new KeyValuePair<string, string>( n.Name, n.InnerText ) )
								.ToList();
			}
		}

		List<string>	contentDirs;
		List<string>	binaryDirs;
		List<string>	assemblies;
		List<AssetDesc>	assetsDesc;
		List<Asset>		assets;

		/// <summary>
		/// Loads content project from file.
		/// </summary>
		/// <param name="fileName"></param>
		public ContentProject ( string fileName )
		{
			var text = File.ReadAllText( fileName );

			var doc = new XmlDocument();
			doc.LoadXml( text );

			contentDirs	=	doc
							.SelectNodes( "/ContentProject/ContentDirectories/Item")
							.Cast<XmlNode>()
							.Select( n => n.InnerText )
							.Distinct()
							.Select( p => ConvertPath( p ) )
							.ToList();

			contentDirs.Insert( 0, Path.GetDirectoryName( fileName ) );

			binaryDirs	=	doc
							.SelectNodes( "/ContentProject/BinaryDirectories/Item")
							.Cast<XmlNode>()
							.Select( n => n.InnerText )
							.Distinct()
							.Select( p => ConvertPath( p ) )
							.ToList();

			assemblies	=	doc
							.SelectNodes( "/ContentProject/Assemblies/Item")
							.Cast<XmlNode>()
							.Select( n => n.InnerText )
							.Distinct()
							.ToList();

	
		   assetsDesc	=	doc.SelectNodes( "/ContentProject/Asset")
							.Cast<XmlNode>()
							.Select( n => new AssetDesc( n ) )
							.ToList();

		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="forced"></param>
		/// <param name="sourceDirectory"></param>
		/// <param name="targetDirectory"></param>
		/// <param name="message"></param>
		/// <param name="selection"></param>
		public bool Build ( bool forced, string sourceDirectory, string targetDirectory, IEnumerable<string> selection )
		{
			Log.Message("Building content...");
			Log.Message("");

			Log.Message("Search content directories:");
			foreach ( var dir in contentDirs ) {
				Log.Message("  {0}", dir );
			}
			Log.Message("");

			Log.Message("Search binary directories:");
			foreach ( var dir in binaryDirs ) {
				Log.Message("  {0}", dir );
			}
			Log.Message("");

			Log.Message("Assemblies:");
			foreach ( var asm in this.assemblies ) {
				Log.Message("  {0}", asm );
			}
			Log.Message("");

			var assemblies = this.assemblies
						.Select( p => ResolvePath( p, binaryDirs ) )
						.ToList();

			//	Load assemblies :
			foreach ( var a in assemblies ) {
				Assembly.LoadFrom( a );
			}

			Log.Message("{0} assets found", assetsDesc.Count );

			//	Gather asset types :
			var types = Asset.GatherAssetTypes();
			Log.Message("{0} assets types found", types.Length );




			//	get assembly dlls
			//	load assemblies	- warning on missing dlls
			//	assign asset list - warning/error on missing asset types
			//	gather asset types :
			//	build

			//var a = Assembly.LoadFrom( "q.dll" );
			//AppDomain.CurrentDomain.Add
			//AppDomain.CurrentDomain.Load( 
			return false;
		}



		/// <summary>
		/// Resolves path using provided list of directories.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="dirs"></param>
		/// <returns></returns>
		string ResolvePath ( string path, IEnumerable<string> dirs )
		{
			if (path==null) {
				throw new ArgumentNullException("path");
			}

			if ( Path.IsPathRooted( path ) ) {
				return path;
			}

			//
			//	make search list :
			//
			foreach ( var dir in dirs ) {
				//Log.Message("...{0}", dir );
				var fullPath = Path.GetFullPath( Path.Combine( dir, path ) );
				if ( File.Exists( fullPath ) ) {
					return fullPath;
				}
			}

			throw new ContentException(string.Format("Path '{0}' not resolved", path));
		}



		/// <summary>
		/// Converts path
		///		1. Relative path to full path.
		///		2. $(EnvVarName) -> Environment variable value.
		///		3. $(HKEY_LOCAL_MACHINE\...) -> Registry value.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		string ConvertPath ( string path )
		{
			string newPath = null;

			if (path.StartsWith("$(") && path.EndsWith(")")) {
				
				//	regestry value :
				if (path.Contains(@"\")) {

					try {
						var pair = path.Substring(2, path.Length-3).Split('@');

						string key, name;

						if (pair.Length!=2) {
							key = pair[0];
							name = null;
						} else {
							key = pair[0];
							name = pair[1];
						}

						var regValue = Registry.GetValue( key, name, null );

						newPath	= regValue as string;

					} catch ( Exception e ) {

						Log.Warning("Bad registry key: {0}\r\n{1}", path, e.Message );
						return null;
					}

				} else {
					//	environment variable
					var varName = path.Substring(2, path.Length-3);
					
					newPath	=	Environment.GetEnvironmentVariable( varName );

				}

			} else {
				if (Path.IsPathRooted( path )) {
					newPath	=	path;
				} else {
					newPath	= path;
				}
			}

			/*if (!Directory.Exists( newPath )) {
				Log.Message("  {0} -> {1} : does not exist", path, newPath ?? "[null]" );
			} else {
				Log.Message("  {0} -> {1}", path, newPath ?? "[null]" );
			} */

			return newPath;
		}
	}
}
