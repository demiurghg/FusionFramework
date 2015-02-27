using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using Fusion.Native.NvApi;
using Device = SharpDX.Direct3D11.Device;
using System.IO;
using Fusion.Graphics;


namespace Fusion.Graphics {

	public partial class GraphicsDevice : DisposableBase {

		#if true
		[Obsolete]
		public VertexBuffer CreateVertexBuffer ( Type vertexType, int capacity )
		{
			return new VertexBuffer( this, vertexType, capacity );
		}

		
		[Obsolete]
		public IndexBuffer CreateIndexBuffer ( int capacity )
		{
			return new IndexBuffer( this, capacity );
		}


		[Obsolete]
		public DynamicTexture CreateDynamicTexture (int width, int height, ColorFormat format = ColorFormat.R32F)
		{
			return new DynamicTexture(this, width, height, format);
		}


		[Obsolete]
		public Texture2D CreateTexture2D ( byte[] fileInMemory )
		{
			return new Texture2D( this, fileInMemory );
		}


		[Obsolete]
		public ConstBufferGeneric<T> CreateConstBuffer<T> () where T: struct
		{
			return new ConstBufferGeneric<T>( this );
		}



		[Obsolete]
		public GenericBuffer CreateGenericBuffer<T>( T type, int capacity, CpuAccessFlags flagsCPU = CpuAccessFlags.None )
		{
			return GenericBuffer.Create( this, type, capacity, flagsCPU );
		}


		/*[Obsolete]
		public GenericBuffer CreateGenericBuffer<T,T2>( T type, T2[] data, CpuAccessFlags flagsCPU = CpuAccessFlags.None ) where T2 : struct
		{
			return GenericBuffer.Create( this, type, data, flagsCPU );
		} */

		[Obsolete]
		public Ubershader CreateUberShader ( string path, Type enumType ) 
		{
			var us = Game.Content.Load<Ubershader>( path );
			us.Map( enumType );
			return us;
		}
		#endif

		[Obsolete]
		public void SetBlendState ( BlendState blendState )
		{
			blendState.Apply( this );
		}


		[Obsolete]
		public void SetRasterizerState ( RasterizerState rasterizerState )
		{
			rasterizerState.Apply( this );
		}


		[Obsolete]
		public void SetDepthStencilState ( DepthStencilState depthStencilState )
		{
			depthStencilState.Apply( this );
		}

		

		[Obsolete] public void SetPSConstant ( int register, ConstantBuffer constBuffer ) { PSConstantBuffers[register] = constBuffer; }
		[Obsolete] public void SetVSConstant ( int register, ConstantBuffer constBuffer ) { VSConstantBuffers[register] = constBuffer; }
		[Obsolete] public void SetGSConstant ( int register, ConstantBuffer constBuffer ) { GSConstantBuffers[register] = constBuffer; }
		[Obsolete] public void SetCSConstant ( int register, ConstantBuffer constBuffer ) { CSConstantBuffers[register] = constBuffer; }
		[Obsolete] public void SetDSConstant ( int register, ConstantBuffer constBuffer ) { DSConstantBuffers[register] = constBuffer; }
		[Obsolete] public void SetHSConstant ( int register, ConstantBuffer constBuffer ) { HSConstantBuffers[register] = constBuffer; }

		[Obsolete] public void SetPSResource ( int register, ShaderResource resource ) { PSShaderResources[register] = resource; }
		[Obsolete] public void SetVSResource ( int register, ShaderResource resource ) { VSShaderResources[register] = resource; }
		[Obsolete] public void SetGSResource ( int register, ShaderResource resource ) { GSShaderResources[register] = resource; }
		[Obsolete] public void SetCSResource ( int register, ShaderResource resource ) { CSShaderResources[register] = resource; }
		[Obsolete] public void SetDSResource ( int register, ShaderResource resource ) { DSShaderResources[register] = resource; }
		[Obsolete] public void SetHSResource ( int register, ShaderResource resource ) { HSShaderResources[register] = resource; }
		
		[Obsolete] public void SetPSSamplerState ( int slot, SamplerState samplerState ) { PSSamplerStates[slot] = samplerState;	}
		[Obsolete] public void SetVSSamplerState ( int slot, SamplerState samplerState ) { VSSamplerStates[slot] = samplerState;	}
		[Obsolete] public void SetGSSamplerState ( int slot, SamplerState samplerState ) { GSSamplerStates[slot] = samplerState;	}
		[Obsolete] public void SetCSSamplerState ( int slot, SamplerState samplerState ) { CSSamplerStates[slot] = samplerState;	}
		[Obsolete] public void SetDSSamplerState ( int slot, SamplerState samplerState ) { DSSamplerStates[slot] = samplerState;	}
		[Obsolete] public void SetHSSamplerState ( int slot, SamplerState samplerState ) { HSSamplerStates[slot] = samplerState;	}
	}
}
