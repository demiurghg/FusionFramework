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

	class AssetDesc {
		public string Path;
		public string Type;
		public List<KeyValuePair<string, string>> Parameters;

		/// <summary>
		/// Create asset description from XML node.
		/// </summary>
		/// <param name="node"></param>
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



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public XmlElement ToXmlElement ( XmlDocument document )
		{
			var element = document.CreateElement("Asset");
			element.SetAttribute("Path", this.Path );
			element.SetAttribute("Type", this.Type );

			foreach ( var p in Parameters ) {
				var pElement = document.CreateElement( p.Key );
				pElement.InnerText = p.Value;

				element.AppendChild( pElement );
			}

			return element;
		}



		/// <summary>
		///	Create asset description from asset.
		/// </summary>
		/// <param name="asset"></param>
		public AssetDesc ( Asset asset ) 
		{
			Path		=	asset.AssetPath;
			Type		=	asset.GetType().Name;
			Parameters	=	new List<KeyValuePair<string,string>>();

			foreach ( var prop in asset.GetType().GetProperties() ) {

				if (!prop.CanWrite || !prop.CanRead) {
					continue;
				}

				if (prop.IsList()) {

					var list = prop.GetList( asset );
					var type = prop.GetListElementType();

					foreach ( var element in list ) {
						var conv	= TypeDescriptor.GetConverter( type );
						var value	= conv.ConvertToInvariantString( element );

						Parameters.Add( new KeyValuePair<string,string>(prop.Name, value) );
					}
						
				} else {

					var conv	= TypeDescriptor.GetConverter( prop.PropertyType );
					var value	= conv.ConvertToInvariantString( prop.GetValue( asset ) );
						
					Parameters.Add( new KeyValuePair<string,string>(prop.Name, value) );
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Asset CreateAsset ( Type[] types )
		{
			var type = types.FirstOrDefault( t => t.Name == Type );

			if (type==null) {
				throw new ContentException(string.Format( "Asset type '{0}' not found", Type ) );
			}

			var asset = (Asset)Activator.CreateInstance( type, Path );

			AssignProperties( asset, Parameters );

			return asset;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="parameters"></param>
		void AssignProperties ( object obj, IEnumerable<KeyValuePair<string,string>> parameters )
		{
			var type	=	obj.GetType();
			var props	=	type.GetProperties().ToDictionary( p => p.Name );

			
			foreach ( var keyValue in parameters ) {

				if ( props.ContainsKey( keyValue.Key ) ) {

					var prop		= props[ keyValue.Key ];

					if ( prop.IsList() ) {

						var propType	=	prop.GetListElementType();
						var converter 	=	TypeDescriptor.GetConverter( propType );

						prop.GetList(obj).Add( Misc.ConvertType( keyValue.Value, propType ) );
						
					} else {

						var propType	=	prop.PropertyType;
						var converter 	=	TypeDescriptor.GetConverter( propType );
						
						prop.SetValue( obj, Misc.ConvertType( keyValue.Value, propType ) );
					}

				} else {
					//	Ignore.
				}
			}

		}
	}
}
