using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;


namespace Fusion.Graphics {

	/// <summary>
	/// Describes rasterizer state.
	/// </summary>
	public class DepthStencilDescription {

		public bool				DepthEnabled				{ get; set; }
		public bool				DepthWriteEnabled			{ get; set; }
		public ComparisonFunc	DepthComparison				{ get; set; }
																
		public bool				StencilEnabled				{ get; set; }
		public byte				StencilReadMask				{ get; set; }
		public byte				StencilWriteMask			{ get; set; }

		public StencilOp		FrontFaceFailOp				{ get; set; }
		public StencilOp		FrontFaceDepthFailOp		{ get; set; }
		public StencilOp		FrontFacePassOp				{ get; set; }
		public ComparisonFunc	FrontFaceStencilComparison	{ get; set; }
		
		public StencilOp		BackFaceFailOp				{ get; set; }
		public StencilOp		BackFaceDepthFailOp			{ get; set; }
		public StencilOp		BackFacePassOp				{ get; set; }
		public ComparisonFunc	BackFaceStencilComparison	{ get; set; }

		public int				StencilReference			{ get; set; }


		/// <summary>
		/// 
		/// </summary>
		public DepthStencilDescription() 
		{
			DepthEnabled				=	false;
			DepthWriteEnabled			=	true;
			DepthComparison				=	ComparisonFunc.LessEqual;
										
			StencilEnabled				=	false;
			StencilReadMask				=	0xFF;
			StencilWriteMask			=	0xFF;

			FrontFaceFailOp				=	StencilOp.Keep;
			FrontFaceDepthFailOp		=	StencilOp.Keep;
			FrontFacePassOp				=	StencilOp.Keep;
			FrontFaceStencilComparison	=	ComparisonFunc.Always;

			BackFaceFailOp				=	StencilOp.Keep;
			BackFaceDepthFailOp			=	StencilOp.Keep;
			BackFacePassOp				=	StencilOp.Keep;
			BackFaceStencilComparison	=	ComparisonFunc.Always;

			StencilReference			=	0;
		}
	}
}
