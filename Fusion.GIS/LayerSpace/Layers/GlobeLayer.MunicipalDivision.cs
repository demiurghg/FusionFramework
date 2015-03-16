using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;
using Fusion.Mathematics;
using TriangleNet;
using TriangleNet.Geometry;


#pragma warning disable 612

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer
	{
		class MD
		{
			public VertexBuffer Vertices;
			public IndexBuffer	Indeces;
			public VertexBuffer Contour;
			public float		Value;
		}

		Dictionary<string, MD> municipalDivisions = new Dictionary<string, MD>();

		public Texture2D paletteMunDiv0;
		public Texture2D paletteMunDiv1;

		public Texture2D paletteMunDiv;


		Random r = new Random();



		public void ChangeMunicipalDivisionPaltte()
		{
			paletteMunDiv = paletteMunDiv == paletteMunDiv0 ? paletteMunDiv1 : paletteMunDiv0;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="contour"></param>
		public void AddMunicipalDivision(string name, List<DVector2> contour)
		{
			if (municipalDivisions.ContainsKey(name)) return;

			var mesh = new TriangleNet.Mesh();
			mesh.Behavior.Quality = true;
			mesh.Behavior.MinAngle = 25;
			mesh.Behavior.Convex = false;

			var ig = new InputGeometry();

			ig.AddPoint(contour[0].X, contour[0].Y);
			for (int v = 1; v < contour.Count; v++) {
				ig.AddPoint(contour[v].X, contour[v].Y);
				ig.AddSegment(v - 1, v);
			}
			ig.AddSegment(contour.Count - 1, 0);

			mesh.Triangulate(ig);

			int n = mesh.Vertices.Count;

			mesh.Renumber();

			// Vertices
			var moVerts = new GeoVert[n];
			int i = 0;
			foreach (var pt in mesh.Vertices) {
				moVerts[i] = new GeoVert {
					Lon			= pt.X * Math.PI / 180.0,
					Lat			= pt.Y * Math.PI / 180.0,
					Position	= Vector3.Zero,
					Tex			= Vector4.Zero,
					Color		= Color.White
				};

				i++;
			}

			// Triangles
			var triangles = new int[3 * mesh.Triangles.Count];
			i = 0;
			foreach (var tri in mesh.Triangles) {
				triangles[i * 3 + 0] = tri.P0;
				triangles[i * 3 + 1] = tri.P1;
				triangles[i * 3 + 2] = tri.P2;
				i++;
			}


			// Contour vertices
			var contourVerts = new GeoVert[contour.Count*2];

			contourVerts[1] = new GeoVert {
				Lon			= contour[0].X * Math.PI / 180.0,
				Lat			= contour[0].Y * Math.PI / 180.0,
				Position	= Vector3.Zero,
				Tex			= Vector4.Zero,
				Color		= Color.Red
			};

			for (int j = 1; j < contour.Count; j++) {
				contourVerts[2*j+1] = new GeoVert {
					Lon			= contour[j].X * Math.PI / 180.0,
					Lat			= contour[j].Y * Math.PI / 180.0,
					Position	= Vector3.Zero,
					Tex			= Vector4.Zero,
					Color		= Color.Red
				};
				contourVerts[2*j] = contourVerts[2*(j - 1) + 1];
			}

			contourVerts[0] = contourVerts[contourVerts.Length-1];


			// Create buffers

			var vb		= new VertexBuffer( Game.GraphicsDevice, typeof(GeoVert), moVerts.Length );
			var inds	= new IndexBuffer( Game.GraphicsDevice, triangles.Length );
			var cont	= new VertexBuffer( Game.GraphicsDevice, typeof(GeoVert), contourVerts.Length );


			vb.SetData(moVerts, 0, moVerts.Length);
			inds.SetData(triangles, 0, triangles.Length);
			cont.SetData(contourVerts, 0, contourVerts.Length);

			municipalDivisions.Add(name, new MD {
					Contour		= cont,
					Indeces		= inds,
					Vertices	= vb,
					Value		= RandomExt.NextFloat( r, 0.0f, 1.0f )
				});
		}


	    public void ClearAllMunicipalDivisions()
	    {
	        foreach (var md in municipalDivisions) {
	            md.Value.Vertices.Dispose();
                md.Value.Indeces.Dispose();
                md.Value.Contour.Dispose();
	        }
            municipalDivisions.Clear();
	    }


		public void UpdateMunicipalDivision(string name, float value)
		{
			if (municipalDivisions.ContainsKey(name)) {

				value = MathUtil.Clamp(value, 0.0f, 1.0f);

				municipalDivisions[name].Value = value;
			}
		}

	}
}
