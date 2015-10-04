using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Fusion.Core;

using D3DBlendState			=	SharpDX.Direct3D11.BlendState		;
using D3DSamplerState		=	SharpDX.Direct3D11.SamplerState		;
using D3DRasterizerState	=	SharpDX.Direct3D11.RasterizerState	;
using D3DDepthStencilState	=	SharpDX.Direct3D11.DepthStencilState;


namespace Fusion.Drivers.Graphics {

	public sealed class DepthStencilState : DisposableBase {

		//GraphicsDevice	device;
		
		public bool				DepthEnabled				{ get; set; }
		public bool				DepthWriteEnabled			{ get; set; }
		public ComparisonFunc	DepthComparison				{ get; set; }
		
		public bool				StencilEnabled				{ get; set; }
		public byte				StencilReadMask				{ get; set; }
		public byte				StencilWriteMask			{ get; set; }

		public StencilOp		FrontFaceFailOp				{ get; set; }
		public StencilOp		FrontFaceDepthFailOp		{ get; set; }
		public StencilOp		FrontFacePassOp				{ get; set; }
		public ComparisonFunc	FrontFaceStencilComparison	{ get; set; }
		
		public StencilOp		BackFaceFailOp				{ get; set; }
		public StencilOp		BackFaceDepthFailOp			{ get; set; }
		public StencilOp		BackFacePassOp				{ get; set; }
		public ComparisonFunc	BackFaceStencilComparison	{ get; set; }

		public int				StencilReference			{ get; set; }


		/*bool			depthEnabled			=	false;
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

		D3DDepthStencilState	state;	 */


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
				DepthEnabled			=	true,
				DepthComparison			=	ComparisonFunc.Less,
				DepthWriteEnabled		=	false,
				StencilEnabled			=	true,
				StencilReadMask			=	0xFF,
				StencilWriteMask		=	0xFF,
				FrontFaceFailOp				=	StencilOp.Keep,
				FrontFaceDepthFailOp		=	StencilOp.Increment,
				FrontFacePassOp				=	StencilOp.Keep,		
				FrontFaceStencilComparison	=	ComparisonFunc.Always,
				BackFaceFailOp				=	StencilOp.Keep,
				BackFaceDepthFailOp			=	StencilOp.Decrement,						
				BackFacePassOp				=	StencilOp.Keep,						
				BackFaceStencilComparison	=	ComparisonFunc.Always,	
			};

			LightPass	=	new DepthStencilState() {
				DepthEnabled			=	false,
				DepthComparison			=	ComparisonFunc.Less,
				DepthWriteEnabled		=	false,
				StencilEnabled			=	true,
				StencilReadMask			=	0xFF,
				StencilWriteMask		=	0xFF,
				FrontFaceFailOp				=	StencilOp.Keep,
				FrontFaceDepthFailOp		=	StencilOp.Keep,
				FrontFacePassOp				=	StencilOp.Decrement,	
				FrontFaceStencilComparison	=	ComparisonFunc.Equal,
				BackFaceFailOp				=	StencilOp.Keep,
				BackFaceDepthFailOp			=	StencilOp.Keep,
				BackFacePassOp				=	StencilOp.Decrement,	
				BackFaceStencilComparison	=	ComparisonFunc.Equal,
			};
		}

		#if false

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
		#endif
	}
}
