using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Development;

namespace DeferredDemo {
	public class DeferredDemo : Game {

		RenderTarget2D	ldrTarget;
		RenderTarget2D	hdrTarget;
		TextureAtlas	spotAtlas;



		/// <summary>
		/// SceneDemo constructor
		/// </summary>
		public DeferredDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = false;
			Parameters.Width		= 1280;
			Parameters.Height		= 720;
			Parameters.VSyncInterval = 1;
			//Parameters.StereoMode	=	StereoMode.NV3DVision;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );
			AddService( new SceneRenderer( this, @"Scenes\testScene" ), false, false, 1000, 1000 );
			AddService( new LightRenderer( this ), false, false, 1000, 1000 );
			AddService( new Sky( this ), false, false, 1000, 1000 );
			AddService( new Filter( this ), false, false, 1000, 1000 );
			AddService( new HdrFilter( this ), false, false, 1000, 1000 );
			AddService( new SsaoFilter( this ), false, false, 1000, 1000 );

			//	setup camera :
			GetService<Camera>().Config.FreeCamEnabled	=	true;
			GetService<Camera>().FreeCamPosition = Vector3.Up * 10;

			//	load configuration :
			LoadConfiguration();

			//	make configuration saved on exit
			Exiting += FusionGame_Exiting;
			InputDevice.KeyDown += InputDevice_KeyDown;

		}



		/// <summary>
		/// Initialize services :
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			LoadContent();
			Reloading += (s,e) => LoadContent();

			CreateTargets();
			GraphicsDevice.DisplayBoundsChanged += (s,e) => CreateTargets();


			var lr = GetService<LightRenderer>();
			var rand = new Random(542);
			
			for (int i=0; i<1024; i++) {
				var light = new OmniLight();
				light.RadiusOuter	=	rand.NextFloat(3, 4);
				light.RadiusInner	=	light.RadiusOuter * 0.5f;
				light.Position		=	rand.NextVector3( new Vector3(-50,3,-50), new Vector3(50,6,50) );
				//light.Position		=	Vector3.Up * 3; //new Vector3(10,2,20);
				//light.Position		=	new Vector3( (i/32)*3-48, 2.5f, (i%32)*3-48 );
				light.Intensity		=	rand.NextColor4()*50;// new Color4(10,10,5,0);
				lr.OmniLights.Add( light );
			}

			var names = spotAtlas.SubImageNames;

			lr.MaskAtlas	=	spotAtlas;

