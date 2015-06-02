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
	public class HdrFilter : GameService {

		[Config]
		public HdrFilterConfig	Config { get; set; }


		Ubershader	shader;
		ConstantBuffer	paramsCB;
		RenderTarget2D	averageLum;
		RenderTarget2D	measuredOld;
		RenderTarget2D	measuredNew;

		RenderTarget2D	bloom0;
		RenderTarget2D	bloom1;
		StateFactory	factory;


		Texture2D		bloomMask;

		//	float AdaptationRate;          // Offset:    0
		//	float LuminanceLowBound;       // Offset:    4
		//	float LuminanceHighBound;      // Offset:    8
		//	float KeyValue;                // Offset:   12
		//	float BloomAmount;             // Offset:   16
		[StructLayout(LayoutKind.Explicit, Size=32)]
		struct Params {
			[FieldOffset( 0)]	public	float	AdaptationRate;
			[FieldOffset( 4)]	public	float 	LuminanceLowBound;
			[FieldOffset( 8)]	public	float	LuminanceHighBound;
			[FieldOffset(12)]	public	float	KeyValue;
			[FieldOffset(16)]	public	float	BloomAmount;
		}


		enum Flags {	
			TONEMAPPING		=	0x001,
			MEASURE_ADAPT	=	0x002,
			LINEAR			=	0x004, 
			REINHARD		=	0x008,
			FILMIC			=	0x010,
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public HdrFilter ( Game game ) : base(game)
		{
			Config	=	new HdrFilterConfig();
		}



		/// <summary>
		/// /
		/// </summary>
		public override void Initialize ()
		{
			base.Initialize();

			averageLum	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, 256,256, true, false );
			measuredOld	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba32F,   1,  1 );
			measuredNew	=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba32F,   1,  1 );
			paramsCB	=	new ConstantBuffer( Game.GraphicsDevice, typeof(Params) );

			CreateTargets();
			LoadContent();

			Game.GraphicsDevice.DisplayBoundsChanged += (s,e) => CreateTargets();
			Game.Reloading += (s,e) => LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void CreateTargets ()
		{
			var disp	=	Game.GraphicsDevice.DisplayBounds;

			SafeDispose( ref bloom0 );
			SafeDispose( ref bloom1 );

			int width	=	( disp.Width/2  ) & 0xFFF0;
			int height	=	( disp.Height/2 ) & 0xFFF0;
			bloom0		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, width, height, true, false );
			bloom1		=	new RenderTarget2D( Game.GraphicsDevice, ColorFormat.Rgba16F, width, height, true, false );
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );

			shader	=	Game.Content.Load<Ubershader>("hdr");
			factory	=	new StateFactory( shader, typeof(Flags), Primitive.TriangleList, VertexInputElement.Empty, BlendState.Opaque, RasterizerState.CullNone, DepthStencilState.None );

			bloomMask	=	Game.Content.Load<Texture2D>("bloomMask");
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref bloom0 );
				SafeDispose( ref bloom1 );
				SafeDispose( ref averageLum	 );
				SafeDispose( ref measuredOld );
				SafeDispose( ref measuredNew );
				SafeDispose( ref paramsCB	 );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Performs luminance measurement, tonemapping, applies bloom.
		/// </summary>
		/// <param name="target">LDR target.</param>
		/// <param name="hdrImage">HDR source image.</param>
		public void Render ( GameTime gameTime, RenderTargetSurface target, ShaderResource hdrImage )
		{
			var device	=	Game.GraphicsDevice;
			var filter	=	Game.GetService<Filter>();
			var ds		=	Game.GetService<DebugStrings>();


			//
			//	Rough downsampling of source HDR-image :
			//
			filter.StretchRect( averageLum.Surface, hdrImage );
			averageLum.BuildMipmaps();

			//
			//	Make bloom :
			//
			filter.StretchRect( bloom0.Surface, hdrImage );
			bloom0.BuildMipmaps();

			filter.GaussBlur( bloom0, bloom1, Config.GaussBlurSigma, 0 );
			filter.GaussBlur( bloom0, bloom1, Config.GaussBlurSigma, 1 );
			filter.GaussBlur( bloom0, bloom1, Config.GaussBlurSigma, 2 );
			filter.GaussBlur( bloom0, bloom1, Config.GaussBlurSigma, 3 );


			//
			//	Setup parameters :
			//
			var paramsData	=	new Params();
			paramsData.AdaptationRate		=	1 - (float)Math.Pow( 0.5f, gameTime.ElapsedSec / Config.AdaptationHalfLife );
			paramsData.LuminanceLowBound	=	Config.LuminanceLowBound;
			paramsData.LuminanceHighBound	=	Config.LuminanceHighBound;
			paramsData.KeyValue				=	Config.KeyValue;
			paramsData.BloomAmount			=	Config.BloomAmount;

			paramsCB.SetData( paramsData );
			device.PixelShaderConstants[0]	=	paramsCB;


			//
			//	Measure and adapt :
			//
			device.SetTargets( null, measuredNew );

			device.PixelShaderResources[0]	=	averageLum;
			device.PixelShaderResources[1]	=	measuredOld;

			device.PipelineState		=	factory[ (int)(Flags.MEASURE_ADAPT) ];
				
			device.Draw( 3, 0 );


			//
			//	Tonemap and compose :
			//
			device.SetTargets( null, target );

			device.PixelShaderResources[0]	=	hdrImage;// averageLum;
			device.PixelShaderResources[1]	=	measuredNew;// averageLum;
			device.PixelShaderResources[2]	=	bloom0;// averageLum;
			device.PixelShaderResources[3]	=	Game.GetService<SpriteBatch>().TextureWhite;
			device.PixelShaderSamplers[0]	=	SamplerState.LinearClamp;

			Flags op = Flags.LINEAR;
			if (Config.TonemappingOperator==TonemappingOperator.Filmic)   { op = Flags.FILMIC;   }
			if (Config.TonemappingOperator==TonemappingOperator.Linear)   { op = Flags.LINEAR;	 }
			if (Config.TonemappingOperator==TonemappingOperator.Reinhard) { op = Flags.REINHARD; }

			device.PipelineState		=	factory[ (int)(Flags.TONEMAPPING|op) ];
				
			device.Draw( 3, 0 );
			
			device.ResetStates();


			//	swap luminanice buffers :
			Misc.Swap( ref measuredNew, ref measuredOld );
		}



	}
}
