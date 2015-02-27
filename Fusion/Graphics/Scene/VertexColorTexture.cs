using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;

namespace Fusion.Graphics {
	public struct VertexColorTexture {

		[Vertex("POSITION")]	public Vector3	Position;
		[Vertex("COLOR")]		public Color	Color	;
		[Vertex("TEXCOORD")]	public Vector2	TexCoord;

		public static VertexColorTexture Bake ( MeshVertex meshVertex )
		{
			VertexColorTexture v;
			v.Position	=	meshVertex.Position;
			v.Color		=	meshVertex.Color0;
			v.TexCoord	=	meshVertex.TexCoord0;
			return v;
		}
	}
}
