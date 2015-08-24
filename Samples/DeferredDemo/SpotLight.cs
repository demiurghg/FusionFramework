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

namespace DeferredDemo {
	class SpotLight {
		public Matrix	SpotView;
		public Matrix	Projection;
		public Color4	Intensity;
		public string	MaskName;
		public float	RadiusInner;
		public float	RadiusOuter;
	}
}
