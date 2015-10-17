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
using Native.NvApi;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics.Display {
	class NV3DVisionDisplay : BaseDisplay {

		StereoEye[] eyeList = new[]{ StereoEye.Left, StereoEye.Right };

		SwapChain				swapChain		=	null;
		SwapChainDescription	swapChainDesc;
		Form					window;

		IntPtr	stereoHandle = new IntPtr(0);
		RenderTarget2D			backbufferColor;
		DepthStencil2D			backbufferDepth;
		int clientWidth;
		int clientHeight;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		public NV3DVisionDisplay( GameEngine game, GraphicsDevice device, GameParameters parameters ) : base( game, device, parameters )
		{
			Log.Message("Using NVidia 3D Vision");

			//
			//	Init NV API and enable stereo :
			//	
			NvApi.Initialize();
			NvApi.Stereo_Enable();
			NvApi.Stereo_SetDriverMode( NvStereoDriverMode.Direct );


			//
			//	Create main window
			//	
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


			//
			//	Create device :
			//
			D3D.Device.CreateWithSwapChain( driverType, deviceFlags, new[]{ featureLevel }, swapChainDesc, out d3dDevice, out swapChain );


			var factory		=	swapChain.GetParent<Factory>();
			factory.MakeWindowAssociation( window.Handle, WindowAssociationFlags.IgnoreAll );


			clientWidth		=	window.ClientSize.Width;
			clientHeight	=	window.ClientSize.Height;


			//
			//	Setup 3DVision :
			//

			try {
				stereoHandle	=	NvApi.Stereo_CreateHandleFromIUnknown( d3dDevice.NativePointer );

				NvApi.Stereo_Activate( stereoHandle );
				NvApi.Stereo_SetActiveEye( stereoHandle, NvStereoActiveEye.Mono );

			} catch ( NVException ) {
				SafeDispose( ref d3dDevice );
				SafeDispose( ref swapChain );
				throw;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override void CreateDisplayResources ()
		{
			base.CreateDisplayResources();

			backbufferColor	=	new RenderTarget2D( device, D3D.Texture2D.FromSwapChain<D3D.Texture2D>( swapChain, 0 ) );
			backbufferDepth	=	new DepthStencil2D( device, DepthFormat.D24S8, backbufferColor.Width, backbufferColor.Height, backbufferColor.SampleCount );
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
				SafeDispose( ref backbufferDepth );

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
				NvApi.Stereo_Deactivate( stereoHandle );
				NvApi.Stereo_DestroyHandle( stereoHandle );

				SafeDispose( ref backbufferColor );
				SafeDispose( ref backbufferDepth );
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
					swapChain.SetFullscreenState( value, null );
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



		StereoEye targetEye = StereoEye.Mono;

		/// <summary>
		/// Target stereo eye
		/// </summary>
		public override StereoEye TargetEye {
			get {
				return targetEye;
			}
			set {
				targetEye = value;
				if (targetEye==StereoEye.Mono) NvApi.Stereo_SetActiveEye( stereoHandle, NvStereoActiveEye.Mono ); else
				if (targetEye==StereoEye.Left) NvApi.Stereo_SetActiveEye( stereoHandle, NvStereoActiveEye.Left ); else
				if (targetEye==StereoEye.Right) NvApi.Stereo_SetActiveEye( stereoHandle, NvStereoActiveEye.Right ); else
					throw new ArgumentException("value");
			}
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
			get { return backbufferColor; }
		}



		/// <summary>
		/// 
		/// </summary>
		public override DepthStencil2D	BackbufferDepth {
			get { return backbufferDepth; }
		}





	}
}
