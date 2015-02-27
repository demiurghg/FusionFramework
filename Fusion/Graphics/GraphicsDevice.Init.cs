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
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using Fusion.Native.NvApi;
using Device = SharpDX.Direct3D11.Device;
using System.IO;
using Fusion.Graphics;


namespace Fusion.Graphics {

	public partial class GraphicsDevice : DisposableBase {

		IntPtr	stereoHandle = new IntPtr(0);

		internal StereoMode	CurrentStereoMode { get; private set; }



		/// <summary>
		/// Sets fullscreen mode via swapchain.
		/// </summary>
		/// <param name="fullscr"></param>
		internal void SetNativeFullscreen ( bool fullscr )
		{
			swapChain.SetFullscreenState( fullscr, null );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		void InitializeInternal ( GameParameters parameters )
		{
			CurrentStereoMode	=	StereoMode.Disabled;
			Log.Message("Initializing : Graphics Device");

			Log.Message("   Mode: {0}x{1} {2}", parameters.Width, parameters.Height, parameters.StereoMode );

			//
			//	Init NVidia API :
			//	On non-NVidia GPUs this will no take effect.
			//
			try {
				Log.Message("   NVAPI");
				NvApiWrapper.Initialize();

				if (parameters.StereoMode==StereoMode.NV3DVision) {
					NvApiWrapper.Stereo_Enable();
					NvApiWrapper.Stereo_SetDriverMode( NvStereoDriverMode.Direct );
				} else {
					NvApiWrapper.Stereo_Disable();
				}

			} catch ( NVException nvex ) {
				Log.Error("Failed to initialize NVAPI: {0}", nvex.Message );
				parameters.StereoMode = StereoMode.Disabled;
			}



			//
			//	Select appropriate flags
			//
			var flags	=	DeviceCreationFlags.None;
				flags	|=	parameters.UseDebugDevice ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;
			var type	=	DriverType.Hardware;



			using ( var factory2 = new Factory() ) {

				Log.Message("Adapters:");

				foreach (var adapter in factory2.Adapters) {
					var aDesc = adapter.Description;
					Log.Message("   {0} - {1}", aDesc.Description, D3D.Device.GetSupportedFeatureLevel(adapter));
					
					foreach ( var output in adapter.Outputs ) {
						var desc = output.Description;
						var bnds = output.Description.DesktopBounds;
						var bndsString = string.Format("x:{0} y:{1} w:{2} h:{3}", bnds.Left, bnds.Right-bnds.Left, bnds.Top, bnds.Bottom-bnds.Top );

						Log.Message("   {0} [{1}] {2}", desc.DeviceName, bndsString, desc.Rotation );
					}
				}
			}



			//
			//	create swapchain and device :
			//
			swapChainDesc = new SwapChainDescription () {
				BufferCount			=	1,
				ModeDescription		=	new ModeDescription( parameters.Width, parameters.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm ),
				IsWindowed			=	true,
				OutputHandle		=	Game.PrimaryWindow.MasterForm.Handle,
				SampleDescription	=	new SampleDescription(parameters.MsaaLevel, 0),
				SwapEffect			=	SwapEffect.Discard,
				Usage				=	Usage.RenderTargetOutput,
				Flags				=	SwapChainFlags.None,
			};

			swapChainDesc2 = new SwapChainDescription () {
				BufferCount			=	1,
				ModeDescription		=	new ModeDescription( parameters.Width, parameters.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm ),
				IsWindowed			=	true,
				OutputHandle		=	Game.SecondaryWindow==null? new IntPtr(0) : Game.SecondaryWindow.MasterForm.Handle,
				SampleDescription	=	new SampleDescription(parameters.MsaaLevel, 0),
				SwapEffect			=	SwapEffect.Discard,
				Usage				=	Usage.RenderTargetOutput,
				Flags				=	SwapChainFlags.None,
			};

			var featureLevel		=	HardwareProfileChecker.GetFeatureLevel( parameters.HardwareProfile ); 


			if (parameters.StereoMode!=StereoMode.DualHead) {
				
				Device.CreateWithSwapChain( type, flags, new[]{ featureLevel }, swapChainDesc, out device, out swapChain );
				deviceContext	=	device.ImmediateContext;

				factory			=	swapChain.GetParent<Factory>();
				factory.MakeWindowAssociation( Game.Window.MasterForm.Handle, WindowAssociationFlags.IgnoreAll );

				CreateBackbuffer( parameters.Width, parameters.Height, swapChain );

			} else {

				Device.CreateWithSwapChain( type, flags, new[]{ featureLevel }, swapChainDesc, out device, out swapChain );
				deviceContext	=	device.ImmediateContext;

				factory			=	swapChain.GetParent<Factory>();
				factory.MakeWindowAssociation( Game.Window.MasterForm.Handle, WindowAssociationFlags.IgnoreAll );

				CreateBackbuffer( parameters.Width, parameters.Height, swapChain );

			}


		
			//
			//	create 3DVision stuff :
			//
			if (parameters.StereoMode==StereoMode.NV3DVision) {

				try {
					Log.Information("Using nVidia 3DVision");

					stereoHandle	=	NvApiWrapper.Stereo_CreateHandleFromIUnknown( device.NativePointer );

					NvApiWrapper.Stereo_Activate( stereoHandle );
					NvApiWrapper.Stereo_SetActiveEye( stereoHandle, NvStereoActiveEye.Mono );

					CurrentStereoMode	=	StereoMode.NV3DVision;
				} catch ( NVException nvex ) {
					Log.Error("Failed to initialize 3DVision: {0}", nvex.Message );
					parameters.StereoMode = StereoMode.Disabled;
				}
			}

		}

	}
}
