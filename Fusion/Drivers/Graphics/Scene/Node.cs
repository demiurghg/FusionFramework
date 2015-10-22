using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Fusion.Drivers.Graphics;
using Fusion.Core;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {

	public sealed class Node : IEquatable<Node> {

		/// <summary>
		/// Node name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Parent index in scene. Zero value means root node.
		/// </summary>
		public int ParentIndex	{ get; set; }

		/// <summary>
		/// Scene mesh index. Negative value means no mesh reference.
		/// </summary>
		public int MeshIndex	{ get; set; }


		/// <summary>
		/// Scene animation track index.
		/// Negative value means no animation track.
		/// </summary>
		public int TrackIndex { get; set; }

		/// <summary>
		/// Node transform
		/// </summary>
		public Matrix Transform	{ get; set; }

		/// <summary>
		/// Global matrix of "bind-posed" node.
		/// For nodes that do not affect skinning this value is always Matrix.Identity.
		/// </summary>
		public Matrix BindPose	{ get; set; }

		/// <summary>
		/// Tag object. This value will not be serialized.
		/// </summary>
		public object Tag { get; set; }


		/// <summary>
		/// Creates instance of the node.
		/// </summary>
		public Node () {
			MeshIndex	=	-1;
			ParentIndex	=	-1;
			TrackIndex	=	-1;
		}


		
		public bool Equals ( Node other )
		{
			if (other==null) return false;

			if (Object.ReferenceEquals( this, other )) {
				return true;
			}

			return ( this.Name			== other.Name			)
				&& ( this.ParentIndex	== other.ParentIndex	)
				&& ( this.MeshIndex		== other.MeshIndex		)
				&& ( this.TrackIndex	== other.TrackIndex		)
				&& ( this.Transform		== other.Transform		)
				&& ( this.BindPose		== other.BindPose		)
				&& ( this.Tag			== other.Tag			);
		}


		public override bool Equals ( object obj )
		{
			if (obj==null) return false;
			if (obj as Node==null) return false;
			return Equals((Node)obj);
		}



		public override int GetHashCode ()
		{
			return Misc.Hash( Name, ParentIndex, MeshIndex, TrackIndex, Transform, BindPose, Tag );
		}



		public static bool operator == (Node obj1, Node obj2)
		{
			if ((object)obj1 == null || ((object)obj2) == null)
				return Object.Equals(obj1, obj2);

			return obj1.Equals(obj2);
		}



		public static bool operator != (Node obj1, Node obj2)
		{
			if (obj1 == null || obj2 == null)
				return ! Object.Equals(obj1, obj2);

			return ! (obj1.Equals(obj2));
		}

	}

}
