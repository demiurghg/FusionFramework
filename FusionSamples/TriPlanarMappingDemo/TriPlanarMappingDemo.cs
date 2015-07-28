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

namespace TriPlanarMappingDemo
{
	public class TriPlanarMappingDemo : Game
	{



		struct CBData {
			public Matrix	ViewProj;
			public Matrix	World;
			public Vector4	ViewPos;
			public Vector4	LodDist;
			public Vector4	ScaleSharp;
		}

		[Flags]
		enum RenderFlags {
			None,
			Wireframe = 0x0001,
		}


		Scene			scene;

		VertexBuffer[]	vertexBuffers;
		IndexBuffer[]	indexBuffers;
		Texture2D[]		textures;
		Matrix[]		worldMatricies;

		ConstantBuffer	constBuffer;
		Ubershader		uberShader;
		StateFactory	factory;
		CBData			constData;


		bool wireframe = false;

		float minLOD			= 1;
		float maxLOD			= 32;
		float minDistance		= 0.0f;
		float maxDistance		= 100.0f;
		float textureScale		= 60;
		float blendSharpness	= 10;



		/// <summary>
		/// TriPlanarMappingDemo constructor
		/// </summary>
		public TriPlanarMappingDemo()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = false;

			//	uncomment to enable debug graphics device:
			//	(MS Platform SDK must be installed)
			//	Parameters.UseDebugDevice	=	true;

			//	add services :
			AddService(new SpriteBatch(this), false, false, 0, 0);
			AddService(new DebugStrings(this), true, true, 9999, 9999);
			AddService(new DebugRender(this), true, true, 9998, 9998);
			AddService(new Camera(this), true, false, 1, 1);

			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			GetService<Camera>().Config.FreeCamEnabled = true;

			//	make configuration saved on exit :
			Exiting += Game_Exiting;
		}


		/// <summary>
		/// Initializes game :
		/// </summary>
		protected override void Initialize()
		{
			//	initialize services :
			base.Initialize();

			//	add keyboard handler :
			InputDevice.KeyDown += InputDevice_KeyDown;

			//	load content & create graphics and audio resources here:
			LoadContent();

			Reloading += (s, e) => LoadContent();

			constBuffer = new ConstantBuffer(GraphicsDevice, typeof(CBData));

			InputDevice.KeyDown += InputDeviceOnKeyDown;
		}

		void InputDeviceOnKeyDown(object sender, InputDevice.KeyEventArgs keyEventArgs)
		{
			float multiplayer = 1.0f;
			if (InputDevice.IsKeyDown(Keys.LeftShift)) multiplayer = 10.0f;

			if (keyEventArgs.Key == Keys.T) minDistance -= 1.0f * multiplayer;
			if (keyEventArgs.Key == Keys.Y) minDistance += 1.0f * multiplayer;

			if (keyEventArgs.Key == Keys.G) maxDistance -= 1.0f * multiplayer;
			if (keyEventArgs.Key == Keys.H) maxDistance += 1.0f * multiplayer;

			if (keyEventArgs.Key == Keys.B) textureScale -= 1.0f * multiplayer;
			if (keyEventArgs.Key == Keys.N) textureScale += 1.0f * multiplayer;

			if (keyEventArgs.Key == Keys.Q) wireframe = !wireframe;
		}


		void EnumFunc(PipelineState p, RenderFlags f)
		{
			p.BlendState			= BlendState.Opaque;
			p.DepthStencilState		= DepthStencilState.Default;
			p.Primitive				= Primitive.PatchList3CP;
			p.VertexInputElements	= VertexColorTextureTBN.Elements;
			p.RasterizerState		= f.HasFlag(RenderFlags.Wireframe) ? RasterizerState.Wireframe : RasterizerState.CullCW;
		}


		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
			SafeDispose( ref factory );
			SafeDispose( ref vertexBuffers );
			SafeDispose( ref indexBuffers );

			uberShader	=	Content.Load<Ubershader>("render");

			//factory		=	new StateFactory( 
			//					uberShader, 
			//					typeof(RenderFlags), 
			//					Primitive.PatchList3CP, 
			//					VertexColorTextureTBN.Elements,
			//					BlendState.Opaque,
			//					RasterizerState.CullCW,
			//					DepthStencilState.Default 
			//				);

			factory = new StateFactory(uberShader, typeof(RenderFlags), (x, i) => EnumFunc(x, (RenderFlags)i));

			scene		=	Content.Load<Scene>(@"scene");


			vertexBuffers	=	scene.Meshes
							.Select(m => VertexBuffer.Create(GraphicsDevice, m.Vertices.Select(VertexColorTextureTBN.Convert).ToArray()))
							.ToArray();

			indexBuffers	=	scene.Meshes
							.Select( m => IndexBuffer.Create( GraphicsDevice, m.GetIndices() ) )
							.ToArray();

