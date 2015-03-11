using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Graphics {

	public class ShaderBytecode {

		byte[] bytecode;

		/// <summary>
		/// 
		/// </summary>
		public ShaderBytecode ( byte[] bytecode )
		{
			this.bytecode	=	bytecode.ToArray();
		}


		/// <summary>
		/// Gets bytecode bytes.
		/// </summary>
		public byte[] Bytecode {
			get {
				return bytecode;
			}
		}


		public bool IsPixelShader	 { get { return true; } }
		public bool IsVertexShader	 { get { return true; } }
		public bool IsGeometryShader { get { return true; } }
		public bool IsHullShader	 { get { return true; } }
		public bool IsDomainShader	 { get { return true; } }
		public bool IsComputeShader	 { get { return true; } }
	}
}
