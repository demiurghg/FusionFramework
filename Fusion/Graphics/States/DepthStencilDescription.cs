using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using D3DBlendState			=	SharpDX.Direct3D11.BlendState		;
using D3DSamplerState		=	SharpDX.Direct3D11.SamplerState		;
using D3DRasterizerState	=	SharpDX.Direct3D11.RasterizerState	;
using D3DDepthStencilState	=	SharpDX.Direct3D11.DepthStencilState;


namespace Fusion.Graphics {

	public class DepthStencilDescription {

		PipelineState pipelineState;
		
		/// <summary>
		/// 
		/// </summary>
		internal DepthStencilDescription ( PipelineState pipelineState )
		{
			if (pipelineState==null) {
				throw new ArgumentNullException("pipelineState");
			}
			this.pipelineState = pipelineState;
		}


		bool			DepthEnabled				{ get { return depthEnabled				; } set { pipelineState.MakeDirty(); depthEnabled			 = value; } }
		bool			DepthWriteEnabled			{ get { return depthWriteEnabled		; } set { pipelineState.MakeDirty(); depthWriteEnabled		 = value; } }
		ComparisonFunc	DepthComparison				{ get { return depthComparison			; } set { pipelineState.MakeDirty(); depthComparison		 = value; } }
																																						
		bool			StencilEnabled				{ get { return stencilEnabled			; } set { pipelineState.MakeDirty(); stencilEnabled			 = value; } }
		byte			StencilReadMask				{ get { return stencilReadMask			; } set { pipelineState.MakeDirty(); stencilReadMask		 = value; } }
		byte			StencilWriteMask			{ get { return stencilWriteMask			; } set { pipelineState.MakeDirty(); stencilWriteMask		 = value; } }

		StencilOp		FrontFaceFailOp				{ get { return frontFailOp				; } set { pipelineState.MakeDirty(); frontFailOp			 = value; } }
		StencilOp		FrontFaceDepthFailOp		{ get { return frontDepthFailOp			; } set { pipelineState.MakeDirty(); frontDepthFailOp		 = value; } }
		StencilOp		FrontFacePassOp				{ get { return frontPassOp				; } set { pipelineState.MakeDirty(); frontPassOp			 = value; } }
		ComparisonFunc	FrontFaceStencilComparison	{ get { return frontStencilComparison	; } set { pipelineState.MakeDirty(); frontStencilComparison  = value; } }
		
		StencilOp		BackFaceFailOp				{ get { return backFailOp				; } set { pipelineState.MakeDirty(); backFailOp				 = value; } }
		StencilOp		BackFaceDepthFailOp			{ get { return backDepthFailOp			; } set { pipelineState.MakeDirty(); backDepthFailOp		 = value; } }
		StencilOp		BackFacePassOp				{ get { return backPassOp				; } set { pipelineState.MakeDirty(); backPassOp				 = value; } }
		ComparisonFunc	BackFaceStencilComparison	{ get { return backStencilComparison	; } set { pipelineState.MakeDirty(); backStencilComparison	 = value; } }

		int				StencilReference			{ get { return stencilReference			; } set { pipelineState.MakeDirty(); stencilReference = value; } }


		bool			depthEnabled			=	false;
		bool			depthWriteEnabled		=	true;
		ComparisonFunc	depthComparison			=	ComparisonFunc.LessEqual;

		bool			stencilEnabled			=	false;
		byte			stencilReadMask			=	0xFF;
		byte			stencilWriteMask		=	0xFF;

		StencilOp		frontFailOp				=	StencilOp.Keep;
		StencilOp		frontDepthFailOp		=	StencilOp.Keep;
		StencilOp		frontPassOp				=	StencilOp.Keep;
		ComparisonFunc	frontStencilComparison	=	ComparisonFunc.Always;

		StencilOp		backFailOp				=	StencilOp.Keep;
		StencilOp		backDepthFailOp			=	StencilOp.Keep;
		StencilOp		backPassOp				=	StencilOp.Keep;
		ComparisonFunc	backStencilComparison	=	ComparisonFunc.Always;

		int				stencilReference		=	0;

	}
}
