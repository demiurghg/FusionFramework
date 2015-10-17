using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Input;
using Fusion.Core.Content;
using Fusion.Core.Development;
using Fusion.Engine.Common;

namespace ParticleDemo2 {
	public class ParticleSystemGS : GameService {


		Texture2D		texture;
		Ubershader		shader;
		StateFactory	factory;

		const int BlockSize				=	512;
		const int MaxInjectingParticles	=	1024;
		const int MaxSimulatedParticles =	1024 * 1024;

		int					injectionCount = 0;
		ParticleVertex[]	injectionBufferCPU = new ParticleVertex[MaxInjectingParticles];
		ConstantBuffer		paramsCB;

		VertexBuffer		injectionVB;
		VertexBuffer		simulationSrcVB;
		VertexBuffer		simulationDstVB;
		

		enum Flags {
			INJECTION	=	0x1,
			SIMULATION	=	0x2,
			RENDER		=	0x4,
		}

		[StructLayout(LayoutKind.Explicit, Size=144)]
		struct Params {
			[FieldOffset(  0)] public Matrix	View;
			[FieldOffset( 64)] public Matrix	Projection;
			[FieldOffset(128)] public int		MaxParticles;
			[FieldOffset(132)] public float		DeltaTime;
		} 

		Random rand = new Random();


		struct ParticleVertex {
			[Vertex("POSITION"	, 0)] public Vector4	Position;
			[Vertex("COLOR"		, 0)] public Color4		Color0;
			[Vertex("COLOR"		, 1)] public Color4		Color1;
			[Vertex("TEXCOORD"	, 0)] public Vector4	VelAccel;
			[Vertex("TEXCOORD"	, 1)] public Vector4	SizeAngle;	//	size0, size1, angle0, angle1
			[Vertex("TEXCOORD"	, 2)] public Vector4	Timing;		//	total, lifetime, fade-in, fade-out;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public ParticleSystemGS ( Game game ) : base (game)
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
			paramsCB			=	new ConstantBuffer( Game.GraphicsDevice, typeof(Params) );

			injectionVB		=	new VertexBuffer( Game.GraphicsDevice, typeof(ParticleVertex), MaxInjectingParticles );
			simulationSrcVB	=	new VertexBuffer( Game.GraphicsDevice, typeof(ParticleVertex), MaxSimulatedParticles, VertexBufferOptions.VertexOutput );
			simulationDstVB	=	new VertexBuffer( Game.GraphicsDevice, typeof(ParticleVertex), MaxSimulatedParticles, VertexBufferOptions.VertexOutput );

			base.Initialize();

			Game.Reloading += Game_Reloading;
			Game_Reloading( this, EventArgs.Empty );
		}



		void Game_Reloading ( object sender, EventArgs e )
		{
			SafeDispose( ref factory );

			texture		=	Game.Content.Load<Texture2D>("particle2");
			shader		=	Game.Content.Load<Ubershader>("test");
			factory		=	new StateFactory( shader, typeof(Flags), (ps,i) => EnumAction( ps, (Flags)i ) );
		}



		void EnumAction ( PipelineState ps, Flags flag )
		{
			ps.Primitive			=	Primitive.PointList;
			ps.VertexInputElements	=	VertexInputElement.FromStructure<ParticleVertex>();
			ps.DepthStencilState	=	DepthStencilState.None;

			var outputElements = new[]{
				new VertexOutputElement("SV_POSITION", 0, 0, 4),
				new VertexOutputElement("COLOR"		 , 0, 0, 4),
				new VertexOutputElement("COLOR"		 , 1, 0, 4),
				new VertexOutputElement("TEXCOORD"	 , 0, 0, 4),
				new VertexOutputElement("TEXCOORD"	 , 1, 0, 4),
				new VertexOutputElement("TEXCOORD"	 , 2, 0, 4),
			};


			if (flag==Flags.INJECTION || flag==Flags.SIMULATION) {
				ps.VertexOutputElements	=	outputElements;
			}

			if (flag==Flags.RENDER) {
				ps.BlendState	=	BlendState.Screen;
				ps.RasterizerState	=	RasterizerState.CullNone;
			}
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
				//injectionCount = 0;
				return;
			}

