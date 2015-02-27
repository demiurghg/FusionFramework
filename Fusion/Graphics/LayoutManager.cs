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

using D3DBlendState			=	SharpDX.Direct3D11.BlendState		;
using D3DSamplerState		=	SharpDX.Direct3D11.SamplerState		;
using D3DRasterizerState	=	SharpDX.Direct3D11.RasterizerState	;
using D3DDepthStencilState	=	SharpDX.Direct3D11.DepthStencilState;


namespace Fusion.Graphics {
	class LayoutManager : IDisposable {

		GraphicsDevice	device;

		class InputLayoutItem {
			public string		Signature;
			public Type			VertexType;
			public InputLayout	Layout;
		}

		InputLayoutItem			lastUsed		= null;
		List<InputLayoutItem>	inputLayouts	= new List<InputLayoutItem>();



		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		public LayoutManager ( GraphicsDevice device )
		{
			this.device	=	device;

			//inputLayouts.AddRange( Enumerable.Range(0,1000).Select( i => new InputLayoutItem{ Layout = null, Signature = "Q", VertexType = typeof(int) } ) );
		}



		/// <summary>
		/// 
		/// </summary>
		public void Dispose ()
		{
			foreach ( var item in inputLayouts ) {
				if (item.Layout!=null) {
					item.Layout.Dispose();
				}
			}
			inputLayouts.Clear();
		}


		
		/// <summary>
		/// Creates input layout or gets cached one
		/// </summary>
		/// <param name="signature"></param>
		/// <param name="elements"></param>
		/// <returns></returns>
		public InputLayout	GetInputLayout ( string signature, Type vertexType )
		{
			if (signature==null || vertexType==null) {
				return null;
			}


			#if false
			if (lastUsed!=null) {
				if (lastUsed.Signature==signature && lastUsed.VertexType==vertexType) {
					return lastUsed.Layout;
				}
			}
			#endif


			int count = inputLayouts.Count;


			for (int i=0; i<count; i++) {
				if ( inputLayouts[i].Signature == signature && inputLayouts[i].VertexType == vertexType ) {
					lastUsed = inputLayouts[i];
					return inputLayouts[i].Layout;
				}
			}


			var item = new InputLayoutItem() {
					Signature	=	signature,
					VertexType	=	vertexType,
					Layout		=	new InputLayout( device.Device, Misc.HexStringToByte(signature), GetInputElements( vertexType ) )
				};
					
			inputLayouts.Add( item );					

			return item.Layout;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		InputElement[] GetInputElements ( Type type )
		{
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
		InputElement FieldToInputElement ( Type type, FieldInfo fieldInfo )
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

			DXGI.Format	format;

			if ( fieldType == typeof( Single	) )	format	=	DXGI.Format.R32_Float;			else 
			if ( fieldType == typeof( Vector2	) )	format	=	DXGI.Format.R32G32_Float;		else 
			if ( fieldType == typeof( Vector3	) )	format	=	DXGI.Format.R32G32B32_Float;	else 
			if ( fieldType == typeof( Vector4	) )	format	=	DXGI.Format.R32G32B32A32_Float;	else 
			if ( fieldType == typeof( Color4	) )	format	=	DXGI.Format.R32G32B32A32_Float;	else 
			if ( fieldType == typeof( Color		) )	format	=	DXGI.Format.R8G8B8A8_UNorm;		else 
			if ( fieldType == typeof( Byte4		) )	format	=	DXGI.Format.R8G8B8A8_UNorm;		else 
			if ( fieldType == typeof( Half2		) )	format	=	DXGI.Format.R16G16_Float;		else 
			if ( fieldType == typeof( Half4		) )	format	=	DXGI.Format.R16G16B16A16_Float;	else 
			if ( fieldType == typeof( UInt32	) )	format	=	DXGI.Format.R32_UInt;			else 
			if ( fieldType == typeof( Int3		) )	format	=	DXGI.Format.R32G32B32_SInt;		else 
			if ( fieldType == typeof( Int4		) )	format	=	DXGI.Format.R32G32B32A32_SInt;	else 
			if ( fieldType == typeof( Double	) )	format	=	DXGI.Format.R32G32_UInt;		else 
				throw new GraphicsException(string.Format("Vertex element type {0} is not supported by VertexBuffer", type.ToString()));

			return new InputElement( 
				name, index, format, InputElement.AppendAligned, slot, 
				rate == 0 ? InputClassification.PerVertexData : InputClassification.PerInstanceData, 
				rate );
		}

	}
}