			for (int i=0; i<16; i++) {
				var light = new SpotLight();

				var position		=	rand.NextVector3( new Vector3(-30,20,-30), new Vector3(30,20,30) );
				var target			=	position + rand.NextVector3( new Vector3(-10, -35,-10), new Vector3(10, -35,10) );

				//position			=	rand.NextVector3( new Vector3(-50,50,-50), new Vector3(50,50,50) );
				//target				=	Vector3.Down * 20;

				light.Intensity		=	rand.NextColor4()*100;// new Color4(10,10,5,0);
				light.SpotView		=	Matrix.LookAtRH( position, target, Vector3.Up );
				light.RadiusOuter	=	(target - position).Length() * 1.5f;
				light.RadiusInner	=	light.RadiusOuter * 0.5f;
				light.MaskName		=	names[ rand.Next(0, names.Length) ];
				light.Projection	=	Matrix.PerspectiveFovRH( MathUtil.DegreesToRadians(45), 1, 1f, light.RadiusOuter );

				lr.SpotLights.Add( light );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public void LoadContent ()
		{
			spotAtlas	=	Content.Load<TextureAtlas>("spots|srgb");
		}



		/// <summary>
		/// Load content
		/// </summary>
		public void CreateTargets ()
		{
			var vp		=	GraphicsDevice.DisplayBounds;

			SafeDispose( ref hdrTarget );
			SafeDispose( ref ldrTarget );

			hdrTarget	=	new RenderTarget2D( GraphicsDevice, ColorFormat.Rgba16F, vp.Width, vp.Height, true );
			ldrTarget	=	new RenderTarget2D( GraphicsDevice, ColorFormat.Rgba8,   vp.Width, vp.Height, true );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {		
				SafeDispose( ref hdrTarget );
				SafeDispose( ref ldrTarget );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Handle keys for each demo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Fusion.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
				//DevCon.Show(this);
			}

			if (e.Key == Keys.F2) {
				Parameters.ToggleVSync();
			}

			if (e.Key == Keys.F5) {
				Reload();
			}

			if (e.Key == Keys.F12) {
				GraphicsDevice.Screenshot();
			}

			if (e.Key == Keys.Escape) {
				Exit();
			}
		}



		/// <summary>
		/// Save configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void FusionGame_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( Color.Orange, "FT  {0}", gameTime.Elapsed.TotalMilliseconds );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - reload content" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );

			var cam	=	GetService<Camera>();
			var dr	=	GetService<DebugRender>();
			var sr	=	GetService<SceneRenderer>();
			var lr	=	GetService<LightRenderer>();
			var sky	=	GetService<Sky>();

			dr.DrawGrid(10);

			base.Update( gameTime );

			dr.View			=	cam.GetViewMatrix( StereoEye.Mono );
			dr.Projection	=	cam.GetProjectionMatrix( StereoEye.Mono );

			lr.DirectLightDirection	=	sky.GetSunDirection();	
			lr.DirectLightIntensity	=	sky.GetSunLightColor();
			lr.AmbientLevel			=	sky.GetAmbientLevel();

			ds.Add( "{0} {1} {2}", lr.AmbientLevel.Red, lr.AmbientLevel.Green, lr.AmbientLevel.Blue );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			GraphicsDevice.ClearBackbuffer( Color.CornflowerBlue, 1, 0 );

			var vp		=	GraphicsDevice.DisplayBounds;
			var cam		=	GetService<Camera>();
			var sr		=	GetService<SceneRenderer>();
			var lr		=	GetService<LightRenderer>();
			var sb		=	GetService<SpriteBatch>();
			var sky		=	GetService<Sky>();
			var hdr		=	GetService<HdrFilter>();
			var flt		=	GetService<Filter>();
			var hbao	=	GetService<SsaoFilter>();

			GraphicsDevice.Clear( hdrTarget.Surface, Color.Black );

			lr.ClearGBuffer();

			var view		=	cam.GetViewMatrix( stereoEye );
			var proj		=	cam.GetProjectionMatrix( stereoEye );
			var viewMono	=	cam.GetViewMatrix( StereoEye.Mono );
			var projMono	=	cam.GetProjectionMatrix( StereoEye.Mono );

			
			//	render shadows once and 
			//	reuse the on next subframe:
			if (gameTime.SubframeID==0) {
				lr.RenderShadows( viewMono, projMono, sr );
			}

			//	render G-buffer :
			sr.RenderGBuffer ( view, proj, lr.DepthBuffer, hdrTarget, lr.DiffuseBuffer, lr.SpecularBuffer, lr.NormalMapBuffer );

			//	render sky :
			sky.Render( gameTime, lr.DepthBuffer.Surface, hdrTarget.Surface, view, proj );

			hbao.Render( view, proj, lr.DepthBuffer, lr.NormalMapBuffer );

			//	render lighting :
			lr.RenderLighting( view, proj, hdrTarget.Surface, hbao.OcclusionMap );

			//	perform HDR filter :
			hdr.Render( gameTime, ldrTarget.Surface, hdrTarget );

			//	perform FXAA :
			flt.Fxaa( GraphicsDevice.BackbufferColor.Surface, ldrTarget );


			GraphicsDevice.ResetStates();
			GraphicsDevice.RestoreBackbuffer();


			/*sb.Begin( BlendState.Opaque, SamplerState.LinearClamp );

				int w = vp.Width  / 2;
				int h = vp.Height / 2;

				if ( InputDevice.IsKeyDown( Keys.T ) ) {
					
					sb.Draw( hdrTarget			,	0, 0, w, h, Color.White );
					sb.Draw( lr.DiffuseBuffer	,	w, 0, w, h, Color.White );
					sb.Draw( lr.SpecularBuffer	,	0, h, w, h, Color.White );
					sb.Draw( lr.NormalMapBuffer	,	w, h, w, h, Color.White );

					sb.Draw( lr.CSMColor,  0,  0,400,100, Color.White );
					sb.Draw( lr.SpotColor, 0,100,400,400, Color.White );

				} else {

					sb.Draw( hdrTarget,	0, 0, vp.Width, vp.Height, Color.White );

				}

			sb.End();*/


			base.Draw( gameTime, stereoEye );
		}
	}
}
