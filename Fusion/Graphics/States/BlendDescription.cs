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


namespace Fusion.Graphics {
	public class BlendDescription {

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


		public	Blend			SrcColor		{ get { return srcColor			; } set { srcColor			= value; } }
		public	Blend			DstColor		{ get { return dstColor			; } set { dstColor			= value; } }
		public	Blend			SrcAlpha		{ get { return srcAlpha			; } set { srcAlpha			= value; } }
		public	Blend			DstAlpha		{ get { return dstAlpha			; } set { dstAlpha			= value; } }
		public	BlendOp			ColorOp			{ get { return colorOp			; } set { colorOp			= value; } }
		public	BlendOp			AlphaOp			{ get { return alphaOp			; } set { alphaOp			= value; } }
		public	ColorChannels	WriteMask		{ get { return writeMask		; } set { writeMask			= value; } }
		public	int				MultiSampleMask	{ get { return multiSampleMask	; } set { multiSampleMask	= value; } }
		public	Color4			BlendFactor		{ get { return blendFactor		; } set { blendFactor		= value; } }

		Blend			srcColor		=	Blend.One;
		Blend			dstColor		=	Blend.Zero;
		Blend			srcAlpha		=	Blend.One;
		Blend			dstAlpha		=	Blend.Zero;
		BlendOp			colorOp			=	BlendOp.Add;
		BlendOp			alphaOp			=	BlendOp.Add;	
		ColorChannels	writeMask		=	ColorChannels.All;
		int				multiSampleMask	=	-1;
		Color4			blendFactor		=	new Color4(0,0,0,0);
	}
}
