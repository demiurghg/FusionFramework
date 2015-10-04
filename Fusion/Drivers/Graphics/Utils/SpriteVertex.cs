using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion.Drivers.Graphics;
using Fusion.Core.Mathematics;

namespace Fusion.Drivers.Graphics {

	public struct SpriteVertex {

		[Vertex("POSITION")]	public Vector3	Position;
		[Vertex("TEXCOORD")]	public Vector2	TexCoord;
		[Vertex("COLOR")]		public Color	Color;

		public SpriteVertex ( Vector3 p, Color c, Vector2 tc ) {
			Position	=	p;
			Color		=	c;
			TexCoord	=	tc;
		}

		public SpriteVertex ( float x, float y, float z, Color c, float u, float v )
		{
			Position	=	new Vector3( x, y, z );
			Color		=	c;
			TexCoord	=	new Vector2( u, v );
		}
	}
}
