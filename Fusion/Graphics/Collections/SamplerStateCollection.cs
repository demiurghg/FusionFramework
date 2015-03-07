using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;

namespace Fusion.Graphics {

	/// <summary>
	/// The sampler state collection.
	/// </summary>
	public sealed class SamplerStateCollection {

		readonly GraphicsDevice			device;
		readonly D3D11.SamplerState[]	samplers;
		readonly SamplerState[]			states;	
		readonly CommonShaderStage		stage;

		internal int DirtyRegs	=	0;


		/// <summary>
		/// Creates instance of sampler state collection.
		/// </summary>
		/// <param name="device"></param>
		internal SamplerStateCollection ( GraphicsDevice device, CommonShaderStage stage )
		{
			states		=	new SamplerState[ Count ];
			samplers	=	new D3D11.SamplerState[ Count ];
			this.device	=	device;
			this.stage	=	stage;
			DirtyRegs	=	Count;
		}


		
		/// <summary>
		/// Total count of sampler states that can be simultaniously bound to pipeline.
		/// </summary>
		public int Count { 
			get { 
				return CommonShaderStage.SamplerRegisterCount;
			}
		}



		/// <summary>
		/// Clears sampler states.
		/// </summary>
		public void Clear ()
		{
			for (int i=0; i<Count; i++) {
				this[i] = null;
			}
		}


		
		/// <summary>
		/// Sets and gets sampler state to given shader stage.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public SamplerState this[int index] {
			set {
				states[ index ] = value;
				#if DEFERRED
					DirtyRegs		=	Math.Max( DirtyRegs, index + 1 );
					samplers[ index ] = (value==null) ? null : value.Apply(device);
				#else
					stage.SetSampler( index, (value==null) ? null : value.Apply(device) );
				#endif
			}
			get {
				return states[ index ];
			}
		}



		/// <summary>
		/// 
		/// </summary>
		internal void Apply ()
		{
			#if DEFERRED
				stage.SetSamplers( 0, DirtyRegs, samplers );
				DirtyRegs = 0;
			#endif
		}
	}
}
