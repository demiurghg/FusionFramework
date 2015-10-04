using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using Format = SharpDX.DXGI.Format;
using D3DCullMode		=	SharpDX.Direct3D11.CullMode;
using D3DFillMode		=	SharpDX.Direct3D11.FillMode;
using D3DFilter			=	SharpDX.Direct3D11.Filter;
using D3DAddressMode	=	SharpDX.Direct3D11.TextureAddressMode;
using D3DComparison		=	SharpDX.Direct3D11.Comparison;


namespace Fusion.Drivers.Graphics {

	public enum GraphicsProfile {
		HiDef,
		Reach,
		Mobile,
	}


	/// <summary>
	/// Defines stereo mode
	/// </summary>
	public enum StereoMode {
		Disabled,	///	No stereo
		NV3DVision,	///	NVidia 3DVision stereo
		DualHead,	///	Dual-head projector stereo
		OculusRift,	///	Oculus Rift
		Interlaced,	///	Interlaced mode for TVs
	}


	/// <summary>
	/// Defines interlacing mode for interlaced stereo
	/// </summary>
	public enum InterlacingMode {
		VerticalLR,
		VerticalRL,
		HorizontalLR,
		HorizontalRL,
	}


	internal enum TextureType {
		Texture2D,
		Texture3D,
		TextureCube,
		TextureArray,
	}


	/// <summary>
	/// Structured buffer flags.
	/// </summary>
	public enum StructuredBufferFlags {
		None,
		Append,
		Counter,
	}


	/// <summary>
	/// Defines currenly rendered eye for stereo rendering
	/// </summary>
	public enum StereoEye {
		Left, Right, Mono
	}


	/// <summary>
	/// Defines color formats for textures and render targets
	/// </summary>
	public enum ColorFormat {
		Unknown,
		R32F,
		R16F,
		Rg16F,
		Rgba8,
		Rgb10A2,
		Rgba16F,
		Rgba32F,
		Rgb32F,
		Dxt1,
		Dxt3,
		Dxt5,
	}


	/// <summary>
	/// Defines color formats for depth-stencil surfaces
	/// </summary>
	public enum DepthFormat {
		D24S8,
		D16,
		D32F,
	}



	/// <summary>
	/// Defines vertex field format
	/// </summary>
	public enum VertexFormat {
		Float,
		Vector2,
		Vector3,
		Vector4,
		Color,
		Color4,
		Byte4,
		Half2,
		Half4,
		UInt,
		UInt2,
		UInt3,
		UInt4,
		SInt,
		SInt2,
		SInt3,
		SInt4,
	}



	/// <summary>
	/// Defines cube face
	/// </summary>
	public enum CubeFace {
		FacePosX = 0,
		FaceNegX = 1,
		FacePosY = 2,
		FaceNegY = 3,
		FacePosZ = 4,
		FaceNegZ = 5,
	}


