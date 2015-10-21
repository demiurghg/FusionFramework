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

namespace Fusion.Core.Configuration {

	/// <summary>
	/// Saves and loads configuration to file
	/// </summary>
	internal static class ConfigSerializer {


		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="path"></param>
		public static void SaveToFile ( object obj, string path )
		{
			Log.Message("Saving : {0}", path );

			try {
		
				//	prepare ini data :			
				IniData iniData = new IniData();
				iniData.Configuration.CommentString	=	"# ";
			
				//	write data :

				foreach ( var cfgProp in GetConfigProperties(obj) ) {

					var cfgName		=	cfgProp.GetCustomAttribute<ConfigAttribute>().Name;
					var cfgValue	=	cfgProp.GetValue(obj);

					KeyDataCollection keysData;
				
					if (cfgName==null) {
						keysData	=	iniData.Global;
					} else {
						iniData.Sections.AddSection( cfgName );
						keysData	=	iniData.Sections.GetSectionData( cfgName ).Keys;
					}

					if (cfgValue==null) {
						Log.Warning("{0} has null value", cfgName);
						continue;
					}


					foreach ( var prop in cfgValue.GetType().GetProperties() ) {
						var name	=	prop.Name;
						var value	=	prop.GetValue( cfgValue );
						var conv	=	TypeDescriptor.GetConverter( prop.PropertyType );
						var keyData	=	new KeyData(name);

						keyData.Value	=	conv.ConvertToInvariantString( value );

						var desc = prop.GetCustomAttribute<DescriptionAttribute>();
						if (desc!=null) {
							keyData.Comments.Add( desc.Description );
						}

						keysData.AddKey( keyData );
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
		/// <param name="obj"></param>
		/// <param name="path"></param>
		public static void LoadFromFile ( object obj, string path )
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

					var cfgProp	=	GetConfigPropertyByName( obj, section.SectionName );

					if (cfgProp==null) {
						Log.Warning("Config {0} does not exist. Section ignored.", section.SectionName );
						continue;
					}

					var cfgName		=	cfgProp.GetCustomAttribute<ConfigAttribute>().Name;
					var cfgValue	=	cfgProp.GetValue(obj);

					foreach ( var keyData in section.Keys ) {
						
						var prop =	cfgValue.GetType().GetProperty( keyData.KeyName );

						if (prop==null) {
							Log.Warning("Config property {0}.{1} does not exist. Key ignored.", section.SectionName, keyData.KeyName );
							continue;
						}

						var conv	=	TypeDescriptor.GetConverter( prop.PropertyType );
						
						prop.SetValue( cfgValue, conv.ConvertFromInvariantString( keyData.Value ));
					}
				}


			} catch (Exception e) {
				Log.Message("{0}", e.Message);
			}
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
		/// <param name="name"></param>
		/// <returns></returns>
		static PropertyInfo GetConfigPropertyByName ( object obj, string name )
		{
			var prop	=	GetConfigProperties( obj )
							.Where( p1 => p1.GetCustomAttribute<ConfigAttribute>().Name == name )
							.FirstOrDefault();
			return prop;
		}

		

		/// <summary>
		/// Gets config property info from service of given type.
		/// </summary>
		/// <returns>Null if no config property defined for this type.</returns>
		static PropertyInfo[] GetConfigProperties ( object obj )
		{
			var configProps = obj.GetType().GetProperties()
				.Where( pi => pi.CustomAttributes.Any( ca => ca.AttributeType == typeof(ConfigAttribute) ) )
				.ToList();

			return configProps.ToArray();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static IEnumerable<ConfigVariable> GetConfigVariables ( object obj, string prefix )
		{
			var list = new List<ConfigVariable>();

			var cfgProps	=	GetConfigProperties(obj);

			foreach ( var cfgProp in cfgProps ) {

				var cfgName		=	cfgProp.GetCustomAttribute<ConfigAttribute>().Name;
				var cfgValue	=	cfgProp.GetValue(obj);

				foreach ( var prop in cfgValue.GetType().GetProperties() ) {

					var cfgVar	=	new ConfigVariable( prefix, cfgName, prop.Name, prop, cfgValue );
					
					list.Add( cfgVar ); 
				}
			}

			return list;
		}
	}
}
