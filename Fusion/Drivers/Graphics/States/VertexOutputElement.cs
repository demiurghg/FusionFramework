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
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {

	public struct VertexOutputElement {

		/// <summary>
		/// Zero-based, stream number.
		/// </summary>
		public int		Stream;

		/// <summary>
		/// Type of output element; possible values include: "POSITION", "NORMAL", or "TEXCOORD0". 
		/// Note that if SemanticName is NULL then ComponentCount can be greater than 4 and 
		/// the described entry will be a gap in the stream out where no data will be written.
		/// </summary>
		public string	SemanticName;
		
		/// <summary>
		/// Output element's zero-based index. Should be used if, for example, 
		/// you have more than one texture coordinate stored in each vertex.
		/// </summary>
		public int		SemanticIndex;

		/// <summary>
		/// Which component of the entry to begin writing out to. Valid values are 0 to 3. 
		/// For example, if you only wish to output to the y and z components of a position, 
		/// then StartComponent should be 1 and ComponentCount should be 2.
		/// </summary>
		public byte		StartComponent;

		/// <summary>
		/// The number of components of the entry to write out to. 
		/// Valid values are 1 to 4. For example, if you only wish to output 
		/// to the y and z components of a position, then StartComponent 
		/// should be 1 and ComponentCount should be 2. 
		/// Note that if SemanticName is NULL then ComponentCount can be greater than 4 
		/// and the described entry will be a gap in the stream out where no data will be written.
		/// </summary>
		public byte		ComponentCount;

		/// <summary>
		/// The associated stream output buffer that is bound to the pipeline. The valid range for OutputSlot is 0 to 3.
		/// </summary>
		public byte		OutputSlot;


		/// <summary>
		/// Creates a new instance of VertexOutputElement class
		/// </summary>
		/// <param name="stream">Zero-based, stream number.</param>
		/// <param name="semanticName">Type of output element; possible values include: "POSITION", "NORMAL", or "TEXCOORD0". Note that if SemanticName is NULL then ComponentCount can be greater than 4 and the described entry will be a gap in the stream out where no data will be written. </param>
		/// <param name="semanticIndex">Output element's zero-based index. Should be used if, for example, you have more than one texture coordinate stored in each vertex.</param>
		/// <param name="startComponent">Which component of the entry to begin writing out to. Valid values are 0 to 3. For example, if you only wish to output to the y and z components of a position, then StartComponent should be 1 and ComponentCount should be 2.</param>
		/// <param name="componentCount">The number of components of the entry to write out to. Valid values are 1 to 4. For example, if you only wish to output to the y and z components of a position, then StartComponent should be 1 and ComponentCount should be 2. Note that if SemanticName is NULL then ComponentCount can be greater than 4 and the described entry will be a gap in the stream out where no data will be written.</param>
		/// <param name="outputSlot">The associated stream output buffer that is bound to the pipeline. The valid range for OutputSlot is 0 to 3.</param>
		public VertexOutputElement( int stream, string semanticName, int semanticIndex, byte startComponent, byte componentCount, byte outputSlot )
		{
			this.Stream			=	stream			;
			this.SemanticName	=	semanticName	;
			this.SemanticIndex	=	semanticIndex	;
			this.StartComponent	=	startComponent	;
			this.ComponentCount	=	componentCount	;
			this.OutputSlot		=	outputSlot		;
		}

		/// <summary>
		/// Creates a new instance of VertexOutputElement class
		/// </summary>
		/// <param name="semanticName">Type of output element; possible values include: "POSITION", "NORMAL", or "TEXCOORD0". Note that if SemanticName is NULL then ComponentCount can be greater than 4 and the described entry will be a gap in the stream out where no data will be written. </param>
		/// <param name="semanticIndex">Output element's zero-based index. Should be used if, for example, you have more than one texture coordinate stored in each vertex.</param>
		/// <param name="startComponent">Which component of the entry to begin writing out to. Valid values are 0 to 3. For example, if you only wish to output to the y and z components of a position, then StartComponent should be 1 and ComponentCount should be 2.</param>
		/// <param name="componentCount">The number of components of the entry to write out to. Valid values are 1 to 4. For example, if you only wish to output to the y and z components of a position, then StartComponent should be 1 and ComponentCount should be 2. Note that if SemanticName is NULL then ComponentCount can be greater than 4 and the described entry will be a gap in the stream out where no data will be written.</param>
		public VertexOutputElement( string semanticName, int semanticIndex, byte startComponent, byte componentCount )
		{
			this.Stream			=	0			;
			this.SemanticName	=	semanticName	;
			this.SemanticIndex	=	semanticIndex	;
			this.StartComponent	=	startComponent	;
			this.ComponentCount	=	componentCount	;
			this.OutputSlot		=	0		;
		}



		/// <summary>
		/// Converts VertexOutputElement to StreamOutputElement
		/// </summary>
		/// <param name="elements"></param>
		/// <returns></returns>
		static internal StreamOutputElement[] Convert ( VertexOutputElement[] elements )
		{
			return elements.Select( e => new StreamOutputElement() {
					ComponentCount	=	e.ComponentCount,
					OutputSlot		=	e.OutputSlot,
					SemanticIndex	=	e.SemanticIndex,
					SemanticName	=	e.SemanticName,
					StartComponent	=	e.StartComponent,
					Stream			=	e.Stream
				} ).ToArray();
		}

	}
}
