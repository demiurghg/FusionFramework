using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Fusion.Core;
using Fusion.Core.Mathematics;

using D3DBlendState			=	SharpDX.Direct3D11.BlendState		;
using D3DSamplerState		=	SharpDX.Direct3D11.SamplerState		;
using D3DRasterizerState	=	SharpDX.Direct3D11.RasterizerState	;
using D3DDepthStencilState	=	SharpDX.Direct3D11.DepthStencilState;


namespace Fusion.Drivers.Graphics {
	public sealed class SamplerState : DisposableBase {

		public Filter			Filter			{ get { return filter		 ; } set { PipelineBoundCheck() ; filter		  = value; } }
		public AddressMode		AddressU		{ get { return addressU		 ; } set { PipelineBoundCheck() ; addressU		  = value; } }
		public AddressMode		AddressV		{ get { return addressV		 ; } set { PipelineBoundCheck() ; addressV		  = value; } }
		public AddressMode		AddressW		{ get { return addressW		 ; } set { PipelineBoundCheck() ; addressW		  = value; } }
		public int				MaxAnisotropy	{ get { return maxAnisotropy ; } set { PipelineBoundCheck() ; maxAnisotropy	  = value; } }
		public int				MaxMipLevel		{ get { return maxMipLevel	 ; } set { PipelineBoundCheck() ; maxMipLevel	  = value; } }
		public int				MinMipLevel		{ get { return minMipLevel	 ; } set { PipelineBoundCheck() ; minMipLevel	  = value; } }
		public float			MipMapBias		{ get { return mipMapBias	 ; } set { PipelineBoundCheck() ; mipMapBias	  = value; } }
		public Color4			BorderColor		{ get { return borderColor	 ; } set { PipelineBoundCheck() ; borderColor	  = value; } }
		public ComparisonFunc	ComparisonFunc	{ get { return compareFunc	 ; } set { PipelineBoundCheck() ; compareFunc	  = value; } }

		public AddressMode	Address	{ 
			set {
				AddressU	=	value;
				AddressV	=	value;
				AddressW	=	value;
			}
		}


		Filter		filter			=	Filter.MinMagMipPoint;
		AddressMode	addressU		=	AddressMode.Wrap;
		AddressMode	addressV		=	AddressMode.Wrap;
		AddressMode	addressW		=	AddressMode.Wrap;
		int			maxAnisotropy	=	4;
		int			maxMipLevel		=	int.MaxValue;
		int			minMipLevel		=	0;
		float		mipMapBias		=	0;
		Color4		borderColor		=	new Color4(0,0,0,0);
		ComparisonFunc	compareFunc		=	ComparisonFunc.Always;


		public static SamplerState LinearWrap			{ get; private set; }
		public static SamplerState LinearClamp			{ get; private set; }
		public static SamplerState LinearPointBorder0	{ get; private set; }
		public static SamplerState LinearPointClamp		{ get; private set; }	
		public static SamplerState LinearPointWrap		{ get; private set; }	
		public static SamplerState PointWrap			{ get; private set; }
		public static SamplerState PointClamp			{ get; private set; }
		public static SamplerState PointBorder1			{ get; private set; }	
		public static SamplerState AnisotropicWrap		{ get; private set; }
		public static SamplerState AnisotropicClamp		{ get; private set; }	
		public static SamplerState ShadowSampler		{ get; private set; }

		D3DSamplerState	state;


		/// <summary>
		/// Creates stock states
		/// </summary>
		static SamplerState ()
		{
			LinearWrap			=	Create( Filter.MinMagMipLinear	, AddressMode.Wrap,		new Color4(0f) );
			LinearClamp			=	Create( Filter.MinMagMipLinear	, AddressMode.Clamp,	new Color4(0f) ); 
			PointWrap			=	Create( Filter.MinMagMipPoint	, AddressMode.Wrap,		new Color4(0f) );
			PointClamp			=	Create( Filter.MinMagMipPoint	, AddressMode.Clamp,	new Color4(0f) ); 
			AnisotropicWrap		=	Create( Filter.Anisotropic		, AddressMode.Wrap,		new Color4(0f) );	
			AnisotropicClamp	=	Create( Filter.Anisotropic		, AddressMode.Clamp,	new Color4(0f) );  	
			ShadowSampler		=	Create( Filter.CmpMinMagLinearMipPoint, AddressMode.Clamp, new Color4( 0.0f, 1.0f, 1.0f, 1.0f ), ComparisonFunc.Less );

			LinearPointBorder0	=	Create( Filter.MinMagLinearMipPoint, AddressMode.Border, new Color4(0f) );
			LinearPointClamp	=	Create( Filter.MinMagLinearMipPoint, AddressMode.Clamp,  new Color4(0f) );
			LinearPointWrap		=	Create( Filter.MinMagLinearMipPoint, AddressMode.Wrap,  new Color4(0f) );
			PointBorder1		=	Create( Filter.MinMagMipPoint, AddressMode.Border, new Color4(1f) );
		}


		/// <summary>
		/// Immediately releases the unmanaged resources used by this object.
		/// </summary>
		static internal void DisposeStates ()
		{
			LinearWrap			.Dispose();
			LinearClamp			.Dispose();
			LinearPointBorder0	.Dispose();
			LinearPointClamp	.Dispose();	
			LinearPointWrap		.Dispose();	
			PointWrap			.Dispose();
			PointClamp			.Dispose();
			PointBorder1		.Dispose();	
			AnisotropicWrap		.Dispose();
			AnisotropicClamp	.Dispose();	
			ShadowSampler		.Dispose();
		}


		/// <summary>
		/// Creates a new instance of sampler state
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="addressMode"></param>
		/// <param name="borderColor"></param>
		/// <param name="comparison"></param>
		/// <returns></returns>
		public static SamplerState Create ( Filter filter, AddressMode addressMode, Color4 borderColor, ComparisonFunc cmpFunc = ComparisonFunc.Always )
		{
			return new SamplerState() {
				Filter			=	filter,
				Address			=	addressMode,
				BorderColor		=	borderColor,
				ComparisonFunc	=	cmpFunc,
			};
		}


		/// <summary>
		/// 
		/// </summary>
		void PipelineBoundCheck ()
		{
			if (state!=null) {
				throw new GraphicsException("Sampler state that has already been bound to the graphics pipeline can not be modified.");
			}
		}



		/// <summary>
		/// Disposes stuff
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				if (state!=null) {
					state.Dispose();
					state = null;
				}
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="device"></param>
		internal D3DSamplerState Apply ( GraphicsDevice device )
		{
			if ( state == null ) {

				var ssd = new SamplerStateDescription();

				ssd.ComparisonFunction	=	Converter.Convert( this.compareFunc );
				ssd.AddressU			=	Converter.Convert( this.addressU );
				ssd.AddressV			=	Converter.Convert( this.addressV );
				ssd.AddressW			=	Converter.Convert( this.addressW );
				ssd.BorderColor			=	SharpDXHelper.Convert( this.borderColor );
				ssd.Filter				=	Converter.Convert( this.filter );
				ssd.MaximumAnisotropy	=	this.maxAnisotropy;
				ssd.MaximumLod			=	this.maxMipLevel;
				ssd.MinimumLod			=	this.minMipLevel;
				ssd.MipLodBias			=	this.mipMapBias;

				state	=	new D3DSamplerState( device.Device, ssd );
			}

			return state;
		}
	}
}