			textures = new Texture2D[5];
			textures[0] = Content.Load<Texture2D>("checker");
			textures[1] = Content.Load<Texture2D>(@"Textures\rockA");
			textures[2] = Content.Load<Texture2D>(@"Textures\rockA_local");
			textures[3] = Content.Load<Texture2D>(@"Textures\sandDune");
			textures[4] = Content.Load<Texture2D>(@"Textures\sandDune_local");


			worldMatricies	=	new Matrix[ scene.Nodes.Count ];
			scene.CopyAbsoluteTransformsTo( worldMatricies );

			Log.Message("{0}", scene.Nodes.Count( n => n.MeshIndex >= 0 ) );
		}


		/// <summary>
		/// Disposes game
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				SafeDispose(ref constBuffer);
				SafeDispose(ref factory);
				SafeDispose(ref vertexBuffers);
				SafeDispose(ref indexBuffers);
			}
			base.Dispose(disposing);
		}



		/// <summary>
		/// Handle keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
		{
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
		void Game_Exiting(object sender, EventArgs e)
		{
			SaveConfiguration();
		}



		/// <summary>
		/// Updates game
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update(GameTime gameTime)
		{
			var ds = GetService<DebugStrings>();

			ds.Add(Color.Orange, "FPS {0}", gameTime.Fps);
			ds.Add("F1   - show developer console");
			ds.Add("F5   - build content and reload textures");
			ds.Add("F12  - make screenshot");
			ds.Add("ESC  - exit");
			ds.Add(" ");
			ds.Add("Press Q to toggle wireframe mode ");
			ds.Add("Press T/Y to dec/inc tes min distance");
			ds.Add("Press G/H to dec/inc tes max distance");
			ds.Add("Press B/N to dec/inc triplanar texture scale");
			ds.Add(" ");
			ds.Add(" ");
			ds.Add("Min Lod : " + minLOD);
			ds.Add("Max Lod : " + maxLOD);
			ds.Add(" ");
			ds.Add("Min Distance : " + minDistance);
			ds.Add("Max Distance : " + maxDistance);
			ds.Add(" ");
			ds.Add("Wireframe : " + wireframe);
			ds.Add("Blend sharpness : " + blendSharpness);
			ds.Add("Texture scale   : " + textureScale);



			var cam	=	GetService<Camera>();
			var dr	=	GetService<DebugRender>();
			dr.View			=	cam.GetViewMatrix( StereoEye.Mono );
			dr.Projection	=	cam.GetProjectionMatrix( StereoEye.Mono );

			//dr.DrawGrid(10);


			base.Update(gameTime);
		}



		/// <summary>
		/// Draws game
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw(GameTime gameTime, StereoEye stereoEye)
		{
			var cam	=	GetService<Camera>();

			GraphicsDevice.ClearBackbuffer(new Color(50, 70, 100, 255), 1, 0);

			constData.World		= Matrix.Identity;
			constData.ViewProj	= cam.GetViewMatrix(stereoEye) * cam.GetProjectionMatrix(stereoEye);
			constData.ViewPos	= new Vector4(cam.GetCameraMatrix(stereoEye).TranslationVector, 1);
			constData.World		= Matrix.Identity;
			constData.LodDist		= new Vector4(minLOD, maxLOD, minDistance, maxDistance);
			constData.ScaleSharp	= new Vector4(textureScale, blendSharpness, 0, 0);

			GraphicsDevice.PipelineState			= factory[wireframe ? (int)RenderFlags.Wireframe : 0];
			GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearPointWrap;
			GraphicsDevice.DomainShaderSamplers[0]	= SamplerState.LinearPointWrap;
			
			GraphicsDevice.PixelShaderConstants[0]	= constBuffer;
			GraphicsDevice.VertexShaderConstants[0] = constBuffer;
			GraphicsDevice.HullShaderConstants[0]	= constBuffer;
			GraphicsDevice.DomainShaderConstants[0] = constBuffer;


			for (int i=0; i<scene.Nodes.Count; i++) {

				int meshId	=	scene.Nodes[i].MeshIndex;

				if (meshId<0) {
					continue;
				}

				constData.World	=	worldMatricies[ i ];
				constBuffer.SetData( constData );

				GraphicsDevice.SetupVertexInput( vertexBuffers[meshId], indexBuffers[meshId] );

				foreach ( var subset in scene.Meshes[meshId].Subsets ) {
					GraphicsDevice.PixelShaderResources[0] = textures[3];
					GraphicsDevice.PixelShaderResources[1] = textures[4];
					GraphicsDevice.PixelShaderResources[2] = textures[1];
					GraphicsDevice.PixelShaderResources[3] = textures[2];

					GraphicsDevice.DomainShaderResources[0] = textures[3];
					GraphicsDevice.DomainShaderResources[1] = textures[4];
					GraphicsDevice.DomainShaderResources[2] = textures[1];
					GraphicsDevice.DomainShaderResources[3] = textures[2];

					GraphicsDevice.DrawIndexed( subset.PrimitiveCount * 3, subset.StartPrimitive * 3, 0 );
				}
			}
			
			
			base.Draw(gameTime, stereoEye);
		}
	}
}
