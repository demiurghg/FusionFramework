using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// The constant buffer collection.
	/// </summary>
	public sealed class ConstantBufferCollection : GraphicsResource {

		readonly ConstantBuffer[]	buffers;	
		readonly CommonShaderStage	stage;


		/// <summary>
		/// Creates instance of sampler state collection.
		/// </summary>
		/// <param name="device"></param>
		internal ConstantBufferCollection ( GraphicsDevice device, CommonShaderStage stage ) : base(device)
		{
			buffers		=	new ConstantBuffer[ Count ];
			this.stage	=	stage;
		}


		
		/// <summary>
		/// Total count of sampler states that can be simultaniously bound to pipeline.
		/// </summary>
		public int Count { 
			get { 
				return CommonShaderStage.ConstantBufferApiSlotCount;
			}
		}


		
		/// <summary>
		/// Clears collection
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
		public ConstantBuffer this[int index] {
			set {
				buffers[ index ] = value;
				stage.SetConstantBuffer( index, (value==null) ? null : value.buffer );
			}
			get {
				return buffers[ index ];
			}
		}
	}
}