	/// <summary>
	/// Defines primitive type
	/// </summary>
	public enum Primitive { 			//	Undefined						= 0,
		PointList			= 1,		//	PointList						= 1,
		LineList			= 2,		//	LineList						= 2,
		LineStrip			= 3,		//	LineStrip						= 3,
		TriangleList		= 4,		//	TriangleList					= 4,
		TriangleStrip		= 5,		//	TriangleStrip					= 5,
		LineListAdj			= 10,		//	LineListWithAdjacency			= 10,
		LineStripAdj		= 11,		//	LineStripWithAdjacency			= 11,
		TriangleListAdj		= 12,		//	TriangleListWithAdjacency		= 12,
		TriangleStripAdj	= 13,		//	TriangleStripWithAdjacency		= 13,
		PatchList1CP		= 33,		//	PatchListWith1ControlPoints		= 33,
		PatchList2CP		= 34,		//	PatchListWith2ControlPoints		= 34,
		PatchList3CP		= 35,		//	PatchListWith3ControlPoints		= 35,
		PatchList4CP		= 36,		//	PatchListWith4ControlPoints		= 36,
		PatchList5CP		= 37,		//	PatchListWith5ControlPoints		= 37,
		PatchList6CP		= 38,		//	PatchListWith6ControlPoints		= 38,
		PatchList7CP		= 39,		//	PatchListWith7ControlPoints		= 39,
		PatchList8CP		= 40,		//	PatchListWith8ControlPoints		= 40,
		PatchList9CP		= 41,		//	PatchListWith9ControlPoints		= 41,
		PatchList10CP		= 42,		//	PatchListWith10ControlPoints	= 42,
		PatchList11CP		= 43,		//	PatchListWith11ControlPoints	= 43,
		PatchList12CP		= 44,		//	PatchListWith12ControlPoints	= 44,
		PatchList13CP		= 45,		//	PatchListWith13ControlPoints	= 45,
		PatchList14CP		= 46,		//	PatchListWith14ControlPoints	= 46,
		PatchList15CP		= 47,		//	PatchListWith15ControlPoints	= 47,
		PatchList16CP		= 48,		//	PatchListWith16ControlPoints	= 48,
		PatchList17CP		= 49,		//	PatchListWith17ControlPoints	= 49,
		PatchList18CP		= 50,		//	PatchListWith18ControlPoints	= 50,
		PatchList19CP		= 51,		//	PatchListWith19ControlPoints	= 51,
		PatchList20CP		= 52,		//	PatchListWith20ControlPoints	= 52,
		PatchList21CP		= 53,		//	PatchListWith21ControlPoints	= 53,
		PatchList22CP		= 54,		//	PatchListWith22ControlPoints	= 54,
		PatchList23CP		= 55,		//	PatchListWith23ControlPoints	= 55,
		PatchList24CP		= 56,		//	PatchListWith24ControlPoints	= 56,
		PatchList25CP		= 57,		//	PatchListWith25ControlPoints	= 57,
		PatchList26CP		= 58,		//	PatchListWith26ControlPoints	= 58,
		PatchList27CP		= 59,		//	PatchListWith27ControlPoints	= 59,
		PatchList28CP		= 60,		//	PatchListWith28ControlPoints	= 60,
		PatchList29CP		= 61,		//	PatchListWith29ControlPoints	= 61,
		PatchList30CP		= 62,		//	PatchListWith30ControlPoints	= 62,
		PatchList31CP		= 63,		//	PatchListWith31ControlPoints	= 63,
		PatchList32CP		= 64,		//	PatchListWith32ControlPoints	= 64,
	}



	/// <summary>
	/// Defines winding orders that may be used to identify back faces for culling. 
	/// </summary>
	public enum CullMode {
		CullNone,
		CullCW,
		CullCCW,
	}


	/// <summary>
	/// Defines comparison functions that can be chosen for alpha, stencil, or depth-buffer tests. 
	/// </summary>
	public enum ComparisonFunc {
		Always			,	///	Always pass the test.
		Equal			,	///	Accept the new pixel if its value is equal to the value of the current pixel.
		Greater			,	///	Accept the new pixel if its value is greater than the value of the current pixel.
		GreaterEqual	,	///	Accept the new pixel if its value is greater than or equal to the value of the current pixel.
		Less			,	///	Accept the new pixel if its value is less than the value of the current pixel.
		LessEqual		,	///	Accept the new pixel if its value is less than or equal to the value of the current pixel.
		Never			,	///	Always fail the test.
		NotEqual		,	///	Accept the new pixel if its value does not equal the value of the current pixel.		
	}


	/// <summary>
	/// Defines color blending factors.
	/// </summary>
	public enum Blend {
		Zero				,	///	Each component of the color is multiplied by (0, 0, 0, 0).
		One					,	///	Each component of the color is multiplied by (1, 1, 1, 1).
		SrcColor			,	///	Each component of the color is multiplied by the source color. This can be represented as (Rs, Gs, Bs, As), where R, G, B, and A respectively stand for the red, green, blue, and alpha source values.
		InvSrcColor			,	///	Each component of the color is multiplied by the inverse of the source color. This can be represented as (1 − Rs, 1 − Gs, 1 − Bs, 1 − As) where R, G, B, and A respectively stand for the red, green, blue, and alpha destination values.
		SrcAlpha			,	///	Each component of the color is multiplied by the alpha value of the source. This can be represented as (As, As, As, As), where As is the alpha source value.
		InvSrcAlpha			,	///	Each component of the color is multiplied by the inverse of the alpha value of the source. This can be represented as (1 − As, 1 − As, 1 − As, 1 − As), where As is the alpha destination value.
		DstAlpha			,	///	Each component of the color is multiplied by the alpha value of the destination. This can be represented as (Ad, Ad, Ad, Ad), where Ad is the destination alpha value.
		InvDstAlpha			,	///	Each component of the color is multiplied by the inverse of the alpha value of the destination. This can be represented as (1 − Ad, 1 − Ad, 1 − Ad, 1 − Ad), where Ad is the alpha destination value.
		DstColor			,	///	Each component color is multiplied by the destination color. This can be represented as (Rd, Gd, Bd, Ad), where R, G, B, and A respectively stand for red, green, blue, and alpha destination values.
		InvDstColor			,	///	Each component of the color is multiplied by the inverse of the destination color. This can be represented as (1 − Rd, 1 − Gd, 1 − Bd, 1 − Ad), where Rd, Gd, Bd, and Ad respectively stand for the red, green, blue, and alpha destination values.
		SrcAlphaSaturation	,	///	Each component of the color is multiplied by either the alpha of the source color, or the inverse of the alpha of the source color, whichever is greater. This can be represented as (f, f, f, 1), where f = min(A, 1 − Ad).
		BlendFactor			,	///	Each component of the color is multiplied by a constant set in BlendFactor.
		InvBlendFactor		,	///	Each component of the color is multiplied by the inverse of a constant set in BlendFactor. 
	}


