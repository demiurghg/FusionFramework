using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Development;
using Fusion.Graphics;
using Fusion.Input;
using System.Runtime.InteropServices;


#pragma warning disable 108, 649

namespace InstancingDemo2 {
	class InstancingDemo2 : Game {

		[StructLayout(LayoutKind.Explicit, Size=80)]
		struct ConstData {
			[FieldOffset(0)]	
			public Matrix Transform;
			[FieldOffset(64)]	
			public float  Time;
		}

		const int InstanceCount = 1000;

		Ubershader			us;
		VertexBuffer		vb;
		VertexBuffer		vbi;
		ConstantBuffer		cb;
		Texture2D			tex;
		ConstData			cbData;
		StateFactory		factory;

		Instance[]			instDataCpu;



		enum UberFlags {
			NONE,
		}


		struct Vertex {
			[Vertex("POSITION", 0, 0, 0)]	public Vector3	Position;
			[Vertex("TEXCOORD", 0, 0, 0)]	public Vector2	TexCoord;
		}

		struct Instance {
			[Vertex("TEXCOORD", 1, 1, 1)]	public Vector4	InstanceTransform;
			[Vertex("COLOR"   , 0, 1, 1)]	public Vector4	InstanceColor;
			[Vertex("TEXCOORD", 2, 1, 1)]	public float	InstanceTextureId;
		}


		struct VertexInstance {
			[Vertex("POSITION", 0, 0, 0)]	public Vector3	Position;
			[Vertex("TEXCOORD", 0, 0, 0)]	public Vector2	TexCoord;
			[Vertex("TEXCOORD", 1, 1, 1)]	public Vector4	InstanceTransform;
			[Vertex("COLOR"   , 0, 1, 1)]	public Vector4	InstanceColor;
			[Vertex("TEXCOORD", 2, 1, 1)]	public float	InstanceTextureId;
		}


		/// <summary>
		///	Add services and set options
		/// </summary>
		public InstancingDemo2 ()
		{
			//	enable object tracking :
			Parameters.TrackObjects		=	false;
			Parameters.VSyncInterval	=	1;
			Parameters.MsaaLevel		=	4;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );

			//	load configuration :
			LoadConfiguration();

			//	make configuration saved on exit
			Exiting += FusionGame_Exiting;
			
			//	handle keys
			InputDevice.KeyDown += InputDevice_KeyDown;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InstancingDemo2_Reloading ( object sender, EventArgs e )
		{
			SafeDispose( ref factory );

			us			=	Content.Load<Ubershader>("test");
			factory		=	new StateFactory( us, typeof(UberFlags), Primitive.TriangleList, VertexInputElement.FromStructure<VertexInstance>(), BlendState.Additive, RasterizerState.CullNone, DepthStencilState.None );
			tex			=	Content.Load<Texture2D>("block" );
		}



		/// <summary>
		/// Handle keys for each demo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Fusion.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
				//DevCon.Show(this);
			}

