﻿using System;
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
using Fusion.Input.Touch;
using Forms = System.Windows.Forms;
using Fusion.Mathematics;


namespace Fusion.Graphics.Display {
	abstract class BaseDisplay : GraphicsResource {

		protected readonly	Game Game;
		public 		D3D.Device d3dDevice = null;

		protected Ubershader	stereo;
		protected StateFactory	factory;

		protected enum Flags {
			VERTICAL_LR		=	0x0001,
			VERTICAL_RL		=	0x0002,
			HORIZONTAL_LR	=	0x0004,
			HORIZONTAL_RL	=	0x0008,
			OCULUS_RIFT		=	0x0010,
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		public BaseDisplay( Game game, GraphicsDevice device, GameParameters parameters ) : base(device)
		{
			this.Game	=	game;

			ShowAdapterInfo( parameters );
		}



		/// <summary>
		/// 
		/// </summary>
		public virtual void CreateDisplayResources ()
		{
			Game.Reloading += (s,e) => LoadContent();
			LoadContent();
		}



		/// <summary>
		/// 
		/// </summary>
		void LoadContent ()
		{
			stereo	=	Game.Content.Load<Ubershader>("stereo");
			factory	=	new StateFactory( stereo, typeof(Flags), Primitive.TriangleList, VertexInputElement.Empty, BlendState.Opaque, RasterizerState.CullNone, DepthStencilState.None );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="left">Left source buffer</param>
		/// <param name="right">Right source buffer</param>
		/// <param name="leftResolved">Buffer to resolve left MSAA buffer. (NULL if left buffer is not MSAA buffer)</param>
		/// <param name="rightResolved">Buffer to resolve right MSAA buffer. (NULL if right buffer is not MSAA buffer)</param>
		/// <param name="destination">Target buffer</param>
		/// <param name="mode">Ubershader flag</param>
		protected void MergeStereoBuffers ( RenderTarget2D left, RenderTarget2D right, RenderTarget2D leftResolved, RenderTarget2D rightResolved, RenderTarget2D destination, Flags flag )
		{
			device.ResetStates();

			device.SetTargets( null, destination );

			if (leftResolved!=null) {
				device.Resolve( left, leftResolved );
			} 
			if (rightResolved!=null) {
				device.Resolve( right, rightResolved );
			} 


			device.PipelineState		=	factory[ (int)flag ];

			device.PixelShaderSamplers[0]	=	SamplerState.LinearClamp;
			device.PixelShaderResources[0]	=	leftResolved  == null ? left  : leftResolved;
			device.PixelShaderResources[1]	=	rightResolved == null ? right : rightResolved;

			device.SetupVertexInput( null, null, null );
			device.Draw( 3, 0 );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref d3dDevice );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Current stereo eye
		/// </summary>
		public abstract StereoEye TargetEye {
			get; set;
		}



		/// <summary>
		/// List of stereo eye to render.
		/// </summary>
		public abstract StereoEye[] StereoEyeList {
			get;
		}



		/// <summary>
		/// Get backbuffer
		/// </summary>
		public abstract RenderTarget2D	BackbufferColor {
			get;
		}



		/// <summary>
		/// Gets default depth buffer
		/// </summary>
		public abstract DepthStencil2D	BackbufferDepth {
			get;
		}



		/// <summary>
		/// Sets and gets fullscreen mode
		/// </summary>
		public abstract bool Fullscreen {
			get;
			set;
		}



		/// <summary>
		/// Gets display bounds.
		/// </summary>
		public abstract Rectangle Bounds {
			get;
		}



		public abstract Form Window {
			get;
		}



		/// <summary>
		/// 
		/// </summary>
		public abstract void Prepare ();



		/// <summary>
		/// 
		/// </summary>
		/// <param name="syncInterval"></param>
		public abstract void SwapBuffers ( int syncInterval );



		public abstract void Update ();


		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public Form CreateForm ( GameParameters parameters, Output output )
		{
			var form = new Form() {
				Text			=	parameters.Title,
				BackColor		=	System.Drawing.Color.Black,
				ClientSize		=	new System.Drawing.Size( parameters.Width, parameters.Height ),
				Icon			=	parameters.Icon ?? Fusion.Properties.Resources.fusionIcon,
				ControlBox		=	false,
				StartPosition	=	output==null ? FormStartPosition.CenterScreen : FormStartPosition.Manual,
			};


			if (output!=null) {

				var bounds		=	output.Description.DesktopBounds;
				var scrW		=	bounds.Right - bounds.Left;
				var scrH		=	bounds.Bottom - bounds.Top;
				
				form.Location	=	new System.Drawing.Point( bounds.Left + (scrW - form.Width)/2, bounds.Top + (scrH - form.Height)/2 );
				form.Text		+=	" - [" + output.Description.DeviceName + "]";
			}

			form.KeyDown += form_KeyDown;
			form.KeyUp += form_KeyUp;
			form.KeyPress += form_KeyPress;
			form.Resize += (s,e) => Game.InputDevice.RemoveAllPressedKey();
			form.Move += (s,e) => Game.InputDevice.RemoveAllPressedKey();

			return form;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public Form CreateTouchForm(GameParameters parameters, Output output)
		{
			var form = new TouchForm()
			{
				Text		= parameters.Title,
				BackColor	= System.Drawing.Color.Black,
				ClientSize	= new System.Drawing.Size(parameters.Width, parameters.Height),
				Icon		= parameters.Icon ?? Fusion.Properties.Resources.fusionIcon,
				ControlBox	= false,
				StartPosition = output == null ? FormStartPosition.CenterScreen : FormStartPosition.Manual,
			};


			if (output != null)
			{

				var bounds = output.Description.DesktopBounds;
				var scrW = bounds.Right - bounds.Left;
				var scrH = bounds.Bottom - bounds.Top;

				form.Location = new System.Drawing.Point(bounds.Left + (scrW - form.Width) / 2, bounds.Top + (scrH - form.Height) / 2);
				form.Text += " - [" + output.Description.DeviceName + "]";
			}

			form.KeyDown += form_KeyDown;
			form.KeyUp += form_KeyUp;
			form.KeyPress += form_KeyPress;
			form.Resize += (s, e) => Game.InputDevice.RemoveAllPressedKey();
			form.Move += (s, e) => Game.InputDevice.RemoveAllPressedKey();

			form.TouchTap		+= (pos) => Game.InputDevice.NotifyTouchTap(pos);
			form.TouchDoubleTap += (pos) => Game.InputDevice.NotifyTouchDoubleTap(pos);
			form.TouchSecondaryTap += (pos) => Game.InputDevice.NotifyTouchSecondaryTap(pos);
			form.TouchManipulation += (center, delta, scale) => Game.InputDevice.NotifyTouchManipulation(center, delta, scale);

			return form;
		}



		void form_KeyPress ( object sender, KeyPressEventArgs e )
		{
			Game.InputDevice.NotifyKeyPress( e.KeyChar );
		}



		void form_KeyUp ( object sender, KeyEventArgs e )
		{
			Game.InputDevice.NotifyKeyUp( (Fusion.Input.Keys)(int)e.KeyCode, e.Alt, e.Shift, e.Control );
		}



		void form_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.Alt && e.KeyCode==Forms.Keys.Enter) {
				Fullscreen = !Fullscreen;
			}

			Game.InputDevice.NotifyKeyDown( (Fusion.Input.Keys)(int)e.KeyCode, e.Alt, e.Shift, e.Control );
		}



