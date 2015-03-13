using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using SharpDX;
using Fusion.Graphics;
using System.Reflection;
using System.ComponentModel.Design;
using Fusion.Mathematics;

namespace Fusion.Graphics {

	/// <summary>
	/// 
	/// </summary>
	public class SceneDrawer<TVertex, TMaterial> : DisposableBase where TVertex: struct{

		IndexBuffer[] ibs;
		VertexBuffer[] vbs;
		VertexInputElement[] vie;

		TMaterial[]	materials;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="bakeFunc"></param>
		public SceneDrawer ( GraphicsDevice device, Scene scene, Func<MeshVertex,TVertex> vertexFunc, Func<MeshMaterial,TMaterial> materialFunc )
		{
			int meshCount = scene.Meshes.Count;

			ibs = new IndexBuffer[ scene.Meshes.Count ];
			vbs = new VertexBuffer[ scene.Meshes.Count ];
			vie = VertexInputElement.FromStructure( typeof(TVertex) );

			materials = new TMaterial[ scene.Materials.Count ];

			//	convert materials :
			for ( int i=0; i<scene.Materials.Count; i++ ) {
				materials[i] = materialFunc( scene.Materials[i] );
			}

			//	convert meshes to vb in ib :
			for ( int i = 0; i<meshCount; i++ ) {

				var mesh = scene.Meshes[i];

				// index buffer :
				ibs[i] = new IndexBuffer( device, mesh.IndexCount );
				ibs[i].SetData( mesh.GetIndices() );

				// vertex buffer :
				vbs[i] = new VertexBuffer( device, typeof(TVertex), mesh.VertexCount );
				var vdata = new TVertex[ mesh.VertexCount ];

				for (int j=0; j<mesh.VertexCount; j++) {
					vdata[j] = vertexFunc( mesh.Vertices[j] );
				}

				vbs[i].SetData( vdata );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				
				for (int i=0; i<ibs.Length; i++) {
					ibs[i].Dispose();
					vbs[i].Dispose();
				}
			}

			base.Dispose( disposing );
		}
	}
}
