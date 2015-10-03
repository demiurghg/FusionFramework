using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Graphics;
using System.ComponentModel;

namespace Fusion {
	public partial class TexturePumpConfig {

		[Category("Debugging")]
		public bool	ShowStatistics	{ get; set; }

		[Category("Cache Parameters")]
		[Description("Memory cache size in megabytes.")]
		public int	MemoryCacheSize	{ get; set; }

		[Category("Cache Parameters")]
		[Description("The maximum number of seconds that the texture can remain in memory.")]
		public int MaximumLifeTimeSeconds { get; set; }

		[Category("Network Parameters")]
		[Description("Network timeout in milliseconds.")]
		public int	NetworkTimeout	{ get; set; }


		public TexturePumpConfig ()
		{
			MemoryCacheSize			=	64;
			NetworkTimeout			=	10 * 1000;
			MaximumLifeTimeSeconds	=	30;
		}
	}
}
