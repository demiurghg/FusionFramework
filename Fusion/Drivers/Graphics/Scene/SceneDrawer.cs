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

	/// <summary>
	/// 
	/// </summary>
	public abstract class SceneDrawer<TContext, TMaterial, TVertex> : DisposableBase where TVertex: struct {

		public Game Game { get; private set; }
		public GraphicsDevice GraphicsDevice  { get { return device; } }

		GraphicsDevice device;

		Scene	scene;
		IndexBuffer[] ibs;
		VertexBuffer[] vbs;
		VertexInputElement[] vie;
		Matrix[] localMatricies;
		Matrix[] worldMatricies;
		Matrix[] boneMatricies;

		TMaterial[]	materials;


		/// <summary>
		/// Gets global evaluates world matricies.
		/// </summary>
		public Matrix[] WorldMatricies {
			get {
				return worldMatricies;
			}
		}


		/// <summary>
		/// Gets bone matricies for skinning with applied bind-pose transform.
		/// </summary>
		public Matrix[] BoneMatricies {
			get {
				return boneMatricies;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="bakeFunc"></param>
		public SceneDrawer ( GraphicsDevice device, Scene scene )
		{								
			Game		=	device.Game;
			this.device	=	device;
			this.scene	=	scene;

			int meshCount = scene.Meshes.Count;

			ibs = new IndexBuffer[ scene.Meshes.Count ];
			vbs = new VertexBuffer[ scene.Meshes.Count ];
			vie = VertexInputElement.FromStructure( typeof(TVertex) );

			materials = new TMaterial[ scene.Materials.Count ];

			//	convert materials :
			for ( int i=0; i<scene.Materials.Count; i++ ) {
				materials[i] = Convert( scene.Materials[i] );
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
					vdata[j] = Convert( mesh.Vertices[j] );
				}

				vbs[i].SetData( vdata );
			}

			localMatricies	=	new Matrix[ scene.Nodes.Count ];
			worldMatricies	=	new Matrix[ scene.Nodes.Count ];	
			boneMatricies	=	new Matrix[ scene.Nodes.Count ];

			EvaluateScene();
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
			if (scene.Nodes.Count!=worldMatricies.Length) {
				throw new InvalidOperationException("Scene had been changed: scene node count does not equal to global matricies count.");
			}

			scene.CopyAbsoluteTransformsTo( worldMatricies );
			scene.CopyLocalTransformsTo( localMatricies );
			scene.ComputeBoneTransforms( localMatricies, boneMatricies );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="firstFrame"></param>
		/// <param name="lastFrame"></param>
		/// <param name="animMode"></param>
		public void EvaluateScene ( float frame, int firstFrame, int lastFrame, AnimationMode animMode )
		{
			EvaluateScene();

			scene.GetAnimSnapshot( frame, firstFrame, lastFrame, AnimationMode.Repeat, localMatricies );
			scene.ComputeBoneTransforms( localMatricies, boneMatricies );
			scene.ComputeAbsoluteTransforms( localMatricies, worldMatricies );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="frame"></param>
		/// <param name="animMode"></param>
		public void EvaluateScene ( float frame, AnimationMode animMode )
		{
			EvaluateScene( frame, scene.FirstFrame, scene.LastFrame, animMode );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="destination"></param>
		public void CopyBoneTransformsTo ( Matrix[] destination )
		{
			int count = Math.Min( destination.Length, boneMatricies.Length );

			Array.Copy( boneMatricies, destination, count );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertex"></param>
		/// <returns></returns>
		public abstract TVertex Convert ( MeshVertex vertex );



		/// <summary>
		/// 
		/// </summary>
		/// <param name="material"></param>
		/// <returns></returns>
		public abstract TMaterial Convert ( MeshMaterial material );



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <returns></returns>
		public abstract TContext Prepare( GameTime gameTime, StereoEye stereoEye );



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <returns></returns>
		public abstract void PrepareMesh ( TContext context, Mesh mesh, VertexBuffer vb, IndexBuffer ib );



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <returns></returns>
		public abstract bool PrepareNode ( TContext context, Node node, Matrix worldMatrix );



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		/// <returns></returns>
		public abstract void DrawSubset ( TContext context, MeshSubset subset, TMaterial material );



		/// <summary>
		/// 
		/// </summary>
		public void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			var context = Prepare( gameTime, stereoEye );

			for (int i=0; i<scene.Nodes.Count; i++) {

				int meshId = scene.Nodes[i].MeshIndex;

				if (meshId<0) {
					continue;
				}

				var node	=	scene.Nodes[ i ];
				var mesh	=	scene.Meshes[ meshId ];
				var vb		=	vbs[ meshId ];
				var ib		=	ibs[ meshId ];
				var wm		=	worldMatricies[ i ];

				PrepareMesh( context, mesh, vb, ib );

				bool vis	=	PrepareNode ( context, scene.Nodes[i], wm );

				if (!vis) {
					return;
				}

				for ( int j=0; j<mesh.Subsets.Count; j++) {
					
					var mtrlId	=	mesh.Subsets[j].MaterialIndex;

					DrawSubset( context, mesh.Subsets[j], materials[ mtrlId ] );
				}
			}
		}
	}
}
