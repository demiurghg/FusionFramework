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
	}
}
