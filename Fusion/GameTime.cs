using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Fusion.Mathematics;


namespace Fusion {

	public class GameTime {

		Stopwatch		stopWatch;
		TimeSpan		total;
		TimeSpan		elapsed;
		float			lastElapsedSec;

		List<TimeSpan>	timeRecord = new List<TimeSpan>();
		double			average;

		public	TimeSpan	Total		{ get { return total; }	}
		public	TimeSpan	Elapsed		{ get { return elapsed; } }
		public	float		ElapsedSec	{ get { return (float)average; } }
		public	float		Fps			{ get { return 1 / ElapsedSec; } }

		public	long		FrameID		{ get; private set; }
		public	int			SubframeID	{ get; private set; }


		
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


			//	median filter :			
			#if true

				while ( timeRecord.Count>=51 ) {
					timeRecord.RemoveAt(0);
				}

				average	=	timeRecord
							.OrderBy( t => t.TotalSeconds )
							.ElementAt( timeRecord.Count/2 )
							.TotalSeconds;

			#else

				average	=	elapsed.TotalSeconds;

			#endif


			total			=	newTotal;
		}
	}
}
