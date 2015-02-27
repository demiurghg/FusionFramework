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

	public enum TonemappingOperator {
		Linear,
		Reinhard,
		Filmic,
	}


	public class HdrFilterConfig {
		
		[Category("Tonemapping & Adaptation")]
		public TonemappingOperator TonemappingOperator { get; set; }
		
		[Category("Tonemapping & Adaptation")]
		public float AdaptationHalfLife { get; set; }

		[Category("Tonemapping & Adaptation")]
		public float KeyValue { get; set; } 

		[Category("Tonemapping & Adaptation")]
		public float LuminanceLowBound { get; set; }

		[Category("Tonemapping & Adaptation")]
		public float LuminanceHighBound { get; set; }
		

		[Category("Bloom")]
		public float GaussBlurSigma { get; set; }
		[Category("Bloom")]
		public float BloomAmount { get; set; }


		public HdrFilterConfig ()
		{
			TonemappingOperator	=	TonemappingOperator.Filmic;
			KeyValue			=	0.18f;
			AdaptationHalfLife	=	0.5f;
			LuminanceLowBound	=	0.0f;
			LuminanceHighBound	=	99999.0f;
			BloomAmount			=	0.1f;
			GaussBlurSigma		=	3.0f;

		}
	}
}
