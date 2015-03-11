using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Fusion.Mathematics;

using D3DBlendState			=	SharpDX.Direct3D11.BlendState		;
using D3DSamplerState		=	SharpDX.Direct3D11.SamplerState		;
using D3DRasterizerState	=	SharpDX.Direct3D11.RasterizerState	;
using D3DDepthStencilState	=	SharpDX.Direct3D11.DepthStencilState;
using D3DCullMode			=	SharpDX.Direct3D11.CullMode;

using D3DPixelShader		=	SharpDX.Direct3D11.PixelShader		;
using D3DVertexShader		=	SharpDX.Direct3D11.VertexShader		;
using D3DGeometryShader		=	SharpDX.Direct3D11.GeometryShader	;
using D3DHullShader			=	SharpDX.Direct3D11.HullShader		;
using D3DDomainShader		=	SharpDX.Direct3D11.DomainShader		;
using D3DComputeShader		=	SharpDX.Direct3D11.ComputeShader	;
using SharpDX.Mathematics.Interop;


namespace Fusion.Graphics {

	/// <summary>
	/// Pipeline state represents all GPU states as single object.
	/// 
	/// </summary>
	public class PipelineState : DisposableBase {

		public int SamplersCount { get { return CommonShaderStage.SamplerRegisterCount; } }

		/// <summary>
		/// Blend description. Null value is not acceptable.
		/// </summary>
		public BlendDescription Blending { get; private set; }

		/// <summary>
		/// DepthStencul description. Null value is not acceptable.
		/// </summary>
		public DepthStencilDescription DepthStencil { get; private set; }

		/// <summary>
		/// Rasterizer state description. Null value is not acceptable.
		/// </summary>
		public RasterizerDescription Rasterizer { get; private set; }

		/// <summary>
		/// Sampler state descriptions. Null values are acceptable.
		/// </summary>
		public SamplerState[] Samplers { get; private set; }

		/// <summary>
		/// Pixel shader. Null value is not acceptable.
		/// </summary>
		public ShaderBytecode PixelShader { get; set; }

		/// <summary>
		/// Vertex Shader. Null value is not acceptable.
		/// </summary>
		public ShaderBytecode VertexShader { get; set; }

		/// <summary>
		/// Geometry Shader. Null value is acceptable.
		/// </summary>
		public ShaderBytecode GeometryShader { get; set; }

		/// <summary>
		/// Hull Shader. Null value is acceptable.
		/// </summary>
		public ShaderBytecode HullShader { get; set; }

		/// <summary>
		/// Domain Shader. Null value is acceptable.
		/// </summary>
		public ShaderBytecode DomainShader { get; set; }

		/// <summary>
		/// Compute Shader. Null value is acceptable.
		/// </summary>
		public ShaderBytecode ComputeShader { get; set; }

		/// <summary>
		/// Vertex input elements. Null value is acceptable.
		/// </summary>
		public VertexInputElement[] VertexInputElements	{ 
			get; 
			set; 
		}

		/// <summary>
		/// Vertex output elements. Null value is acceptable.
		/// </summary>
		public VertexOutputElement[] VertexOutputElements	{ 
			get; 
			set; 
		}


		bool					isReady	=	false;

		private GraphicsDevice	device;
		D3DBlendState			blendState;
		D3DRasterizerState		rasterState;
		D3DDepthStencilState	depthState;
		D3DSamplerState[]		samplerStates;
		InputLayout				inputLayout;

		D3DPixelShader			ps;
		D3DVertexShader			vs;
		D3DGeometryShader		gs;
		D3DHullShader			hs;
		D3DDomainShader			ds;
		D3DComputeShader		cs;
		RawColor4				blendFactor;
		int						blendMsaaMask;
		int						dssReference;




		/// <summary>
		/// 
		/// </summary>
		public PipelineState ( GraphicsDevice device )
		{	
			this.device	=	device;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				DisposeStates();
			}

			base.Dispose();
		}



