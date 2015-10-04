using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Fusion.Core;
using Fusion.Core.Mathematics;

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


namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// Pipeline state represents all GPU states as single object.
	/// 
	/// </summary>
	public sealed class PipelineState : GraphicsResource {

		public int SamplersCount { get { return CommonShaderStage.SamplerRegisterCount; } }

		/// <summary>
		/// Blend description. Null value is not acceptable.
		/// </summary>
		public BlendState BlendState { get; set; }

		/// <summary>
		/// Rasterizer state description. Null value is not acceptable.
		/// </summary>
		public RasterizerState RasterizerState { get; set; }

		/// <summary>
		/// DepthStencil state description. Null value is not acceptable.
		/// </summary>
		public DepthStencilState DepthStencilState { get; set; }

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
		/// Primitive type.
		/// </summary>
		public Primitive Primitive { get; set; }

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

		/// <summary>
		/// 
		/// </summary>
		public int RasterizedStream { 
			get;
			set;
		}

		bool					isReady	=	false;

		D3DBlendState			blendState;
		D3DRasterizerState		rasterState;
		D3DDepthStencilState	depthStencilState;
		int						depthStencilRef;
		InputLayout				inputLayout;

		D3DPixelShader			ps;
		D3DVertexShader			vs;
		D3DGeometryShader		gs;
		D3DHullShader			hs;
		D3DDomainShader			ds;
		D3DComputeShader		cs;
		RawColor4				blendFactor;
		int						blendMsaaMask;




		/// <summary>
		/// 
		/// </summary>
		public PipelineState ( GraphicsDevice device ) : base(device)
		{	
			BlendState			=	BlendState.Opaque;
			RasterizerState		=	RasterizerState.CullNone;
			DepthStencilState	=	DepthStencilState.Default;
			RasterizedStream	=	-1;
			Primitive			=	Primitive.TriangleList;

			isReady		=	false;
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

			base.Dispose(disposing);
		}



		/// <summary>
		/// Disposes states.
		/// </summary>
		void DisposeStates ()
		{
			SafeDispose( ref blendState  );
			SafeDispose( ref rasterState  );
			SafeDispose( ref depthStencilState  );

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

			device.DeviceContext.InputAssembler.PrimitiveTopology	=	Converter.Convert( Primitive );

			//	set blending :
			device.DeviceContext.OutputMerger.BlendState		=	blendState;
			device.DeviceContext.OutputMerger.BlendFactor		=	blendFactor;
			device.DeviceContext.OutputMerger.BlendSampleMask	=	blendMsaaMask;

			//	set rasterizer :
			device.DeviceContext.Rasterizer.State	=	rasterState;

			//	shaders :
			device.DeviceContext.PixelShader	.Set( ps );
			device.DeviceContext.VertexShader	.Set( vs );
			device.DeviceContext.GeometryShader	.Set( gs );
			device.DeviceContext.HullShader		.Set( hs );
			device.DeviceContext.DomainShader	.Set( ds );
			device.DeviceContext.ComputeShader	.Set( cs );

			device.DeviceContext.OutputMerger.DepthStencilState		=	depthStencilState;
			device.DeviceContext.OutputMerger.DepthStencilReference	=	depthStencilRef;

			//	layout :
			device.DeviceContext.InputAssembler.InputLayout	=	inputLayout;
		}






		/// <summary>
		/// Applies changes to this pipeline state.
		/// <remarks>This method is quite slow because performs a lot of checks and creates new internal states and objects.
		/// Do not call this method each frame, instead create pipeline state object for each case on init.</remarks>
		/// </summary>
		public void ApplyChanges ()
		{
			lock (device.DeviceContext) {
				
				DisposeStates();

				SetupDepthStencilState();

				SetupBlendState();

				SetupRasterizerState();

				SetupShadersAndLayouts();

				isReady = true;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void SetupShadersAndLayouts ()
		{
			ps	=	PixelShader		== null ? null : new D3DPixelShader		( device.Device, PixelShader	.Bytecode );
			vs	=	VertexShader	== null ? null : new D3DVertexShader	( device.Device, VertexShader	.Bytecode );
			gs	=	GeometryShader	== null ? null : new D3DGeometryShader	( device.Device, GeometryShader	.Bytecode );
			hs	=	HullShader		== null ? null : new D3DHullShader		( device.Device, HullShader		.Bytecode );
			ds	=	DomainShader	== null ? null : new D3DDomainShader	( device.Device, DomainShader	.Bytecode );
			cs	=	ComputeShader	== null ? null : new D3DComputeShader	( device.Device, ComputeShader	.Bytecode );

			if (cs!=null) {
				if ( ps!=null || vs!=null || gs!=null || hs!=null || ds!=null ) {
					throw new InvalidOperationException("If ComputeShader is set, other shader must be set null.");
				}
			} else {
				if ( vs==null ) {
					throw new InvalidOperationException("Vertex shader must be set.");
				}
			}


			
			if (VertexInputElements==null) {
				inputLayout =	null ;
			} else {
				inputLayout	=	new InputLayout( device.Device, VertexShader.Bytecode, VertexInputElement.Convert( VertexInputElements ) );
			}



			if (VertexOutputElements!=null) {

				if (GeometryShader==null) {
					throw new InvalidOperationException("Geometry shader is required for vertex output.");
				}

				var outputElements	=	VertexOutputElement.Convert( VertexOutputElements );
				int maxBuffers		=	outputElements.Max( oe => oe.OutputSlot ) + 1;
				var bufferedStrides	=	new int[ maxBuffers ];

				for (int i=0; i<maxBuffers; i++) {
					bufferedStrides[i] = outputElements	
										.Where( oe1 => oe1.OutputSlot==i )
										.Sum( oe2 => oe2.ComponentCount	) * 4;
				}

				gs	=	new D3DGeometryShader( device.Device, GeometryShader.Bytecode, outputElements, bufferedStrides, RasterizedStream ); 
			}
		}


		/// <summary>
		/// 
		/// </summary>
		void SetupDepthStencilState ()
		{
			var dss	=	new DepthStencilStateDescription();

			dss.DepthComparison		=	Converter.Convert( DepthStencilState.DepthComparison );
			dss.DepthWriteMask		=	DepthStencilState.DepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero;
			dss.IsDepthEnabled		=	DepthStencilState.DepthEnabled;
			dss.IsStencilEnabled	=	DepthStencilState.StencilEnabled;
			dss.StencilReadMask		=	DepthStencilState.StencilReadMask;
			dss.StencilWriteMask	=	DepthStencilState.StencilWriteMask;

			dss.BackFace.Comparison				=	Converter.Convert( DepthStencilState.BackFaceStencilComparison	);
			dss.BackFace.FailOperation			=	Converter.Convert( DepthStencilState.BackFaceFailOp				);
			dss.BackFace.DepthFailOperation		=	Converter.Convert( DepthStencilState.BackFaceDepthFailOp		);
			dss.BackFace.PassOperation			=	Converter.Convert( DepthStencilState.BackFacePassOp				);

			dss.FrontFace.Comparison			=	Converter.Convert( DepthStencilState.FrontFaceStencilComparison	);
			dss.FrontFace.FailOperation			=	Converter.Convert( DepthStencilState.FrontFaceFailOp				);
			dss.FrontFace.DepthFailOperation	=	Converter.Convert( DepthStencilState.FrontFaceDepthFailOp		);
			dss.FrontFace.PassOperation			=	Converter.Convert( DepthStencilState.FrontFacePassOp				);

			depthStencilRef		=	DepthStencilState.StencilReference;
			depthStencilState	=	new D3DDepthStencilState( device.Device, dss );
		}



		/// <summary>
		/// SetupRasterizerState
		/// </summary>
		void SetupRasterizerState ()
		{
			var rsd = new RasterizerStateDescription();

			if ( RasterizerState.CullMode == CullMode.CullNone ) {

				rsd.CullMode				=	D3DCullMode.None;
				rsd.IsFrontCounterClockwise	=	false;

			} else if ( RasterizerState.CullMode == CullMode.CullCW ) {

				rsd.CullMode				=	D3DCullMode.Front;
				rsd.IsFrontCounterClockwise	=	false;

			} else if ( RasterizerState.CullMode == CullMode.CullCCW ) {

				rsd.CullMode				=	D3DCullMode.Front;
				rsd.IsFrontCounterClockwise	=	true;
			}


			rsd.FillMode				=	Converter.Convert( RasterizerState.FillMode );
			rsd.DepthBias				=	RasterizerState.DepthBias;
			rsd.DepthBiasClamp			=	0;
			rsd.IsMultisampleEnabled	=	RasterizerState.MsaaEnabled;
			rsd.IsScissorEnabled		=	RasterizerState.ScissorEnabled;
			rsd.SlopeScaledDepthBias	=	RasterizerState.SlopeDepthBias;

			rasterState	=	new D3DRasterizerState( device.Device, rsd );
		}



		/// <summary>
		/// SetupBlendState
		/// </summary>
		void SetupBlendState ()
		{
			var	rtbd	=	new RenderTargetBlendDescription();

			bool enabled	=	true;

			if ( BlendState.DstAlpha==Blend.Zero && BlendState.SrcAlpha==Blend.One &&
				 BlendState.DstColor==Blend.Zero && BlendState.SrcColor==Blend.One ) {

				 enabled = false;
			}

			rtbd.IsBlendEnabled			=	enabled	;
			rtbd.BlendOperation			=	Converter.Convert( BlendState.ColorOp );
			rtbd.AlphaBlendOperation	=	Converter.Convert( BlendState.AlphaOp );
			rtbd.RenderTargetWriteMask	=	(ColorWriteMaskFlags)(int)BlendState.WriteMask;
			rtbd.DestinationBlend		=	Converter.Convert( BlendState.DstColor );
			rtbd.SourceBlend			=	Converter.Convert( BlendState.SrcColor );
			rtbd.DestinationAlphaBlend	=	Converter.Convert( BlendState.DstAlpha );
			rtbd.SourceAlphaBlend		=	Converter.Convert( BlendState.SrcAlpha );
				
			var	bsd		=	new BlendStateDescription();
			bsd.AlphaToCoverageEnable	=	false;
			bsd.IndependentBlendEnable	=	false;
			bsd.RenderTarget[0]			=	rtbd;

			blendFactor	=	SharpDXHelper.Convert( BlendState.BlendFactor );
			blendMsaaMask	=	BlendState.MultiSampleMask;

			blendState	=	new D3DBlendState( device.Device, bsd );
		}

	}
}
