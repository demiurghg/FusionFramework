using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Fusion.Core.Mathematics;
		 

namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// Describes blending state.
	/// </summary>
	public sealed class BlendState {

		public	Blend			SrcColor		{ get; set; }
		public	Blend			DstColor		{ get; set; }
		public	Blend			SrcAlpha		{ get; set; }
		public	Blend			DstAlpha		{ get; set; }
		public	BlendOp			ColorOp			{ get; set; }
		public	BlendOp			AlphaOp			{ get; set; }
		public	ColorChannels	WriteMask		{ get; set; }
		public	int				MultiSampleMask	{ get; set; }
		public	Color4			BlendFactor		{ get; set; }


		public static  BlendState	Opaque			 { get; private set; }
		public static  BlendState	NoWrite			 { get; private set; }
		public static  BlendState	AlphaBlend		 { get; private set; }
		public static  BlendState	AlphaBlendPremul { get; private set; }
		public static  BlendState	AlphaMaskWrite	 { get; private set; }
		public static  BlendState	Additive		 { get; private set; }
		public static  BlendState	Screen			 { get; private set; }
		public static  BlendState	Multiply		 { get; private set; }
		public static  BlendState	NegMultiply		 { get; private set; }
		public static  BlendState	ClearAlpha		 { get; private set; }



		static BlendState()
		{
			Opaque				=	Create();
			NoWrite				=	Create( ColorChannels.None ); 
			AlphaBlend			=	Create( ColorChannels.All, Blend.SrcAlpha,		Blend.InvSrcAlpha	);
			AlphaBlendPremul	=	Create( ColorChannels.All, Blend.One,			Blend.InvSrcAlpha	);						
			AlphaMaskWrite		=	Create( ColorChannels.Alpha);
			Additive			=	Create( ColorChannels.All, Blend.One,			Blend.One,			Blend.One, Blend.One );	
			Screen				=	Create( ColorChannels.All, Blend.InvDstColor,	Blend.One			);						
			Multiply			=	Create( ColorChannels.All, Blend.Zero,			Blend.SrcColor		);						
			NegMultiply			=	Create( ColorChannels.All, Blend.Zero,			Blend.InvSrcColor	);
			ClearAlpha			=	Create(ColorChannels.Alpha, Blend.SrcAlpha, Blend.InvSrcAlpha, Blend.One, Blend.Zero);
		}



		public BlendState() {
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
		/// Creates new instance of blend state
		/// </summary>
		/// <param name="mask"></param>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		/// <param name="srcA"></param>
		/// <param name="dstA"></param>
		public static BlendState Create ( ColorChannels mask=ColorChannels.All, Blend src = Blend.One, Blend dst = Blend.Zero, Blend srcA = Blend.One, Blend dstA = Blend.Zero )
		{
			BlendState bs		=	new BlendState();
			bs.SrcColor			=	src;
			bs.DstColor			=	dst;
			bs.SrcAlpha			=	srcA;
			bs.DstAlpha			=	dstA;
			bs.ColorOp			=	BlendOp.Add;
			bs.AlphaOp			=	BlendOp.Add;
			bs.WriteMask		=	mask;
			bs.MultiSampleMask	=	-1;
			bs.BlendFactor		=	new Color4(0,0,0,0);
			return bs;
		}
	}
}