		/// <summary>
		/// 
		/// </summary>
		protected void ShowAdapterInfo ( GameParameters parameters )
		{
			Log.Message("Mode : {0}x{1} {3} MS:{2} Stereo:{5} {4}", 
				parameters.Width, 
				parameters.Height, 
				parameters.MsaaLevel,
				parameters.FullScreen ? "FS" : "W", 
				parameters.UseDebugDevice ? "(Debug)" : "",
				parameters.StereoMode );

			using ( var factory2 = new Factory() ) {

				Log.Message("Adapters:");

				try {
					foreach (var adapter in factory2.Adapters) {
						var aDesc = adapter.Description;
						Log.Message("   {0} - {1}", aDesc.Description, D3D.Device.GetSupportedFeatureLevel(adapter));
					
						foreach ( var output in adapter.Outputs ) {
							var desc = output.Description;
							var bnds = output.Description.DesktopBounds;
							var bndsString = string.Format("x:{0} y:{1} w:{2} h:{3}", bnds.Left, bnds.Top, bnds.Right-bnds.Left, bnds.Bottom-bnds.Top );

							Log.Message("   {0} [{1}] {2}", desc.DeviceName, bndsString, desc.Rotation );
						}
					}
				} catch ( Exception e ) {
					Log.Warning( e.Message );
				}
			}
		}
	}
}