	/// <summary>
	/// Defines how to combine a source color with the destination color already on the render target for color blending. 
	/// </summary>
	public enum BlendOp {
		Add, 
		Max,
		Min,
		InvSub,
		Sub,
	}


	/// <summary>
	/// Defines color channel mask
	/// </summary>
	[Flags]
	public enum ColorChannels {
		None	=	0x00,
		Red		=	0x01,
		Green	=	0x02,
		Blue	=	0x04,
		Alpha	=	0x08,
		All		=	0x0F,
	}


	/// <summary>
	/// Defines fill mode
	/// </summary>
	public enum FillMode {
		Solid,
		Wireframe,
	}



	/// <summary>
	/// Defines texture filter
	/// </summary>
	public enum Filter {
		MinMagMipPoint = 0,
		MinMagPointMipLinear = 1,
		MinPointMagLinearMipPoint = 4,
		MinPointMagMipLinear = 5,
		MinLinearMagMipPoint = 16,
		MinLinearMagPointMipLinear = 17,
		MinMagLinearMipPoint = 20,
		MinMagMipLinear = 21,
		Anisotropic = 85,
		CmpMinMagMipPoint = 128,
		CmpMinMagPointMipLinear = 129,
		CmpMinPointMagLinearMipPoint = 132,
		CmpMinPointMagMipLinear = 133,
		CmpMinLinearMagMipPoint = 144,
		CmpMinLinearMagPointMipLinear = 145,
		CmpMinMagLinearMipPoint = 148,
		CmpMinMagMipLinear = 149,
		CmpAnisotropic = 213,
	}



	/// <summary>
	/// Defines texture adressing mode
	/// </summary>
	public enum AddressMode {
		Wrap = 1,
		Mirror = 2,
		Clamp = 3,
		Border = 4,
		MirrorOnce = 5,
	}



	/// <summary>
	/// Defines stencil operation
	/// </summary>
	public enum StencilOp {
		Keep = 1,
		Zero = 2,
		Replace = 3,
		IncrementAndClamp = 4,
		DecrementAndClamp = 5,
		Invert = 6,
		Increment = 7,
		Decrement = 8,
	}


	/// <summary>
	/// Fusion-to-SharpDX enum converter
	/// </summary>
	static class Converter {

		public static int SizeOf( ColorFormat format )
		{
			switch ( format ) {
				case ColorFormat.R32F		: return 4;
				case ColorFormat.R16F		: return 2;
				case ColorFormat.Rg16F		: return 4;
				case ColorFormat.Rgba8		: return 4;
				case ColorFormat.Rgb10A2	: return 4;
				case ColorFormat.Rgba16F	: return 8;
				case ColorFormat.Rgba32F	: return 16;
				case ColorFormat.Rgb32F		: return 12;
				case ColorFormat.Dxt1		: return -1;
				case ColorFormat.Dxt3		: return -1;
				case ColorFormat.Dxt5		: return -1;
			}

			throw new ArgumentException("bad format");
		}
		
		
		public static int SizeOf( VertexFormat format )
		{
			switch ( format ) {
				case VertexFormat.Float		: return 4;
				case VertexFormat.Vector2	: return 8;
				case VertexFormat.Vector3	: return 12;
				case VertexFormat.Vector4	: return 16;
				case VertexFormat.Color		: return 4;
				case VertexFormat.Color4	: return 16;
				case VertexFormat.Byte4		: return 4;
				case VertexFormat.Half2		: return 4;
				case VertexFormat.Half4		: return 8;
				case VertexFormat.UInt		: return 4;
				case VertexFormat.UInt2		: return 8;
				case VertexFormat.UInt3		: return 12;
				case VertexFormat.UInt4		: return 16;
				case VertexFormat.SInt		: return 4;
				case VertexFormat.SInt2		: return 8;
				case VertexFormat.SInt3		: return 12;
				case VertexFormat.SInt4		: return 16;
			}

			throw new ArgumentException("bad format");
		}
		
