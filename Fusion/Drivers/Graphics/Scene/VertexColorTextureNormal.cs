using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;

namespace Fusion.Drivers.Graphics {
	public struct VertexColorTextureNormal {

		[Vertex("POSITION")]	public Vector3	Position;
		[Vertex("COLOR")]		public Color	Color	;
		[Vertex("TEXCOORD")]	public Vector2	TexCoord;
		[Vertex("NORMAL")]		public Vector3	Normal;


		static public VertexInputElement[] Elements {
			get {
				return VertexInputElement.FromStructure( typeof(VertexColorTextureNormal) );
			}
		}

		public static VertexColorTextureNormal Convert ( MeshVertex meshVertex )
		{
			VertexColorTextureNormal v;
			v.Position	=	meshVertex.Position;
			v.Color		=	meshVertex.Color0;
			v.TexCoord	=	meshVertex.TexCoord0;
			v.Normal	=	meshVertex.Normal;
			return v;
		}
	}
}
