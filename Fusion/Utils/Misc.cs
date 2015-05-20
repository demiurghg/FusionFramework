using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace Fusion {
	public static class Misc {

		/// <summary>
		/// http://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp/298990#298990
		/// </summary>
		/// <param name="str"></param>
		/// <param name="controller"></param>
		/// <returns></returns>
		public static IEnumerable<string> Split(this string str, Func<char, bool> controller)
		{
			int nextPiece = 0;

			for (int c = 0; c < str.Length; c++) {
				if (controller(str[c])) {
					yield return str.Substring(nextPiece, c - nextPiece);
					nextPiece = c + 1;
				}
			}

			yield return str.Substring(nextPiece);
		}


		/// <summary>
		/// http://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp/298990#298990
		/// </summary>
		public static string TrimMatchingQuotes(this string input, char quote)
		{
			if ((input.Length >= 2) && 
				(input[0] == quote) && (input[input.Length - 1] == quote))
				return input.Substring(1, input.Length - 2);

			return input;
		}



	
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		/// <summary>
		/// Same as Misc.Hash but without recursion and type reflection.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static int FastHash (params object[] args)
		{
			int hashCode = 0;
			unchecked {
				foreach( var arg in args ) {
					hashCode = (hashCode * 397) ^ arg.GetHashCode();
				}
			}
			return hashCode;
		}


		/// <summary>
		/// http://stackoverflow.com/questions/5450696/c-sharp-generic-hashcode-implementation-for-classes
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static int Hash(params object[] args)
		{
			if (args == null)
			{
				return 0;
			}

			int num = 42;

			unchecked
			{
				foreach(var item in args)
				{
					if (ReferenceEquals(item, null))
					{ }
					else if (item.GetType().IsArray)
					{
						foreach (var subItem in (IEnumerable)item)
						{
							num = num * 37 + Hash(subItem);
						}
					}
					else
					{
						num = num * 37 + item.GetHashCode();
					}
				}
			}

			return num;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="?"></param>
		/// <param name="magic"></param>
		public static void ExpectMagic ( this BinaryReader reader, string magic, string fileType )
		{
			if (!reader.CheckMagic(magic)) {
				throw new IOException("Bad '" + fileType + "' file: reader expects '" + magic + "' word in binary stream");
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="magic"></param>
		/// <returns></returns>
		public static bool CheckMagic ( this BinaryReader reader, string magic )
		{
			if (magic.Length!=4) {
				throw new ArgumentException("Magic string must contain exactly 4 characters");
			}

			var value = new string(reader.ReadChars(4));

			if (value!=magic) {
				return false;
			} else {
				return true;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="targetObject"></param>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool SetProperty<T> ( object targetObject, string propertyName, T value )
		{
			var type = targetObject.GetType();

			try {

				var pi = type.GetProperty( propertyName, typeof(T) );

				pi.SetValue( targetObject, value );

				return true;

			} catch ( Exception ) {
				return false;
			}
		}



		/// <summary>
		/// Reads the contents of the stream into a byte array.
		/// data is returned as a byte array. An IOException is
		/// thrown if any of the underlying IO calls fail.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <returns>A byte array containing the contents of the stream.</returns>
		/// <exception cref="NotSupportedException">The stream does not support reading.</exception>
		/// <exception cref="ObjectDisposedException">Methods were called after the stream was closed.</exception>
		/// <exception cref="System.IO.IOException">Anor occurs.</exception>
		public static byte[] ReadAllBytes( this Stream source )
		{
			byte[] readBuffer = new byte[4096];
 
			int totalBytesRead = 0;
			int bytesRead;
 
			while ((bytesRead = source.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
			{
				totalBytesRead += bytesRead;
 
				if (totalBytesRead == readBuffer.Length)
				{
					int nextByte = source.ReadByte();
					if (nextByte != -1)
					{
						byte[] temp = new byte[readBuffer.Length * 2];
						Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
						Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
						readBuffer = temp;
						totalBytesRead++;
					}
				}
			}
 
			byte[] buffer = readBuffer;
			if (readBuffer.Length != totalBytesRead)
			{
				buffer = new byte[totalBytesRead];
				Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
			}
			return buffer;
		}



		/// <summary>
		/// http://stackoverflow.com/questions/2296288/how-to-decide-a-type-is-a-custom-struct
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsStruct(this Type type)
		{
			return type.IsValueType 
					&& !type.IsPrimitive 
					&& !type.IsEnum 
					&& type != typeof(decimal)
					;
		}



		/// <summary>
		/// Copies property values from one object to another
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		public static void CopyPropertyValues(object source, object destination)
		{
			var destProperties = destination.GetType().GetProperties();

			foreach (var sourceProperty in source.GetType().GetProperties())
			{
				foreach (var destProperty in destProperties)
				{
					if (!destProperty.CanWrite) {
						continue;
					}

					if (destProperty.Name == sourceProperty.Name && destProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
					{
						destProperty.SetValue(destination, sourceProperty.GetValue(
							source, new object[] { }), new object[] { });

						break;
					}
				}
			}
		}



		/// <summary>
		/// Get attribute of given type
		/// </summary>
		/// <typeparam name="AttributeType"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T GetCustomAttribute<T>( this Type type ) where T : Attribute 
		{
			var ca = type.GetCustomAttributes( typeof(T), true );
			if (ca.Count()!=1) {
				throw new InvalidOperationException("Type has more than one attribute of type \"" + type.Name + "\"" );
			}
			return (T)ca.Single(); 
		}


		/// <summary>
		/// Checks whether type has attribute of given type
		/// </summary>
		/// <typeparam name="AttributeType"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool HasAttribute<T> ( this Type type ) where T : Attribute
		{
			return type.GetCustomAttributes( typeof(T), true ).Any();
		}



		/// <summary>
		/// Searches all loaded assemblies for all public subclasses of given type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type[] GetAllClassesWithAttribute<T>() where T : Attribute
		{	
			List<Type> types = new List<Type>();

			foreach ( var a in AppDomain.CurrentDomain.GetAssemblies() ) {

				foreach ( var t in a.GetTypes() ) {
					if (t.HasAttribute<T>()) {
						types.Add(t);						
					}
				}
			}

			return types.ToArray();
		}



		/// <summary>
		/// Searches all loaded assemblies for all public subclasses of given type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type[] GetAllSubclassedOf ( Type type )
		{	
			List<Type> types = new List<Type>();

			foreach ( var a in AppDomain.CurrentDomain.GetAssemblies() ) {

				foreach ( var t in a.GetTypes() ) {

					if (t.IsSubclassOf( type )) {

						types.Add(t);						
					}
				}
			}

			return types.ToArray();
		}



		/// <summary>
		/// Saves object to Xml file 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <param name="fileName"></param>
		static public void SaveObjectToXml ( object obj, Type type, Stream fileStream, Type[] extraTypes = null ) 
		{
			XmlSerializer serializer = new XmlSerializer( type, extraTypes ?? new Type[0] );
			TextWriter textWriter = new StreamWriter( fileStream );
			serializer.Serialize( textWriter, obj );
			textWriter.Close();
		}


		/// <summary>
		/// Saves object to Xml file 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <param name="fileName"></param>
		static public void SaveObjectToXml ( object obj, Type type, string fileName, Type[] extraTypes = null ) 
		{
			XmlSerializer serializer = new XmlSerializer( type, extraTypes ?? new Type[0] );
			TextWriter textWriter = new StreamWriter( fileName );
			serializer.Serialize( textWriter, obj );
			textWriter.Close();
		}



		/// <summary>
		/// Loads object from Xml file
		/// </summary>
		/// <param name="type"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		static public object LoadObjectFromXml( Type type, Stream fileStream, Type[] extraTypes )
		{
			var newTypes = extraTypes.ToList();
			newTypes.Remove( type );

			XmlSerializer serializer = new XmlSerializer( type, extraTypes ?? newTypes.ToArray() );

			using (TextReader textReader = new StreamReader( fileStream )) {
				object obj = serializer.Deserialize( textReader );
				return obj;
			}
		}



		/// <summary>
		/// Loads object from Xml file
		/// </summary>
		/// <param name="type"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		static public object LoadObjectFromXml( Type type, string fileName, Type[] extraTypes )
		{
			var newTypes = extraTypes.ToList();
			newTypes.Remove( type );

			XmlSerializer serializer = new XmlSerializer( type, extraTypes ?? newTypes.ToArray() );

			var fi = new FileInfo(fileName);

			if (fi.Length==0) {
				return Activator.CreateInstance( type );
			}
			
			using (TextReader textReader = new StreamReader( fileName )) {
				object obj = serializer.Deserialize( textReader );
				return obj;
			}
		}




		/// <summary>
		/// Returns max enum value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T MaxEnumValue<T>()
		{
			return (T)Enum.GetValues(typeof(T)).Cast<T>().Max();
		}
		  


		/// <summary>
		/// Build relative path from given full path, even wrong one :
		/// </summary>
		/// <param name="absolutePath"></param>
		/// <param name="relativeTo"></param>
		/// <returns></returns>
	    static public string RelativePath(string absolutePath, string relativeTo)
        {
            string[] absoluteDirectories = absolutePath.Split('\\');
            string[] relativeDirectories = relativeTo.Split('\\');

            //Get the shortest of the two paths
            int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            //Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            //Find common root
            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            //If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
                throw new ArgumentException("Paths do not have a common base");

            //Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            //Add on the ..
            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0)
                    relativePath.Append("..\\");

            //Add on the folders
            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + "\\");
            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
        }

		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		public static byte[] HexStringToByte( string hex )
		{
			hex = hex.Replace("-", "");

			int numChars = hex.Length;
			byte[] bytes = new byte[numChars / 2];

			for (int i = 0; i < numChars; i += 2) {
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}
			return bytes;
		}



		/// <summary>
		/// Makes string signature from byte array. Example: [0xAA 0xBB] will be converted to "AA-BB".
		/// </summary>
		/// <param name="sig"></param>
		/// <returns></returns>
		public static string MakeStringSignature ( byte[] sig ) 
		{
			return BitConverter.ToString(sig);
		}
	}
}
