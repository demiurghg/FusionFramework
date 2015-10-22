using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace Fusion.Core.Configuration {

	public class ConfigVariable {

		public readonly string Prefix;
		public readonly string Name;
		public readonly PropertyInfo Property;
		public readonly object Object;

		readonly TypeConverter converter;
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="pi"></param>
		/// <param name="obj"></param>
		public ConfigVariable ( string prefix, string name, PropertyInfo pi, object obj )
		{
			Prefix		=	prefix;
			Name		=	name;
			Property	=	pi;
			Object		=	obj;
			converter	=	TypeDescriptor.GetConverter( Property.PropertyType );
		}


		/// <summary>
		/// Sets config variable from text value.
		/// </summary>
		/// <param name="value"></param>
		public void Set ( string value )
		{
			Property.SetValue( Object, converter.ConvertFromInvariantString( value ) );
		}


		
		/// <summary>
		/// Gets config variable value as string.
		/// </summary>
		/// <returns></returns>
		public string Get ()
		{
			return converter.ConvertToInvariantString( Property.GetValue(Object) );
		}
	}

}
