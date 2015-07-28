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
using BEPUphysics;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;
using BEPUutilities;
using Vector3BEPU = BEPUutilities.Vector3;
using MatrixBEPU = BEPUutilities.Matrix;
using BEPUphysics.BroadPhaseEntries;
using Vector3Fusion = Fusion.Mathematics.Vector3;
using Vector4Fusion = Fusion.Mathematics.Vector4;


namespace PhysicsDemo {
	public class PhysicsDemo: Game {
		/// <summary>
		/// PhysicsDemo constructor
		/// </summary>
		public PhysicsDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = false;
			Parameters.VSyncInterval = 0;
			Parameters.MsaaLevel = 4;

			//	add services :
			AddService(new SpriteBatch(this), false, false, 0, 0);
			AddService(new DebugStrings(this), true, true, 9999, 9999);
			AddService(new DebugRender(this), true, true, 9998, 9998);
			AddService(new Camera(this), true, false, 1, 1);

			//	load configuration :
			LoadConfiguration();

			//	Force to enable free camera.
			GetService<Camera>().Config.FreeCamEnabled	=	true;

			//	make configuration saved on exit
			Exiting += FusionGame_Exiting;
			InputDevice.KeyDown += InputDevice_KeyDown;
		}

		Space space;
		ConstantBuffer constBuffer;
		Ubershader uberShader;
		StateFactory factory;
		bool flag = false;
		Texture2D texture;
		Random random = new Random();
		int numberOfBoxes = 500;
		CubeVertex[]  data;


		struct CBData {
			public Fusion.Mathematics.Matrix Projection;
			public Fusion.Mathematics.Matrix View;
			public Fusion.Mathematics.Matrix World;
			public Vector4Fusion ViewPos;
			public Vector4Fusion Color;
		}


		enum RenderFlags {
			None,
		}


		struct CubeVertex {
			[Vertex("POSITION")]
			public Vector3Fusion Position;
			[Vertex("NORMAL")]
			public Vector3Fusion Normal;
			[Vertex("COLOR")]
			public Vector4Fusion Color;
			[Vertex("TEXCOORD")]
			public Fusion.Mathematics.Vector2 TexCoord;
		}

		VertexBuffer vb;
		IndexBuffer ib;

		/// <summary>
		/// Add services :
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			constBuffer = new ConstantBuffer(GraphicsDevice, typeof(CBData));

			LoadContent();
			Reloading += (s, e) => LoadContent();

			GetService<Camera>().FreeCamPosition = new Vector3Fusion(0, 12, 21);
						
			//	fill vertex buffer for cube:
			Vector4Fusion color = new Vector4Fusion(Vector3Fusion.Zero, 1);
			Fusion.Mathematics.Vector2 texcoord = Fusion.Mathematics.Vector2.Zero;

			// back face
			var v0 = new CubeVertex { Position = new Vector3Fusion(-0.5f, -0.5f, -0.5f), Normal = new Vector3Fusion(-1.0f, 0, 0), Color = color, TexCoord = texcoord };
			var v1 = new CubeVertex { Position = new Vector3Fusion(-0.5f, -0.5f, 0.5f), Normal = new Vector3Fusion(-1.0f, 0, 0), Color = color, TexCoord = texcoord };
			var v2 = new CubeVertex { Position = new Vector3Fusion(-0.5f, 0.5f, 0.5f), Normal = new Vector3Fusion(-1.0f, 0, 0), Color = color, TexCoord = texcoord };
			var v3 = new CubeVertex { Position = new Vector3Fusion(-0.5f, 0.5f, -0.5f), Normal = new Vector3Fusion(-1.0f, 0, 0), Color = color, TexCoord = texcoord };

			// front
			var v4 = new CubeVertex { Position = new Vector3Fusion(0.5f, -0.5f, -0.5f), Normal = new Vector3Fusion(1.0f, 0, 0), Color = color, TexCoord = texcoord };
			var v5 = new CubeVertex { Position = new Vector3Fusion(0.5f, -0.5f, 0.5f), Normal = new Vector3Fusion(1.0f, 0, 0), Color = color, TexCoord = texcoord };
			var v6 = new CubeVertex { Position = new Vector3Fusion(0.5f, 0.5f, 0.5f), Normal = new Vector3Fusion(1.0f, 0, 0), Color = color, TexCoord = texcoord };
			var v7 = new CubeVertex { Position = new Vector3Fusion(0.5f, 0.5f, -0.5f), Normal = new Vector3Fusion(1.0f, 0, 0), Color = color, TexCoord = texcoord };

			// left
			var v8 = new CubeVertex { Position = new Vector3Fusion(-0.5f, -0.5f, -0.5f), Normal = new Vector3Fusion(0, 0, -1.0f), Color = color, TexCoord = texcoord };
			var v9 = new CubeVertex { Position = new Vector3Fusion(0.5f, -0.5f, -0.5f), Normal = new Vector3Fusion(0, 0, -1.0f), Color = color, TexCoord = texcoord };
			var v10 = new CubeVertex { Position = new Vector3Fusion(0.5f, 0.5f, -0.5f), Normal = new Vector3Fusion(0, 0, -1.0f), Color = color, TexCoord = texcoord };
			var v11 = new CubeVertex { Position = new Vector3Fusion(-0.5f, 0.5f, -0.5f), Normal = new Vector3Fusion(0, 0, -1.0f), Color = color, TexCoord = texcoord };

