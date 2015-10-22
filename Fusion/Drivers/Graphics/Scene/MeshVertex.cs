using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SharpDX;
using Fusion.Core;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {

	public struct MeshVertex : IEquatable<MeshVertex> {

		/// <summary>
		/// XYZ postiion
		/// </summary>
		public	Vector3		Position	;

		/// <summary>
		/// Tangential vector. Depends on Texture coordinates from TexCoord0
		/// </summary>
		public	Vector3		Tangent		;

		/// <summary>
		/// Binormal vector. Depends on Texture coordinates from TexCoord0
		/// </summary>
		public	Vector3		Binormal	;

		/// <summary>
		/// Normal vector.
		/// </summary>
		public	Vector3		Normal		;

		/// <summary>
		/// Texture coordianets.
		/// </summary>
		public	Vector2		TexCoord0	;

		/// <summary>
		/// Additional texture coordianets.
		/// </summary>
		public	Vector2		TexCoord1	;

		/// <summary>
		/// Primary vertex color.
		/// </summary>
		public	Color		Color0		;

		/// <summary>
		/// Secondary vertex color.
		/// </summary>
		public	Color		Color1		;

		/// <summary>
		/// Four component skin vertices.
		/// </summary>
		public	Int4		SkinIndices	;

		/// <summary>
		/// Four component skin weights.
		/// </summary>
		public	Vector4		SkinWeights	;






		public bool Equals(MeshVertex other) 
		{
			return (this.Position		==	other.Position	 
				 && this.Tangent		==	other.Tangent	 
				 && this.Binormal		==	other.Binormal	 
				 && this.Normal			==	other.Normal		 
				 && this.TexCoord0		==	other.TexCoord0	 
				 && this.TexCoord1		==	other.TexCoord1	 
				 && this.Color0			==	other.Color0		 
				 && this.Color1			==	other.Color1		 
				 && this.SkinIndices	==	other.SkinIndices 
				 && this.SkinWeights	==	other.SkinWeights );
		}


		public override bool Equals(Object obj)
		{
            if (ReferenceEquals(null, obj)) return false;
            return obj is MeshVertex && Equals((MeshVertex) obj);
		}   


		public override int GetHashCode()
		{
			return Misc.FastHash( Position, Tangent, Binormal, Normal, TexCoord0, TexCoord1, Color0, Color1, SkinIndices, SkinWeights );
		}


		public static bool operator == (MeshVertex vertex1, MeshVertex vertex2)
		{
			return vertex1.Equals(vertex2);
		}


		public static bool operator != (MeshVertex vertex1, MeshVertex vertex2)
		{
			return ! (vertex1.Equals(vertex2));
		}
	}
}
