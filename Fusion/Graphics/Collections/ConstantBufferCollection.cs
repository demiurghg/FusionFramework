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
	/// The constant buffer collection.
	/// </summary>
	public sealed class ConstantBufferCollection {

		readonly GraphicsDevice		device;
		readonly ConstantBuffer[]	buffers;
		readonly D3D11.Buffer[]		cbs;	
		readonly CommonShaderStage	stage;

		internal int DirtyRegs = 0;

		/// <summary>
		/// Creates instance of sampler state collection.
		/// </summary>
		/// <param name="device"></param>
		internal ConstantBufferCollection ( GraphicsDevice device, CommonShaderStage stage )
		{
			buffers		=	new ConstantBuffer[ Count ];
			cbs			=	new D3D11.Buffer[ Count ];
			this.device	=	device;
			this.stage	=	stage;
			DirtyRegs	=	Count;
		}


		
		/// <summary>
		/// Total count of sampler states that can be simultaniously bound to pipeline.
		/// </summary>
		public int Count { 
			get { 
				return CommonShaderStage.ConstantBufferApiSlotCount;// ConstantBufferRegisterCount;
			}
		}




		/// <summary>
		/// Unbinds all constant buffers.
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
				buffers[ index ] =	value;
				#if DEFERRED
					DirtyRegs	 =	Math.Max( DirtyRegs, index + 1 );
					cbs[ index ] =	(value==null) ? null : value.buffer;
				#else
					stage.SetConstantBuffer( index, (value==null) ? null : value.buffer );
				#endif
			}
			get {
				return buffers[ index ];
			}
		}



		/// <summary>
		/// 
		/// </summary>
		internal void Apply ()
		{
			#if DEFERRED
				stage.SetConstantBuffers( 0, DirtyRegs, cbs );
				DirtyRegs = 0;
			#endif
		}
	}
}
