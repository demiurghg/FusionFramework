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
			scene =	Content.Load<Scene>(@"tube");

			scene.Bake<VertexColorSkin>( GraphicsDevice, VertexColorSkin.Bake );

			uberShader	=	Content.Load<Ubershader>("render");
			uberShader.Map( typeof(RenderFlags) );

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


			uberShader.SetPixelShader(0);
			uberShader.SetVertexShader(0);

			var dr	=	GetService<DebugRender>();

			
			var localMatricies = new Matrix[ scene.Nodes.Count ];
			var worldMatricies = new Matrix[ scene.Nodes.Count ];
			var boneMatricies = new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );

			//	Animate :
			/*frame++;
			if (frame>scene.LastFrame) {
				frame = scene.FirstFrame;
			}
			if (frame<scene.FirstFrame) {
				frame = scene.FirstFrame;
			} */
			frame += 0.1f;

			scene.GetAnimSnapshot( frame, scene.FirstFrame, scene.LastFrame, AnimationMode.Repeat, localMatricies );
			scene.ComputeAbsoluteTransforms( localMatricies, worldMatricies );
			scene.ComputeBoneTransforms( localMatricies, boneMatricies );


			for (int j=1; j<worldMatricies.Length; j++) {
				dr.DrawLine( worldMatricies[j-1].TranslationVector, worldMatricies[j].TranslationVector, Color.LightYellow );
			}
			foreach ( var wm in worldMatricies ) {
				dr.DrawBasis( wm, 0.3f );
			}

			for (int j = 0; j<1; j++) {
				for ( int i=0; i<scene.Nodes.Count; i++ ) {

					var node = scene.Nodes[i];
				
					if (node.MeshIndex==-1) {
						continue;
					}

					var mesh = scene.Meshes[ node.MeshIndex ];

					cbData.Projection	=	cam.GetProjectionMatrix( stereoEye );
					cbData.View			=	cam.GetViewMatrix( stereoEye );
					cbData.World		=	Matrix.RotationYawPitchRoll(j*0.01f,j*0.02f,j*0.03f) * worldMatricies[ i ] * Matrix.Scaling( (float)Math.Pow(0.9,j) );
					cbData.ViewPos		=	new Vector4( cam.GetCameraMatrix( stereoEye ).TranslationVector, 1 );

					constBuffer.SetData( cbData );
					constBufferBones.SetData( boneMatricies ); 

					GraphicsDevice.RasterizerState		= RasterizerState.CullCW ;
					GraphicsDevice.DepthStencilState	= DepthStencilState.Default ;
					GraphicsDevice.BlendState			= BlendState.Opaque ;
					GraphicsDevice.PixelShaderConstants[0]	= constBuffer ;
					GraphicsDevice.VertexShaderConstants[0]	= constBuffer ;
					GraphicsDevice.VertexShaderConstants[1]	= constBufferBones ;
					GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.AnisotropicWrap ;

					mesh.SetupVertexInput();

					foreach ( var subset in mesh.Subsets ) {
						//GraphicsDevice.PSShaderResources[0]	=	mesh.Materials[ subset.MaterialIndex ].Tag as Texture2D ;
						mesh.Draw( subset.StartPrimitive, subset.PrimitiveCount );
					}
				}
			}

			base.Draw( gameTime, stereoEye );
		}
	}
}
