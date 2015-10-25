using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;
using Fusion;
using System.ComponentModel;
using System.Threading;

namespace TestGame2 {
	class CustomGameServer : Fusion.Engine.Common.GameServer {

		string[] messages = new[] {
			"Locating hamsters"								,
			"Greasing wheels"								,
			"Filling foodbowls"								,
			"Spinning up the warp drive"					,
			"Waiter... there's a pixel in my soup"			,
			"Don't touch that cable!"						,
			"What made that noise?"							,
			"Tomorrow's lottery numbers are..."				,
			"Searching for marbles"							,
			"Tightening loose screws"						,
			"Brushing your hair"							,
			"Engaging warp drive"							,
			"Paying your parking tickets"					,
			"Putting the square peg in a round hole"		,
			"Pixelating the pixels"							,
			"Watering the plants"							,
			"Scanning for schadenfreude"					,
			"Polishing the servers"							,
			"Shooing away rain clouds"						,
			"Tuning up air guitars"							,
			"Anti gravity zone; remain seated"				,
			"Waterproofing swimming pools"					,
			"Weighing pound cakes"							,
			"Watering sidewalks"							,
			"Planetary alignment complete"					,
			"Painting the lawn"								,
		};
			
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="engine"></param>
		public CustomGameServer ( GameEngine gameEngine ) : base(gameEngine)
		{
		}


		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}


		/// <summary>
		/// Starts server with given map/level.
		/// </summary>
		/// <param name="map"></param>
		public override void Start ( string map )
		{
			var rand = new Random();

			foreach ( var msg in messages ) {
				Log.Message("INIT: {0}...", msg);
				Thread.Sleep( rand.Next(10,50) );
			}


			Log.Message("[SERVER STARTED]");
		}


		/// <summary>
		/// Kills server
		/// </summary>
		public override void Kill ()
		{
			var rand = new Random();

			Log.Message("SV: [SERVER IS GONNA DIE]");

			foreach ( var msg in messages ) {
				Log.Message("KILL: {0}...", msg);
				Thread.Sleep( rand.Next(100,250) );
			}


			Log.Message("SV: [SERVER WAS KILLED]");
		}


		/// <summary>
		/// Runs one step of server-side world simulation.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			Thread.Sleep(333);
			Log.Message("SV: [{0}]", gameTime.ElapsedSec );
		}

		/// <summary>
		/// Gets world snapshot for particular client.
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public override byte[] GetSnapshot ( int clientId = -1 )
		{
			return Encoding.ASCII.GetBytes("[SNAPSHOT]");
		}

		/// <summary>
		/// Feed client commands from particular client.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="clientId"></param>
		public override void FeedCommand ( UserCmd[] commands, int clientId )
		{
		}
	}
}
