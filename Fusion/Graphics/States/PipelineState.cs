using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Mathematics;

namespace Fusion.Graphics {

	public class VertexOuputElement {
	}

	public class VertexInputElement {
	}

	public class RasterizerStateDesc {
	}

	public class SamplerStateDesc {
	}

	public class DepthStencilStateDesc {
	}

	public class BlendingStateDesc {
	}



	/// <summary>
	/// 
	/// </summary>
	public class PipelineState : DisposableBase {


		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

			}

			base.Dispose( disposing );
		} 
		


		/// <summary>
		/// 
		/// </summary>
		/// <param name="cullMode"></param>
		/// <param name="depthBias"></param>
		/// <param name="slopeDepthBias"></param>
		/// <param name="msaaEnables"></param>
		/// <param name="fillMode"></param>
		/// <param name="depthClipEnable"></param>
		/// <param name="scissorEnable"></param>
		public void SetupRasterizer ( 
			CullMode	cullMode	, 
			int			depthBias	, 
			float		slopeDepthBias,
			bool		msaaEnables	, 
			FillMode	fillMode	, 
			bool		depthClipEnable, 
			bool		scissorEnable )
		{
		}
		

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sampler"></param>
		/// <param name="filter"></param>
		/// <param name="addressU"></param>
		/// <param name="addressV"></param>
		/// <param name="addressW"></param>
		/// <param name="maxAnisotropy"></param>
		/// <param name="maxMipLevel"></param>
		/// <param name="minMipLevel"></param>
		/// <param name="mipMapBias"></param>
		/// <param name="borderColor"></param>
		/// <param name="comparisonFunc"></param>
		public void SetupSampler ( 
			int		sampler		, 
			Filter	filter		, 
			AddressMode addressU, 
			AddressMode addressV, 
			AddressMode addressW, 
			int		maxAnisotropy, 
			int		maxMipLevel	, 
			int		minMipLevel	, 
			float	mipMapBias	, 
			Color4	borderColor	, 
			ComparisonFunc comparisonFunc )
		{
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="enabled"></param>
		/// <param name="srcColor"></param>
		/// <param name="dstColor"></param>
		/// <param name="srcAlpha"></param>
		/// <param name="dstAlpha"></param>
		/// <param name="colorOp"></param>
		/// <param name="alphaOp"></param>
		/// <param name="writeMask"></param>
		/// <param name="multiSampleMask"></param>
		/// <param name="blendFactor"></param>
		public void SetupBlending (
			bool	enabled	,	
			Blend	srcColor,	
			Blend	dstColor,	
			Blend	srcAlpha,	
			Blend	dstAlpha,	
			BlendOp colorOp	,	
			BlendOp alphaOp	,	
			ColorChannels writeMask,	
			int		multiSampleMask,
			Color4	blendFactor )
		{
			
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="depthEnabled"></param>
		/// <param name="depthWriteEnabled"></param>
		/// <param name="depthComparison"></param>
		/// <param name="stencilEnabled"></param>
		/// <param name="stencilReadMask"></param>
		/// <param name="stencilWriteMask"></param>
		/// <param name="frontFailOp"></param>
		/// <param name="frontDepthFailOp"></param>
		/// <param name="frontPassOp"></param>
		/// <param name="frontStencilComparison"></param>
		/// <param name="backFailOp"></param>
		/// <param name="backDepthFailOp"></param>
		/// <param name="backPassOp"></param>
		/// <param name="backStencilComparison"></param>
		/// <param name="stencilReference"></param>
		public void SetupDepthStencil (
			bool			depthEnabled	,
			bool			depthWriteEnabled,
			ComparisonFunc	depthComparison	,
			bool			stencilEnabled	,
			byte			stencilReadMask	,
			byte			stencilWriteMask,
			StencilOp		frontFailOp		,
			StencilOp		frontDepthFailOp,
			StencilOp		frontPassOp		,
			ComparisonFunc	frontStencilComparison,
			StencilOp		backFailOp		,
			StencilOp		backDepthFailOp	,
			StencilOp		backPassOp		,
			ComparisonFunc	backStencilComparison,
			int				stencilReference ) 
		{
		}


		/// <summary>
		/// 
		/// </summary>
		VertexOuputElement[] VertexOutputElements { get; set; }

		/// <summary>
		/// 
		/// </summary>
		VertexInputElement[] VertexInputElements { get; set; }

		/// <summary>
		/// 
		/// </summary>
		PixelShader	PixelShader		{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		PixelShader	VertexShader	{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		PixelShader	GeometryShader	{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		PixelShader	HullShader		{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		PixelShader	DomainShader	{ get; set; }

		/// <summary>
		/// 
		/// </summary>
		PixelShader	ComputeShader	{ get; set; }



		/// <summary>
		/// 
		/// </summary>
		internal void Apply ()
		{
		}
	}

}
