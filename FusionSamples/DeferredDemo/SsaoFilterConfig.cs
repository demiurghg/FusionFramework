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
using System.Runtime.InteropServices;
using System.ComponentModel;


namespace DeferredDemo {
	public class SsaoFilterConfig {

		[Category("HBAO")]
		public float TraceStep { get; set; }
		
		[Category("HBAO")]
		public float DecayRate { get; set; }
		
		[Category("HBAO")]
		public float BlurSigma { get; set; }


		/// <summary>
		///
		/// </summary>
		public SsaoFilterConfig()
		{
			TraceStep = 1.0f;
			DecayRate = 1.0f;
		}
	}
}
