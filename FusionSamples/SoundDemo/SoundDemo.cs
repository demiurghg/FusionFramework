using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Input;
using Fusion.Graphics;
using Fusion.Audio;
using Fusion.Development;


namespace SoundDemo {
	class SoundDemo : Game {

		/// <summary>
		/// 
		/// </summary>
		public SoundDemo ()
		{
			//	Add Camera service :
			Parameters.TrackObjects	=	true;
			Parameters.MsaaLevel	=	4;

			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );
		}



		SoundEffect	soundEffect;
		SoundEffect	soundEffectBite;
		SoundEffectInstance soundInstance = null;
		AudioEmitter	emitter		=	new AudioEmitter();
		AudioListener	listener	=	new AudioListener();



		/// <summary>
		/// 
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			var cam	=	GetService<Camera>();
			cam.Config.FreeCamEnabled	=	true;
			
			soundEffect		=	Content.Load<SoundEffect>("rfly.wav");
			soundEffectBite	=	Content.Load<SoundEffect>("break_glass3.wav");

			InputDevice.KeyDown+=InputDevice_KeyDown;

			LoadConfiguration();
			Exiting += SoundDemo_Exiting;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void SoundDemo_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}

			

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown(object sender, InputDevice.KeyEventArgs e)
		{
			if (e.Key == Keys.F1) {
				//DevCon.Show( this );
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

			if (e.Key==Keys.P) {
				if (soundInstance==null) {
					Log.Message("Play sound");
					soundInstance	=	soundEffect.CreateInstance();
					soundInstance.Apply3D( listener, emitter );
					soundInstance.IsLooped	=	true;
					soundInstance.Play();
				} else {
					soundInstance.Play();
				}
			}
			if (e.Key==Keys.I && soundInstance!=null) {
				soundInstance.Stop(false);
			}
			if (e.Key==Keys.O && soundInstance!=null) {
				var state = soundInstance.State;

				if (state==SoundState.Paused) {
					soundInstance.Resume();
				}
				if (state==SoundState.Playing) {
					soundInstance.Pause();
				}
			}
			if (e.Key==Keys.U && soundInstance!=null) {
				soundInstance.Stop(true);
			}

			if (e.Key==Keys.B) {
				soundEffectBite.Play();
			}
		}



		float angle = 0;

		Vector3 GetPosition ( float angle )
		{
			return new Vector3( 15*(float)Math.Cos(angle), 0, 15*(float)Math.Sin(angle) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds = GetService<DebugStrings>();

			var dr = GetService<DebugRender>();
			var cam = GetService<Camera>();

			dr.View			=	cam.GetViewMatrix( StereoEye.Mono );
			dr.Projection	=	cam.GetProjectionMatrix( StereoEye.Mono );;
			dr.DrawGrid(20);
			dr.DrawRing( Vector3.Zero, 15, Color.Orange, 64 );

			angle	+= 0.1f;

			if (emitter!=null) {
				emitter.DopplerScale	=	1;
				emitter.Position		=	GetPosition(angle);
				emitter.Velocity		=	(GetPosition(angle) - GetPosition(angle-0.1f)) * (1/gameTime.ElapsedSec);
				emitter.DistanceScale	=	2;
				//emitter.VolumeCurve		=	Enumerable.Range(0, 11).Select( i => new CurvePoint{ Distance = i, DspSetting = (float)Math.Pow((10-i)/10.0f,2) } ).ToArray();
				dr.DrawPoint( GetPosition(angle), 0.5f, Color.Yellow );
				dr.DrawRing( GetPosition(angle), 10, Color.Orange, 64 );
			}

			//AudioDevice.SetupListener( cam.GetCameraMatrix( stereoEye ).TranslationVector, cam.GetCameraMatrix( stereoEye ).Forward, cam.GetCameraMatrix( stereoEye ).Up, cam.FreeCameraVelocity );*/
			listener	=	cam.Listener;

			if (soundInstance!=null) {
				soundInstance.Apply3D( listener, emitter );
			}

			base.Update(gameTime);

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );
			ds.Add("");


			ds.Add("B - break glass");
			ds.Add("P - play 3D sound");
			ds.Add("O - pause/unpause 3D sound");
			ds.Add("I - stop");
			ds.Add("U - immediate stop");
			ds.Add(Color.Orange, "See Camera config for controls");
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, Fusion.Graphics.StereoEye stereoEye )
		{
			base.Draw(gameTime, stereoEye);
		}
	}
}
