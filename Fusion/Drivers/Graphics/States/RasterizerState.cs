using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;


namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// Describes rasterizer state.
	/// </summary>
	public sealed class RasterizerState {

		public CullMode	CullMode			{ get; set; }
		public int		DepthBias			{ get; set; }
		public float	SlopeDepthBias		{ get; set; }
		public bool		MsaaEnabled			{ get; set; }
		public FillMode	FillMode			{ get; set; }
		public bool		DepthClipEnabled	{ get; set; }
		public bool		ScissorEnabled		{ get; set; }


		public static RasterizerState CullNone	{ get; private set; }
		public static RasterizerState CullCW	{ get; private set; }
		public static RasterizerState CullCCW	{ get; private set; }
		public static RasterizerState Wireframe	{ get; private set; }


		/// <summary>
		/// Initializes a new instance of the rasterizer class. 
		/// </summary>
		public RasterizerState () {
			CullMode			=	CullMode.CullNone;
			DepthBias			=	0;
			SlopeDepthBias		=	0;
			MsaaEnabled			=	true;
			FillMode			=	FillMode.Solid;
			DepthClipEnabled	=	true;
			ScissorEnabled		=	false;
		}



		static RasterizerState ()
		{
			CullNone	=	Create( CullMode.CullNone ); 
			CullCW		=	Create( CullMode.CullCW ); 
			CullCCW		=	Create( CullMode.CullCCW ); 
			Wireframe	=	Create( CullMode.CullNone, FillMode.Wireframe ); 
		}


		/// <summary>
		/// Creates a new instance of the rasterizer state.
		/// </summary>
		/// <param name="cullMode"></param>
		/// <param name="fillMode"></param>
		/// <param name="depthBias"></param>
		/// <param name="slopeDepthBias"></param>
		/// <returns></returns>
		public static RasterizerState Create ( CullMode cullMode, FillMode fillMode = FillMode.Solid, int depthBias = 0, float slopeDepthBias = 0 )
		{
			var rs = new RasterizerState();
			rs.CullMode			=	cullMode;
			rs.DepthBias		=	depthBias;
			rs.SlopeDepthBias	=	slopeDepthBias;
			rs.MsaaEnabled		=	true;
			rs.FillMode			=	fillMode;
			rs.DepthClipEnabled	=	true;
			rs.ScissorEnabled	=	false;
			return rs;
		}
	}
}
