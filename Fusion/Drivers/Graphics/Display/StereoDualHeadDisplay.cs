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
using Drawing = System.Drawing;
using Fusion.Core.Mathematics;
using Native.NvApi;
using Fusion.Core;
using Fusion.Engine.Common;


namespace Fusion.Drivers.Graphics.Display {
	class StereoDualHeadDisplay : BaseDisplay {

		StereoEye[] eyeList = new[]{ StereoEye.Left, StereoEye.Right };

		SwapChain				swapChain1		=	null;
		SwapChain				swapChain2		=	null;
		SwapChainDescription	swapChainDesc1;
		SwapChainDescription	swapChainDesc2;
		Form					window1;
		Form					window2;

		RenderTarget2D			backbufferColor1;
		RenderTarget2D			backbufferColor2;
		DepthStencil2D			backbufferDepth1;
		DepthStencil2D			backbufferDepth2;
		int clientWidth;
		int clientHeight;

											   
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		public StereoDualHeadDisplay( GameEngine game, GraphicsDevice device, GameParameters parameters ) : base( game, device, parameters )
		{
			try {
				NvApi.Initialize();
				NvApi.Stereo_Disable();
			}
			catch (NVException nvex) {
				Log.Debug(nvex.Message);
			}

			var featureLevel	=	HardwareProfileChecker.GetFeatureLevel( parameters.GraphicsProfile ); 

			Adapter	adapter;
			Output leftOut, rightOut;

			GetDualHeadAdapter( featureLevel, out adapter, out leftOut, out rightOut );
			


			window1	=	CreateForm( parameters, leftOut );
			window2	=	CreateForm( parameters, rightOut );
			window1.Tag	=	leftOut.Description;
			window2.Tag	=	rightOut.Description;

			window1.AddOwnedForm( window2 );

			window2.Show();
			window1.Show();

			window1.Resize += window_Resize;
			window2.Resize += window_Resize;
			window1.Move += window_Move;	
			window2.Move += window_Move;
			window1.Activated += window_Activated;
			window2.Activated += window_Activated;
			targetForm	=	window1;



			var deviceFlags			=	DeviceCreationFlags.None;
				deviceFlags			|=	parameters.UseDebugDevice ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;


			swapChainDesc1 = new SwapChainDescription () {
				BufferCount			=	1,
				ModeDescription		=	new ModeDescription( parameters.Width, parameters.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm ),
				IsWindowed			=	true,
				OutputHandle		=	window1.Handle,
				SampleDescription	=	new SampleDescription(parameters.MsaaLevel, 0),
				SwapEffect			=	SwapEffect.Discard,
				Usage				=	Usage.RenderTargetOutput,
				Flags				=	SwapChainFlags.None,
			};

			swapChainDesc2 = new SwapChainDescription () {
				BufferCount			=	1,
				ModeDescription		=	new ModeDescription( parameters.Width, parameters.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm ),
				IsWindowed			=	true,
				OutputHandle		=	window2.Handle,
				SampleDescription	=	new SampleDescription(parameters.MsaaLevel, 0),
				SwapEffect			=	SwapEffect.Discard,
				Usage				=	Usage.RenderTargetOutput,
				Flags				=	SwapChainFlags.None,
			};


			d3dDevice	=	new D3D.Device( adapter, deviceFlags, featureLevel );


			using ( var factory = adapter.GetParent<Factory>() ) {
			
				swapChain1	=	new SwapChain( factory, d3dDevice, swapChainDesc1 );
				swapChain2	=	new SwapChain( factory, d3dDevice, swapChainDesc2 );

				//factory.MakeWindowAssociation( new IntPtr(0), WindowAssociationFlags.None );
				factory.MakeWindowAssociation( window1.Handle, WindowAssociationFlags.IgnoreAll | WindowAssociationFlags.IgnoreAltEnter );
				factory.MakeWindowAssociation( window2.Handle, WindowAssociationFlags.IgnoreAll | WindowAssociationFlags.IgnoreAltEnter );
			}


			clientWidth		=	window1.ClientSize.Width;
			clientHeight	=	window1.ClientSize.Height;
		}


		Form targetForm;


		void window_Activated ( object sender, EventArgs e )
		{
			targetForm	=	sender as Form;
		}




