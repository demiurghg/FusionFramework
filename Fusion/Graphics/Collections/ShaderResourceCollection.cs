using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace Fusion.Graphics {

	/// <summary>
	/// The texture collection.
	/// </summary>
	public sealed class ShaderResourceCollection {

		readonly ShaderResource[]		resources;
		readonly ShaderResourceView[]	srvs;	
		readonly CommonShaderStage		stage;
		readonly GraphicsDevice			device;

		internal int DirtyRegs = 0;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		internal ShaderResourceCollection ( GraphicsDevice device, CommonShaderStage stage )
		{
			resources	=	new ShaderResource[ Count ];
			srvs		=	new ShaderResourceView[ Count ];
			this.stage	=	stage;
			this.device	=	device;
			DirtyRegs	=	Count;
		}



		/// <summary>
		/// Total count of sampler states that can be simultaniously bound to pipeline.
		/// </summary>
		public int Count { 
			get { 
				return CommonShaderStage.InputResourceRegisterCount;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public void Clear ()
		{
			for (int i=0; i<Count; i++) {
				resources[i] = null;
				srvs[i]		 = null;
			}
			DirtyRegs = Count;
		}

		

		
		/// <summary>
		/// Sets and gets shader resources bound to given shader stage.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public ShaderResource this[int index] {
			set {
				/*if (index>Count) {	
					throw new ArgumentOutOfRangeException("index", index, "must be less than " + Count.ToString() );
				} */

				resources[ index ]	=	value;

				#if DEFERRED
					srvs[ index ]	=	(value == null) ? null : value.SRV;
					DirtyRegs		=	Math.Max( DirtyRegs, index + 1 );
				#else
					stage.SetShaderResources( index, (value == null) ? null : value.SRV );
				#endif
			}
			get {
				return resources[ index ];
			}
		}



		/// <summary>
		/// 
		/// </summary>
		internal void Apply ()
		{
			#if DEFERRED
				stage.SetShaderResources( 0, DirtyRegs, srvs );
				DirtyRegs = 0;
			#endif
		}
	}
}