		public static Format Convert ( ColorFormat format )
		{
			switch ( format ) {
				case ColorFormat.R32F		: return Format.R32_Float;
				case ColorFormat.R16F		: return Format.R16_Float;
				case ColorFormat.Rg16F		: return Format.R16G16_Float;
				case ColorFormat.Rgba8		: return Format.R8G8B8A8_UNorm;
				case ColorFormat.Rgb10A2	: return Format.R10G10B10A2_UNorm;
				case ColorFormat.Rgba16F	: return Format.R16G16B16A16_Float;
				case ColorFormat.Rgba32F	: return Format.R32G32B32A32_Float;
				case ColorFormat.Rgb32F		: return Format.R32G32B32_Float;
				case ColorFormat.Dxt1		: return Format.BC1_UNorm;
				case ColorFormat.Dxt3		: return Format.BC2_UNorm;
				case ColorFormat.Dxt5		: return Format.BC3_UNorm;
			}

			throw new ArgumentException("bad format");
		}


		public static Format ConvertToTex ( DepthFormat format )
		{
			switch ( format ) {
				case DepthFormat.D24S8	: return Format.R24G8_Typeless;
				case DepthFormat.D16	: return Format.R16_Typeless;
			}
			throw new ArgumentException("bad format");
		}


		public static Format ConvertToSRV ( DepthFormat format )
		{
			switch ( format ) {
				case DepthFormat.D24S8	: return Format.R24_UNorm_X8_Typeless;
				case DepthFormat.D16	: return Format.R16_UNorm;
				case DepthFormat.D32F	: return Format.R32_Float;
			}
			throw new ArgumentException("bad format");
		}


		public static Format ConvertToDSV ( DepthFormat format )
		{
			switch ( format ) {
				case DepthFormat.D24S8	: return Format.D24_UNorm_S8_UInt;
				case DepthFormat.D16	: return Format.R16_UNorm;
				case DepthFormat.D32F	: return Format.D32_Float;
			}
			throw new ArgumentException("bad format");
		}



		public static Format Convert ( VertexFormat format )
		{
			switch ( format ) {
				case VertexFormat.Float		: return Format.R32_Float;
				case VertexFormat.Vector2	: return Format.R32G32_Float;
				case VertexFormat.Vector3	: return Format.R32G32B32_Float;
				case VertexFormat.Vector4	: return Format.R32G32B32A32_Float;
				case VertexFormat.Color4	: return Format.R32G32B32A32_Float;
				case VertexFormat.Color		: return Format.R8G8B8A8_UNorm; 
				case VertexFormat.Byte4		: return Format.R8G8B8A8_UInt; 
				case VertexFormat.Half2		: return Format.R16G16_Float;
				case VertexFormat.Half4		: return Format.R16G16B16A16_Float;
				case VertexFormat.UInt		: return Format.R32_UInt;
				case VertexFormat.UInt2		: return Format.R32G32_UInt;
				case VertexFormat.UInt3		: return Format.R32G32B32_UInt;
				case VertexFormat.UInt4		: return Format.R32G32B32A32_UInt;
				case VertexFormat.SInt		: return Format.R32_SInt;
				case VertexFormat.SInt2		: return Format.R32G32_SInt;
				case VertexFormat.SInt3		: return Format.R32G32B32_SInt;
				case VertexFormat.SInt4		: return Format.R32G32B32A32_SInt;
			}
			throw new ArgumentException("bad format");
		}


		public static PrimitiveTopology	Convert ( Primitive primitive )
		{
			return (PrimitiveTopology) (int) primitive;
			//throw new ArgumentException("Bad primitive");
		}



		public static BlendOperation Convert ( BlendOp op )
		{
			switch ( op ) {
				case BlendOp.Add 	:	return BlendOperation.Add;
				case BlendOp.Max	:	return BlendOperation.Maximum;
				case BlendOp.Min	:	return BlendOperation.Minimum;
				case BlendOp.InvSub	:	return BlendOperation.ReverseSubtract;
				case BlendOp.Sub	:	return BlendOperation.Subtract;
			}

			throw new ArgumentException("Bad operation");
		}



