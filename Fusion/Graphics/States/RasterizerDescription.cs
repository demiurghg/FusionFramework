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
using D3DCullMode			=	SharpDX.Direct3D11.CullMode;


namespace Fusion.Graphics {
	public class RasterizerDescription {

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


		public CullMode	CullMode			{ get { return cullMode			; } set { cullMode			=	value; } }
		public int		DepthBias			{ get { return depthBias		; } set { depthBias			=	value; } }
		public float	SlopeDepthBias		{ get { return slopeDepthBias	; } set { slopeDepthBias	=	value; } }
		public bool		MsaaEnabled			{ get { return msaaEnabled		; } set { msaaEnabled		=	value; } }
		public FillMode	FillMode			{ get { return fillMode			; } set { fillMode			=	value; } }
		public bool		DepthClipEnabled	{ get { return depthClipEnabled	; } set { depthClipEnabled	=	value; } }
		public bool		ScissorEnabled		{ get { return scissorEnabled	; } set { scissorEnabled	=	value; } }

		CullMode	cullMode			=	CullMode.CullNone;
		int			depthBias			=	0;
		float		slopeDepthBias		=	0;
		bool		msaaEnabled			=	true;
		FillMode	fillMode			=	FillMode.Solid;
		bool		depthClipEnabled	=	true;
		bool		scissorEnabled		=	false;
	}
}