		void DisposeStates ()
		{
			SafeDispose( ref blendState  );
			SafeDispose( ref rasterState  );
			SafeDispose( ref depthState );

			foreach ( var s in samplerStates ) {
				if (s!=null) {	
					s.Dispose();
				}
				samplerStates = null;
			}

			SafeDispose( ref ps );
			SafeDispose( ref vs );
			SafeDispose( ref gs );
			SafeDispose( ref hs );
			SafeDispose( ref ds );
			SafeDispose( ref cs );

			SafeDispose( ref inputLayout );
		}



		/// <summary>
		/// Sets pipeline state.
		/// </summary>
		internal void Set()
		{
			if (!isReady) {
				ApplyChanges();
			}

			//	set blending :
			device.DeviceContext.OutputMerger.BlendState		=	blendState;
			device.DeviceContext.OutputMerger.BlendFactor		=	blendFactor;
			device.DeviceContext.OutputMerger.BlendSampleMask	=	blendMsaaMask;

			//	set depth-stencil :
			device.DeviceContext.OutputMerger.DepthStencilState		=	depthState;
			device.DeviceContext.OutputMerger.DepthStencilReference	=	dssReference;

			//	set rasterizer :
			device.DeviceContext.Rasterizer.State	=	rasterState;

			//	samplers :
			device.
		}






