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
		internal BlendDescription ( PipelineState pipelineState )
		{
			if (pipelineState==null) {
				throw new ArgumentNullException("pipelineState");
			}
			this.pipelineState = pipelineState;
		}


		bool			DepthEnabled				{ get { return depthEnabled				; } set { depthEnabled			 = value; } }
		bool			DepthWriteEnabled			{ get { return depthWriteEnabled		; } set { depthWriteEnabled		 = value; } }
		ComparisonFunc	DepthComparison				{ get { return depthComparison			; } set { depthComparison		 = value; } }
		
		bool			StencilEnabled				{ get { return stencilEnabled			; } set { stencilEnabled		 = value; } }
		byte			StencilReadMask				{ get { return stencilReadMask			; } set { stencilReadMask		 = value; } }
		byte			StencilWriteMask			{ get { return stencilWriteMask			; } set { stencilWriteMask		 = value; } }

		StencilOp		FrontFaceFailOp				{ get { return frontFailOp				; } set { frontFailOp			 = value; } }
		StencilOp		FrontFaceDepthFailOp		{ get { return frontDepthFailOp			; } set { frontDepthFailOp		 = value; } }
		StencilOp		FrontFacePassOp				{ get { return frontPassOp				; } set { frontPassOp			 = value; } }
		ComparisonFunc	FrontFaceStencilComparison	{ get { return frontStencilComparison	; } set { frontStencilComparison = value; } }
		
		StencilOp		BackFaceFailOp				{ get { return backFailOp				; } set { backFailOp			 = value; } }
		StencilOp		BackFaceDepthFailOp			{ get { return backDepthFailOp			; } set { backDepthFailOp		 = value; } }
		StencilOp		BackFacePassOp				{ get { return backPassOp				; } set { backPassOp			 = value; } }
		ComparisonFunc	BackFaceStencilComparison	{ get { return backStencilComparison	; } set { backStencilComparison	 = value; } }

		int				StencilReference			{ get { return stencilReference			; } set { stencilReference = value; } }


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