			if (e.Key == Keys.F2) {
				Parameters.VSyncInterval = (Parameters.VSyncInterval == 0) ? 1 : 0;
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
		/// Save configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void FusionGame_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}



		/// <summary>
		/// Load stuff here
		/// </summary>
		protected override void Initialize ()
		{
			var device	=	GraphicsDevice;

			base.Initialize();

			vb			=	new VertexBuffer(device,  typeof(Vertex),   6 );
			vbi			=	new VertexBuffer(device,  typeof(Instance), InstanceCount );
			cb			=	new ConstantBuffer(GraphicsDevice, typeof(ConstData) );

			instDataCpu	=	new Instance[ InstanceCount ];

			var rand = new Random();
			for (int i=0; i<InstanceCount; i++) {
				
				var offset		=	rand.NextVector2( new Vector2(-2.5f,-2f), new Vector2( 2.5f,2f) );
				var scale		=	rand.NextFloat( 0, 0.7f);
				var rotation	=	rand.NextFloat( 0, MathUtil.TwoPi );

				instDataCpu[ i ].InstanceTransform		=	new Vector4( offset, scale, rotation );
				instDataCpu[ i ].InstanceColor			=	rand.NextVector4( Vector4.Zero, Vector4.One * 0.7f );
				instDataCpu[ i ].InstanceTextureId		=	rand.Next(4);
			}

			//	handle hot reload :
			Reloading += InstancingDemo2_Reloading;
			InstancingDemo2_Reloading( this, EventArgs.Empty );
		}



		/// <summary>
		/// Kill stuff here
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {			
				SafeDispose( ref factory );
				SafeDispose( ref us );
				SafeDispose( ref vb );
				SafeDispose( ref vbi );
				SafeDispose( ref cb );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Update stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds = GetService<DebugStrings>();

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );
			ds.Add("");

			base.Update( gameTime );

			var rand = new Random();
			for (int i=0; i<InstanceCount; i++) {


				instDataCpu[ i ].InstanceTransform.Z	=	0.2f * (0.5f + 0.4f*(float)Math.Cos( 0.17*i + gameTime.Total.TotalSeconds * 2 ));
				instDataCpu[ i ].InstanceTransform.W	+=	(MathUtil.IsOdd(i) ? 0.01f : -0.01f);
				instDataCpu[ i ].InstanceTransform.Y	+= (MathUtil.IsOdd(i) ? 0.001f : -0.001f);
				
				if (instDataCpu[ i ].InstanceTransform.Y > 2) {
					instDataCpu[ i ].InstanceTransform.Y = -2;
				}
				if (instDataCpu[ i ].InstanceTransform.Y < -2) {
					instDataCpu[ i ].InstanceTransform.Y = 2;
				}
			}

			vbi.SetData( instDataCpu );
		}



		/// <summary>
		/// Draw stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			//	Clear back buffer :
			GraphicsDevice.ClearBackbuffer( new Color4(0,0,0,1) );

			//	Update constant buffer and bound it to pipeline:
			cbData.Transform	=	Matrix.OrthoRH( 4, 3, -2, 2 );
			cbData.Time = 0.001f * (float)gameTime.Total.TotalMilliseconds;
			cb.SetData( cbData );

			GraphicsDevice.VertexShaderConstants[0]	= cb ;
			GraphicsDevice.PixelShaderConstants[0]	= cb ;

			//	Fill vertex buffer :
			var v0	=	new Vertex{ Position = new Vector3( -1.0f, -1.0f, 0 ), TexCoord = new Vector2(0,1) };
			var v1	=	new Vertex{ Position = new Vector3(  1.0f,  1.0f, 0 ), TexCoord = new Vector2(1,0) };
			var v2	=	new Vertex{ Position = new Vector3( -1.0f,  1.0f, 0 ), TexCoord = new Vector2(0,0) };
			var v3	=	new Vertex{ Position = new Vector3( -1.0f, -1.0f, 0 ), TexCoord = new Vector2(0,1) };
			var v4	=	new Vertex{ Position = new Vector3(  1.0f, -1.0f, 0 ), TexCoord = new Vector2(1,1) };
			var v5	=	new Vertex{ Position = new Vector3(  1.0f,  1.0f, 0 ), TexCoord = new Vector2(1,0) };//*/

			var data = new Vertex[]{ v0, v1, v2, v3, v4, v5 };

			vb.SetData( data, 0, 6 );

			//	Set required ubershader :
			GraphicsDevice.PipelineState		= factory[0];
			GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearWrap ;

			//	Setup texture :
			GraphicsDevice.PixelShaderResources[0]	= tex ;
					
			//	Setup vertex data and draw :			
			GraphicsDevice.SetupVertexInput( new[]{ vb, vbi }, new[]{0,0}, null );

			GraphicsDevice.DrawInstanced( 6, InstanceCount, 0, 0 );

			base.Draw( gameTime, stereoEye );
		}
	}
}
