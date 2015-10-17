using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Native.NvApi;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics.Display {
	class StereoInterlacedDisplay : BaseDisplay {

		StereoEye[] eyeList = new[]{ StereoEye.Left, StereoEye.Right };

		SwapChain				swapChain		=	null;
		SwapChainDescription	swapChainDesc;
		Form					window;

		RenderTarget2D			backbufferColor1;
		RenderTarget2D			backbufferColor2;
		DepthStencil2D			backbufferDepth1;
		DepthStencil2D			backbufferDepth2;
		RenderTarget2D			backbufferColor1Resolved;
		RenderTarget2D			backbufferColor2Resolved;
		RenderTarget2D			backbufferColor;
		int clientWidth;
		int clientHeight;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		public StereoInterlacedDisplay( GameEngine game, GraphicsDevice device, GameParameters parameters ) : base( game, device, parameters )
		{
			try {
				NvApi.Initialize();
				NvApi.Stereo_Disable();
			}
			catch (NVException nvex) {
				Log.Debug(nvex.Message);
			}


			window	=	CreateForm( parameters, null );

			var deviceFlags			=	DeviceCreationFlags.None;
				deviceFlags			|=	parameters.UseDebugDevice ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;

			var driverType			=	DriverType.Hardware;

			var featureLevel	=	HardwareProfileChecker.GetFeatureLevel( parameters.GraphicsProfile ); 


			swapChainDesc = new SwapChainDescription () {
				BufferCount			=	1,
				ModeDescription		=	new ModeDescription( parameters.Width, parameters.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm ),
				IsWindowed			=	true,
				OutputHandle		=	window.Handle,
				SampleDescription	=	new SampleDescription(parameters.MsaaLevel, 0),
				SwapEffect			=	SwapEffect.Discard,
				Usage				=	Usage.RenderTargetOutput,
				Flags				=	SwapChainFlags.None,
			};


			D3D.Device.CreateWithSwapChain( driverType, deviceFlags, new[]{ featureLevel }, swapChainDesc, out d3dDevice, out swapChain );


			var factory		=	swapChain.GetParent<Factory>();
			factory.MakeWindowAssociation( window.Handle, WindowAssociationFlags.IgnoreAll );


			clientWidth		=	window.ClientSize.Width;
			clientHeight	=	window.ClientSize.Height;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void CreateDisplayResources ()
		{
			base.CreateDisplayResources();

			backbufferColor = new RenderTarget2D(device, swapChain.GetBackBuffer<D3D.Texture2D>(0));

			int ms	= backbufferColor.SampleCount;
			int w	= backbufferColor.Width;
			int h	= backbufferColor.Height;

			backbufferColor1	=	new RenderTarget2D( device, ColorFormat.Rgba8, w, h, ms );
			backbufferDepth1	=	new DepthStencil2D( device, DepthFormat.D24S8, w, h, ms );
			backbufferColor2	=	new RenderTarget2D( device, ColorFormat.Rgba8, w, h, ms );
			backbufferDepth2	=	new DepthStencil2D( device, DepthFormat.D24S8, w, h, ms );

			if (ms>1) {
				backbufferColor1Resolved	=	new RenderTarget2D( device, ColorFormat.Rgba8, w, h );
				backbufferColor2Resolved	=	new RenderTarget2D( device, ColorFormat.Rgba8, w, h );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Update ()
		{
			if ( clientWidth!=window.ClientSize.Width || clientHeight!=window.ClientSize.Height ) {

				clientWidth		=	window.ClientSize.Width;
				clientHeight	=	window.ClientSize.Height;

				SafeDispose( ref backbufferColor );
				SafeDispose( ref backbufferColor1 );
				SafeDispose( ref backbufferDepth1 );
				SafeDispose( ref backbufferColor2 );
				SafeDispose( ref backbufferDepth2 );

				SafeDispose( ref backbufferColor1Resolved );
				SafeDispose( ref backbufferColor2Resolved );

				swapChain.ResizeBuffers( swapChainDesc.BufferCount, Bounds.Width, Bounds.Height, Format.R8G8B8A8_UNorm, swapChainDesc.Flags );

				CreateDisplayResources();

				device.NotifyViewportChanges();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref backbufferColor );
				SafeDispose( ref backbufferColor1 );
				SafeDispose( ref backbufferDepth1 );
				SafeDispose( ref backbufferColor2 );
				SafeDispose( ref backbufferDepth2 );

				SafeDispose( ref backbufferColor1Resolved );
				SafeDispose( ref backbufferColor2Resolved );

				SafeDispose( ref swapChain );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Prepare ()
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="syncInterval"></param>
		public override void SwapBuffers( int syncInterval )
		{
			Flags flag = Flags.HORIZONTAL_LR;

			var location	=	window.ClientRectangle.Location;

			var interlacingMode	=	InterlacingMode.VerticalLR;
			
			if (location.X%2==0) {
				if (interlacingMode==InterlacingMode.VerticalLR)   flag = Flags.VERTICAL_LR;
				if (interlacingMode==InterlacingMode.VerticalRL)   flag = Flags.VERTICAL_RL;
			} else {
				if (interlacingMode==InterlacingMode.VerticalLR)   flag = Flags.VERTICAL_RL;
				if (interlacingMode==InterlacingMode.VerticalRL)   flag = Flags.VERTICAL_LR;
			}

			if (location.Y%2==0) {
				if (interlacingMode==InterlacingMode.HorizontalLR) flag = Flags.HORIZONTAL_LR;
				if (interlacingMode==InterlacingMode.HorizontalRL) flag = Flags.HORIZONTAL_RL;
			} else {
				if (interlacingMode==InterlacingMode.HorizontalLR) flag = Flags.HORIZONTAL_RL;
				if (interlacingMode==InterlacingMode.HorizontalRL) flag = Flags.HORIZONTAL_LR;
			}
			
			MergeStereoBuffers( backbufferColor1, backbufferColor2, backbufferColor1Resolved, backbufferColor2Resolved, backbufferColor, flag );

			swapChain.Present( syncInterval, PresentFlags.None );
		}



		bool fullscr = false;


		/// <summary>
		/// Gets and sets fullscreen mode.
		/// </summary>
		public override bool Fullscreen {
			get	{
				return fullscr;
			}
			set {
				if (value!=fullscr) {
					fullscr = value;

					if (fullscr) {
						window.FormBorderStyle	=	FormBorderStyle.None;
						window.WindowState		=	FormWindowState.Maximized;
						window.TopMost			=	true;
					} else {
						window.FormBorderStyle	=	FormBorderStyle.Sizable;
						window.WindowState		=	FormWindowState.Normal;
						window.TopMost			=	false;
					}
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override Rectangle Bounds
		{
			get { 
				return new Rectangle( 0, 0, window.ClientSize.Width, window.ClientSize.Height );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override Form Window
		{
			get { return window; }
		}

		
		/// <summary>
		/// 
		/// </summary>
		public override StereoEye TargetEye {
			get; set;
		}


		/// <summary>
		/// List of stereo eye to render.
		/// </summary>
		public override StereoEye[] StereoEyeList {
			get { return eyeList; }
		}



		/// <summary>
		/// 
		/// </summary>
		public override RenderTarget2D	BackbufferColor {
			get { 
				if (TargetEye==StereoEye.Left) {
					return backbufferColor1; 
				} else if ( TargetEye==StereoEye.Right ) {
					return backbufferColor2; 
				} else {
					throw new InvalidOperationException("TargetEye must be StereoEye.Left or StereoEye.Right");
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override DepthStencil2D	BackbufferDepth {
			get {
				if (TargetEye==StereoEye.Left) {
					return backbufferDepth1; 
				} else if ( TargetEye==StereoEye.Right ) {
					return backbufferDepth2; 
				} else {
					throw new InvalidOperationException("TargetEye must be StereoEye.Left or StereoEye.Right");
				}
			}
		}
	}
}
