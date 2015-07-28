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
		MySceneDrawer	sceneDrawer;


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

			public static VertexColorSkin Convert ( MeshVertex meshVertex )
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


		class Material {
		}


		class Context {
			public Matrix	View;
			public Matrix	Projection;
			public Vector4	ViewPosition;
		}



		class MySceneDrawer : SceneDrawer<Context,Material,VertexColorSkin> {
													
			ConstantBuffer	constBuffer;
			ConstantBuffer	constBufferBones;
			Ubershader		uberShader;
			StateFactory	factory;
			
			CBData			constData;

			Matrix[]		boneTransforms;


			public MySceneDrawer ( GraphicsDevice device, Scene scene ) : base(device, scene)
			{
				constBuffer			=	new ConstantBuffer( GraphicsDevice, typeof(CBData) );
				constBufferBones	=	new ConstantBuffer( GraphicsDevice, typeof(Matrix), BoneCount );
				uberShader			=	Game.Content.Load<Ubershader>("render");
				factory				=	new StateFactory( uberShader, typeof(RenderFlags), Primitive.TriangleList, VertexInputElement.FromStructure<VertexColorSkin>() );

				boneTransforms		=	new Matrix[ BoneCount ];
			}


			protected override void Dispose ( bool disposing )
			{
				if (disposing) {
					SafeDispose( ref factory );
					SafeDispose( ref constBuffer );
					SafeDispose( ref constBufferBones );
				}
				base.Dispose( disposing );
			}


			public override Material Convert ( MeshMaterial material )
			{
				return new Material();
			}


			public override VertexColorSkin Convert ( MeshVertex vertex )
			{
				return VertexColorSkin.Convert( vertex );
			}


			public override Context Prepare ( GameTime gameTime, StereoEye stereoEye )
			{
				var cam = Game.GetService<Camera>();

				CopyBoneTransformsTo( boneTransforms );
				constBufferBones.SetData( boneTransforms );

				return new Context() {
					View			=	cam.GetViewMatrix( stereoEye ),
					Projection		=	cam.GetProjectionMatrix( stereoEye ),
					ViewPosition	=	cam.GetCameraPosition4( stereoEye )
				};
			}


			public override void PrepareMesh ( Context context, Mesh mesh, VertexBuffer vb, IndexBuffer ib )
			{
				GraphicsDevice.SetupVertexInput( vb, ib );
			}


			public override bool PrepareNode ( Context context, Node node, Matrix worldMatrix )
			{
				constData.View			=	context.View;
				constData.Projection	=	context.Projection;
				constData.ViewPos		=	context.ViewPosition;
				constData.World			=	worldMatrix;

				constBuffer.SetData( constData );

				GraphicsDevice.PipelineState			=	factory[0];
				GraphicsDevice.PixelShaderSamplers[0]	=	SamplerState.AnisotropicWrap;
				GraphicsDevice.VertexShaderConstants[0]	=	constBuffer;
				GraphicsDevice.VertexShaderConstants[1]	=	constBufferBones;

				return true;
			}


			public override void DrawSubset ( Context context, MeshSubset subset, Material material )
			{
				GraphicsDevice.DrawIndexed( subset.PrimitiveCount * 3, subset.StartPrimitive, 0 );
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

			//	Force to enable free camera.
			GetService<Camera>().Config.FreeCamEnabled	=	true;

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

			scene =	Content.Load<Scene>(@"tube");
			sceneDrawer	=	new MySceneDrawer( GraphicsDevice, scene );
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
				//DevCon.Show( this );
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
			GraphicsDevice.ClearBackbuffer( Color.CornflowerBlue, 1, 0 );

			sceneDrawer.EvaluateScene( frame, AnimationMode.Repeat );
			sceneDrawer.Draw( gameTime, stereoEye );

			base.Draw( gameTime, stereoEye );
		}
	}
}
