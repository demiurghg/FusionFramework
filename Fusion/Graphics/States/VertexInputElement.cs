using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using SharpDX.DXGI;
using SharpDX.Windows;
using System.Runtime.InteropServices;
using Fusion.Mathematics;


namespace Fusion.Graphics {
	public struct VertexInputElement {

		public static int AppendAligned { get { return -1; } }

		/// <summary>
		/// The HLSL semantic associated with this element in a shader input-signature.
		/// </summary>
		public string		SemanticName;

		/// <summary>
		/// The semantic index for the element. 
		/// A semantic index modifies a semantic, with an integer index number. 
		/// A semantic index is only needed in a case where there is more than one element with the same semantic. 
		/// For example, a 4x4 matrix would have four components each with the semantic name "Matrix", 
		/// however each of the four component would have different semantic indices (0, 1, 2, and 3
		/// </summary>
		public int			SemanticIndex;

		/// <summary>
		/// The data type of the element data. 
		/// </summary>
		public VertexFormat	Format;

		/// <summary>
		/// An integer value that identifies the input-assembler (see input slot). Valid values are between 0 and 15.
		/// </summary>
		public int			InputSlot;

		/// <summary>
		/// Optional. Offset (in bytes) between each element. 
		/// Use VertexInputElement.AppendAligned for convenience to define the current element directly after the previous one, 
		/// including any packing if necessary.
		/// </summary>
		public int			ByteOffset;

		/// <summary>
		/// The number of instances to draw using the same per-instance data before advancing in the buffer by one element. 
		/// This value must be 0 for an element that contains per-vertex data.
		/// </summary>
		public int			InstanceStepRate;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="index"></param>
		/// <param name="format"></param>
		/// <param name="slot"></param>
		/// <param name="offset"></param>
		/// <param name="instanceStepRate"></param>
		public VertexInputElement( string name, int index, VertexFormat format, int slot, int offset = -1, int instanceStepRate = 0 )
		{
			SemanticName		=	name;
			SemanticIndex		=	index;
			Format				=	format;
			InputSlot			=	slot;
			ByteOffset			=	offset;
			InstanceStepRate	=	instanceStepRate;
		}
	}
}
