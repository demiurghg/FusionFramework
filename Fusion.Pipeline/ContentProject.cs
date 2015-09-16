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
using System.ComponentModel;


namespace Fusion.Pipeline {
	public class ContentProject {


		List<string>	contentDirs;
		List<string>	binaryDirs;
		List<string>	contentDirsExpanded;
		List<string>	binaryDirsExpanded;
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
							.ToList();

			binaryDirs	=	doc
							.SelectNodes( "/ContentProject/BinaryDirectories/Item")
							.Cast<XmlNode>()
							.Select( n => n.InnerText )
							.Distinct()
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
		/// <returns></returns>
		public void SaveToFile (string path)
		{
			var doc		=	new XmlDocument();

			var root	=	doc.CreateElement("ContentProject");
			doc.AppendChild( root );

			var xmlContentDirs	=	doc.CreateElement("ContentDirectories");
			var xmlBinaryDirs	=	doc.CreateElement("BinaryDirectories");
			var xmlAssemblies	=	doc.CreateElement("Assemblies");
			
			root.AppendChild( doc.CreateComment(@"Directories to search for content.") );
			root.AppendChild( doc.CreateComment(@"Non-existing directories will be ignored.") );
			root.AppendChild( doc.CreateComment(@"Possible formats:") );
			root.AppendChild( doc.CreateComment(@"  Full path: ""C:\MyProject\BinaryDir\""") );
			root.AppendChild( doc.CreateComment(@"  Relative path: ""..\MyProject\BinaryDir""") );
			root.AppendChild( doc.CreateComment(@"  Environment variable: ""$(MY_ENV_VARIABLE)""") );
			root.AppendChild( doc.CreateComment(@"  Registry variable: ""$(HOTKEY_LOCAL_MACHINE\MyVariable)""") );
			root.AppendChild( doc.CreateComment(@"Note: Provide both x86 and x64 registry variables.") );
			root.AppendChild( doc.CreateComment(@"Note: Source directory (project) and current directory have highest priority.") );
			root.AppendChild( xmlContentDirs );

			root.AppendChild( doc.CreateComment(@"Directories to search for executable tools and assemblies.") );
			root.AppendChild( doc.CreateComment(@"Non-existing directories will be ignored.") );
			root.AppendChild( doc.CreateComment(@"Possible formats:") );
			root.AppendChild( doc.CreateComment(@"  Full path: ""C:\MyProject\ContentDir\""") );
			root.AppendChild( doc.CreateComment(@"  Relative path: ""..\MyProject\ContentDir""") );
			root.AppendChild( doc.CreateComment(@"  Environment variable: ""$(MY_ENV_VARIABLE)""") );
			root.AppendChild( doc.CreateComment(@"  Registry variable: ""$(HOTKEY_LOCAL_MACHINE\MyVariable)""") );
			root.AppendChild( doc.CreateComment(@"Note: Provide both x86 and x64 registry variables.") );
			root.AppendChild( doc.CreateComment(@"Note: Current directory has highest priority.") );
			root.AppendChild( xmlBinaryDirs );

			root.AppendChild( doc.CreateComment(@"Assemblies to load.") );
			root.AppendChild( doc.CreateComment(@"Non-existing assemblies will cause build error.") );
			root.AppendChild( xmlAssemblies );

			foreach ( var cdir in contentDirs ) {
				var item = doc.CreateElement("Item");
				item.InnerText = cdir;
				xmlContentDirs.AppendChild( item );
			}

			foreach ( var bdir in binaryDirs ) {
				var item = doc.CreateElement("Item");
				item.InnerText = bdir;
				xmlBinaryDirs.AppendChild( item );
			}

			foreach ( var assembly in assemblies ) {
				var item = doc.CreateElement("Item");
				item.InnerText = assembly;
				xmlAssemblies.AppendChild( item );
			}


			root.AppendChild( doc.CreateComment(@"Asset list.") );
			root.AppendChild( doc.CreateComment(@"Attributes 'Path' and 'Type' are required.") );
			root.AppendChild( doc.CreateComment(@"Path is unique asset identifier.") );
			root.AppendChild( doc.CreateComment(@"Type is an asset's class name.") );
			root.AppendChild( doc.CreateComment(@"Each child element is an asset parameter.") );
			root.AppendChild( doc.CreateComment(@"Parameters are parsed using TypeConverter.") );
			root.AppendChild( doc.CreateComment(@"Examples:") );
			root.AppendChild( doc.CreateComment(@"<A>3.14</A>    - float parameter A") );
			root.AppendChild( doc.CreateComment(@"<B>Foo</B>     - string parameter B") );
			root.AppendChild( doc.CreateComment(@"<C>8,8,8,8</C> - vector or color parameter C") );
			root.AppendChild( doc.CreateComment(@"<L>1</L>       - first element of list L") );
			root.AppendChild( doc.CreateComment(@"<L>2</L>       - second element of list L") );

			foreach ( var desc in assetsDesc )	{
				root.AppendChild( desc.ToXmlElement(doc) );
			} 

			doc.Save( path );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="forced"></param>
		/// <param name="sourceDirectory"></param>
		/// <param name="targetDirectory"></param>
		/// <param name="message"></param>
		/// <param name="selection"></param>
		public bool Build ( bool forced, string sourceDirectory, string tempDirectory, string targetDirectory, IEnumerable<string> selection )
		{
			int succeeded = 0;
			int failed = 0;

			contentDirsExpanded	=	this.contentDirs.Select( p => ConvertPath( p ) ).ToList();
			binaryDirsExpanded	=	this.binaryDirs.Select( p => ConvertPath( p ) ).ToList();

			//	add source and current directory :
			contentDirsExpanded.Insert( 0, sourceDirectory );
			contentDirsExpanded.Insert( 1, Directory.GetCurrentDirectory() );

			//	add current directory :
			binaryDirsExpanded.Insert( 0, Directory.GetCurrentDirectory() );

			Log.Message("---------- Content Build Started ----------");

			Log.Message("Source directory: {0}", Path.GetFullPath(sourceDirectory) );
			Log.Message("Target directory: {0}", Path.GetFullPath(targetDirectory) );
			Log.Message("Temp directory:   {0}", Path.GetFullPath(tempDirectory) );

			var targetDI = Directory.CreateDirectory( Path.GetFullPath(targetDirectory) );
			var tempDI = Directory.CreateDirectory( Path.GetFullPath(tempDirectory) );
			tempDI.Attributes = FileAttributes.Directory|FileAttributes.Hidden;



			Log.Message("Search content directories:");
			foreach ( var dir in contentDirsExpanded ) {
				Log.Message("  - {0}", dir );
			}

			Log.Message("Search binary directories:");
			foreach ( var dir in binaryDirsExpanded ) {
				Log.Message("  - {0}", dir );
			}

			Log.Message("Assemblies:");
			foreach ( var asm in this.assemblies ) {
				Log.Message("  - {0}", asm );
			}

			var assemblies = this.assemblies
						.Select( p => ResolvePath( p, binaryDirs ) )
						.ToList();


			//
			//	Load assemblies :
			//
			foreach ( var a in assemblies ) {
				Assembly.LoadFrom( a );
			}

			Log.Message("{0} assets found", assetsDesc.Count );


			//
			//	Gather asset types :
			//
			var types = Asset.GatherAssetTypes();
			Log.Message("{0} assets types found", types.Length );


			//
			//	Build content :
			//
			assets = assetsDesc
					.Select( a => a.CreateAsset( types ) )
					.ToList();

			//	reflect assets back to description :
			assetsDesc = assets
					.Select( a => new AssetDesc(a) )
					.ToList();


			HashSet<string> selectedNames = null;
			if ( selection!=null ) {
				selectedNames = new HashSet<string>( selection.Select( n => ContentUtils.GetHashedFileName( n, "") ) );
			}


			var buildContext	=	new BuildContext( sourceDirectory, tempDirectory, targetDirectory, this );

			//	check collisions :
			var collisions = GetHashCollisions(assets);

			if (collisions.Any()) {
				foreach (var coll in collisions) {
					Log.Error("  {0}", coll);
				}
				throw new ContentException("Hash collisions have been detected!");
			}


			//	build :
			foreach ( var asset in assets ) {

				if (selectedNames!=null) {
					if ( !selectedNames.Contains( asset.Hash ) ) {
						continue;
					}
				}

				try {
					
					if ( buildContext.IsOutOfDate( asset, forced ) ) {
						Log.Message("...building: {0}", asset.AssetPath );
						asset.Build( buildContext );
					}

					succeeded ++;

				} catch ( AggregateException ae ) {

					foreach ( var e in ae.InnerExceptions ) {

						Log.Error("{0}: {1}", asset.AssetPath, e.Message );

						/*errors.AppendFormat("{0}: {1}", asset.AssetPath, e.Message );
						errors.AppendFormat( "\r\n" );*/
					}

					failed++;

				} catch ( Exception e )	{

					Log.Error("{0}: {1}", asset.AssetPath, e.Message );

					/*errors.AppendFormat("{0}: {1}", asset.AssetPath, e.Message );
					errors.AppendFormat( "\r\n" );*/

					failed++;
				}
			}


			Log.Message("---------- Build: {0} succeeded, {1} failed ----------", succeeded, failed);

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
		/// Searchs for all possible hash collisions.
		/// Returns list of collided asset paths.
		/// </summary>
		/// <returns></returns>
		public string[] GetHashCollisions ( ICollection<Asset> assets )
		{
			return assets
				.Select( a => a.Hash )
				.GroupBy( tp => tp )
				.Where( tpg => tpg.Count()>1 )
				.Select( tpg1 => tpg1.First() )
				.Distinct()
				.ToArray();
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
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		internal string ResolveContentPath ( string path )
		{
			return ResolvePath( path, contentDirsExpanded );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		internal string ResolveBinaryPath ( string path )
		{
			return ResolvePath( path, binaryDirsExpanded );
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
