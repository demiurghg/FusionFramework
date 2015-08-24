using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using System.Runtime.InteropServices;

namespace ParticleDemo {
	public class ParticleSystem : GameService {


		Texture2D		texture;
		Ubershader		shader;
		StateFactory	factory;

		const int BlockSize				=	512;
		const int MaxInjectingParticles	=	1024;
		const int MaxSimulatedParticles =	1024 * 1024;

		int					injectionCount = 0;
		Particle[]			injectionBufferCPU = new Particle[MaxInjectingParticles];
		StructuredBuffer	injectionBuffer;
		StructuredBuffer	simulationBufferSrc;
		StructuredBuffer	simulationBufferDst;
		ConstantBuffer		paramsCB;

//       float2 Position;               // Offset:    0
//       float2 Velocity;               // Offset:    8
//       float2 Acceleration;           // Offset:   16
//       float4 Color0;                 // Offset:   24
//       float4 Color1;                 // Offset:   40
//       float Size0;                   // Offset:   56
//       float Size1;                   // Offset:   60
//       float Angle0;                  // Offset:   64
//       float Angle1;                  // Offset:   68
//       float TotalLifeTime;           // Offset:   72
//       float LifeTime;                // Offset:   76
//       float FadeIn;                  // Offset:   80
//       float FadeOut;                 // Offset:   84
		[StructLayout(LayoutKind.Explicit)]
		struct Particle {
			[FieldOffset( 0)] public Vector2	Position;
			[FieldOffset( 8)] public Vector2	Velocity;
			[FieldOffset(16)] public Vector2	Acceleration;
			[FieldOffset(24)] public Vector4	Color0;
			[FieldOffset(40)] public Vector4	Color1;
			[FieldOffset(56)] public float	Size0;
			[FieldOffset(60)] public float	Size1;
			[FieldOffset(64)] public float	Angle0;
			[FieldOffset(68)] public float	Angle1;
			[FieldOffset(72)] public float	TotalLifeTime;
			[FieldOffset(76)] public float	LifeTime;
			[FieldOffset(80)] public float	FadeIn;
			[FieldOffset(84)] public float	FadeOut;

			public override string ToString ()
			{
				return string.Format("life time = {0}/{1}", LifeTime, TotalLifeTime );
			}
		}

		enum Flags {
			INJECTION	=	0x1,
			SIMULATION	=	0x2,
			DRAW		=	0x4,
		}


//       row_major float4x4 View;       // Offset:    0
//       row_major float4x4 Projection; // Offset:   64
//       int MaxParticles;              // Offset:  128
//       float DeltaTime;               // Offset:  132

		[StructLayout(LayoutKind.Explicit, Size=144)]
		struct Params {
			[FieldOffset(  0)] public Matrix	View;
			[FieldOffset( 64)] public Matrix	Projection;
			[FieldOffset(128)] public int		MaxParticles;
			[FieldOffset(132)] public float		DeltaTime;
		} 

		Random rand = new Random();


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public ParticleSystem ( Game game ) : base (game)
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{

			paramsCB			=	new ConstantBuffer( Game.GraphicsDevice, typeof(Params) );

			injectionBuffer		=	new StructuredBuffer( Game.GraphicsDevice, typeof(Particle), MaxInjectingParticles, StructuredBufferFlags.None );
			simulationBufferSrc	=	new StructuredBuffer( Game.GraphicsDevice, typeof(Particle), MaxSimulatedParticles, StructuredBufferFlags.Append );
			simulationBufferDst	=	new StructuredBuffer( Game.GraphicsDevice, typeof(Particle), MaxSimulatedParticles, StructuredBufferFlags.Append );

			base.Initialize();
			Game.Reloading += Game_Reloading;
			Game_Reloading(this, EventArgs.Empty);
			
		}

