using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace Fusion.Graphics {
	#if false
	public sealed class ResourceCollection {

		bool	dirty	=	true;
		
		ShaderResource[]	resources;	


		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		internal ResourceCollection ( GraphicsDevice device )
		{
			resources	=	ShaderResource[CommonShaderStage.InputResourceRegisterCount];
			
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="stage"></param>
		internal void Apply ( CommonShaderStage stage )
		{
			if (dirty) {
				
				stage.Set

			}
		}
	}
	#endif
}
