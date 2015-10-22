using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;

namespace Fusion.Drivers.Graphics {

	public sealed class ShaderBytecode {

		byte[] bytecode;

		/// <summary>
		/// Initializes a new instance of the ShaderBytecode class
		/// </summary>
		/// <param name="bytecode">Bytecode</param>
		public ShaderBytecode ( byte[] bytecode )
		{
			this.bytecode	=	bytecode.ToArray();
		}


		/// <summary>
		/// Initializes a new instance of the ShaderBytecode class
		/// </summary>
		/// <param name="bytecode">Hex string</param>
		public ShaderBytecode ( string bytecode )
		{
			this.bytecode	=	Misc.HexStringToByte( bytecode );
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
