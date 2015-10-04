using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// The texture collection.
	/// </summary>
	public sealed class ShaderResourceCollection : GraphicsResource {

		readonly ShaderResource[]	resources;	
		readonly CommonShaderStage	stage;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		internal ShaderResourceCollection ( GraphicsDevice device, CommonShaderStage stage ) : base(device)
		{
			resources	=	new ShaderResource[ Count ];
			this.stage	=	stage;
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
		/// Clears collection
		/// </summary>
		public void Clear ()
		{
			for (int i=0; i<Count; i++) {
				this[i] = null;
			}
		}
	

		
		/// <summary>
		/// Sets and gets shader resources bound to given shader stage.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public ShaderResource this[int index] {
			set {
				resources[ index ] = value;
				stage.SetShaderResource( index, (value==null) ? null : value.SRV );
			}
			get {
				return resources[ index ];
			}
		}
	}
}
