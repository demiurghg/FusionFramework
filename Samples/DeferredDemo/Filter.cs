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

namespace DeferredDemo
{
	/// <summary>
	/// Class for base image processing such as copying, blurring, enhancement, anti-aliasing etc.
	/// </summary>
	public class Filter : GameService {
		readonly GraphicsDevice rs;

		const int MaxBlurTaps	=	33;

		[Flags]
		enum ShaderFlags : int
		{
			PASS1								= 1 << 0,
			PASS2								= 1 << 1,
			FXAA								= 1 << 2,
			COPY								= 1 << 3,
			DOWNSAMPLE_2_2x2					= 1 << 4,
			DOWNSAMPLE_2_4x4					= 1 << 5,
			DOWNSAMPLE_4						= 1 << 6,
			GAUSS_BLUR_3x3						= 1 << 7,
			GAUSS_BLUR							= 1 << 8,
			TAPS_2								= 1 << 9,
			TAPS_3								= 1 << 10,
			TAPS_4								= 1 << 11,
			TAPS_5								= 1 << 12,
			TAPS_6								= 1 << 13,
			TAPS_7								= 1 << 14,
			TAPS_8								= 1 << 15,
			TAPS_9								= 1 << 16,
			TO_CUBE_FACE						= 1 << 17,
			LINEARIZE_DEPTH						= 1 << 18,
			RESOLVE_AND_LINEARIZE_DEPTH_MSAA	= 1 << 19,
		}

		[StructLayout( LayoutKind.Explicit )]
		struct LinearDepth
		{
			[FieldOffset(0)]	public	float	linearizeDepthA;        
			[FieldOffset(4)]	public	float	linearizeDepthB;        
		}

		Ubershader		shaders;
		StateFactory	factory;
		ConstantBuffer	gaussWeightsCB;
		ConstantBuffer	bufLinearizeDepth;
		
		public Filter( Game game ) : base( game )
		{
			rs = game.GraphicsDevice;
		}



		/// <summary>
		/// Initializes Filter service
		/// </summary>
		public override void Initialize() 
		{
			bufLinearizeDepth	= new ConstantBuffer( rs, 128 );
			gaussWeightsCB		= new ConstantBuffer( rs, typeof(Vector4), MaxBlurTaps );

			LoadContent();
			Game.Reloading += (s,e) => LoadContent();

			base.Initialize();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			SafeDispose( ref factory );
			shaders = Game.Content.Load<Ubershader>( "filter" );
			factory	= new StateFactory( shaders, typeof(ShaderFlags), Primitive.TriangleList, VertexInputElement.Empty, BlendState.Opaque, RasterizerState.CullNone, DepthStencilState.None );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref gaussWeightsCB );
				SafeDispose( ref bufLinearizeDepth );
			}

