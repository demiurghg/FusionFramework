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
	public class SamplerDescription {

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


		public Filter			Filter			{ get { return filter		 ; } set { filter		  = value; } }
		public AddressMode		AddressU		{ get { return addressU		 ; } set { addressU		  = value; } }
		public AddressMode		AddressV		{ get { return addressV		 ; } set { addressV		  = value; } }
		public AddressMode		AddressW		{ get { return addressW		 ; } set { addressW		  = value; } }
		public int				MaxAnisotropy	{ get { return maxAnisotropy ; } set { maxAnisotropy  = value; } }
		public int				MaxMipLevel		{ get { return maxMipLevel	 ; } set { maxMipLevel	  = value; } }
		public int				MinMipLevel		{ get { return minMipLevel	 ; } set { minMipLevel	  = value; } }
		public float			MipMapBias		{ get { return mipMapBias	 ; } set { mipMapBias	  = value; } }
		public Color4			BorderColor		{ get { return borderColor	 ; } set { borderColor	  = value; } }
		public ComparisonFunc	ComparisonFunc	{ get { return compareFunc	 ; } set { compareFunc	  = value; } }

		Filter		filter			=	Filter.MinMagMipPoint;
		AddressMode	addressU		=	AddressMode.Wrap;
		AddressMode	addressV		=	AddressMode.Wrap;
		AddressMode	addressW		=	AddressMode.Wrap;
		int			maxAnisotropy	=	4;
		int			maxMipLevel		=	int.MaxValue;
		int			minMipLevel		=	0;
		float		mipMapBias		=	0;
		Color4		borderColor		=	new Color4(0,0,0,0);
		ComparisonFunc	compareFunc		=	ComparisonFunc.Always;

	}
}
