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
using System.Runtime.InteropServices;

namespace DeferredDemo {
	public class SsaoFilter : GameService {

		[Config]
		public SsaoFilterConfig	Config { get; set; }


		public ShaderResource	OcclusionMap { 
			get {
				return occlusionMap0;
			}
		}


		Ubershader		shader;
		StateFactory	factory;
		ConstantBuffer	paramsCB;
		RenderTarget2D	downsampledDepth;
		RenderTarget2D	downsampledNormals;
		RenderTarget2D	occlusionMap0;
		RenderTarget2D	occlusionMap1;
		Texture2D		randomDir;


		#pragma warning disable 649
		struct Params {
			public	Matrix	ProjMatrix;
			public	Matrix	View;
			public	Matrix	ViewProj;
			public	Matrix	InvViewProj;
			public	float	TraceStep;
			public	float	DecayRate;
			public float	dummy0;
			public float	dummy1;
		}


		enum Flags {	
			HBAO		=	0x001,
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public SsaoFilter ( Game game ) : base(game)
		{
			Config	=	new SsaoFilterConfig();
		}



		/// <summary>
		/// /
		/// </summary>
		public override void Initialize ()
		{
			base.Initialize();

			paramsCB	=	new ConstantBuffer( Game.GraphicsDevice, typeof(Params) );

			CreateTargets();
			LoadContent();

			Random	rand = new Random();

			randomDir	=	new Texture2D( Game.GraphicsDevice, 64,64, ColorFormat.Rgba8, false );
			randomDir.SetData( Enumerable.Range(0,4096).Select( i => rand.NextColor() ).ToArray() );

			Game.GraphicsDevice.DisplayBoundsChanged += (s,e) => CreateTargets();
			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void CreateTargets ()
		{
			var disp	=	Game.GraphicsDevice.DisplayBounds;

			SafeDispose( ref downsampledDepth );
			SafeDispose( ref downsampledNormals );
			SafeDispose( ref occlusionMap0 );
			SafeDispose( ref occlusionMap1 );

			downsampledDepth	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.R32F,  disp.Width/2, disp.Height/2, false, false );
			downsampledNormals	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, disp.Width/2, disp.Height/2, false, false );
			occlusionMap0		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, disp.Width/2, disp.Height/2, false, false );
			occlusionMap1		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba8, disp.Width/2, disp.Height/2, false, false );
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );
			shader	=	Game.Content.Load<Ubershader>("ssao");
			factory	=	new StateFactory( shader, typeof(Flags), Primitive.TriangleList, VertexInputElement.Empty, BlendState.Opaque, RasterizerState.CullNone, DepthStencilState.None ); 
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref downsampledNormals );
				SafeDispose( ref downsampledDepth );
				SafeDispose( ref occlusionMap0 );
				SafeDispose( ref occlusionMap1 );
				SafeDispose( ref paramsCB	 );
				SafeDispose( ref randomDir );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Performs luminance measurement, tonemapping, applies bloom.
		/// </summary>
		/// <param name="target">LDR target.</param>
		/// <param name="hdrImage">HDR source image.</param>
		public void Render ( Matrix view, Matrix projection, ShaderResource depthBuffer, ShaderResource wsNormals )
		{
			var device	=	Game.GraphicsDevice;
			var filter	=	Game.GetService<Filter>();
			var ds		=	Game.GetService<DebugStrings>();

			filter.StretchRect( downsampledDepth.Surface, depthBuffer );
			filter.StretchRect( downsampledNormals.Surface, wsNormals );


			//
			//	Setup parameters :
			//
			var paramsData	=	new Params();
			paramsData.ProjMatrix	=	projection;
			paramsData.View			=	view;
			paramsData.ViewProj		=	view * projection;
			paramsData.InvViewProj	=	Matrix.Invert( view * projection );
			paramsData.TraceStep	=	Config.TraceStep;
			paramsData.DecayRate	=	Config.DecayRate;

			paramsCB.SetData( paramsData );
			device.PixelShaderConstants[0]	=	paramsCB;

			//
			//	Measure and adapt :
			//
			device.SetTargets( null, occlusionMap0 );

			device.PixelShaderResources[0]	=	downsampledDepth;
			device.PixelShaderResources[1]	=	downsampledNormals;
			device.PixelShaderResources[2]	=	randomDir;
			device.PixelShaderSamplers[0]	=	SamplerState.LinearClamp;
			device.PipelineState			=	factory[ (int)Flags.HBAO ];

				
			device.Draw( 3, 0 );
			
			device.ResetStates();


			if (Config.BlurSigma!=0) {
				filter.GaussBlur( occlusionMap0, occlusionMap1, Config.BlurSigma, 0 );
			}
		}



	}
}
