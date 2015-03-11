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
	/// Pipeline state represents all GPU states as single object.
	/// </summary>
	public class PipelineState {

		public int SamplersCount { get { return CommonShaderStage.SamplerRegisterCount; } }

		/// <summary>
		/// Blend description.
		/// </summary>
		public BlendDescription Blend { get; set; }

		/// <summary>
		/// DepthStencul description.
		/// </summary>
		public DepthStencilDescription DepthStencil { get; set; }

		/// <summary>
		/// Rasterizer state description.
		/// </summary>
		public RasterizerDescription Rasterizer { get; set; }

		/// <summary>
		/// Sampler state descriptions.
		/// </summary>
		public SamplerState[] Samplers { get; set; }

		/// <summary>
		/// Pixel shader.
		/// </summary>
		public ShaderBytecode PixelShader { get; set; }

		/// <summary>
		/// Vertex Shader.
		/// </summary>
		public ShaderBytecode VertexShader { get; set; }

		/// <summary>
		/// Geometry Shader.
		/// </summary>
		public ShaderBytecode GeometryShader { get; set; }

		/// <summary>
		/// Hull Shader.
		/// </summary>
		public ShaderBytecode HullShader { get; set; }

		/// <summary>
		/// Domain Shader.
		/// </summary>
		public ShaderBytecode DomainShader { get; set; }

		/// <summary>
		/// Compute Shader
		/// </summary>
		public ShaderBytecode ComputeShader { get; set; }

		/// <summary>
		/// Vertex input elements
		/// </summary>
		public VertexInputElement[] VertexInputElements	{ 
			get; 
			set; 
		}

		/// <summary>
		/// Vertex output elements
		/// </summary>
		public VertexOutputElement[] VertexOutputElements	{ 
			get; 
			set; 
		}



		/// <summary>
		/// 
		/// </summary>
		internal void MakeDirty ()
		{
		}



		/// <summary>
		/// 
		/// </summary>
		public void ApplyChanges ()
		{
		}
	}
}