		void window_Move ( object sender, EventArgs e )
		{
			var b1 = ((OutputDescription)window1.Tag).DesktopBounds;
			var b2 = ((OutputDescription)window2.Tag).DesktopBounds;

			if (sender==window1) {
				window2.Move -= window_Move;
				window2.Location = new Drawing.Point( window1.Location.X - b1.Left + b2.Left, window1.Location.Y - b1.Top + b2.Top );
				window2.Move += window_Move;
			}

			
			if (sender==window2) {
				window1.Move -= window_Move;
				window1.Location = new Drawing.Point( window2.Location.X - b2.Left + b1.Left, window2.Location.Y - b2.Top + b1.Top );
				window1.Move += window_Move;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void window_Resize ( object sender, EventArgs e )
		{
			if (sender==window1) {
				window2.Resize -= window_Resize;
				window2.Size = window1.Size;
				window2.Resize += window_Resize;
			}
			if (sender==window2) {
				window1.Resize -= window_Resize;
				window1.Size = window2.Size;
				window1.Resize += window_Resize;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="adapter"></param>
		/// <param name="leftOut"></param>
		/// <param name="rightOut"></param>
		void GetDualHeadAdapter ( FeatureLevel fl, out Adapter adapter, out Output leftOut, out Output rightOut )
		{
			using ( var factory2 = new Factory() ) {

				adapter	=	factory2.Adapters.FirstOrDefault( a => a.Outputs.Length>=2 && D3D.Device.IsSupportedFeatureLevel( a, fl ) );

				if (adapter==null) {
					throw new GraphicsException("No DualHead adapters with Direct3D11 support found.");
				}

				leftOut		=	adapter.Outputs[0];
				rightOut	=	adapter.Outputs[1];

				if (leftOut.Description.DesktopBounds.Left > rightOut.Description.DesktopBounds.Left ) {
					Misc.Swap( ref leftOut, ref rightOut );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override void CreateDisplayResources ()
		{
			base.CreateDisplayResources();

			backbufferColor1	=	new RenderTarget2D( device, D3D.Texture2D.FromSwapChain<D3D.Texture2D>( swapChain1, 0 ) );
			backbufferDepth1	=	new DepthStencil2D( device, DepthFormat.D24S8, backbufferColor1.Width, backbufferColor1.Height, backbufferColor1.SampleCount );

			backbufferColor2	=	new RenderTarget2D( device, D3D.Texture2D.FromSwapChain<D3D.Texture2D>( swapChain2, 0 ) );
			backbufferDepth2	=	new DepthStencil2D( device, DepthFormat.D24S8, backbufferColor2.Width, backbufferColor2.Height, backbufferColor2.SampleCount );
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Update ()
		{
			if ( clientWidth!=window1.ClientSize.Width || clientHeight!=window1.ClientSize.Height ) {

				clientWidth		=	window1.ClientSize.Width;
				clientHeight	=	window1.ClientSize.Height;

				SafeDispose( ref backbufferColor1 );
				SafeDispose( ref backbufferDepth1 );
				SafeDispose( ref backbufferColor2 );
				SafeDispose( ref backbufferDepth2 );

				swapChain1.ResizeBuffers( swapChainDesc1.BufferCount, Bounds.Width, Bounds.Height, Format.R8G8B8A8_UNorm, swapChainDesc1.Flags );
				swapChain2.ResizeBuffers( swapChainDesc2.BufferCount, Bounds.Width, Bounds.Height, Format.R8G8B8A8_UNorm, swapChainDesc2.Flags );

				CreateDisplayResources();

				device.NotifyViewportChanges();
			} //*/
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref backbufferColor1 );
				SafeDispose( ref backbufferDepth1 );
				SafeDispose( ref backbufferColor2 );
				SafeDispose( ref backbufferDepth2 );
				SafeDispose( ref swapChain1 );
				SafeDispose( ref swapChain2 );
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
			swapChain1.Present( 0,			  PresentFlags.None );
			swapChain2.Present( syncInterval, PresentFlags.None );
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
						window1.FormBorderStyle	=	FormBorderStyle.None;
						window1.WindowState		=	FormWindowState.Maximized;
						window1.TopMost			=	true;
						window2.FormBorderStyle	=	FormBorderStyle.None;
						window2.WindowState		=	FormWindowState.Maximized;
						window2.TopMost			=	true;
					} else {
						window1.FormBorderStyle	=	FormBorderStyle.Sizable;
						window1.WindowState		=	FormWindowState.Normal;
						window1.TopMost			=	false;
						window2.FormBorderStyle	=	FormBorderStyle.Sizable;
						window2.WindowState		=	FormWindowState.Normal;
						window2.TopMost			=	false;
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
				return new Rectangle( 0, 0, window1.ClientSize.Width, window1.ClientSize.Height );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override Form Window
		{
			get { return targetForm; }
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