			base.Dispose( disposing );
		}



		/// <summary>
		/// Sets default render state
		/// </summary>
		void SetDefaultRenderStates()
		{
			rs.ResetStates();
		}

		/*-----------------------------------------------------------------------------------------------
		 * 
		 *	Base filters
		 * 
		-----------------------------------------------------------------------------------------------*/

		/// <summary>
		/// Performs good-old StretchRect to destination buffer with blending.
		/// </summary>
		/// <param name="dst"></param>
		/// <param name="src"></param>
		/// <param name="blendState"></param>
		public void StretchRect( RenderTargetSurface dst, ShaderResource src, SamplerState filter = null )
		{
			SetDefaultRenderStates();

			using( new PixEvent() ) {

				SetViewport(dst);
				rs.SetTargets( null, dst );

				rs.PipelineState			=	factory[ (int)ShaderFlags.DOWNSAMPLE_2_2x2 ];
				rs.VertexShaderResources[0] =	src;
				rs.PixelShaderResources[0]	=	src;
				rs.PixelShaderSamplers[0]	=  filter ?? SamplerState.LinearPointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		public void StretchRect4x4( RenderTargetSurface dst, RenderTarget2D src, SamplerState filter = null )
		{
			SetDefaultRenderStates();


			using( new PixEvent() ) {

				rs.SetTargets( null, dst );
				SetViewport(dst);
				
				rs.PipelineState			=	factory[ (int)ShaderFlags.DOWNSAMPLE_2_4x4 ];
				rs.VertexShaderResources[0] =	src;
				rs.PixelShaderResources[0]	=	src;
				rs.PixelShaderSamplers[0]	=	filter ?? SamplerState.LinearPointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		public void DownSample4( RenderTarget2D dst, RenderTarget2D src )
		{
			SetDefaultRenderStates();

			using( new PixEvent() ) {

				dst.SetViewport();
				rs.SetTargets( null, dst );

				rs.PipelineState			=	factory[ (int)ShaderFlags.DOWNSAMPLE_4 ];
				rs.VertexShaderResources[0] =	src;
				rs.PixelShaderResources[0]	=	src;
				rs.PixelShaderSamplers[0]	=	SamplerState.LinearPointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		public void LinearizeDepth( RenderTargetSurface dst, ShaderResource src )
		{
			throw new NotImplementedException();
		#if false
			Debug.Assert( Game.IsServiceExist<Camera>() );

			var camera = Game.GetService<Camera>();

			bufLinearizeDepth.Data.linearizeDepthA = 1.0f / camera.FrustumZFar - 1.0f / camera.FrustumZNear;
			bufLinearizeDepth.Data.linearizeDepthB = 1.0f / camera.FrustumZNear;
			bufLinearizeDepth.UpdateCBuffer();


			var isDepthMSAA = ( src.SampleCount > 1 );
			var depthShader = (int)( isDepthMSAA ? ShaderFlags.RESOLVE_AND_LINEARIZE_DEPTH_MSAA : ShaderFlags.LINEARIZE_DEPTH );

			string signature;

			SetDefaultRenderStates();

			using( new PixEvent() ) {
				bufLinearizeDepth.SetCBufferPS( 0 );

				shaders.SetPixelShader( depthShader );
				shaders.SetVertexShader( depthShader );

				dst.SetViewport();
				rs.SetRenderTargets( dst );
				src.SetPS( 0 );

				rs.Draw( Primitive.TriangleList, 3, 0 );
			}
			rs.ResetStates();
		#endif
		}



		void SetViewport ( RenderTargetSurface dst )
		{
			rs.SetViewport( 0,0, dst.Width, dst.Height );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="dst">target to copy to</param>
		/// <param name="src">target to copy from</param>
		public void Copy( RenderTargetSurface dst, ShaderResource src )
		{
			SetDefaultRenderStates();

			using( new PixEvent() ) {

				if(dst == null) {
					rs.RestoreBackbuffer();
				} else {
					SetViewport(dst);
					rs.SetTargets( null, dst );
				}

				rs.PipelineState			=	factory[ (int)ShaderFlags.COPY ];
				rs.PixelShaderResources[0]	= src;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		/// <summary>
		/// Performs FXAA antialiasing.
		/// </summary>
		/// <param name="dst">Target buffer to render FXAA to</param>
		/// <param name="src">Source image with luminance in alpha</param>
		public void Fxaa( RenderTargetSurface dst, ShaderResource src )
		{
			SetDefaultRenderStates();

			using( new PixEvent() ) {

				if(dst == null) {
					rs.RestoreBackbuffer();
				} else {
					SetViewport( dst );
					rs.SetTargets( null, dst );
				}

				rs.PipelineState			=	factory[ (int)ShaderFlags.FXAA ];
				rs.VertexShaderResources[0] =	src;
				rs.PixelShaderResources[0]	=	src;
				rs.PixelShaderSamplers[0]	=	SamplerState.LinearPointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="srcDst">source and destination target</param>
		/// <param name="temporary">temporaru target for two pass filter</param>
		/*public void GaussBlur3x3( RenderTarget2D srcDst, RenderTarget2D temporary )
		{
			SetDefaultRenderStates();

			using( new PixEvent() ) {
				srcDst.SetViewport();
				rs.PixelShaderSamplers[0] = SamplerState.LinearPointClamp;

				rs.PipelineState			=	factory[ (int)ShaderFlags.GAUSS_BLUR_3x3 ];
                shaders.SetPixelShader( (int)(ShaderFlags.GAUSS_BLUR_3x3) );
                shaders.SetVertexShader( (int)(ShaderFlags.GAUSS_BLUR_3x3) );

                rs.SetTargets( null, temporary );
				rs.VertexShaderResources[0] = srcDst;
				rs.PixelShaderResources[0] = srcDst;
                
				rs.Draw( Primitive.TriangleList, 3, 0 );

                shaders.SetPixelShader( (int)(ShaderFlags.GAUSS_BLUR_3x3 | ShaderFlags.PASS2) );
                shaders.SetVertexShader( (int)(ShaderFlags.GAUSS_BLUR_3x3 | ShaderFlags.PASS2) );

                rs.SetTargets( null, srcDst );
				rs.VertexShaderResources[0] = temporary;
				rs.PixelShaderResources[0] = temporary;

                rs.Draw( Primitive.TriangleList, 3, 0 );
			}
			rs.ResetStates();
		}	*/



		/// <summary>
		/// 
		/// </summary>
		/// <param name="srcDst"></param>
		/// <param name="temporary"></param>
		/// <param name="sigma"></param>
		/// <param name="kernelSize"></param>
		public void GaussBlur( RenderTarget2D srcDst, RenderTarget2D temporary, float sigma, int mipLevel )
		{
			var taps = GetGaussWeightsBuffer( sigma, mipLevel );

			SetDefaultRenderStates();

			gaussWeightsCB.SetData( taps );

			using( new PixEvent() ) {


				SetViewport(temporary.GetSurface(mipLevel));
				rs.SetTargets( null, temporary.GetSurface(mipLevel) );

				rs.PipelineState			=	factory[ (int)(ShaderFlags.GAUSS_BLUR | ShaderFlags.PASS1) ];
				rs.VertexShaderResources[0]	=	srcDst;
				rs.PixelShaderResources[0]	=	srcDst;
				rs.PixelShaderConstants[0]	=	gaussWeightsCB;
				
				rs.PixelShaderSamplers[0]	=	SamplerState.LinearPointClamp;

				rs.Draw( 3, 0 );


				rs.VertexShaderResources[0] =	null;
				rs.PixelShaderResources[0]	=	null;

				SetViewport(srcDst.GetSurface(mipLevel));
				rs.SetTargets( null, srcDst.GetSurface(mipLevel) );

				rs.PipelineState			=	factory[ (int)(ShaderFlags.GAUSS_BLUR | ShaderFlags.PASS2) ];
				rs.VertexShaderResources[0] =	temporary;
				rs.PixelShaderResources[0]	=	temporary;
				rs.PixelShaderConstants[0]	=	gaussWeightsCB;

				rs.PixelShaderSamplers[0]	=	SamplerState.LinearPointClamp;

				rs.Draw( 3, 0 );
			}
			rs.ResetStates();
		}



		float GaussDistribution ( float x, float sigma )
		{
			var k1 =  1.0 / (sigma * Math.Sqrt(2.0*Math.PI));
			var k2 = -1.0 / (2.0 * sigma * sigma);
			return (float)( k1 * Math.Exp( k2 * x * x ) );
		}


		Vector4[] GetGaussWeightsBuffer( float sigma, int mipLevel ) 
		{
			var taps = new Vector4[MaxBlurTaps];

			for ( int i=0; i<MaxBlurTaps; i++) {

				float x = i - (MaxBlurTaps/2);

				taps[i].X = GaussDistribution( x, sigma );
				taps[i].Y = mipLevel;
				taps[i].W = x * (1 << mipLevel);
			}

			return taps;
			//bufGaussWeights.UpdateCBuffer();
			//#endif
		}
	}
}
