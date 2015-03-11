using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Fusion.Mathematics;

using D3DBlendState			=	SharpDX.Direct3D11.BlendState		;
using D3DSamplerState		=	SharpDX.Direct3D11.SamplerState		;
using D3DRasterizerState	=	SharpDX.Direct3D11.RasterizerState	;
using D3DDepthStencilState	=	SharpDX.Direct3D11.DepthStencilState;


namespace Fusion.Graphics {
	public class BlendState : DisposableBase {

		public	bool			Enabled			{ get { return enabled			; } set { PipelineBoundCheck(); enabled			= value; } }
		public	Blend			SrcColor		{ get { return srcColor			; } set { PipelineBoundCheck(); srcColor		= value; } }
		public	Blend			DstColor		{ get { return dstColor			; } set { PipelineBoundCheck(); dstColor		= value; } }
		public	Blend			SrcAlpha		{ get { return srcAlpha			; } set { PipelineBoundCheck(); srcAlpha		= value; } }
		public	Blend			DstAlpha		{ get { return dstAlpha			; } set { PipelineBoundCheck(); dstAlpha		= value; } }
		public	BlendOp			ColorOp			{ get { return colorOp			; } set { PipelineBoundCheck(); colorOp			= value; } }
		public	BlendOp			AlphaOp			{ get { return alphaOp			; } set { PipelineBoundCheck(); alphaOp			= value; } }
		public	ColorChannels	WriteMask		{ get { return writeMask		; } set { PipelineBoundCheck(); writeMask		= value; } }
		public	int				MultiSampleMask	{ get { return multiSampleMask	; } set { PipelineBoundCheck(); multiSampleMask = value; } }
		public	Color4			BlendFactor		{ get { return blendFactor		; } set { PipelineBoundCheck(); blendFactor		= value; } }

		bool			enabled			=	false;
		Blend			srcColor		=	Blend.One;
		Blend			dstColor		=	Blend.Zero;
		Blend			srcAlpha		=	Blend.One;
		Blend			dstAlpha		=	Blend.Zero;
		BlendOp			colorOp			=	BlendOp.Add;
		BlendOp			alphaOp			=	BlendOp.Add;	
		ColorChannels	writeMask		=	ColorChannels.All;
		int				multiSampleMask	=	-1;
		Color4			blendFactor		=	new Color4(0,0,0,0);

		D3DBlendState	state	=	null;


		public static  BlendState	Opaque			 { get; private set; }
		public static  BlendState	NoWrite			 { get; private set; }
		public static  BlendState	AlphaBlend		 { get; private set; }
		public static  BlendState	AlphaBlendPreMul { get; private set; }
		public static  BlendState	AlphaMaskWrite	 { get; private set; }
		public static  BlendState	Additive		 { get; private set; }
		public static  BlendState	Screen			 { get; private set; }
		public static  BlendState	Multiply		 { get; private set; }
		public static  BlendState	NegMultiply		 { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		public BlendState ()
		{
		}



		/// <summary>
		/// Creates stock states
		/// </summary>
		static BlendState ()
		{
			Opaque				=	Create( false, ColorChannels.All );

			NoWrite			 	=	Create( false, ColorChannels.None );
			AlphaMaskWrite		=	Create( false, ColorChannels.Alpha);

			AlphaBlend			=	Create( true, ColorChannels.All, Blend.SrcAlpha, Blend.InvSrcAlpha );
			AlphaBlendPreMul	=	Create( true, ColorChannels.All, Blend.One,		 Blend.InvSrcAlpha );
			Additive			=	Create( true, ColorChannels.All, Blend.One, Blend.One, Blend.One, Blend.One );
			Screen				=	Create( true, ColorChannels.All, Blend.InvDstColor, Blend.One );
			Multiply			=	Create( true, ColorChannels.All, Blend.Zero, Blend.SrcColor );
			NegMultiply			=	Create( true, ColorChannels.All, Blend.Zero, Blend.InvSrcColor );
		}



		/// <summary>
		/// 
		/// </summary>
		internal static void DisposeStates ()
		{
			Opaque			 .Dispose();
			NoWrite			 .Dispose();
			AlphaBlend		 .Dispose();
			AlphaBlendPreMul .Dispose();
			AlphaMaskWrite	 .Dispose();
			Additive		 .Dispose();
			Screen			 .Dispose();
			Multiply		 .Dispose();
			NegMultiply		 .Dispose();
		}



		/// <summary>
		/// 
		/// </summary>
		void PipelineBoundCheck ()
		{
			if (state!=null) {
				throw new GraphicsException("Blend state that has already been bound to the graphics pipeline can not be modified.");
			}
		}



		/// <summary>
		/// Creates blend state
		/// </summary>
		/// <param name="enabled"></param>
		/// <param name="mask"></param>
		/// <param name="src"></param>
		/// <param name="dst"></param>
		/// <param name="srcA"></param>
		/// <param name="dstA"></param>
		public static BlendState Create ( bool enabled, ColorChannels mask, Blend src = Blend.One, Blend dst = Blend.Zero, Blend srcA = Blend.One, Blend dstA = Blend.Zero )
		{
			var bs = new BlendState {
				Enabled		=	enabled,
				WriteMask	=	mask,
				SrcColor	=	src,
				DstColor	=	dst,
				SrcAlpha	=	srcA,
				DstAlpha	=	dstA,
			};

			bs.Enabled = enabled;

			return bs;
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
		/// Applies state to pipeline
		/// </summary>
		/// <param name="device"></param>
		internal void Apply ( GraphicsDevice device )
		{
			if ( state == null ) {

				var	rtbd	=	new RenderTargetBlendDescription();

				rtbd.IsBlendEnabled			=	Enabled	;
				rtbd.BlendOperation			=	Converter.Convert( colorOp );
				rtbd.AlphaBlendOperation	=	Converter.Convert( alphaOp );
				rtbd.RenderTargetWriteMask	=	(ColorWriteMaskFlags)(int)writeMask;
				rtbd.DestinationBlend		=	Converter.Convert( dstColor );
				rtbd.SourceBlend			=	Converter.Convert( srcColor );
				rtbd.DestinationAlphaBlend	=	Converter.Convert( dstAlpha );
				rtbd.SourceAlphaBlend		=	Converter.Convert( srcAlpha );
				
				var	bsd		=	new BlendStateDescription();
				bsd.AlphaToCoverageEnable	=	false;
				bsd.IndependentBlendEnable	=	false;
				bsd.RenderTarget[0]			=	rtbd;

				state	=	new D3DBlendState( device.Device, bsd );
			}

			device.DeviceContext.OutputMerger.BlendState		=	state;
			device.DeviceContext.OutputMerger.BlendFactor		=	SharpDXHelper.Convert( blendFactor );
			device.DeviceContext.OutputMerger.BlendSampleMask	=	multiSampleMask;
		}
	}
}
