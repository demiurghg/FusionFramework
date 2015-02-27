using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using D3DBlendState			=	SharpDX.Direct3D11.BlendState		;
using D3DSamplerState		=	SharpDX.Direct3D11.SamplerState		;
using D3DRasterizerState	=	SharpDX.Direct3D11.RasterizerState	;
using D3DDepthStencilState	=	SharpDX.Direct3D11.DepthStencilState;


namespace Fusion.Graphics {

	public class DepthStencilState : DisposableBase {

		//GraphicsDevice	device;
		
		bool			DepthEnabled			{ get { return depthEnabled				; } set { PipelineBoundCheck(); depthEnabled			 = value; } }
		bool			DepthWriteEnabled		{ get { return depthWriteEnabled		; } set { PipelineBoundCheck(); depthWriteEnabled		 = value; } }
		ComparisonFunc	DepthComparison			{ get { return depthComparison			; } set { PipelineBoundCheck(); depthComparison			 = value; } }
		
		bool			StencilEnabled			{ get { return stencilEnabled			; } set { PipelineBoundCheck(); stencilEnabled			 = value; } }
		byte			StencilReadMask			{ get { return stencilReadMask			; } set { PipelineBoundCheck(); stencilReadMask			 = value; } }
		byte			StencilWriteMask		{ get { return stencilWriteMask			; } set { PipelineBoundCheck(); stencilWriteMask		 = value; } }

		StencilOp		FrontFaceFailOp				{ get { return frontFailOp				; } set { PipelineBoundCheck(); frontFailOp				 = value; } }
		StencilOp		FrontFaceDepthFailOp		{ get { return frontDepthFailOp			; } set { PipelineBoundCheck(); frontDepthFailOp		 = value; } }
		StencilOp		FrontFacePassOp				{ get { return frontPassOp				; } set { PipelineBoundCheck(); frontPassOp				 = value; } }
		ComparisonFunc	FrontFaceStencilComparison	{ get { return frontStencilComparison	; } set { PipelineBoundCheck(); frontStencilComparison	 = value; } }
		
		StencilOp		BackFaceFailOp				{ get { return backFailOp				; } set { PipelineBoundCheck(); backFailOp				 = value; } }
		StencilOp		BackFaceDepthFailOp			{ get { return backDepthFailOp			; } set { PipelineBoundCheck(); backDepthFailOp			 = value; } }
		StencilOp		BackFacePassOp				{ get { return backPassOp				; } set { PipelineBoundCheck(); backPassOp				 = value; } }
		ComparisonFunc	BackFaceStencilComparison	{ get { return backStencilComparison	; } set { PipelineBoundCheck(); backStencilComparison	 = value; } }

		int				StencilReference		{ get { return stencilReference; } set { PipelineBoundCheck(); stencilReference = value; } }


		bool			depthEnabled			=	false;
		bool			depthWriteEnabled		=	true;
		ComparisonFunc	depthComparison			=	ComparisonFunc.LessEqual;

		bool			stencilEnabled			=	false;
		byte			stencilReadMask			=	0xFF;
		byte			stencilWriteMask		=	0xFF;

		StencilOp		frontFailOp				=	StencilOp.Keep;
		StencilOp		frontDepthFailOp		=	StencilOp.Keep;
		StencilOp		frontPassOp				=	StencilOp.Keep;
		ComparisonFunc	frontStencilComparison	=	ComparisonFunc.Always;

		StencilOp		backFailOp				=	StencilOp.Keep;
		StencilOp		backDepthFailOp			=	StencilOp.Keep;
		StencilOp		backPassOp				=	StencilOp.Keep;
		ComparisonFunc	backStencilComparison	=	ComparisonFunc.Always;

		int				stencilReference		=	0;

		D3DDepthStencilState	state;


		public static DepthStencilState		Default		{ get; private set; }
		public static DepthStencilState		Readonly	{ get; private set; }
		public static DepthStencilState		None		{ get; private set; }
		public static DepthStencilState		Sky			{ get; private set; }
		public static DepthStencilState		LightMark	{ get; private set; }
		public static DepthStencilState		LightPass	{ get; private set; }


