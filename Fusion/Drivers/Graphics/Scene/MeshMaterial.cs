using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SharpDX;
using Fusion.Drivers.Graphics;
using System.Reflection;
using System.ComponentModel.Design;


namespace Fusion.Drivers.Graphics {


	/// <summary>
	/// Material type
	/// </summary>
	public sealed class MeshMaterial : IEquatable<MeshMaterial> {

		/// <summary>
		/// Base texture path.
		/// </summary>
		public	string	Name { get; set; }

		/// <summary>
		/// Base texture path.
		/// </summary>
		public	string	TexturePath { get; set; }


		/// <summary>
		/// Gets or sets an object identifying this model.
		/// </summary>
		public	object  Tag { get; set; }


		public MeshMaterial ()
		{
			Name			=	"";
			TexturePath		=	null;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public void Deserialize( BinaryReader reader )
		{
			Name =  reader.ReadString();

			TexturePath = null;
			if ( reader.ReadBoolean() == true ) {
				TexturePath =  reader.ReadString();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public void Serialize( BinaryWriter writer )
		{
			writer.Write( Name );

			if ( TexturePath == null ) {
				writer.Write( false );
			} else {
				writer.Write( true );
				writer.Write( TexturePath );
			}
		}



		public bool Equals ( MeshMaterial other )
		{
			if (other==null) return false;

			return ( Name		 == other.Name			)
				&& ( TexturePath == other.TexturePath	)
				&& ( Tag		 == other.Tag			)
				;
		}


		public override bool Equals ( object obj )
		{
			if (obj==null) return false;
			if (obj as MeshMaterial==null) return false;
			return Equals((MeshMaterial)obj);
		}

		public override int GetHashCode ()
		{
			int hashCode = 0;
            hashCode = (hashCode * 397) ^ Name.GetHashCode();
            hashCode = (hashCode * 397) ^ TexturePath.GetHashCode();
			hashCode = (hashCode * 397) ^ Tag.GetHashCode();
			return hashCode;
		}


		public static bool operator == (MeshMaterial obj1, MeshMaterial obj2)
		{
			if ((object)obj1 == null || ((object)obj2) == null)
				return Object.Equals(obj1, obj2);

			return obj1.Equals(obj2);
		}

		public static bool operator != (MeshMaterial obj1, MeshMaterial obj2)
		{
			if (obj1 == null || obj2 == null)
				return ! Object.Equals(obj1, obj2);

			return ! (obj1.Equals(obj2));
		}
	}
}
