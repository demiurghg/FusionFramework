using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Fusion.Mathematics;
		 

namespace Fusion.Graphics {

	/// <summary>
	/// Describes blending state.
	/// </summary>
	public class BlendDescription {

		public	Blend			SrcColor		{ get; set; }
		public	Blend			DstColor		{ get; set; }
		public	Blend			SrcAlpha		{ get; set; }
		public	Blend			DstAlpha		{ get; set; }
		public	BlendOp			ColorOp			{ get; set; }
		public	BlendOp			AlphaOp			{ get; set; }
		public	ColorChannels	WriteMask		{ get; set; }
		public	int				MultiSampleMask	{ get; set; }
		public	Color4			BlendFactor		{ get; set; }


		public BlendDescription() {
			SrcColor		=	Blend.One;
			DstColor		=	Blend.Zero;
			SrcAlpha		=	Blend.One;
			DstAlpha		=	Blend.Zero;
			ColorOp			=	BlendOp.Add;
			AlphaOp			=	BlendOp.Add;	
			WriteMask		=	ColorChannels.All;
			MultiSampleMask	=	-1;
			BlendFactor		=	new Color4(0,0,0,0);
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="mask"></param>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		/// <param name="srcA"></param>
		/// <param name="dstA"></param>
		public void Set ( ColorChannels mask=ColorChannels.All, Blend src = Blend.One, Blend dst = Blend.Zero, Blend srcA = Blend.One, Blend dstA = Blend.Zero )
		{
			SrcColor		=	src;
			DstColor		=	dst;
			SrcAlpha		=	srcA;
			DstAlpha		=	dstA;
			ColorOp			=	BlendOp.Add;
			AlphaOp			=	BlendOp.Add;
			WriteMask		=	mask;
			MultiSampleMask	=	-1;
			BlendFactor		=	new Color4(0,0,0,0);
		}



		public void SetOpaque			 () { Set( ); }
		public void SetNoWrite			 () { Set( ColorChannels.None ); }
		public void SetAlphaBlend		 () { Set( ColorChannels.All, Blend.SrcAlpha,		Blend.InvSrcAlpha	);						  }
		public void SetAlphaBlendPreMul  () { Set( ColorChannels.All, Blend.One,			Blend.InvSrcAlpha	);						  }
		public void SetAdditive		 	 () { Set( ColorChannels.All, Blend.One,			Blend.One,			Blend.One, Blend.One );	  }
		public void SetScreen			 () { Set( ColorChannels.All, Blend.InvDstColor,	Blend.One			);						  }
		public void SetMultiply		 	 () { Set( ColorChannels.All, Blend.Zero,			Blend.SrcColor		);						  }
		public void SetNegMultiply		 () { Set( ColorChannels.All, Blend.Zero,			Blend.InvSrcColor	);						  }
	}
}