		public static BlendOption	Convert ( Blend blend )
		{
			switch ( blend ) {
				case Blend.Zero					: return BlendOption.Zero	;
				case Blend.One					: return BlendOption.One	;

				case Blend.SrcColor				: return BlendOption.SourceColor	;
				case Blend.InvSrcColor			: return BlendOption.InverseSourceColor	;
				case Blend.SrcAlpha				: return BlendOption.SourceAlpha	;
				case Blend.InvSrcAlpha			: return BlendOption.InverseSourceAlpha	;

				case Blend.DstAlpha				: return BlendOption.DestinationAlpha	;
				case Blend.InvDstAlpha			: return BlendOption.InverseDestinationAlpha	;
				case Blend.DstColor				: return BlendOption.DestinationColor	;
				case Blend.InvDstColor			: return BlendOption.InverseDestinationColor	;

				case Blend.SrcAlphaSaturation	: return BlendOption.SourceAlphaSaturate	;

				case Blend.BlendFactor			: return BlendOption.BlendFactor	;
				case Blend.InvBlendFactor		: return BlendOption.InverseBlendFactor	;
			}

			throw new ArgumentException("Bad blend");
		}



		public static D3DFillMode Convert ( FillMode fillMode )
		{
			switch ( fillMode ) {
				case FillMode.Solid		:	return D3DFillMode.Solid;
				case FillMode.Wireframe	:	return D3DFillMode.Wireframe;
			}

			throw new ArgumentException("Bad fill mode");
		}



		public static D3DComparison Convert ( ComparisonFunc cmp )
		{
			switch ( cmp ) {
				case ComparisonFunc.Always			:	return D3DComparison.Always;
				case ComparisonFunc.Equal			:	return D3DComparison.Equal;
				case ComparisonFunc.Greater			:	return D3DComparison.Greater;
				case ComparisonFunc.GreaterEqual	:	return D3DComparison.GreaterEqual;
				case ComparisonFunc.Less			:	return D3DComparison.Less;
				case ComparisonFunc.LessEqual		:	return D3DComparison.LessEqual;
				case ComparisonFunc.Never			:	return D3DComparison.Never;
				case ComparisonFunc.NotEqual		:	return D3DComparison.NotEqual;
			}

			throw new ArgumentException("Bad comparison function");
		}



		public static D3DFilter Convert ( Filter filter ) 
		{
			return (D3DFilter)(int)filter;
		}



		public static D3DAddressMode Convert ( AddressMode addressMode ) 
		{
			return (D3DAddressMode)(int)addressMode;
		}


		public static StencilOperation Convert ( StencilOp op )
		{
			return (StencilOperation)(int)op;
		}


		public static ColorFormat Convert ( Format format )
		{
			switch ( format ) {
				case Format.R32_Typeless	:
				case Format.R32_Float		:
					return ColorFormat.R32F;

				case Format.R16_Typeless	:
				case Format.R16_Float		:
					return ColorFormat.R16F;

				case Format.R16G16_Typeless	:
				case Format.R16G16_Float	:
					return ColorFormat.Rg16F;

				case Format.R8G8B8A8_Typeless	:
				case Format.R8G8B8A8_UNorm		:
				case Format.R8G8B8A8_UNorm_SRgb :
					return ColorFormat.Rgba8;

				case Format.R10G10B10A2_Typeless	:
				case Format.R10G10B10A2_UNorm		:
					return ColorFormat.Rgb10A2;

				case Format.R16G16B16A16_Typeless	:
				case Format.R16G16B16A16_Float		:
					return ColorFormat.Rgba16F;

				case Format.R32G32B32A32_Typeless	:
				case Format.R32G32B32A32_Float		:
					return ColorFormat.Rgba32F;

				case Format.R32G32B32_Typeless	:
				case Format.R32G32B32_Float		:
					return ColorFormat.Rgb32F;

				case Format.BC1_Typeless	:
				case Format.BC1_UNorm		:
				case Format.BC1_UNorm_SRgb	:
					return ColorFormat.Dxt1;

				case Format.BC2_Typeless	:
				case Format.BC2_UNorm		:
				case Format.BC2_UNorm_SRgb	:
					return ColorFormat.Dxt3;

				case Format.BC3_Typeless	:
				case Format.BC3_UNorm		:
				case Format.BC3_UNorm_SRgb	:
					return ColorFormat.Dxt5;
			}

			return ColorFormat.Unknown;
		}
	}
}
