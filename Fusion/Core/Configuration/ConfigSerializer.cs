using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Fusion.Core.IniParser;
using Fusion.Core.IniParser.Model;
using System.ComponentModel;
using Fusion.Engine.Common;

namespace Fusion.Core.Configuration {

	/// <summary>
	/// Saves and loads configuration to file.
	/// 
	/// Note on multi-threading:
	///		Be sure that all structure properties 
	///		larger than 4 (32-bit) or 8 (64-bit) bytes in config classes 
	///		have lock on set and get.
	/// </summary>
	internal static class ConfigSerializer {


		public static void SaveToFile ( IEnumerable<GameModule.ModuleBinding> bindings, string path )
		{
			Log.Message("Saving : {0}", path );

			try {
		
				//	prepare ini data :			
				IniData iniData = new IniData();
				iniData.Configuration.CommentString	=	"# ";

				foreach ( var bind in bindings ) {

					var sectionName		=	bind.NiceName;
					var configObject	=	GetConfigObject( bind.Module );

					iniData.Sections.AddSection( sectionName );

					var sectionData	=	iniData.Sections.GetSectionData( sectionName );

					if (configObject==null) {
						continue;
					}
				
					foreach ( var prop in configObject.GetType().GetProperties() ) {

						var name	=	prop.Name;
						var value	=	prop.GetValue( configObject );
						var conv	=	TypeDescriptor.GetConverter( prop.PropertyType );
						var keyData	=	new KeyData(name);

						keyData.Value	=	conv.ConvertToInvariantString( value );

						sectionData.Keys.AddKey( keyData );
					}
				}


				//	write file :
				Directory.CreateDirectory( Path.GetDirectoryName(path) );

				var parser = new StreamIniDataParser();

				using ( var sw = new StreamWriter(path) ) {
					parser.WriteData( sw, iniData );
				}

			} catch (Exception e) {
				Log.Message("{0}", e.Message);
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="bindings"></param>
		/// <param name="path"></param>
		public static void LoadFromFile	( IEnumerable<GameModule.ModuleBinding> bindings, string path )
		{
			Log.Message("Loading : {0}", path );

			try {
		
				var iniData = new IniData();
				var parser = new StreamIniDataParser();

				parser.Parser.Configuration.CommentString	=	"# ";

				using ( var sw = new StreamReader(path) ) {
					iniData	= parser.ReadData( sw );
				}
			

				//	read data :
				foreach ( var section in iniData.Sections ) {

					var bind	=	bindings
								.Where( b => b.NiceName == section.SectionName )
								.SingleOrDefault();

					if (bind==null) {
						Log.Warning("Module {0} does not exist. Section ignored.", section.SectionName );
					}

					var configObject	=	GetConfigObject( bind.Module );

					foreach ( var keyData in section.Keys ) {
						
						var prop =	configObject.GetType().GetProperty( keyData.KeyName );

						if (prop==null) {
							Log.Warning("Config property {0}.{1} does not exist. Key ignored.", section.SectionName, keyData.KeyName );
							continue;
						}

						var conv	=	TypeDescriptor.GetConverter( prop.PropertyType );
						
						prop.SetValue( configObject, conv.ConvertFromInvariantString( keyData.Value ));
					}

				}

			} catch (Exception e) {
				Log.Message("{0}", e.Message);
			}
		}



		/// <summary>
		/// Gets single property marked with [Config] attribute.
		/// If objects does no have such property returns null.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		static object GetConfigObject ( object obj )
		{
			var cfgobj = obj.GetType().GetProperties()
							.Where( prop1 => prop1.GetCustomAttribute<ConfigAttribute>() != null )
							.Select( prop1 => prop1.GetValue(obj) )
							.SingleOrDefault();

			return cfgobj;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string GetConfigPath ( string fileName )
		{
			string myDocs	=	Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string appName	=	Path.GetFileNameWithoutExtension( AppDomain.CurrentDomain.FriendlyName.Replace(".vshost","") );
			return Path.Combine( myDocs, appName, fileName );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static IEnumerable<ConfigVariable> GetConfigVariables ( IEnumerable<GameModule.ModuleBinding> bindings )
		{
			var list = new List<ConfigVariable>();

			foreach ( var bind in bindings ) {

				var prefix			=	bind.ShortName;
				var configObject	=	GetConfigObject( bind.Module );

				if (configObject==null) {
					continue;
				}

				foreach ( var prop in configObject.GetType().GetProperties() ) {

					var cfgVar	=	new ConfigVariable( prefix, prop.Name, prop, configObject );
					
					list.Add( cfgVar ); 
				}
			}

			return list;
		}
	}
}
