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

namespace ShooterDemo2
{
    public class PhysicsDemo : Game
    {
        /// <summary>
        /// ShooterDemo2 constructor
        /// </summary>
        public PhysicsDemo()
            : base()
        {
            //	enable object tracking :
            Parameters.TrackObjects = false;
            Parameters.VSyncInterval = 0;
            Parameters.MsaaLevel = 4;

            //Parameters.StereoMode	=	StereoMode.NV3DVision;

            //	add services :
            AddService(new SpriteBatch(this), false, false, 0, 0);
            AddService(new DebugStrings(this), true, true, 9999, 9999);
            AddService(new DebugRender(this), true, true, 9998, 9998);
            AddService(new Camera(this), true, false, 1, 1);

            //	load configuration :
            LoadConfiguration();

            //	make configuration saved on exit
            Exiting += FusionGame_Exiting;
            InputDevice.KeyDown += InputDevice_KeyDown;

        }


        Scene scene;
        Space space;
        Box[] boxes;
        ConstantBuffer constBuffer;
        Ubershader uberShader;
		bool flag = false;
		ConstantBuffer	constCurrentParticleBuffer;
		Scene			sceneCube;
		Texture2D		texture;
		Random random = new Random();


        struct CBData
        {
            public Fusion.Mathematics.Matrix Projection;
            public Fusion.Mathematics.Matrix View;
            public Fusion.Mathematics.Matrix World;
            public Fusion.Mathematics.Vector4 ViewPos;
        }


        enum RenderFlags
        {
            None,
        }


		struct CubeVertex
		{
			[Vertex("POSITION")]
			public Fusion.Mathematics.Vector3 Position;
			[Vertex("NORMAL")]
			public Fusion.Mathematics.Vector3 Normal;
			[Vertex("COLOR")]
			public Fusion.Mathematics.Vector4 Color;
			[Vertex("TEXCOORD")]
			public Fusion.Mathematics.Vector2 TexCoord;
		}

		VertexBuffer vb;
		VertexInputLayout vil;
		IndexBuffer ib;

        /// <summary>
        /// Add services :
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            constBuffer = new ConstantBuffer(GraphicsDevice, typeof(CBData));

            LoadContent();
            Reloading += (s, e) => LoadContent();
			            
            GetService<Camera>().FreeCamPosition = Fusion.Mathematics.Vector3.Up * 10;
        }



        /// <summary>
        /// Load content
        /// </summary>
        public void LoadContent()
        {
			scene = Content.Load<Scene>(@"Scenes\testScene");
			sceneCube = Content.Load<Scene>(@"Scenes\cube");

            foreach (var mtrl in scene.Materials)
            {
                mtrl.Tag = Content.Load<Texture2D>(mtrl.TexturePath);
            }

			scene.Bake<VertexColorTextureNormal>(GraphicsDevice, VertexColorTextureNormal.Bake);
			sceneCube.Bake<VertexColorTextureNormal>(GraphicsDevice, VertexColorTextureNormal.Bake);

            uberShader = Content.Load<Ubershader>("render");
            uberShader.Map(typeof(RenderFlags));
			texture = Content.Load<Texture2D>(@"Scenes\lena");

			vb = new VertexBuffer(GraphicsDevice, typeof(CubeVertex), 24);
			vil = new VertexInputLayout(GraphicsDevice, typeof(CubeVertex));
			ib = new IndexBuffer(GraphicsDevice, 36);
                        
            space = new Space();

            //space.ForceUpdater.Gravity = new Vector3BEPU(0, -2.5f, 0);
            space.ForceUpdater.Gravity = new Vector3BEPU(0, -9.81f, 0);

            Box ground = new Box(new Vector3BEPU(0, 0, 0), 50, 1, 50);
			space.Add( ground );

            boxes = new Box[20];
			for (int i = 0; i < 20; i++)
			{
				Fusion.Mathematics.Vector3 vector = RandomExt.NextVector3(random, new Fusion.Mathematics.Vector3(-10, 20, -10), new Fusion.Mathematics.Vector3(10, 30, 10));
				boxes[i] = new Box(new Vector3BEPU(vector.X, vector.Y, vector.Z), 1, 1, 1, 1);
			}
			//boxes[0] = new Box(new Vector3BEPU(0, 20, 0), 1, 1, 1, 1);
			//boxes[1] = new Box(new Vector3BEPU(0.5f, 30, 0), 1, 1, 1, 1);
			//boxes[2] = new Box(new Vector3BEPU(-0.3f, 40, 0), 1, 1, 1, 1);

			foreach (var box in boxes)
			{
				box.Tag = RandomExt.NextColor(random);
				space.Add(box);
			}

			//space.Add(boxes[0]);
			//space.Add(boxes[1]);
			//space.Add(boxes[2]);         

            Log.Message("{0}", sceneCube.Nodes.Count(n => n.MeshIndex >= 0));
		}


       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        { 
            if (disposing)
            {
                if (constBuffer != null)
                {
                    constBuffer.Dispose();
                }
            }
			vb.Dispose();
			ib.Dispose();
			vil.Dispose();
            base.Dispose(disposing);
        }



