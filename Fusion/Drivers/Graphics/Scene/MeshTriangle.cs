using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SharpDX;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {

	public struct MeshTriangle : IEquatable<MeshTriangle> {

		public int		Index0			;
		public int		Index1			;
		public int		Index2			;
		public int		MaterialIndex	;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="i0"></param>
		/// <param name="i1"></param>
		/// <param name="i2"></param>
		/// <param name="mtrlId"></param>
		public MeshTriangle( int i0 = 0, int i1 = 0, int i2 = 0, int mtrlId = 0 ) {
			Index0	= i0;
			Index1	= i1;
			Index2	= i2;
			MaterialIndex	= mtrlId;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool	IsDegenerate () 
		{
			bool isDegnerate = ( Index0 == Index1 || Index0 == Index2 || Index1 == Index2 );
			return isDegnerate;
		}



		/// <summary>
		/// Computes normal
		/// </summary>
		/// <param name="mesh">Mesh owning the triangle</param>
		/// <returns></returns>
		public Vector3 ComputeNormal ( Mesh mesh ) 
		{
			var p0	=	mesh.Vertices[ Index0 ].Position;
			var p1	=	mesh.Vertices[ Index1 ].Position;
			var p2	=	mesh.Vertices[ Index2 ].Position;
			var n	=	Vector3.Normalize( Vector3.Cross( p1 - p0, p2 - p0 ) );
			return	n;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="mesh"></param>
		/// <returns></returns>
		public Vector3 Centroid ( Mesh mesh )
		{
			var p0	=	mesh.Vertices[ Index0 ].Position;
			var p1	=	mesh.Vertices[ Index1 ].Position;
			var p2	=	mesh.Vertices[ Index2 ].Position;

			return ( p0 + p1 + p2 ) / 3;
		}



		public bool Equals(MeshTriangle other) 
		{
			return (this.Index0 == other.Index0 && this.Index1 == other.Index1 && this.Index2 == other.Index2 && this.MaterialIndex == other.MaterialIndex);
		}


		public override bool Equals(Object obj)
		{
            if (ReferenceEquals(null, obj)) return false;
            return obj is MeshTriangle && Equals((MeshTriangle) obj);
		}   


		public override int GetHashCode()
		{
            unchecked {
                var hashCode = Index0;
                hashCode = (hashCode * 397) ^ Index1;
                hashCode = (hashCode * 397) ^ Index2;
                hashCode = (hashCode * 397) ^ MaterialIndex;
                return hashCode;
            }
		}


		public static bool operator == (MeshTriangle triangle1, MeshTriangle triangle2)
		{
			return triangle1.Equals(triangle2);
		}


		public static bool operator != (MeshTriangle triangle1, MeshTriangle triangle2)
		{
			return ! (triangle1.Equals(triangle2));
		}
	}
}
