using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace Fusion.Graphics {

	/// <summary>
	/// The sampler state collection.
	/// </summary>
	public sealed class SamplerStateCollection {

		readonly GraphicsDevice		device;
		readonly SamplerState[]		states;	
		readonly CommonShaderStage	stage;


		/// <summary>
		/// Creates instance of sampler state collection.
		/// </summary>
		/// <param name="device"></param>
		internal SamplerStateCollection ( GraphicsDevice device, CommonShaderStage stage )
		{
			states		=	new SamplerState[ Count ];
			this.device	=	device;
			this.stage	=	stage;
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
		/// Sets and gets sampler state to given shader stage.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public SamplerState this[int index] {
			set {
				states[ index ] = value;
				stage.SetSampler( index, (value==null) ? null : value.Apply(device) );
			}
			get {
				return states[ index ];
			}
		}
	}
}
