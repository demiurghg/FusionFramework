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
	public struct VertexColorTexture {

		[Vertex("POSITION")]	public Vector3	Position;
		[Vertex("COLOR")]		public Color	Color	;
		[Vertex("TEXCOORD")]	public Vector2	TexCoord;

		static public VertexInputElement[] Elements {
			get {
				return VertexInputElement.FromStructure( typeof(VertexColorTexture) );
			}
		}

		public static VertexColorTexture Convert ( MeshVertex meshVertex )
		{
			VertexColorTexture v;
			v.Position	=	meshVertex.Position;
			v.Color		=	meshVertex.Color0;
			v.TexCoord	=	meshVertex.TexCoord0;
			return v;
		}
	}
}
