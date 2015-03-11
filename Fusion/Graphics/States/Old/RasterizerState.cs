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
using D3DCullMode			=	SharpDX.Direct3D11.CullMode;


namespace Fusion.Graphics {
	public class RasterizerState : DisposableBase {

		public CullMode	CullMode			{ get { return cullMode			; } set { PipelineBoundCheck(); cullMode			=	value; } }
		public int		DepthBias			{ get { return depthBias		; } set { PipelineBoundCheck(); depthBias			=	value; } }
		public float	SlopeDepthBias		{ get { return slopeDepthBias	; } set { PipelineBoundCheck(); slopeDepthBias		=	value; } }
		public bool		MsaaEnabled			{ get { return msaaEnabled		; } set { PipelineBoundCheck(); msaaEnabled			=	value; } }
		public FillMode	FillMode			{ get { return fillMode			; } set { PipelineBoundCheck(); fillMode			=	value; } }
		public bool		DepthClipEnabled	{ get { return depthClipEnabled	; } set { PipelineBoundCheck(); depthClipEnabled	=	value; } }
		public bool		ScissorEnabled		{ get { return scissorEnabled	; } set { PipelineBoundCheck(); scissorEnabled		=	value; } }

		CullMode	cullMode			=	CullMode.CullNone;
		int			depthBias			=	0;
		float		slopeDepthBias		=	0;
		bool		msaaEnabled			=	true;
		FillMode	fillMode			=	FillMode.Solid;
		bool		depthClipEnabled	=	true;
		bool		scissorEnabled		=	false;

		D3DRasterizerState	state;


		public static RasterizerState	CullNone	{ get; set; }
		public static RasterizerState	CullCW		{ get; set; }
		public static RasterizerState	CullCCW		{ get; set; }
		public static RasterizerState	Wireframe	{ get; set; }


		/// <summary>
		/// 
		/// </summary>
		void PipelineBoundCheck ()
		{
			if (state!=null) {
				throw new GraphicsException("Rasterizer state that has already been bound to the graphics pipeline can not be modified.");
			}
		}


		/// <summary>
		/// 
		/// </summary>
		static RasterizerState ()
		{
			CullNone	=	new RasterizerState() { CullMode = CullMode.CullNone };
			CullCW		=	new RasterizerState() { CullMode = CullMode.CullCW };
			CullCCW		=	new RasterizerState() { CullMode = CullMode.CullCCW };
			Wireframe	=	new RasterizerState() { FillMode = FillMode.Wireframe };
		}



		/// <summary>
		/// 
		/// </summary>
		internal static void DisposeStates ()
		{
			CullNone	.Dispose();
			CullCW		.Dispose();
			CullCCW		.Dispose();
			Wireframe	.Dispose();
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
		/// Applies state to the pipeline
		/// </summary>
		/// <param name="device"></param>
		internal void Apply ( GraphicsDevice device )
		{
			if ( state == null ) {

				var rsd = new RasterizerStateDescription();

				if ( cullMode == CullMode.CullNone ) {

					rsd.CullMode				=	D3DCullMode.None;
					rsd.IsFrontCounterClockwise	=	false;

				} else if ( cullMode == CullMode.CullCW ) {

					rsd.CullMode				=	D3DCullMode.Front;
					rsd.IsFrontCounterClockwise	=	false;

				} else if ( cullMode == CullMode.CullCCW ) {

					rsd.CullMode				=	D3DCullMode.Front;
					rsd.IsFrontCounterClockwise	=	true;
				}


				rsd.FillMode				=	Converter.Convert( this.fillMode );
				rsd.DepthBias				=	this.depthBias;
				rsd.DepthBiasClamp			=	0;
				rsd.IsMultisampleEnabled	=	this.msaaEnabled;
				rsd.IsScissorEnabled		=	this.scissorEnabled;
				rsd.SlopeScaledDepthBias	=	this.slopeDepthBias;

				state	=	new D3DRasterizerState( device.Device, rsd );
			}

			device.DeviceContext.Rasterizer.State	=	state;
		}
	}
}
