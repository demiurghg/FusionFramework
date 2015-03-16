using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;

using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Input;
using Fusion.Content;
using Fusion.Development;

namespace SkinningDemo {
	public class SkinningDemo : Game {

		const int BoneCount	=	128;

		Scene			scene;
		ConstantBuffer	constBuffer;
		ConstantBuffer	constBufferBones;
		Ubershader		uberShader;
		StateFactory	factory;
		SceneDrawer<VertexColorSkin, object>	sceneDrawer;


		struct CBData {
			public Matrix	Projection;
			public Matrix	View;
			public Matrix	World;
			public Vector4	ViewPos;
		}


		enum RenderFlags {
			None,
		}


		public struct VertexColorSkin {

			[Vertex("POSITION")]		public Vector3	Position;
			[Vertex("COLOR")]			public Color	Color	;
			[Vertex("NORMAL")]			public Vector4	Normal	;
			[Vertex("BLENDINDICES")]	public Int4		BoneIndices;
			[Vertex("BLENDWEIGHT")]		public Vector4	BoneWeights;

			public static VertexColorSkin Bake ( MeshVertex meshVertex )
			{
				VertexColorSkin v;
				v.Position		=	meshVertex.Position;
				v.Color			=	meshVertex.Color0;
				v.Normal		=	new Vector4( meshVertex.Normal, 0 );
				v.BoneIndices	=	meshVertex.SkinIndices;
				v.BoneWeights	=	meshVertex.SkinWeights;
				return v;
			}
		}


		/// <summary>
		/// AnimationDemo constructor
		/// </summary>
		public SkinningDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;
			Parameters.MsaaLevel = 4;

			//	uncomment to enable debug graphics device:
			//	(MS Platform SDK must be installed)
			//	Parameters.UseDebugDevice	=	true;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );

			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += Game_Exiting;
		}


		/// <summary>
		/// Initializes game :
		/// </summary>
		protected override void Initialize ()
		{
			//	initialize services :
			base.Initialize();

			//	add keyboard handler :
			InputDevice.KeyDown += InputDevice_KeyDown;

			//	load content & create graphics and audio resources here:
			constBuffer			=	new ConstantBuffer( GraphicsDevice, typeof(CBData) );
			constBufferBones	=	new ConstantBuffer( GraphicsDevice, typeof(Matrix), BoneCount );

			LoadContent();
			Reloading += (s,e) => LoadContent();

			GetService<Camera>().FreeCamPosition = Vector3.Up * 5 + Vector3.BackwardRH * 10;
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
			SafeDispose( ref sceneDrawer );
			SafeDispose( ref factory );

			scene =	Content.Load<Scene>(@"tube");

			uberShader	=	Content.Load<Ubershader>("render");
			factory		=	new StateFactory( uberShader, typeof(RenderFlags), VertexInputElement.FromStructure<VertexColorSkin>() );

			sceneDrawer	=	new SceneDrawer<VertexColorSkin,object>( GraphicsDevice, scene,
								VertexColorSkin.Bake, (m)=>null );

			Log.Message("{0}", scene.Nodes.Count( n => n.MeshIndex >= 0 ) );
		}



		/// <summary>
		/// Disposes game
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				//	dispose disposable stuff here
				//	Do NOT dispose objects loaded using ContentManager.
				SafeDispose( ref sceneDrawer );
				SafeDispose( ref factory );
				SafeDispose( ref constBuffer );
				SafeDispose( ref constBufferBones );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Handle keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Fusion.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
				DevCon.Show( this );
			}

			if (e.Key == Keys.F2) {
				Parameters.ToggleVSync();
			}

			if (e.Key == Keys.F5) {
				Reload();
			}

			if (e.Key == Keys.F12) {
				GraphicsDevice.Screenshot();
			}

			if (e.Key == Keys.Escape) {
				Exit();
			}
		}



		/// <summary>
		/// Saves configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Game_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}



		/// <summary>
		/// Updates game
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );

			var cam	=	GetService<Camera>();
			var dr	=	GetService<DebugRender>();
			dr.View			=	cam.GetViewMatrix( StereoEye.Mono );
			dr.Projection	=	cam.GetProjectionMatrix( StereoEye.Mono );

			dr.DrawGrid(10);

			frame += 0.1f;

			base.Update( gameTime );
		}


		float frame = 0;

		/// <summary>
		/// Draws game
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			CBData cbData = new CBData();
			var cam	=	GetService<Camera>();

			//GraphicsDevice.ClearBackbuffer( Color.DarkGray, 1, 0 );
			GraphicsDevice.ClearBackbuffer( Color.CornflowerBlue, 1, 0 );


			var dr	=	GetService<DebugRender>();

			
			var localMatricies = new Matrix[ scene.Nodes.Count ];
			var worldMatricies = new Matrix[ scene.Nodes.Count ];
			var boneMatricies = new Matrix[ BoneCount ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );

			//	Animate :
			scene.GetAnimSnapshot( frame, scene.FirstFrame, scene.LastFrame, AnimationMode.Repeat, localMatricies );
			scene.ComputeAbsoluteTransforms( localMatricies, worldMatricies );
			scene.ComputeBoneTransforms( localMatricies, boneMatricies );

			constBufferBones.SetData( boneMatricies );

			sceneDrawer.EvaluateScene();

			sceneDrawer.Draw( gameTime, stereoEye, 

				(time,eye) => new { 
					View = cam.GetViewMatrix(eye), 
					Projection = cam.GetProjectionMatrix(eye),
					ViewPos = cam.GetCameraPosition4(eye),
				},

				(ctxt,node,mesh,vb,ib,w) => {

					cbData.Projection	=	ctxt.Projection;
					cbData.View			=	ctxt.View;
					cbData.World		=	w;
					cbData.ViewPos		=	ctxt.ViewPos;
					constBuffer.SetData( cbData );

					GraphicsDevice.PipelineState		= factory[0];
					GraphicsDevice.DepthStencilState	= DepthStencilState.Default ;

					GraphicsDevice.VertexShaderConstants[0] = constBuffer;
					GraphicsDevice.VertexShaderConstants[1] = constBufferBones;
					GraphicsDevice.PixelShaderConstants[0] = constBuffer;

					GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;

					GraphicsDevice.SetupVertexInput( ib, vb );

					return true;
				},

				(ctxt,subset,mtrl) => {
					GraphicsDevice.DrawIndexed( Primitive.TriangleList, subset.PrimitiveCount*3, subset.StartPrimitive*3, 0);
				}
			);

			base.Draw( gameTime, stereoEye );
		}
	}
}