        /// <summary>
        /// Handle keys for each demo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InputDevice_KeyDown(object sender, Fusion.Input.InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.F1)
            {
                DevCon.Show(this);
            }

            if (e.Key == Keys.F2)
            {
                Parameters.ToggleVSync();
            }

            if (e.Key == Keys.F5)
            {
                Reload();
            }

            if (e.Key == Keys.F12)
            {
                GraphicsDevice.Screenshot();
            }

            if (e.Key == Keys.Escape)
            {
                Exit();
            }

			if (e.Key == Keys.P)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					flag = true;
				}
			}
			if (e.Key == Keys.LeftButton)
			{
				Fusion.Mathematics.Vector3 vector = GetService<Camera>().FreeCamPosition;
				Box box = new Box(new Vector3BEPU(vector.X, vector.Y, vector.Z), 1, 1, 1, 1);
				Fusion.Mathematics.Vector3 velocity = 10 * GetService<Camera>().GetCameraMatrix(StereoEye.Mono).Forward;
				box.LinearVelocity = new Vector3BEPU(velocity.X, velocity.Y, velocity.Z);
				//box.AngularVelocity = new Vector3BEPU(vector.X, vector.Y, vector.Z);
				box.Tag = RandomExt.NextColor(random);
				space.Add(box);	
			}
			if (e.Key == Keys.O)
			{
				Fusion.Mathematics.Vector3 vector = RandomExt.NextVector3(random, new Fusion.Mathematics.Vector3(-10, 20, -10), new Fusion.Mathematics.Vector3(10, 30, 10));
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
        void FusionGame_Exiting(object sender, EventArgs e)
        {
            SaveConfiguration();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
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

			if (flag)
			{
				space.Update(gameTime.ElapsedSec);				
			}
			foreach (var e in space.Entities)
			{
				Box box = e as Box;
				if (box != null) //This won't create any graphics for an entity that isn't a box since the model being used is a box.
				{
					if (box.IsDynamic)
					{

						Fusion.Mathematics.Vector3 vector = new Fusion.Mathematics.Vector3(box.Position.X - box.HalfLength, box.Position.Y - box.HalfHeight, box.Position.Z - box.HalfWidth);
						Fusion.Mathematics.Matrix matrix = new Fusion.Mathematics.Matrix(box.WorldTransform.M11, box.WorldTransform.M12, box.WorldTransform.M13, box.WorldTransform.M14,
																									box.WorldTransform.M21, box.WorldTransform.M22, box.WorldTransform.M23, box.WorldTransform.M24,
																									box.WorldTransform.M31, box.WorldTransform.M32, box.WorldTransform.M33, box.WorldTransform.M34,
																									box.WorldTransform.M41, box.WorldTransform.M42, box.WorldTransform.M43, box.WorldTransform.M44);
						//dr.DrawBox(new Fusion.Mathematics.BoundingBox(vector, vector + new Fusion.Mathematics.Vector3(box.Length, box.Height, box.Width)), matrix, Color.White);
						//dr.DrawBox(new Fusion.Mathematics.BoundingBox(new Fusion.Mathematics.Vector3(-0.5f, -0.5f, -0.5f), new Fusion.Mathematics.Vector3(0.5f, 0.5f, 0.5f)), matrix, (Color) box.Tag);
						//Console.WriteLine(vector.X + ", " + vector.Y + ", " + vector.Z);
					}
					else
					{
						//Console.WriteLine( box.Position.X + ", " + box.Position.Y + ", " + box.Position.Z );
						//Fusion.Mathematics.Vector3 vector = new Fusion.Mathematics.Vector3( box.Position.X, box.Position.Y, box.Position.Z );
						//dr.DrawBox(new Fusion.Mathematics.BoundingBox(vector, vector + new Fusion.Mathematics.Vector3(box.Length, box.Height, box.Width)), Color.Red);
					}
				}

			}
            
            base.Update(gameTime);
        }

		
	

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="stereoEye"></param>
        protected override void Draw(GameTime gameTime, StereoEye stereoEye)
        {
            CBData cbData = new CBData();			

            var cam = GetService<Camera>();


  //          GraphicsDevice.ClearBackbuffer(Color.Black, 1, 0);
            GraphicsDevice.ClearBackbuffer(Color.CornflowerBlue, 1, 0);




			//	Fill vertex buffer :
			//back
			var v0 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, -0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(-1.0f, 0, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero};
			var v1 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, -0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(-1.0f, 0, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v2 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, 0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(-1.0f, 0, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v3 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, 0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(-1.0f, 0, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };

			//front
			var v4 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, -0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(1.0f, 0, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v5 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, -0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(1.0f, 0, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v6 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, 0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(1.0f, 0, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v7 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, 0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(1.0f, 0, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };

			//left
			var v8 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, -0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(0, 0, -1.0f), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v9 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, -0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(0, 0, -1.0f), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v10 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, 0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(0, 0, -1.0f), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v11 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, 0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(0, 0, -1.0f), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };

			//right
			var v12 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, -0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(0, 0, 1.0f), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v13 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, -0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(0, 0, 1.0f), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v14 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, 0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(0, 0, 1.0f), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v15 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, 0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(0, 0, 1.0f), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };

			//top
			var v16 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, 0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(0, 1.0f, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v17 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, 0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(0, 1.0f, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v18 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, 0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(0, 1.0f, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v19 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, 0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(0, 1.0f, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };

			//bottom
			var v20 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, -0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(0, -1.0f, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v21 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(0.5f, -0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(0, -1.0f, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v22 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, -0.5f, 0.5f), Normal = new Fusion.Mathematics.Vector3(0, -1.0f, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };
			var v23 = new CubeVertex { Position = new Fusion.Mathematics.Vector3(-0.5f, -0.5f, -0.5f), Normal = new Fusion.Mathematics.Vector3(0, -1.0f, 0), Color = new Fusion.Mathematics.Vector4(Fusion.Mathematics.Vector3.Zero, 1), TexCoord = Fusion.Mathematics.Vector2.Zero };


			var data = new CubeVertex[] { v0, v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17, v18, v19, v20, v21, v22, v23 };

			vb.SetData(data, 0, 24);

			var index = new int[] { 3, 1, 0,
									3, 2, 1,
									4, 5, 7,
									7, 5, 6,
									8, 9, 11,
									11, 9, 10,
									14, 13, 12,
									14, 12, 15,
									19, 16, 17,
									19, 17, 18,
									20, 23, 22,
									20, 22, 21};

			ib.SetData(index);

			uberShader.SetPixelShader(0);
			uberShader.SetVertexShader(0);


			//GraphicsDevice.PixelShaderResources[0] = texture;
			GraphicsDevice.RasterizerState = RasterizerState.CullCW;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.PixelShaderConstants[0] = constBuffer;
			GraphicsDevice.VertexShaderConstants[0] = constBuffer;
			GraphicsDevice.PixelShaderSamplers[0] = SamplerState.AnisotropicWrap;
			GraphicsDevice.SetupVertexInput(vil, vb, ib);

			GraphicsDevice.DrawIndexed(Primitive.TriangleList, 36, 0, 0);

			//foreach (var e in space.Entities)
			//{
			//	Box box = e as Box;
			//	if (box != null) //This won't create any graphics for an entity that isn't a box since the model being used is a box.
			//	{
			//		if (box.IsDynamic)
			//		{

			//			var worldMatricies = new Fusion.Mathematics.Matrix[sceneCube.Nodes.Count];
			//			sceneCube.CopyAbsoluteTransformsTo(worldMatricies);
			//			Fusion.Mathematics.Matrix matrix = new Fusion.Mathematics.Matrix(box.WorldTransform.M11, box.WorldTransform.M12, box.WorldTransform.M13, box.WorldTransform.M14,
			//																						box.WorldTransform.M21, box.WorldTransform.M22, box.WorldTransform.M23, box.WorldTransform.M24,
			//																						box.WorldTransform.M31, box.WorldTransform.M32, box.WorldTransform.M33, box.WorldTransform.M34,
			//																						box.WorldTransform.M41, box.WorldTransform.M42, box.WorldTransform.M43, box.WorldTransform.M44);
			//			cbData.Projection = cam.GetProjectionMatrix(stereoEye);
			//			cbData.View = cam.GetViewMatrix(stereoEye);
			//			cbData.World = worldMatricies[0] * matrix;
			//			//								cbData.World = Fusion.Mathematics.Matrix.RotationYawPitchRoll(j * 0.01f, j * 0.02f, j * 0.03f) * worldMatricies[i] * Fusion.Mathematics.Matrix.Scaling((float)Math.Pow(0.9, j));
			//			cbData.ViewPos = new Fusion.Mathematics.Vector4(cam.GetCameraMatrix(stereoEye).TranslationVector, 1);

			//			constBuffer.SetData(cbData);

			//			GraphicsDevice.RasterizerState = RasterizerState.CullCW;
			//			GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			//			GraphicsDevice.BlendState = BlendState.Opaque;
			//			GraphicsDevice.PixelShaderConstants[0] = constBuffer;
			//			GraphicsDevice.VertexShaderConstants[0] = constBuffer;
			//			GraphicsDevice.PixelShaderSamplers[0] = SamplerState.AnisotropicWrap;

			//			GraphicsDevice.SetupVertexInput(vil, vb, ib);
						//GraphicsDevice.DrawIndexed(Primitive.TriangleList, 36, 0, 0);

						//for (int j = 0; j < 1; j++)
						//{
						//	for (int i = 0; i < sceneCube.Nodes.Count; i++)
						//	{

						//		var node = sceneCube.Nodes[i];

						//		if (node.MeshIndex == -1)
						//		{
						//			continue;
						//		}

						//		var mesh = sceneCube.Meshes[node.MeshIndex];
						//		cbData.Projection = cam.GetProjectionMatrix(stereoEye);
						//		cbData.View = cam.GetViewMatrix(stereoEye);
						//		cbData.World = worldMatricies[0] * matrix;
						//		//								cbData.World = Fusion.Mathematics.Matrix.RotationYawPitchRoll(j * 0.01f, j * 0.02f, j * 0.03f) * worldMatricies[i] * Fusion.Mathematics.Matrix.Scaling((float)Math.Pow(0.9, j));
						//		cbData.ViewPos = new Fusion.Mathematics.Vector4(cam.GetCameraMatrix(stereoEye).TranslationVector, 1);

						//		constBuffer.SetData(cbData);

						//		GraphicsDevice.RasterizerState = RasterizerState.CullCW;
						//		GraphicsDevice.DepthStencilState = DepthStencilState.Default;
						//		GraphicsDevice.BlendState = BlendState.Opaque;
						//		GraphicsDevice.PixelShaderConstants[0] = constBuffer;
						//		GraphicsDevice.VertexShaderConstants[0] = constBuffer;
						//		GraphicsDevice.PixelShaderSamplers[0] = SamplerState.AnisotropicWrap;

						//		//Console.WriteLine(mesh.VertexCount);

						//		//for (int k = 0; k < mesh.VertexCount; k++)
						//		//{
						//		//	var vertex = mesh.Vertices.ElementAt(k);
						//		//	vertex.Position = new Fusion.Mathematics.Vector3();
						//		//}

						//		////	Setup vertex data and draw :
						//		//GraphicsDevice.SetupVertexInput(vil, vb, ib);
						//		//GraphicsDevice.DrawInstanced(Primitive.TriangleList, 6, 1, 0, 0);

						//		mesh.SetupVertexInput();

						//		foreach (var subset in mesh.Subsets)
						//		{
						//			//lock (Game
						//			GraphicsDevice.PixelShaderResources[0] = sceneCube.Materials[subset.MaterialIndex].Tag as Texture2D;
						//			mesh.Draw(subset.StartPrimitive, subset.PrimitiveCount);
						//		}
						//	}
						//}
			//		}
			//	}
			//}

			


            base.Draw(gameTime, stereoEye);
        }
    }
}
