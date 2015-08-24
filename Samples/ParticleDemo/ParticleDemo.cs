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

namespace ParticleDemo {
	public class ParticleDemo : Game {
		/// <summary>
		/// ParticleDemo constructor
		/// </summary>
		public ParticleDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );

			//	add here additional services :
			AddService( new ParticleSystem( this ), true, true, 500, 500 );

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += FusionGame_Exiting;
		}


		/// <summary>
		/// Add services :
		/// </summary>
		protected override void Initialize ()
		{
			//	initialize services :
			base.Initialize();

			//	add keyboard handler :
			InputDevice.KeyDown += InputDevice_KeyDown;
		}



		/// <summary>
		/// Handle keys for each demo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Fusion.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
			//	DevCon.Show(this);
			}

			if (e.Key == Keys.F2) {
				Parameters.VSyncInterval = (Parameters.VSyncInterval == 0) ? 1 : 0;
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


		Vector2 lastPoint;
		Vector2	lastVel;
		Random	rand = new Random();

		float Gauss ( float mean, float stdDev )
		{
			//Random rand = new Random(); //reuse this if you are generating many
			double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
			double u2 = rand.NextDouble();
			double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
						 Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
			double randNormal =
						 mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

			return (float)randNormal;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );


			var ps = GetService<ParticleSystem>();

			var vp = GraphicsDevice.DisplayBounds;

			Vector2 target = InputDevice.MousePosition;
			var vel = (lastPoint - target);

			if (InputDevice.IsKeyDown(Keys.LeftButton)) {

				float len = (lastPoint - target).Length() + 0.001f;

				for ( float t=0; t<=len; t+=0.15f) {
					ps.AddParticle( Vector2.Lerp( lastPoint, target, t/len ), Vector2.Lerp( lastVel,vel,t/len) * Gauss(10,1), 5, 3, 130, 0.2f );
				}
			}

			if (InputDevice.IsKeyDown(Keys.Space)) {
				for (int i=0; i<100; i++) {
					ps.AddParticle( rand.NextVector2( Vector2.Zero, new Vector2(vp.Width, vp.Height) ), Vector2.Zero, 10, 80,100, 0.1f );
				}
			}

			lastPoint = InputDevice.MousePosition;
			lastVel = vel;


			base.Update( gameTime );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			GraphicsDevice.ClearBackbuffer(Color4.Black);

			base.Draw( gameTime, stereoEye );
		}
	}
}
