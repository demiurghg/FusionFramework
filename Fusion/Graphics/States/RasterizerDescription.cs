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
	public class RasterizerDescription {

		public CullMode	CullMode			{ get; set; }
		public int		DepthBias			{ get; set; }
		public float	SlopeDepthBias		{ get; set; }
		public bool		MsaaEnabled			{ get; set; }
		public FillMode	FillMode			{ get; set; }
		public bool		DepthClipEnabled	{ get; set; }
		public bool		ScissorEnabled		{ get; set; }


		/// <summary>
		/// 
		/// </summary>
		public RasterizerDescription () {
			CullMode			=	CullMode.CullNone;
			DepthBias			=	0;
			SlopeDepthBias		=	0;
			MsaaEnabled			=	true;
			FillMode			=	FillMode.Solid;
			DepthClipEnabled	=	true;
			ScissorEnabled		=	false;
		}
	}
}
