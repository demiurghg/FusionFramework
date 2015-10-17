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
	class GenericDisplay : BaseDisplay {

		StereoEye[] eyeList = new[]{ StereoEye.Mono };

		SwapChain				swapChain		=	null;
		SwapChainDescription	swapChainDesc;
		Form					window;

		RenderTarget2D			backbufferColor;
		DepthStencil2D			backbufferDepth;
		int clientWidth;
		int clientHeight;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		public GenericDisplay( GameEngine game, GraphicsDevice device, GameParameters parameters ) : base( game, device, parameters )
		{
			window	=	CreateForm( parameters, null );

			try {
				NvApi.Initialize();
				NvApi.Stereo_Disable();
			}
			catch (NVException nvex) {
				Log.Debug(nvex.Message);
			}

			//var deviceFlags			=	DeviceCreationFlags.SingleThreaded;
			var deviceFlags			=	DeviceCreationFlags.None;
				deviceFlags			|=	parameters.UseDebugDevice ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;

			var driverType			=	DriverType.Hardware;

			var featureLevel	=	HardwareProfileChecker.GetFeatureLevel( parameters.GraphicsProfile ); 


			swapChainDesc = new SwapChainDescription () {
				BufferCount			=	2,
				ModeDescription		=	new ModeDescription( parameters.Width, parameters.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm ),
				IsWindowed			=	true,
				OutputHandle		=	window.Handle,
				SampleDescription	=	new SampleDescription(parameters.MsaaLevel, 0),
				SwapEffect			=	SwapEffect.Discard,
				Usage				=	Usage.RenderTargetOutput,
				Flags				=	SwapChainFlags.None,
			};


			D3D.Device.CreateWithSwapChain( driverType, deviceFlags, new[]{ featureLevel }, swapChainDesc, out d3dDevice, out swapChain );

			//Log.Message("   compute shaders : {0}", d3dDevice.CheckFeatureSupport(Feature.ComputeShaders) );
			//Log.Message("   shader doubles  : {0}", d3dDevice.CheckFeatureSupport(Feature.ShaderDoubles) );
			//Log.Message("   threading       : {0}", d3dDevice.CheckFeatureSupport(Feature.Threading) );
			bool driverConcurrentCreates;
			bool driverCommandLists;
			d3dDevice.CheckThreadingSupport( out driverConcurrentCreates, out driverCommandLists );
			d3dDevice.GetCounterCapabilities();
			Log.Message("   Concurrent Creates : {0}", driverConcurrentCreates );
			Log.Message("   Command Lists      : {0}", driverCommandLists );


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
				SafeDispose( ref backbufferColor );
				SafeDispose( ref backbufferDepth );
				SafeDispose( ref swapChain );
			}
			base.Dispose( disposing );
		}



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