		/// <summary>
		/// Creates stock states
		/// </summary>
		static DepthStencilState()
		{
			Default	= new DepthStencilState() {
				DepthEnabled		=	true,
				DepthComparison		=	ComparisonFunc.LessEqual,
				DepthWriteEnabled	=	true,
			};

			Readonly = new DepthStencilState() {
				DepthEnabled		=	true,
				DepthComparison		=	ComparisonFunc.LessEqual,
				DepthWriteEnabled	=	false,
			};

			Sky = new DepthStencilState() {
				DepthEnabled		=	true,
				DepthComparison		=	ComparisonFunc.Equal,
				DepthWriteEnabled	=	false,
			};

			None = new DepthStencilState() {
				DepthEnabled = false,
				DepthWriteEnabled = false
			};

			LightMark	=	new DepthStencilState() {
				depthEnabled			=	true,
				depthComparison			=	ComparisonFunc.Less,
				depthWriteEnabled		=	false,
				stencilEnabled			=	true,
				stencilReadMask			=	0xFF,
				stencilWriteMask		=	0xFF,
				frontFailOp				=	StencilOp.Keep,
				frontDepthFailOp		=	StencilOp.Increment,
				frontPassOp				=	StencilOp.Keep,		
				frontStencilComparison	=	ComparisonFunc.Always,
				backFailOp				=	StencilOp.Keep,
				backDepthFailOp			=	StencilOp.Decrement,						
				backPassOp				=	StencilOp.Keep,						
				backStencilComparison	=	ComparisonFunc.Always,	
			};

			LightPass	=	new DepthStencilState() {
				depthEnabled			=	false,
				depthComparison			=	ComparisonFunc.Less,
				depthWriteEnabled		=	false,
				stencilEnabled			=	true,
				stencilReadMask			=	0xFF,
				stencilWriteMask		=	0xFF,
				frontFailOp				=	StencilOp.Keep,
				frontDepthFailOp		=	StencilOp.Keep,
				frontPassOp				=	StencilOp.Decrement,	
				frontStencilComparison	=	ComparisonFunc.Equal,
				backFailOp				=	StencilOp.Keep,
				backDepthFailOp			=	StencilOp.Keep,
				backPassOp				=	StencilOp.Decrement,	
				backStencilComparison	=	ComparisonFunc.Equal,
			};
		}



		/// <summary>
		/// 
		/// </summary>
		internal static void DisposeStates ()
		{
			Default		.Dispose();
			Readonly	.Dispose();
			None		.Dispose();
			LightMark	.Dispose();
			LightPass	.Dispose();
			Sky			.Dispose();
		}



		/// <summary>
		/// 
		/// </summary>
		void PipelineBoundCheck ()
		{
			if (state!=null) {
				throw new GraphicsException("DepthStencilState that has already been bound to the graphics pipeline can not be modified.");
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
		internal void Apply ( GraphicsDevice device )
		{
			if ( state == null ) {
				
				var dss	=	new DepthStencilStateDescription();

				dss.DepthComparison		=	Converter.Convert( this.depthComparison );
				dss.DepthWriteMask		=	this.depthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero;
				dss.IsDepthEnabled		=	this.depthEnabled;
				dss.IsStencilEnabled	=	this.stencilEnabled;
				dss.StencilReadMask		=	this.stencilReadMask;
				dss.StencilWriteMask	=	this.stencilWriteMask;

				dss.BackFace.Comparison				=	Converter.Convert( this.backStencilComparison	);
				dss.BackFace.FailOperation			=	Converter.Convert( this.backFailOp				);
				dss.BackFace.DepthFailOperation		=	Converter.Convert( this.backDepthFailOp			);
				dss.BackFace.PassOperation			=	Converter.Convert( this.backPassOp				);

				dss.FrontFace.Comparison			=	Converter.Convert( this.backStencilComparison	);
				dss.FrontFace.FailOperation			=	Converter.Convert( this.backFailOp				);
				dss.FrontFace.DepthFailOperation	=	Converter.Convert( this.backDepthFailOp			);
				dss.FrontFace.PassOperation			=	Converter.Convert( this.backPassOp				);

				state	=	new D3DDepthStencilState( device.Device, dss );
			}

			device.DeviceContext.OutputMerger.DepthStencilState		=	state;
			device.DeviceContext.OutputMerger.DepthStencilReference	=	stencilReference;
		}
	}
}
