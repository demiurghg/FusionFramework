using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// Represents the group of primitives with specified material.
	/// </summary>
	public struct MeshSubset : IEquatable<MeshSubset> {

		/// <summary>
		/// Start vertex index for given shading group
		/// </summary>
		public int StartPrimitive	;	

		/// <summary>
		/// Length of vertex group in primitives
		/// </summary>
		public int PrimitiveCount	;	

		/// <summary>
		/// Material's index
		/// </summary>
		public int MaterialIndex	;



		public bool Equals(MeshSubset other) 
		{
			return (this.StartPrimitive == other.StartPrimitive 
				 && this.PrimitiveCount == other.PrimitiveCount 
				 && this.MaterialIndex  == other.MaterialIndex);
		}


		public override bool Equals(Object obj)
		{
            if (ReferenceEquals(null, obj)) return false;
            return obj is MeshSubset && Equals((MeshSubset) obj);
		}   


		public override int GetHashCode()
		{
            unchecked {
                var hashCode = StartPrimitive;
                hashCode = (hashCode * 397) ^ PrimitiveCount;
                hashCode = (hashCode * 397) ^ MaterialIndex;
                return hashCode;
            }
		}


		public static bool operator == (MeshSubset subset1, MeshSubset subset2)
		{
			return subset1.Equals(subset2);
		}


		public static bool operator != (MeshSubset subset1, MeshSubset subset2)
		{
			return ! (subset1.Equals(subset2));
		}
	}
}
