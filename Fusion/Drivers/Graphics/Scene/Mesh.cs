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
using Fusion.Core;
using Fusion.Core.Mathematics;

namespace Fusion.Drivers.Graphics {

	public sealed partial class Mesh : IEquatable<Mesh> {

		public List<MeshVertex>		Vertices		{ get; private set; }	
		public List<MeshTriangle>	Triangles		{ get; private set; }	
		public List<MeshSubset>		Subsets			{ get; private set; }	
		public int					TriangleCount	{ get { return Triangles.Count; } }
		public int					VertexCount		{ get { return Vertices.Count; } }
		public int					IndexCount		{ get { return TriangleCount * 3; } }



		/// <summary>
		/// Mesh constructor
		/// </summary>
		public Mesh ()
		{
			Vertices		=	new List<MeshVertex>();
			Triangles		=	new List<MeshTriangle>();
			Subsets			=	new List<MeshSubset>();
		}



		/// <summary>
		/// Gets indices ready for hardware use
		/// </summary>
		/// <returns></returns>
		public int[] GetIndices ()
		{
			int[] array = new int[ Triangles.Count * 3];

			for (int i=0; i<Triangles.Count; i++) {
				array[i*3+0] = Triangles[i].Index0;
				array[i*3+1] = Triangles[i].Index1;
				array[i*3+2] = Triangles[i].Index2;
			}

			return array;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="tolerance"></param>
		public void Prepare ( Scene scene, float tolerance )
		{
			MergeVertices( tolerance );
			DefragmentSubsets(scene, true);
			ComputeTangentFrame();
			ComputeBoundingBox();//*/
		}




		/// <summary>
		/// Creates index buffer for given mesh.
		/// </summary>
		/// <returns></returns>
		public IndexBuffer CreateIndexBuffer ( GraphicsDevice device )
		{
			return IndexBuffer.Create( device, GetIndices() );
		}



		/// <summary>
		/// Creates vertex buffer for given mesh.
		/// </summary>
		/// <typeparam name="TVertex"></typeparam>
		/// <param name="device"></param>
		/// <param name="convert"></param>
		/// <returns></returns>
		public VertexBuffer CreateVertexBuffer<TVertex> ( GraphicsDevice device, Func<MeshVertex,TVertex> convert )	where TVertex: struct
		{
			return VertexBuffer.Create<TVertex>( device, Vertices.Select( v => convert(v) ).ToArray() );
		}



		/// <summary>
		/// This methods check equality of two diferrent mesh by 
		/// the following criterias performing early quit if any of the fails:
		///		- Tag object
		///		- Vertex count
		///		- Triangle count
		///		- Subsets count
		///		- Materials count.
		///		- Vertex buffer
		///		- Index buffer
		///		- Vertices list
		/// 	- Triangles list
		/// 	- Subsets list
		/// 	- Materials list
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals ( Mesh other )
		{
			if (other==null) return false;

			if (Object.ReferenceEquals( this, other )) {
				return true;
			}

			if ( this.VertexCount		!= other.VertexCount	) return false;
			if ( this.TriangleCount		!= other.TriangleCount	) return false;
			if ( this.IndexCount		!= other.IndexCount		) return false;
			if ( this.Subsets.Count		!= other.Subsets.Count	) return false;
			
			if ( !this.Vertices .SequenceEqual( other.Vertices  ) ) return false;
			if ( !this.Triangles.SequenceEqual( other.Triangles ) ) return false;
			if ( !this.Subsets  .SequenceEqual( other.Subsets   ) ) return false;

			return true;
		}


		public override bool Equals ( object obj )
		{
			if (obj==null) return false;
			if (obj as Mesh==null) return false;
			return Equals((Mesh)obj);
		}



		public override int GetHashCode ()
		{
			return Misc.Hash( Vertices, Triangles, Subsets );
		}



		public static bool operator == (Mesh obj1, Mesh obj2)
		{
			if ((object)obj1 == null || ((object)obj2) == null)
				return Object.Equals(obj1, obj2);

			return obj1.Equals(obj2);
		}



		public static bool operator != (Mesh obj1, Mesh obj2)
		{
			if (obj1 == null || obj2 == null)
				return ! Object.Equals(obj1, obj2);

			return ! (obj1.Equals(obj2));
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Computational and optimization stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Defragmentates mesh subsets with same materials
		/// </summary>
		public void DefragmentSubsets ( Scene scene, bool takeFromTriangleMtrlIndices )
		{
			//	if there are not shading groups, 
			//	take them from per triangle material indices
			if (!Subsets.Any() || takeFromTriangleMtrlIndices) {
				
				for ( int i=0; i<Triangles.Count; i++ ) {

					MeshSubset sg = new MeshSubset();
					sg.MaterialIndex	=	Triangles[i].MaterialIndex;
					sg.StartPrimitive	=	i;
					sg.PrimitiveCount	=	1;

					Subsets.Add( sg );
					//Console.Write( "*{0}", Triangles[i].MaterialIndex );
				}
			}


			if ( Subsets.Count==1 ) {
				return;
			}


			List<List<MeshTriangle>>	perMtrlTris = new List<List<MeshTriangle>>();

			foreach ( var mtrl in scene.Materials ) {
				perMtrlTris.Add( new List<MeshTriangle>() );
			}

			foreach ( var sg in Subsets ) {

				for ( int i = sg.StartPrimitive; i < sg.StartPrimitive + sg.PrimitiveCount; i++ ) {
					perMtrlTris[ sg.MaterialIndex ].Add( Triangles[i] );
				}
			}

			Subsets.Clear();
			Triangles.Clear();

			for ( int i=0; i<perMtrlTris.Count; i++ ) {
				var sg = new MeshSubset();
				sg.MaterialIndex	=	i;
				sg.StartPrimitive	=	Triangles.Count;
				sg.PrimitiveCount	=	perMtrlTris[i].Count;

				if (sg.PrimitiveCount==0) {
					continue;
				}

				Triangles.AddRange( perMtrlTris[i] );
				Subsets.Add( sg );
			}
			
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Vertex merge stuff :
		 * 
		-----------------------------------------------------------------------------------------*/

		class OctNode {
			public int index;
			public OctNode[] nodes = new OctNode[8];
		}


		/// <summary>
		/// Merges vertices
		/// </summary>
		public void MergeVertices ( float tolerance )
		{
			OctNode	root = null;
			List<MeshVertex> oldVertices = new List<MeshVertex>( Vertices );
			Vertices.Clear();

			for (int i=0; i<Triangles.Count; i++) {
				
				MeshVertex v0	=	oldVertices[ Triangles[i].Index0 ];
				MeshVertex v1	=	oldVertices[ Triangles[i].Index1 ];
				MeshVertex v2	=	oldVertices[ Triangles[i].Index2 ];
				
				int i0		=	InsertVertex( ref root, v0, tolerance );	
				int i1		=	InsertVertex( ref root, v1, tolerance );	
				int i2		=	InsertVertex( ref root, v2, tolerance );
				int mtrl	=	Triangles[i].MaterialIndex;	

				Triangles[i] = new MeshTriangle(i0,i1,i2, mtrl);
			}
		}



		/// <summary>
		/// Inserts node to oct tree 
		/// </summary>
		int InsertVertex ( ref OctNode node, MeshVertex v, float tolerance )
		{
			if (node==null) {
				node = new OctNode();				node.index = Vertices.Count;
				Vertices.Add( v );
				return node.index;
			}

			if ( CompareVertexPair( Vertices[node.index], v, tolerance ) ) {
				return node.index;
			}

			int r = ClassifyVertexPair( Vertices[node.index], v );

			return InsertVertex( ref node.nodes[r], v, tolerance * tolerance );
		}



		/// <summary>
		/// Compares vertices
		/// </summary>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <returns></returns>
		bool CompareVertexPair ( MeshVertex v0, MeshVertex v1, float toleranceSquared )
		{
			if ( Vector3.DistanceSquared( v0.Position	, v1.Position	 ) > toleranceSquared ) return false;
			if ( Vector3.DistanceSquared( v0.Tangent	, v1.Tangent	 ) > toleranceSquared ) return false;
			if ( Vector3.DistanceSquared( v0.Binormal	, v1.Binormal	 ) > toleranceSquared ) return false;
			if ( Vector3.DistanceSquared( v0.Normal		, v1.Normal		 ) > toleranceSquared ) return false;
			if ( Vector2.DistanceSquared( v0.TexCoord0	, v1.TexCoord0	 ) > toleranceSquared ) return false;
			if ( Vector2.DistanceSquared( v0.TexCoord1	, v1.TexCoord1	 ) > toleranceSquared ) return false;

			if ( v0.Color0 != v1.Color0 ) return false;
			if ( v0.Color1 != v1.Color1 ) return false;

			if ( Vector4.DistanceSquared( v0.SkinWeights, v1.SkinWeights ) > toleranceSquared ) return false;
			if ( v0.SkinIndices != v1.SkinIndices ) return false;

			return true;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="v0"></param>
		/// <param name="v1"></param>
		/// <returns></returns>
		int ClassifyVertexPair ( MeshVertex v0, MeshVertex v1 )
		{
			int res = 0;
			if (v0.Position.X < v1.Position.X) res |= 0x1;
			if (v0.Position.Y < v1.Position.Y) res |= 0x2;
			if (v0.Position.Z < v1.Position.Z) res |= 0x4;
			return res;
		}



		/// <summary>
		/// 
		/// </summary>
		public BoundingBox ComputeBoundingBox() 
		{
			Vector3 min = new Vector3( float.MaxValue );
			Vector3 max = new Vector3( float.MinValue );

			for( int i = Vertices.Count; --i >= 0; ) {
				min = Vector3.Min( min, Vertices[i].Position );
				max = Vector3.Max( max, Vertices[i].Position );
			}
			return new BoundingBox( min, max );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ray"></param>
		/// <returns></returns>
		public float Intersects( ref Ray ray )
		{
			float minDistance = float.MaxValue;
			
			/*if ( !BBox.Intersects( ref ray ) )
				return minDistance;*/

			float distance;
			for( int i = Triangles.Count; --i >= 0; ) {
				var v0	= Vertices[ Triangles[i].Index0 ].Position;
				var v1	= Vertices[ Triangles[i].Index1 ].Position;
				var v2	= Vertices[ Triangles[i].Index2 ].Position;
				if ( ray.Intersects( ref v0, ref v1, ref v2, out distance) ) {
					minDistance = (minDistance > distance) ? distance : minDistance;
				}
			}

			return minDistance;//*/
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Tangent space :
		 * 
		-----------------------------------------------------------------------------------------*/
	
		/// <summary>
		/// Computes tangent frame
		/// </summary>
		public void ComputeTangentFrame ()
		{
			for ( int i=0; i<TriangleCount; i++) {

				var tri	  = Triangles[i];
				
				var inds  = new[]{ tri.Index0, tri.Index1, tri.Index2 };

				for (uint j=0; j<3; j++)
				{
					MeshVertex	vert0	=	Vertices[inds[(0+j)%3]];
					MeshVertex	vert1	=	Vertices[inds[(1+j)%3]];
					MeshVertex	vert2	=	Vertices[inds[(2+j)%3]];

			
					Vector3	v0	= vert1.Position  - vert0.Position;
					Vector3	v1	= vert2.Position  - vert0.Position;
					Vector2	t0	= vert1.TexCoord0 - vert0.TexCoord0;	
					Vector2	t1	= vert2.TexCoord0 - vert0.TexCoord0;	

					{	// X :
						float	det		= t0.X * t1.Y  -  t1.X * t0.Y;
						float	dett	= v0.X * t1.Y  -  v1.X * t0.Y;
						float	detb	= t0.X * v1.X  -  t1.X * v0.X;
						//if (Math.Abs(det)<float.Epsilon) Log.Warning("Tri is too small");
						vert0.Tangent.X		= dett / det;								
						vert0.Binormal.X	= detb / det;								
					}
					{	// Y :
						float	det		= t0.X * t1.Y  -  t1.X * t0.Y;
						float	dett	= v0.Y * t1.Y  -  v1.Y * t0.Y;
						float	detb	= t0.X * v1.Y  -  t1.X * v0.Y;
						//if (Math.Abs(det)<float.Epsilon) Log.Warning("Tri is too small");
						vert0.Tangent.Y		= dett / det;								
						vert0.Binormal.Y	= detb / det;								
					}
					{	// Z :
						float	det		= t0.X * t1.Y  -  t1.X * t0.Y;
						float	dett	= v0.Z * t1.Y  -  v1.Z * t0.Y;
						float	detb	= t0.X * v1.Z  -  t1.X * v0.Z;
						//if (Math.Abs(det)<float.Epsilon) Log.Warning("Tri is too small");
						vert0.Tangent.Z		= dett / det;								
						vert0.Binormal.Z	= detb / det;								
					}

					//vert0.Normal	= Vector3.Cross(v1, v0);
			
					if ( vert0.Tangent.Length()  > float.Epsilon * 8 ) vert0.Tangent.Normalize();
					if ( vert0.Binormal.Length() > float.Epsilon * 8 ) vert0.Binormal.Normalize();
					if ( vert0.Normal.Length()   > float.Epsilon * 8 ) vert0.Normal.Normalize();
					//vert0.Tangent.Normalize()  ;
					//vert0.Binormal.Normalize() ;
					//vert0.Normal.Normalize()   ;
			
					Vector3	temp;
					temp = Vector3.Cross( vert0.Tangent, vert0.Normal );
					vert0.Tangent = Vector3.Cross( vert0.Normal, temp );
			
					temp = Vector3.Cross( vert0.Binormal, vert0.Normal );
					vert0.Binormal = Vector3.Cross( vert0.Normal, temp );//*/

					//	assign vertex
					Vertices[inds[(0+j)%3]] = vert0;
				}
			}
		}


		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Some stuff
		 * 
		-----------------------------------------------------------------------------------------*/



		/// <summary>
		///	Removes degenerate triangles
		/// </summary>
		public void RemoveDegenerateTriangles ()
		{
			Triangles.RemoveAll( tri => tri.IsDegenerate() );
		}



		/// <summary>
		/// PrintMeshInfo
		/// </summary>
		public void PrintMeshInfo ()
		{
			Console.WriteLine("Vertex count   : {0}", Vertices.Count );
			Console.WriteLine("Triangle count : {0}", Triangles.Count );
			Console.WriteLine("Shading groups : {0}", Subsets.Count );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public void Deserialize( BinaryReader reader )
		{
			//	read vertices :
			int vertexCount	=	reader.ReadInt32();
			Vertices		=	reader.Read<MeshVertex>( vertexCount ).ToList();
			
			//	read trinagles :
			int trisCount	=	reader.ReadInt32();
			Triangles		=	reader.Read<MeshTriangle>( trisCount ).ToList();
							
			//	read subsets :
			int subsetCount	=	reader.ReadInt32();
			Subsets			=	reader.Read<MeshSubset>( subsetCount ).ToList();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public void Serialize( BinaryWriter writer )
		{
			writer.Write( VertexCount );
			writer.Write( Vertices.ToArray() );
			writer.Write( TriangleCount );
			writer.Write( Triangles.ToArray() );
			writer.Write( Subsets.Count );
			writer.Write( Subsets.ToArray() );
		}
	}
}