			// right
			var v12 = new CubeVertex { Position = new Vector3Fusion(-0.5f, -0.5f, 0.5f), Normal = new Vector3Fusion(0, 0, 1.0f), Color = color, TexCoord = texcoord };
			var v13 = new CubeVertex { Position = new Vector3Fusion(0.5f, -0.5f, 0.5f), Normal = new Vector3Fusion(0, 0, 1.0f), Color = color, TexCoord = texcoord };
			var v14 = new CubeVertex { Position = new Vector3Fusion(0.5f, 0.5f, 0.5f), Normal = new Vector3Fusion(0, 0, 1.0f), Color = color, TexCoord = texcoord };
			var v15 = new CubeVertex { Position = new Vector3Fusion(-0.5f, 0.5f, 0.5f), Normal = new Vector3Fusion(0, 0, 1.0f), Color = color, TexCoord = texcoord };

			// top
			var v16 = new CubeVertex { Position = new Vector3Fusion(0.5f, 0.5f, -0.5f), Normal = new Vector3Fusion(0, 1.0f, 0), Color = color, TexCoord = texcoord };
			var v17 = new CubeVertex { Position = new Vector3Fusion(0.5f, 0.5f, 0.5f), Normal = new Vector3Fusion(0, 1.0f, 0), Color = color, TexCoord = texcoord };
			var v18 = new CubeVertex { Position = new Vector3Fusion(-0.5f, 0.5f, 0.5f), Normal = new Vector3Fusion(0, 1.0f, 0), Color = color, TexCoord = texcoord };
			var v19 = new CubeVertex { Position = new Vector3Fusion(-0.5f, 0.5f, -0.5f), Normal = new Vector3Fusion(0, 1.0f, 0), Color = color, TexCoord = texcoord };

			// bottom
			var v20 = new CubeVertex { Position = new Vector3Fusion(0.5f, -0.5f, -0.5f), Normal = new Vector3Fusion(0, -1.0f, 0), Color = color, TexCoord = texcoord };
			var v21 = new CubeVertex { Position = new Vector3Fusion(0.5f, -0.5f, 0.5f), Normal = new Vector3Fusion(0, -1.0f, 0), Color = color, TexCoord = texcoord };
			var v22 = new CubeVertex { Position = new Vector3Fusion(-0.5f, -0.5f, 0.5f), Normal = new Vector3Fusion(0, -1.0f, 0), Color = color, TexCoord = texcoord };
			var v23 = new CubeVertex { Position = new Vector3Fusion(-0.5f, -0.5f, -0.5f), Normal = new Vector3Fusion(0, -1.0f, 0), Color = color, TexCoord = texcoord };

			data = new CubeVertex[] { v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17, v18, v19, v20, v21, v22, v23 };
			vb.SetData(data, 0, 24);