			//Log.LogMessage("...particle added");
			var v = vel + RadialRandomVector() * 5;
			var a = Vector2.UnitY * 10 - v * 0.2f;
			var r = rand.NextFloat( -MathUtil.Pi, MathUtil.Pi );
			var s = (rand.NextFloat(0,1)>0.5f) ? -1 : 1;

			var p = new ParticleVertex () {
				Position		=	new Vector4(pos,0,1),
				VelAccel		=	new Vector4(v.X, v.Y, a.X, a.Y ),
				Color0			=	Color4.Zero,
				Color1			=	rand.NextColor4() * colorBoost,
				SizeAngle		=	new Vector4( size0, size1, r, r + 2*s ),
				Timing			=	new Vector4( rand.NextFloat(lifeTime/7, lifeTime), 0, 0.01f, 0.01f ),
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
				injectionBufferCPU[i].Timing.X = -999999;
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

				injectionVB.Dispose();
				simulationSrcVB.Dispose();
				simulationDstVB.Dispose();
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

			//ds.Add( Color.Yellow, "Total particles DST: {0}", simulationBufferDst.GetStructureCount() );
			//ds.Add( Color.Yellow, "Total particles SRC: {0}", simulationBufferSrc.GetStructureCount() );
		}




		/// <summary>
		/// 
		/// </summary>
		void SwapParticleBuffers ()
		{
			var temp = simulationDstVB;
			simulationDstVB = simulationSrcVB;
			simulationSrcVB = temp;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			var ds		=	Game.GetService<DebugStrings>();
			ds.Add("{0}", injectionCount );

			var device	=	Game.GraphicsDevice;
			device.ResetStates();

			device.SetTargets( null, device.BackbufferColor );

			int	w	=	device.DisplayBounds.Width;
			int h	=	device.DisplayBounds.Height;

			//var map	=	"SV_POSITION.xyzw;COLOR0.xyzw;COLOR1.xyzw;TEXCOORD0.xyzw;TEXCOORD1.xyzw;TEXCOORD2.xyzw";

			Params param = new Params();
			param.View			=	Matrix.Identity;
			param.Projection	=	Matrix.OrthoOffCenterRH(0, w, h, 0, -9999, 9999);
			param.MaxParticles	=	100;
			param.DeltaTime		=	gameTime.ElapsedSec;

			paramsCB.SetData( param );



			device.VertexShaderConstants[0]		= paramsCB ;
			device.GeometryShaderConstants[0]	= paramsCB ;
			device.PixelShaderConstants[0]		= paramsCB ;
			
			device.PixelShaderSamplers[0]		= SamplerState.LinearWrap ;


			//
			//	Simulate :
			//
			device.PipelineState	=	factory[ (int)Flags.SIMULATION ];

			device.SetupVertexInput( simulationSrcVB, null );
			device.SetupVertexOutput( simulationDstVB, 0 );
		
			device.DrawAuto();

			//
			//	Inject :
			//
			injectionVB.SetData( injectionBufferCPU );

			device.PipelineState	=	factory[ (int)Flags.INJECTION ];

			device.SetupVertexInput( injectionVB, null );
			device.SetupVertexOutput( simulationDstVB, -1 );
		
			device.Draw(injectionCount, 0 );

			SwapParticleBuffers();	

			//
			//	Render
			//
			paramsCB.SetData( param );
			device.VertexShaderConstants[0]		= paramsCB ;
			device.GeometryShaderConstants[0]	= paramsCB ;
			device.PixelShaderConstants[0]		= paramsCB ;

			device.PipelineState	=	factory[ (int)Flags.RENDER ];

			device.PixelShaderResources[0]	=	texture ;

			device.SetupVertexOutput( null, 0 );
			device.SetupVertexInput( simulationSrcVB, null );

			//device.Draw( Primitive.PointList, injectionCount, 0 );

			device.DrawAuto();
			//device.Draw( Primitive.PointList, MaxSimulatedParticles, 0 );


			ClearParticleBuffer();

			base.Draw( gameTime, stereoEye );
		}

	}
}