		/// <summary>
		/// Applies changes to this pipeline state.
		/// <remarks>This method is quite slow because performs a lot of checks and creates new internal states and objects.
		/// Do not call this method each frame, instead create pipeline state object for each case.</remarks>
		/// </summary>
		public void ApplyChanges ()
		{
			lock (device.DeviceContext) {
				
				DisposeStates();

				SetupBlendState();

				SetupRasterizerState();

				SetupDepthStencilState();

				SetupSamplerStates();

				SetupShadersAndLayouts();

				isReady = true;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void SetupShadersAndLayouts ()
		{
			ps	=	new D3DPixelShader( device.Device, PixelShader.Bytecode );
			vs	=	new D3DVertexShader( device.Device, VertexShader.Bytecode );

			inputLayout	=	new InputLayout( device.Device, VertexShader.Bytecode, VertexInputElement.Convert( VertexInputElements ) );
		}



		/// <summary>
		/// SetupSamplerStates
		/// </summary>
		void SetupSamplerStates ()
		{
			samplerStates = new D3DSamplerState[ Samplers.Length ];

			for ( int i=0; i<Samplers.Length; i++) {

				var sd = Samplers[i];
				
				var ssd = new SamplerStateDescription();

				ssd.ComparisonFunction	=	Converter.Convert( sd.ComparisonFunc );
				ssd.AddressU			=	Converter.Convert( sd.AddressU );
				ssd.AddressV			=	Converter.Convert( sd.AddressV );
				ssd.AddressW			=	Converter.Convert( sd.AddressW );
				ssd.BorderColor			=	SharpDXHelper.Convert( sd.BorderColor );
				ssd.Filter				=	Converter.Convert( sd.Filter );
				ssd.MaximumAnisotropy	=	sd.MaxAnisotropy;
				ssd.MaximumLod			=	sd.MaxMipLevel;
				ssd.MinimumLod			=	sd.MinMipLevel;
				ssd.MipLodBias			=	sd.MipMapBias;

				var ss	=	new D3DSamplerState( device.Device, ssd );

				samplerStates[i] = ss;
			}
		}



		/// <summary>
		/// SetupDepthStencilState
		/// </summary>
		void SetupDepthStencilState ()
		{
			var dss	=	new DepthStencilStateDescription();

			dss.DepthComparison		=	Converter.Convert( DepthStencil.DepthComparison );
			dss.DepthWriteMask		=	DepthStencil.DepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero;
			dss.IsDepthEnabled		=	DepthStencil.DepthEnabled;
			dss.IsStencilEnabled	=	DepthStencil.StencilEnabled;
			dss.StencilReadMask		=	DepthStencil.StencilReadMask;
			dss.StencilWriteMask	=	DepthStencil.StencilWriteMask;

			dss.BackFace.Comparison				=	Converter.Convert( DepthStencil.BackFaceStencilComparison	);
			dss.BackFace.FailOperation			=	Converter.Convert( DepthStencil.BackFaceFailOp				);
			dss.BackFace.DepthFailOperation		=	Converter.Convert( DepthStencil.BackFaceDepthFailOp			);
			dss.BackFace.PassOperation			=	Converter.Convert( DepthStencil.BackFacePassOp				);

			dss.FrontFace.Comparison			=	Converter.Convert( DepthStencil.FrontFaceStencilComparison	);
			dss.FrontFace.FailOperation			=	Converter.Convert( DepthStencil.FrontFaceFailOp				);
			dss.FrontFace.DepthFailOperation	=	Converter.Convert( DepthStencil.FrontFaceDepthFailOp		);
			dss.FrontFace.PassOperation			=	Converter.Convert( DepthStencil.FrontFacePassOp				);

			dssReference	=	DepthStencil.StencilReference;

			this.depthState	=	new D3DDepthStencilState( device.Device, dss );
		}



		/// <summary>
		/// SetupRasterizerState
		/// </summary>
		void SetupRasterizerState ()
		{
			var rsd = new RasterizerStateDescription();

			if ( Rasterizer.CullMode == CullMode.CullNone ) {

				rsd.CullMode				=	D3DCullMode.None;
				rsd.IsFrontCounterClockwise	=	false;

			} else if ( Rasterizer.CullMode == CullMode.CullCW ) {

				rsd.CullMode				=	D3DCullMode.Front;
				rsd.IsFrontCounterClockwise	=	false;

			} else if ( Rasterizer.CullMode == CullMode.CullCCW ) {

				rsd.CullMode				=	D3DCullMode.Front;
				rsd.IsFrontCounterClockwise	=	true;
			}


			rsd.FillMode				=	Converter.Convert( Rasterizer.FillMode );
			rsd.DepthBias				=	Rasterizer.DepthBias;
			rsd.DepthBiasClamp			=	0;
			rsd.IsMultisampleEnabled	=	Rasterizer.MsaaEnabled;
			rsd.IsScissorEnabled		=	Rasterizer.ScissorEnabled;
			rsd.SlopeScaledDepthBias	=	Rasterizer.SlopeDepthBias;

			rasterState	=	new D3DRasterizerState( device.Device, rsd );
		}



		/// <summary>
		/// SetupBlendState
		/// </summary>
		void SetupBlendState ()
		{
			var	rtbd	=	new RenderTargetBlendDescription();

			bool enabled	=	true;

			if ( Blending.DstAlpha==Blend.Zero && Blending.SrcAlpha==Blend.One &&
				 Blending.DstColor==Blend.Zero && Blending.SrcColor==Blend.One ) {

				 enabled = false;
			}

			rtbd.IsBlendEnabled			=	enabled	;
			rtbd.BlendOperation			=	Converter.Convert( Blending.ColorOp );
			rtbd.AlphaBlendOperation	=	Converter.Convert( Blending.AlphaOp );
			rtbd.RenderTargetWriteMask	=	(ColorWriteMaskFlags)(int)Blending.WriteMask;
			rtbd.DestinationBlend		=	Converter.Convert( Blending.DstColor );
			rtbd.SourceBlend			=	Converter.Convert( Blending.SrcColor );
			rtbd.DestinationAlphaBlend	=	Converter.Convert( Blending.DstAlpha );
			rtbd.SourceAlphaBlend		=	Converter.Convert( Blending.SrcAlpha );
				
			var	bsd		=	new BlendStateDescription();
			bsd.AlphaToCoverageEnable	=	false;
			bsd.IndependentBlendEnable	=	false;
			bsd.RenderTarget[0]			=	rtbd;

			blendFactor	=	SharpDXHelper.Convert( Blending.BlendFactor );
			blendMsaaMask	=	Blending.MultiSampleMask;

			blendState	=	new D3DBlendState( device.Device, bsd );
		}

	}
}
