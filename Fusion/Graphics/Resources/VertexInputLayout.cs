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
	public class VertexInputLayout : DisposableBase {

		readonly GraphicsDevice	device;

		InputElement[] inputElements;

		InputLayout	lastUsedInputLayout = null;
		string		lastUsedSignature = null;
		Dictionary<string,InputLayout> layoutDictionary = new Dictionary<string,InputLayout>();

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertexType"></param>
		public VertexInputLayout ( GraphicsDevice device, Type vertexType )
		{
			this.device		=	device;
			inputElements	=	Convert( GetInputElements( vertexType ) );	
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		/// <param name="elements"></param>
		public VertexInputLayout ( GraphicsDevice device, VertexInputElement[] elements )
		{
			this.device		=	device;
			inputElements	=	Convert( elements );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="elements"></param>
		/// <returns></returns>
		InputElement[] Convert( VertexInputElement[] elements ) 
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
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {

				foreach ( var entry in layoutDictionary ) {
					entry.Value.Dispose();
				}

				layoutDictionary.Clear();				
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="signature"></param>
		/// <returns></returns>
		internal InputLayout GetInputLayout ( string signature )
		{
			if (lastUsedSignature==signature) {
				return lastUsedInputLayout;
			}

			InputLayout layout;

			if (!layoutDictionary.TryGetValue( signature, out layout )) {
				
				layout	=	new InputLayout( device.Device, Misc.HexStringToByte(signature), inputElements );

				layoutDictionary.Add( signature, layout );
			}

			lastUsedInputLayout	=	layout;
			lastUsedSignature	=	signature;

			return layout;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		VertexInputElement[] GetInputElements ( Type type )
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
		VertexInputElement FieldToInputElement ( Type type, FieldInfo fieldInfo )
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
			if ( fieldType == typeof( Int3		) )	format	=	VertexFormat.SInt3		; else 
			if ( fieldType == typeof( Int4		) )	format	=	VertexFormat.SInt4		; else 
			if ( fieldType == typeof( Double	) )	format	=	VertexFormat.UInt2		; else 
				throw new GraphicsException(string.Format("Vertex element type {0} is not supported by VertexBuffer", type.ToString()));

			return new VertexInputElement( name, index, format, slot, VertexInputElement.AppendAligned, rate );
		}
	}
}
