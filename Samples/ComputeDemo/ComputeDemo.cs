using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Development;

namespace ComputeDemo {
	public class ComputeDemo : Game {
		/// <summary>
		/// ComputeDemo constructor
		/// </summary>
		public ComputeDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );

			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += FusionGame_Exiting;
		}

		#pragma warning disable 649 

		struct Result {
			public float	Sum;
			public float	Mul;
			public int		GroupID;
			public int		GroupThreadID;
			public int		DispatchThreadID;
			public int		GroupIndex;
			public override string ToString ()
			{
				return string.Format("{0,10:0.00} {1,10:0.00} {2,5} {3,5} {4,5} {5,5}", Sum, Mul, GroupID, GroupThreadID, DispatchThreadID, GroupIndex );
			}
		}

		struct Params {
			public int Size;
			public int Dummy0;
			public int Dummy1;
			public int Dummy2;
		}


		enum ShaderFlags {
			NONE = 0,
		}

		const int BufferSize =	1000;

		ConstantBuffer		paramsCB;
		StructuredBuffer	argA;
		StructuredBuffer	argB;
		StructuredBuffer	result;
		Ubershader			shader;
		StateFactory		factory;


		/// <summary>
		/// Add services :
		/// </summary>
		protected override void Initialize ()
		{
			
			//	initialize services :
			base.Initialize();

			//	create structured buffers and shaders :
			argA		=	new StructuredBuffer( GraphicsDevice, typeof(float), BufferSize  , StructuredBufferFlags.None );
			argB		=	new StructuredBuffer( GraphicsDevice, typeof(float), BufferSize  , StructuredBufferFlags.None );
			result		=	new StructuredBuffer( GraphicsDevice, typeof(Result), BufferSize , StructuredBufferFlags.None );
			paramsCB	=	new ConstantBuffer( GraphicsDevice, typeof(Params) );
			shader		=	Content.Load<Ubershader>("test");
			factory		=	new StateFactory( shader, typeof(ShaderFlags), Primitive.TriangleList, VertexInputElement.Empty );

			//	write data :
			var	rand	=	new Random();

			var	a		=	Enumerable.Range(0, BufferSize).Select( i => rand.NextFloat(-1000,1000) ).ToArray();
			var	b		=	Enumerable.Range(0, BufferSize).Select( i => rand.NextFloat(-1000,1000) ).ToArray();
			var r		=	Enumerable.Range(0, BufferSize).Select( i => new Result() ).ToArray();

			argA.SetData( a );
			argB.SetData( b );

			paramsCB.SetData( new Params(){ Size = BufferSize } );
			
			//	bind objects :
			GraphicsDevice.SetCSRWBuffer( 0, argA );
			GraphicsDevice.SetCSRWBuffer( 1, argB );
			GraphicsDevice.SetCSRWBuffer( 2, result );
			GraphicsDevice.ComputeShaderConstants[0]	= paramsCB ;
		
			//	set compute shader and dispatch threadblocks :
			GraphicsDevice.PipelineState	=	factory[0];

			GraphicsDevice.Dispatch( MathUtil.IntDivUp(BufferSize,256) );

			//	get data :
			result.GetData( r );

			Log.Message("    id :        Sum    Product   gID  gtID  dtID  gIdx");

			for (int i=0; i<BufferSize; i++) {
				Log.Message("[{0,4}] : {1}", i, r[i] );
			}
			
			
			//	add keyboard handler :
			InputDevice.KeyDown += InputDevice_KeyDown;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref argA );
				SafeDispose( ref argB );
				SafeDispose( ref result );
				SafeDispose( ref paramsCB );
			}
			base.Dispose( disposing );
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
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );

			base.Update( gameTime );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			base.Draw( gameTime, stereoEye );
		}
	}
}
