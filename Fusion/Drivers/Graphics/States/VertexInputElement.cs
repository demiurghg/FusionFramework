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
using Fusion.Core;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {
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
		/// Creates a new instance of VertexInputElement class
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



		/// <summary>
		/// Converts array of VertexInputElement to array of InputElement.
		/// </summary>
		/// <param name="elements"></param>
		/// <returns></returns>
		static internal InputElement[] Convert ( VertexInputElement[] elements )
		{
			return	elements.Select( e => new InputElement( 
						e.SemanticName, 
						e.SemanticIndex, 
						Converter.Convert( e.Format ), 
						e.ByteOffset, 
						e.InputSlot, 
						e.InstanceStepRate == 0 ? InputClassification.PerVertexData : InputClassification.PerInstanceData,
						e.InstanceStepRate )
					).ToArray();
		}



		/// <summary>
		/// Returns empty object
		/// </summary>
		public static VertexInputElement[] Empty {
			get {
				return null;
			}
		}



		/// <summary>
		/// Creates an instance of VertexInputElement from structure
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static VertexInputElement[] FromStructure<T> () where T: struct
		{
			return FromStructure( typeof(T) );
		}




		/// <summary>
		/// Creates an instance of VertexInputElement from structure with certain type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static VertexInputElement[] FromStructure ( Type type )
		{
			if (!type.IsStruct()) {
				throw new ArgumentException("Vertex type must be structure. Got: " + type.ToString() );
			}

			var	elements	= type
				.GetFields()
				.Select( fi => FieldToInputElement( type, fi ) )
				.ToArray();

			return elements;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="fieldInfo"></param>
		/// <returns></returns>
		static VertexInputElement FieldToInputElement ( Type type, FieldInfo fieldInfo )
		{
			var fieldType	= fieldInfo.FieldType;
			var attr		= (VertexAttribute)fieldInfo.GetCustomAttributes(true).FirstOrDefault( a => a is VertexAttribute );
			var name		= attr.Name.ToUpper();
			var index		= attr.Index;
			var slot		= attr.InputSlot;
			var offset		= (short)Marshal.OffsetOf( type, fieldInfo.Name );
			var rate		= attr.InstanceStepRate;

			if ( attr==null ) {
				throw new GraphicsException(string.Format("Field {0}.{1} must be declared with [VertexAttribute]", type.Name, fieldInfo.Name));
			}

			VertexFormat	format;

			if ( fieldType == typeof( Single	) )	format	=	VertexFormat.Float		; else 
			if ( fieldType == typeof( Vector2	) )	format	=	VertexFormat.Vector2	; else 
			if ( fieldType == typeof( Vector3	) )	format	=	VertexFormat.Vector3	; else 
			if ( fieldType == typeof( Vector4	) )	format	=	VertexFormat.Vector4	; else 
			if ( fieldType == typeof( Color4	) )	format	=	VertexFormat.Color4		; else 
			if ( fieldType == typeof( Color		) )	format	=	VertexFormat.Color		; else 
			if ( fieldType == typeof( Byte4		) )	format	=	VertexFormat.Byte4		; else 
			if ( fieldType == typeof( Half2		) )	format	=	VertexFormat.Half2		; else 
			if ( fieldType == typeof( Half4		) )	format	=	VertexFormat.Half4		; else 
			if ( fieldType == typeof( UInt32	) )	format	=	VertexFormat.UInt		; else 
			if ( fieldType == typeof( Int32		) )	format	=	VertexFormat.SInt		; else 
			if ( fieldType == typeof( Int3		) )	format	=	VertexFormat.SInt3		; else 
			if ( fieldType == typeof( Int4		) )	format	=	VertexFormat.SInt4		; else 
			if ( fieldType == typeof( Double	) )	format	=	VertexFormat.UInt2		; else 
				throw new GraphicsException(string.Format("Vertex element type {0} is not supported by VertexBuffer", type.ToString()));

			return new VertexInputElement( name, index, format, slot, VertexInputElement.AppendAligned, rate );
		}
	}
}
