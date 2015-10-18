using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Fusion.Core.Mathematics;


namespace Fusion.Drivers.Graphics {


	public enum SpriteBlend {
		Opaque				,
		AlphaBlend			,
		AlphaBlendPremul	,
		Additive			,
		Screen				,
		Multiply			,
		NegMultiply			,
		ClearAlpha			,
	}
}
