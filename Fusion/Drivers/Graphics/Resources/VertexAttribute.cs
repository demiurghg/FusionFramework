using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Fusion.Drivers.Graphics {

	/// <summary>
	/// A description of a single element for the input-assembler stage.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class VertexAttribute : Attribute {

		public const string	Position        =	"POSITION"		;
		public const string	Color           =	"COLOR"			;
		public const string	TexCoord        =	"TEXCOORD"		;
		public const string	Tangent         =	"TANGENT"		;
		public const string	Binormal        =	"BINORMAL"		;
		public const string	Normal          =	"NORMAL"		;
		public const string	BlendWeights    =	"BLENDWEIGHTS"	;
		public const string	BlendIndices    =	"BLENDINDICES"	;

		/// <summary>
		/// The HLSL semantic associated with this element in a shader input-signature.
		/// </summary>
		public readonly string	Name;

		/// <summary>
		/// The semantic index for the element. A semantic index modifies a semantic, 
		/// with an integer index number. A semantic index is only needed in a case 
		/// where there is more than one element with the same semantic.
		/// </summary>
		public readonly int		Index;

		/// <summary>
		/// An integer value that identifies the input-assembler (see input slot). 
		/// Valid values are between 0 and 15.
		/// </summary>
		public readonly int		InputSlot;

		/// <summary>
		/// The number of instances to draw using the same per-instance data 
		/// before advancing in the buffer by one element. 
		/// Zero means per-vertex data.
		/// </summary>
		public readonly int		InstanceStepRate;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">The HLSL semantic associated with this element in a shader input-signature.</param>
		/// <param name="index">The semantic index for the element. A semantic index modifies a semantic, with an integer index number. A semantic index is only needed in a case where there is more than one element with the same semantic.</param>
		/// <param name="inputSlot">An integer value that identifies the input-assembler (see input slot). Valid values are between 0 and 15.</param>
		/// <param name="instanceStepRate">The number of instances to draw using the same per-instance data before advancing in the buffer by one element. Zero means pervertex data.</param>
		public VertexAttribute ( string name, int index = 0, int inputSlot = 0, int instanceStepRate = 0 )
		{
			if (name.Length>64) {
				throw new ArgumentException("'name.Length' must be < 64");
			}

			if (inputSlot<0 || inputSlot>15) {
				throw new ArgumentException("'inputSlot' must be within range [0..15]");
			}

			Name				=	name;
			Index				=	index;
			InputSlot			=	inputSlot;
			InstanceStepRate	=	instanceStepRate;
		}
	}
}
