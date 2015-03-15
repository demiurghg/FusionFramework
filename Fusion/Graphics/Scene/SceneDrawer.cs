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
	public class SceneDrawer<TVertex, TMaterial, TContext> : DisposableBase where TVertex: struct {

		GraphicsDevice device;

		Scene	scene;
		IndexBuffer[] ibs;
		VertexBuffer[] vbs;
		VertexInputElement[] vie;
		Matrix[] matricies;

		TMaterial[]	materials;
		TContext	context;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="bakeFunc"></param>
		public SceneDrawer ( GraphicsDevice device, Scene scene, Func<MeshVertex,TVertex> vertexFunc, Func<MeshMaterial,TMaterial> materialFunc )
		{
			this.device	=	device;
			this.scene	=	scene;

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

			matricies	=	new Matrix[ scene.Nodes.Count ];
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



		/// <summary>
		/// Evaluates scene. Performs integrity scene and scene drawer integrity check.
		/// Calculates absolute transforms.
		/// Call this method before draw.
		/// </summary>
		public void EvaluateScene ()
		{
			if (scene.Meshes.Count!=vbs.Length) {
				throw new InvalidOperationException("Scene had been changed: mesh count does not equal to vertex buffers count.");
			}
			if (scene.Meshes.Count!=ibs.Length) {
				throw new InvalidOperationException("Scene had been changed: mesh count does not equal to index buffers count.");
			}
			if (scene.Materials.Count!=materials.Length) {
				throw new InvalidOperationException("Scene had been changed: scene material count does not equal to material count.");
			}
			if (scene.Nodes.Count!=matricies.Length) {
				throw new InvalidOperationException("Scene had been changed: scene node count does not equal to global matricies count.");
			}

			scene.CopyAbsoluteTransformsTo( matricies );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public delegate TContext Prepare ( GameTime gameTime, StereoEye stereoEye );


		/// <summary>
		/// Mesh preparation function.
		/// Usually this method sets up vertex and index buffer, binds per node constant buffers, performs frustum culling.
		/// </summary>
		/// <param name="node">Node to prepare</param>
		/// <param name="mesh">Mesh to prepare</param>
		/// <param name="vertexBuffer">Vertex buffer with mesh data</param>
		/// <param name="indexBuffer">Index buffer with mesh data</param>
		/// <param name="worldMatrix">Evaluates world matrix</param>
		/// <returns>Whether this node or mesh should be rendered</returns>
		public delegate bool MeshPrepare ( TContext context, Node node, Mesh mesh, VertexBuffer vertexBuffer, IndexBuffer indexBuffer, Matrix worldMatrix );


		/// <summary>
		/// Subset draw function.
		/// </summary>
		/// <param name="meshSubset">Mesh subset to render</param>
		/// <param name="material">Material to render</param>
		public delegate void SubsetDraw ( TContext context, MeshSubset meshSubset, TMaterial material );



		/// <summary>
		/// 
		/// </summary>
		public void Draw ( GameTime gameTime, StereoEye stereoEye, Prepare prepare, MeshPrepare meshPrepare, SubsetDraw subsetDraw )
		{
			context = prepare( gameTime, stereoEye );

			for (int i=0; i<scene.Nodes.Count; i++) {

				int meshId = scene.Nodes[i].MeshIndex;

				if (meshId<0) {
					continue;
				}

				var node	=	scene.Nodes[ i ];
				var mesh	=	scene.Meshes[ meshId ];
				var vb		=	vbs[ meshId ];
				var ib		=	ibs[ meshId ];
				var wm		=	matricies[ i ];

				bool draw	=	meshPrepare( context, node, mesh, vb, ib, wm );

				if (!draw) {
					continue;
				}


				for ( int j=0; j<mesh.Subsets.Count; j++) {
					
					var mtrlId	=	mesh.Subsets[j].MaterialIndex;

					subsetDraw( context, mesh.Subsets[j], materials[ mtrlId ] );
				}

			}
		}
	}
}
