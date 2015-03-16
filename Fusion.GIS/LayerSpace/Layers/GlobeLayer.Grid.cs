using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;
using Fusion.Mathematics;

#pragma warning disable 612

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer
	{
		VertexBuffer	vBuf;
		IndexBuffer		iBuf;
		Texture2D		eTex;


		void CreateSphere(int Stacks, int Slices)
		{
			//calculates the resulting number of vertices and indices  
			int nVertices = (Stacks + 1) * (Slices + 1);
			int dwIndices = (3 * Stacks * (Slices + 1)) * 2;
			
			int[]	indices		= new int[dwIndices];
			GeoVert[]	vertices	= new GeoVert[nVertices];

			double stackAngle = Math.PI / Stacks;
			double sliceAngle = (Math.PI * 2.0) / Slices;

			int wVertexIndex = 0;
			//Generate the group of Stacks for the sphere  
			int vertcount = 0;
			int indexcount = 0;

			for (int stack = 0; stack < (Stacks + 1); stack++) {

				double phi = stack * stackAngle - Math.PI/2.0;

				//Generate the group of segments for the current Stack  
				for (int slice = 0; slice < (Slices + 1); slice++) {
					
					double lambda = slice * sliceAngle;

					vertices[vertcount].Lon = lambda + Math.PI;
					vertices[vertcount].Lat = phi;
					vertices[vertcount].Position	= new Vector3();
					vertices[vertcount].Tex			= new Vector4((float)slice / (float)Slices, 1.0f - (float)stack / (float)Stacks, 0, 0);
					vertices[vertcount].Color		= Color.White;

					//vertices[vertcount].TextureCoordinate = new Vector2((float)slice / (float)Slices, (float)stack / (float)Stacks);  
					vertcount++;
					if (stack != (Stacks - 1)) {
						indices[indexcount] = wVertexIndex;
						indexcount++;
						indices[indexcount] = wVertexIndex + 1;
						indexcount++;
						indices[indexcount] = wVertexIndex + (Slices + 1);
						indexcount++;
						indices[indexcount] = wVertexIndex;
						indexcount++;
						indices[indexcount] = wVertexIndex + (Slices + 1);
						indexcount++;
						indices[indexcount] = wVertexIndex + (Slices);
						indexcount++;
						wVertexIndex++;
					}
				}
			}

			if (vBuf != null) vBuf.Dispose();
			if (iBuf != null) iBuf.Dispose();
			if (eTex == null) eTex = Game.Content.Load<Texture2D>("Core/NE2_50M_SR_W_4096.jpg");

			vBuf = new VertexBuffer( Game.GraphicsDevice, typeof(GeoVert), vertices.Length );
			iBuf = new IndexBuffer( Game.GraphicsDevice, indices.Length );

			vBuf.SetData(vertices, 0, vertices.Length);
			iBuf.SetData(indices, 0, indices.Length);

			return;
		}
	}
}