			// fill the index buffer
			var index = new int[] { 3, 1, 0,	// back
									3, 2, 1,
									4, 5, 7,	// front
									7, 5, 6,
									8, 9, 11,	// left
									11, 9, 10,
									14, 13, 12,	// right
									14, 12, 15,
									19, 16, 17,	// top
									19, 17, 18,
									20, 23, 22,	// bottom
									20, 22, 21};
			ib.SetData(index);
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void LoadContent ()
		{
			uberShader = Content.Load<Ubershader>("render");
			factory = new StateFactory(uberShader, typeof(RenderFlags), Primitive.TriangleList, VertexInputElement.FromStructure<CubeVertex>());
			texture = Content.Load<Texture2D>(@"Scenes\lena");

			vb = new VertexBuffer(GraphicsDevice, typeof(CubeVertex), 24);
			ib = new IndexBuffer(GraphicsDevice, 36);

			// create a new space with physics
			space = new Space();

			// update gravity force
			space.ForceUpdater.Gravity = new Vector3BEPU(0, -9.81f, 0);

			// add ground, ground has infinite mass
			Box ground = new Box(new Vector3BEPU(0, 0, 0), 50, 1, 50);
			space.Add(ground);

			// create boxes with random position and add color as a tag, then add box to space
			for ( int i = 0; i < numberOfBoxes; i++ ) {
				Vector3Fusion vector = RandomExt.NextVector3(random, new Vector3Fusion(-10, 20, -10), new Vector3Fusion(10, 80, 10));
				Box box = new Box(new Vector3BEPU(vector.X, vector.Y, vector.Z), 1, 1, 1, 1);

				box.Tag = RandomExt.NextColor(random);
				space.Add(box);
			}
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose (bool disposing)
		{
			if ( disposing ) {
				SafeDispose(ref constBuffer);
				SafeDispose(ref vb);
				SafeDispose(ref ib);
			}

			base.Dispose(disposing);
		}



		/// <summary>
		/// Handle keys for each demo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown (object sender, Fusion.Input.InputDevice.KeyEventArgs e)
		{
			if ( e.Key == Keys.F1 ) {
				//DevCon.Show(this);
			}

			if ( e.Key == Keys.F2 ) {
				Parameters.ToggleVSync();
			}

			if ( e.Key == Keys.F5 ) {
				Reload();
			}

			if ( e.Key == Keys.F12 ) {
				GraphicsDevice.Screenshot();
			}

			if ( e.Key == Keys.Escape ) {
				Exit();
			}

			if (e.Key==Keys.T) {
				foreach ( var box in space.Entities ) {
					box.AngularMomentum *= 10.1f;
				}
			}

			// pause physics
			if ( e.Key == Keys.P ) {
				if ( flag ) {
					flag = false;
				} else {
					flag = true;
				}
			}

			// shoot box from camera
			if ( e.Key == Keys.LeftButton ) {
				Vector3Fusion vector = GetService<Camera>().FreeCamPosition;
				Box box = new Box(new Vector3BEPU(vector.X, vector.Y, vector.Z), 1, 1, 1, 1);
				Vector3Fusion velocity = 10 * GetService<Camera>().GetCameraMatrix(StereoEye.Mono).Forward;
				box.LinearVelocity = new Vector3BEPU(velocity.X, velocity.Y, velocity.Z);
				box.Tag = RandomExt.NextColor(random);
				space.Add(box);
			}

			// add new box somewhere
			if ( e.Key == Keys.O ) {
				Vector3Fusion vector = RandomExt.NextVector3(random, new Vector3Fusion(-10, 20, -10), new Vector3Fusion(10, 30, 10));
				Box box = new Box(new Vector3BEPU(vector.X, vector.Y, vector.Z), 1, 1, 1, 1);
				box.Tag = RandomExt.NextColor(random);
				space.Add(box);
			}
		}



		/// <summary>
		/// Save configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void FusionGame_Exiting (object sender, EventArgs e)
		{
			SaveConfiguration();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update (GameTime gameTime)
		{
			var ds = GetService<DebugStrings>();

			ds.Add(Color.Orange, "FPS {0}", gameTime.Fps);
			ds.Add("F1   - show developer console");
			ds.Add("F2   - toggle vsync");
			ds.Add("F5   - build content and reload textures");
			ds.Add("F12  - make screenshot");
			ds.Add("ESC  - exit");


			var cam = GetService<Camera>();
			var dr = GetService<DebugRender>();
			dr.View = cam.GetViewMatrix(StereoEye.Mono);
			dr.Projection = cam.GetProjectionMatrix(StereoEye.Mono);

			dr.DrawGrid(10);

			// physics updates here
			if ( flag ) {
				space.Update(gameTime.ElapsedSec);
			}

			base.Update(gameTime);
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw (GameTime gameTime, StereoEye stereoEye)
		{
			CBData cbData = new CBData();

			var cam = GetService<Camera>();

			GraphicsDevice.ClearBackbuffer(Color.CornflowerBlue, 1, 0);


			foreach ( var e in space.Entities ) {
				Box box = e as Box;
				if ( box != null ) // this won't create any graphics for an entity that isn't a box
				{
					if ( box.IsDynamic ) // draw only dynamic boxes
					{
						// fill world matrix
						Fusion.Mathematics.Matrix matrix = new Fusion.Mathematics.Matrix(box.WorldTransform.M11, box.WorldTransform.M12, box.WorldTransform.M13, box.WorldTransform.M14,
																									box.WorldTransform.M21, box.WorldTransform.M22, box.WorldTransform.M23, box.WorldTransform.M24,
																									box.WorldTransform.M31, box.WorldTransform.M32, box.WorldTransform.M33, box.WorldTransform.M34,
																									box.WorldTransform.M41, box.WorldTransform.M42, box.WorldTransform.M43, box.WorldTransform.M44);
						cbData.Projection = cam.GetProjectionMatrix(stereoEye);
						cbData.View = cam.GetViewMatrix(stereoEye);
						cbData.World = matrix;
						cbData.ViewPos = new Vector4Fusion(cam.GetCameraMatrix(stereoEye).TranslationVector, 1);
						Color c = (Color) box.Tag;
						cbData.Color =  c.ToVector4();

						constBuffer.SetData(cbData);
						
						GraphicsDevice.PipelineState = factory[0];

						GraphicsDevice.PixelShaderConstants[0] = constBuffer;
						GraphicsDevice.VertexShaderConstants[0] = constBuffer;
						GraphicsDevice.PixelShaderSamplers[0] = SamplerState.AnisotropicWrap;
						GraphicsDevice.PixelShaderResources[0] = texture;

						// setup data and draw box
						GraphicsDevice.SetupVertexInput(vb, ib);
						GraphicsDevice.DrawIndexed( 36, 0, 0);
					}
				}
			}

			base.Draw(gameTime, stereoEye);
		}
	}
}
