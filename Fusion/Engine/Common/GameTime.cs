using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Fusion.Core.Mathematics;


namespace Fusion.Engine.Common {

	public class GameTime {
		
		/// <summary>
		/// Averaging frame count.
		/// </summary>
		public static int	AveragingFrameCount	=	60;

		Stopwatch		stopWatch;
		TimeSpan		total;
		TimeSpan		elapsed;
		float			lastElapsedSec;

		List<TimeSpan>	timeRecord = new List<TimeSpan>();
		double			average;

		/// <summary>
		/// Total game time since game had been started.
		/// </summary>
		public	TimeSpan Total { get { return total; }	}

		/// <summary>
		/// Elapsed time since last frame.
		/// </summary>
		public	TimeSpan Elapsed { get { return elapsed; } }

		/// <summary>
		/// Elapsed time in seconds since last frame.
		/// </summary>
		public	float ElapsedSec { get { return (float)elapsed.TotalSeconds; } }

		/// <summary>
		/// Frames per second.
		/// </summary>
		public	float Fps { get { return 1 / ElapsedSec; } }

		/// <summary>
		/// Average frame time (milliseconds) within AveragingFrameCount frames.
		/// </summary>
		public	float AverageFrameTime	{ get { return (float)average; } }

		/// <summary>
		/// Average frame rate (FPS) within AveragingFrameCount frames.
		/// </summary>
		public	float AverageFrameRate	{ get { return 1000.0f / (float)average; } }

		/// <summary>
		/// Frame count since game had been started.
		/// </summary>
		public	long FrameID		{ get; private set; }

		/// <summary>
		/// Subframe index. For stereo rendering Game.Draw and GameService.Draw are called twice for each eye.
		/// </summary>
		public	int	SubframeID	{ get; private set; }


		
		/// <summary>
		/// Constructor
		/// </summary>
		public GameTime ()
		{
			FrameID		= -1;
			stopWatch	= new Stopwatch();
			stopWatch.Start();
			total		= stopWatch.Elapsed;
		}



		internal void AddSubframe ()
		{
			SubframeID ++;
		}


		/// <summary>
		/// Updates timer
		/// </summary>
		public void Update()
		{
			FrameID ++;
			SubframeID = 0;
			lastElapsedSec	= (float)elapsed.TotalSeconds;	

			var newTotal	=	stopWatch.Elapsed;
			elapsed			=	newTotal - total;

			timeRecord.Add( elapsed );

			#if true

				while ( timeRecord.Count>=AveragingFrameCount ) {
					timeRecord.RemoveAt(0);
				}

				average	=	timeRecord.Average( t => t.TotalMilliseconds );

			#else

				average	=	elapsed.TotalSeconds;

			#endif

			total			=	newTotal;
		}
	}
}