		void Game_Reloading ( object sender, EventArgs e )
		{
			SafeDispose( ref factory );

			texture		=	Game.Content.Load<Texture2D>("particle");
			shader		=	Game.Content.Load<Ubershader>("test");


			factory		=	new StateFactory( shader, typeof(Flags), (ps,i) => StateEnum( ps, (Flags)i) );
			
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="flags"></param>
		void StateEnum ( PipelineState ps, Flags flags )
		{
			ps.BlendState			=	BlendState.Additive;
			ps.DepthStencilState	=	DepthStencilState.None;
			ps.Primitive			=	Primitive.PointList;
		}



		/// <summary>
		/// Returns random radial vector
		/// </summary>
		/// <returns></returns>
		Vector2 RadialRandomVector ()
		{
			Vector2 r;
			do {
				r	=	rand.NextVector2( -Vector2.One, Vector2.One );
			} while ( r.Length() > 1 );

			//r.Normalize();

			return r;
		}



		/// <summary>
		/// Adds random particle at specified position
		/// </summary>
		/// <param name="p"></param>
		public void AddParticle ( Vector2 pos, Vector2 vel, float lifeTime, float size0, float size1, float colorBoost = 1 )
		{
			if (injectionCount>=MaxInjectingParticles) {
				Log.Warning("Too much injected particles per frame");
				return;
			}

			//Log.LogMessage("...particle added");
			var v = vel + RadialRandomVector() * 5;
			var a = rand.NextFloat( -MathUtil.Pi, MathUtil.Pi );
			var s = (rand.NextFloat(0,1)>0.5f) ? -1 : 1;

			var p = new Particle () {
				Position		=	pos,
				Velocity		=	vel + RadialRandomVector() * 5,
				Acceleration	=	Vector2.UnitY * 10 - v * 0.2f,
				Color0			=	Vector4.Zero,
				//Color1			=	rand.NextVector4( Vector4.Zero, Vector4.One ) * colorBoost,
				Color1			=	Vector4.One * colorBoost,//rand.NextVector4( Vector4.Zero, Vector4.One ),
				Size0			=	size0,
				Size1			=	size1,
				Angle0			=	a,
				Angle1			=	a + 2 * s,
				TotalLifeTime	=	rand.NextFloat(lifeTime/2, lifeTime),
				LifeTime		=	0,
				FadeIn			=	0.01f,
				FadeOut			=	0.01f,
			};

			injectionBufferCPU[ injectionCount ] = p;
			injectionCount ++;
		}



		/// <summary>
		/// Makes all particles wittingly dead
		/// </summary>
		void ClearParticleBuffer ()
		{
			for (int i=0; i<MaxInjectingParticles; i++) {
				injectionBufferCPU[i].TotalLifeTime = -999999;
			}
			injectionCount = 0;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );

				paramsCB.Dispose();

				injectionBuffer.Dispose();
				simulationBufferSrc.Dispose();
				simulationBufferDst.Dispose();
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			base.Update( gameTime );

			var ds = Game.GetService<DebugStrings>();

			ds.Add( Color.Yellow, "Total particles DST: {0}", simulationBufferDst.GetStructureCount() );
			ds.Add( Color.Yellow, "Total particles SRC: {0}", simulationBufferSrc.GetStructureCount() );
		}




		/// <summary>
		/// 
		/// </summary>
		void SwapParticleBuffers ()
		{
			var temp = simulationBufferDst;
			simulationBufferDst = simulationBufferSrc;
			simulationBufferSrc = temp;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public override void Draw ( GameTime gameTime, Fusion.Graphics.StereoEye stereoEye )
		{
			var device	=	Game.GraphicsDevice;

			int	w	=	device.DisplayBounds.Width;
			int h	=	device.DisplayBounds.Height;

			Params param = new Params();
			param.View			=	Matrix.Identity;
			param.Projection	=	Matrix.OrthoOffCenterRH(0, w, h, 0, -9999, 9999);
			param.MaxParticles	=	0;
			param.DeltaTime		=	gameTime.ElapsedSec;


			device.ComputeShaderConstants[0]	= paramsCB ;
			device.VertexShaderConstants[0]	= paramsCB ;
			device.GeometryShaderConstants[0]	= paramsCB ;
			device.PixelShaderConstants[0]	= paramsCB ;
			
			device.PixelShaderSamplers[0]	= SamplerState.LinearWrap ;


			//
			//	Inject :
			//
			injectionBuffer.SetData( injectionBufferCPU );
			device.Clear( simulationBufferDst, Int4.Zero );

			device.ComputeShaderResources[1]	= injectionBuffer ;
			device.SetCSRWBuffer( 0, simulationBufferDst, 0 );

			param.MaxParticles	=	injectionCount;
			paramsCB.SetData( param );
			//device.CSConstantBuffers[0] = paramsCB ;

			device.PipelineState	=	factory[ (int)Flags.INJECTION ];
			device.Dispatch( MathUtil.IntDivUp( MaxInjectingParticles, BlockSize ) );

			ClearParticleBuffer();

			//
			//	Simulate :
			//
			device.ComputeShaderResources[1]	= simulationBufferSrc ;

			param.MaxParticles	=	MaxSimulatedParticles;
			paramsCB.SetData( param );
			device.ComputeShaderConstants[0] = paramsCB ;

			device.PipelineState	=	factory[ (int)Flags.SIMULATION ];
			device.Dispatch( MathUtil.IntDivUp( MaxSimulatedParticles, BlockSize ) );//*/

			SwapParticleBuffers();


			//
			//	Render
			//
			device.PipelineState	=	factory[ (int)Flags.DRAW ];
			device.SetCSRWBuffer( 0, null );	
			device.PixelShaderResources[0]	=	texture ;
			device.GeometryShaderResources[1]	=	simulationBufferSrc ;

			device.Draw( MaxSimulatedParticles, 0 );


			/*var testSrc = new Particle[MaxSimulatedParticles];
			var testDst = new Particle[MaxSimulatedParticles];

			simulationBufferSrc.GetData( testSrc );
			simulationBufferDst.GetData( testDst );*/

			base.Draw( gameTime, stereoEye );
		}

	}
}
