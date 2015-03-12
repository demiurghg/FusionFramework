using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Fusion.Graphics;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Fusion.Mathematics;


namespace Fusion.Graphics {


	public enum SpriteBlend {
		Opaque				,
		AlphaBlend			,
		AlphaBlendPreMul	,
		Additive			,
		Screen				,
		Multiply			,
		NegMultiply			,
	}
}
