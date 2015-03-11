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

	/// <summary>
	/// Descripbes sampler state.
	/// </summary>
	public class SamplerDescription {

		public Filter			Filter			{ get; set; }
		public AddressMode		AddressU		{ get; set; }
		public AddressMode		AddressV		{ get; set; }
		public AddressMode		AddressW		{ get; set; }
		public int				MaxAnisotropy	{ get; set; }
		public int				MaxMipLevel		{ get; set; }
		public int				MinMipLevel		{ get; set; }
		public float			MipMapBias		{ get; set; }
		public Color4			BorderColor		{ get; set; }
		public ComparisonFunc	ComparisonFunc	{ get; set; }


		public SamplerDescription () {
			Filter			=	Filter.MinMagMipPoint;
			AddressU		=	AddressMode.Wrap;
			AddressV		=	AddressMode.Wrap;
			AddressW		=	AddressMode.Wrap;
			MaxAnisotropy	=	4;
			MaxMipLevel		=	int.MaxValue;
			MinMipLevel		=	0;
			MipMapBias		=	0;
			BorderColor		=	new Color4(0,0,0,0);
			ComparisonFunc	=	ComparisonFunc.Always;
		}

	}
}
