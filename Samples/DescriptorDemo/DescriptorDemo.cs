using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Pipeline;
using Fusion.Development;
using System.Runtime.InteropServices;


namespace DescriptorDemo {
	public class DescriptorDemo : Game {

		[Asset("Monsters", "Monster")]
		public class Monster : AbstractAsset {
			public string Name { get; set; }
			public int Health { get; set; }
			public int Armor { get; set; }

			public Monster() {
				Name	=	"Some Monster";
				Health	=	100;
				Armor	=	50;
			}

			public override string ToString ()
			{
				return string.Format("'{0}' h={1} a={2}", Name, Health, Armor);
			}
		}

		[Asset("Monsters", "Guard")]
		public class Guard : Monster {
			public int Damage  { get; set; }

			public Guard() {
				Name	=	"Some Guard";
				Health	=	25;
				Armor	=	0;
				Damage	=	10;
			}
		}

		[Asset("Monsters", "Infantry")]
		public class Infantry : Monster {
			public int Damage  { get; set; }
			public int MeleeDamage  { get; set; }

			public Infantry() {
				Name		=	"Some Infantry";
				Health		=	50;
				Armor		=	25;
				Damage		=	20;
				MeleeDamage	=	50;
			}
		}

		[Asset("Monsters", "RoboTank")]
		public class RoboTank : Monster {
			public int Damage  { get; set; }
			public int MeleeDamage  { get; set; }
			public int RocketDamage  { get; set; }

			public RoboTank() {
				Name			=	"Some RoboTank";
				Health			=	100;
				Armor			=	100;
				Damage			=	40;
				MeleeDamage		=	100;
				RocketDamage	=	100;
			}
		}

		[Asset("Monsters", "MegaRoboTank")]
		public class MegaRoboTank : RoboTank {
			public int LaserDamage  { get; set; }

			public MegaRoboTank() {
				LaserDamage	=	150;
			}
		}


		[Asset("Weapon", "Blaster")]
		public class Blaster : AbstractAsset {
			public int Damage  { get; set; }
			public int AttackingRate  { get; set; }
			public int OverheatCoolingTime  { get; set; }

			public Blaster() {
				Damage				=	15;
				AttackingRate		=	300;
				OverheatCoolingTime	=	2500;
			}
		}
		
		[Asset("Weapon", "Machinegun")]
		public class Machinegun : AbstractAsset {
			public int Damage  { get; set; }
			public int AttackingRate { get; set; }
			public int ReloadTime  { get; set; }
			public int AmmoCapacity  { get; set; }

			public Machinegun() {
				Damage			=	15;
				AttackingRate	=	120;
				ReloadTime		=	2000;
				AmmoCapacity	=	30;
			}
		}
		
		[Asset("Weapon", "Rocket Launcher")]
		public class RocketLauncher : AbstractAsset {
			public int Damage  { get; set; }
			public int AttackingRate { get; set; }
			public int ReloadTime  { get; set; }
			public int AmmoCapacity  { get; set; }
			public bool Homing { get; set; }

			public RocketLauncher() {
				Damage			=	100;
				AttackingRate	=	700;
				ReloadTime		=	3000;
				AmmoCapacity	=	5;
				Homing			=	true;
			}
		}



		/// <summary>
		///	Add services and set options
		/// </summary>
		public DescriptorDemo ()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;
			Parameters.MsaaLevel = 4;

			//	uncomment to enable debug graphics device:
			//	(MS Platform SDK must be installed)
			//	Parameters.UseDebugDevice	=	true;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );
			AddService( new Camera( this ), true, false, 1, 1 );

			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += DescriptorDemo_Exiting;

			InputDevice.KeyDown += InputDevice_KeyDown;
		}


		void InputDevice_KeyDown ( object sender, InputDevice.KeyEventArgs e )
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
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void DescriptorDemo_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}



		string text;

		/// <summary>
		/// Load stuff here
		/// </summary>
		protected override void Initialize ()
		{
			base.Initialize();

			text = Content.Load<string>("text|utf8");

			Log.Message( text );
		}



		/// <summary>
		/// Update stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds = GetService<DebugStrings>();

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F2   - toggle vsync" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );

			var monster	=	Content.Load<Monster>("Monsters\\Monster");

			ds.Add("Monster name   : {0}", monster.Name );
			ds.Add("Monster health : {0}", monster.Health );
			ds.Add("Monster armor  : {0}", monster.Armor );

			/*
			var paths = Content.Descriptors.Select( d => d.DomainPath ).ToList();

			foreach ( var path in paths ) {

				var desc = Content.GetDescriptor<Asset>( path );
				ds.Add("Descriptor : {0} - {1}", path, desc.ToString() );
			}
			*/

			base.Update( gameTime );
		}


		/// <summary>
		/// Draw stuff here
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			base.Draw( gameTime, stereoEye );
		}
	}
}
