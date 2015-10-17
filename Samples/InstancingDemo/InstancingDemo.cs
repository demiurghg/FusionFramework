using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Development;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Engine.Common;
using Fusion.Build;
using System.Runtime.InteropServices;



namespace InstancingDemo2D {
	class InstancingDemo : Game {

		[StructLayout(LayoutKind.Explicit, Size=80)]
		struct ConstData {
			[FieldOffset(0)]	
			public Matrix Transform;
			[FieldOffset(64)]	
			public float  Time;
		}

		const int InstanceCount = 1000;


		struct InstData {
			public Vector2	Offset;
			public float	Rotation;
			public float	Scale;
			public Vector4	Color;
			public float	TexId;
		}

		Ubershader			us;
		VertexBuffer		vb;
		ConstantBuffer		cb;
		InstData[]			instDataCpu;
		StructuredBuffer	instDataGpu;
		Texture2D			tex;
		ConstData			cbData;
		StateFactory		factory;




		enum UberFlags {
			NONE,
		}


		struct Vertex {
			[Vertex("POSITION")]	public Vector3	Position;
			[Vertex("TEXCOORD", 0)]	public Vector2	TexCoord;
			[Vertex("COLOR")]		public Color	Color;
		}



		/// <summary>
		///	Add services and set options
		/// </summary>
		public InstancingDemo ()
		{
			//	enable object tracking :
			Parameters.TrackObjects		=	false;
			Parameters.VSyncInterval	=	1;
			Parameters.MsaaLevel		=	4;
			Parameters.StereoMode		=	StereoMode.Disabled;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );

			//	load configuration :
			LoadConfiguration();

			//	make configuration saved on exit
			Exiting += InstancingDemo_Exiting;
			
			
			//	handle keys
			InputDevice.KeyDown += InputDevice_KeyDown;
		}


	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, InputDevice.KeyEventArgs e )
		{
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
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InstancingDemo_Exiting ( object sender, EventArgs e )
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

			vb			=	new VertexBuffer(device,  typeof(Vertex), 6 );
			cb			=	new ConstantBuffer(GraphicsDevice, typeof(ConstData) );
			instDataGpu	=	new StructuredBuffer( device, typeof(InstData), InstanceCount, StructuredBufferFlags.None ); 
			instDataCpu	=	new InstData[ InstanceCount ];

			var rand = new Random();
			for (int i=0; i<InstanceCount; i++) {
				instDataCpu[ i ].Offset		=	rand.NextVector2( new Vector2(-2.5f,-2f), new Vector2( 2.5f,2f) );
				instDataCpu[ i ].Scale		=	rand.NextFloat( 0, 0.7f);
				instDataCpu[ i ].Rotation	=	rand.NextFloat( 0, MathUtil.TwoPi );
				instDataCpu[ i ].Color		=	rand.NextVector4( Vector4.Zero, Vector4.One * 0.7f );
				instDataCpu[ i ].TexId		=	rand.Next(4);
			}


			Reloading += InstancingDemo_Reloading;

			InstancingDemo_Reloading ( this, EventArgs.Empty );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InstancingDemo_Reloading ( object sender, EventArgs e )
		{
			SafeDispose( ref factory );
			us		=	Content.Load<Ubershader>("test");
			factory	=	new StateFactory( us, typeof(UberFlags), Primitive.TriangleList, VertexInputElement.FromStructure( typeof(Vertex) ), BlendState.Additive, RasterizerState.CullNone, DepthStencilState.None );
			tex		=	Content.Load<Texture2D>("block" );
		}



		/// <summary>
		/// Kill stuff here
		/// </summary>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {			
				SafeDispose( ref factory );
				SafeDispose( ref vb );
				SafeDispose( ref cb );
				SafeDispose( ref instDataGpu );
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
				instDataCpu[ i ].Scale		=	0.2f * (0.5f + 0.4f*(float)Math.Cos( 0.17*i + gameTime.Total.TotalSeconds * 2 ));
				instDataCpu[ i ].Rotation	+= (MathUtil.IsOdd(i) ? 0.01f : -0.01f);
				instDataCpu[ i ].Offset.Y	+= (MathUtil.IsOdd(i) ? 0.001f : -0.001f);
				
				if (instDataCpu[ i ].Offset.Y > 2) {
					instDataCpu[ i ].Offset.Y = -2;
				}
				if (instDataCpu[ i ].Offset.Y < -2) {
					instDataCpu[ i ].Offset.Y = 2;
				}
				/*float c = (1 - (instDataCpu[ i ].Offset.Y + 2)/4);
				c *= c * 0.5f;
				instDataCpu[ i ].Color = new Vector4(c,c,c,c);*/
			}

			instDataGpu.SetData( instDataCpu );
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
			var v0	=	new Vertex{ Position = new Vector3( -1.0f, -1.0f, 0 ), Color = Color.Red,   TexCoord = new Vector2(0,1) };
			var v1	=	new Vertex{ Position = new Vector3(  1.0f,  1.0f, 0 ), Color = Color.White, TexCoord = new Vector2(1,0) };
			var v2	=	new Vertex{ Position = new Vector3( -1.0f,  1.0f, 0 ), Color = Color.Blue,  TexCoord = new Vector2(0,0) };
			var v3	=	new Vertex{ Position = new Vector3( -1.0f, -1.0f, 0 ), Color = Color.Red,   TexCoord = new Vector2(0,1) };
			var v4	=	new Vertex{ Position = new Vector3(  1.0f, -1.0f, 0 ), Color = Color.Lime,  TexCoord = new Vector2(1,1) };
			var v5	=	new Vertex{ Position = new Vector3(  1.0f,  1.0f, 0 ), Color = Color.White, TexCoord = new Vector2(1,0) };//*/

			var data = new Vertex[]{ v0, v1, v2, v3, v4, v5 };

			vb.SetData( data, 0, 6 );

			//	Set required ubershader :

			//	Set device states :
			GraphicsDevice.PipelineState		= factory[0];
			GraphicsDevice.PixelShaderSamplers[0]	= SamplerState.LinearWrap ;

			//	Setup texture :
			GraphicsDevice.PixelShaderResources[0]	= tex ;
			GraphicsDevice.VertexShaderResources[1]	= instDataGpu ;
					
			//	Setup vertex data and draw :			
			GraphicsDevice.SetupVertexInput( vb, null );

			GraphicsDevice.DrawInstanced( 6, InstanceCount, 0, 0 );

			base.Draw( gameTime, stereoEye );
		}
	}
}
