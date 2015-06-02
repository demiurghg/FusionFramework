using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Development;

namespace DeferredDemo {
	public class LightRendererConfig {

		int csmSize;

		/// <summary>
		/// Cascaded shadow map size.
		/// </summary>
		[Category("Cascaded Shadow Maps")]
		public int CSMSize {
			get {
				return csmSize;
			}
			set {
				csmSize =	value;
				csmSize =	MathUtil.Clamp( 1 << (MathUtil.LogBase2( csmSize )-1), 64, 2048 );
			}
		}

		/// <summary>
		/// First split size
		/// </summary>
		[Category("Cascaded Shadow Maps")]
		public float CSMDepth { get; set; }

		/// <summary>
		/// First split size
		/// </summary>
		[Category("Cascaded Shadow Maps")]
		public float SplitSize { get; set; }

		/// <summary>
		/// First split size
		/// </summary>
		[Category("Cascaded Shadow Maps")]
		public float SplitOffset { get; set; }

		/// <summary>
		/// Split size and offset magnification factor
		/// </summary>
		[Category("Cascaded Shadow Maps")]
		public float SplitFactor { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Category("Cascaded Shadow Maps")]
		public float CSMSlopeBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Category("Cascaded Shadow Maps")]
		public float CSMDepthBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Category("Cascaded Shadow Maps")]
		public float CSMFilterSize { get; set; }



		int spotShadowSize;

		[Category("Spot Lights")]
		public int SpotShadowSize {
			get {
				return spotShadowSize;
			}
			set {
				spotShadowSize =	value;
				spotShadowSize =	MathUtil.Clamp( 1 << (MathUtil.LogBase2( spotShadowSize )-1), 64, 1024 );
			}
		}


		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Category("Spot Lights")]
		public float SpotSlopeBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Category("Spot Lights")]
		public float SpotDepthBias { get; set; }

		/// <summary>
		/// Split magnification factor
		/// </summary>
		[Category("Spot Lights")]
		public float SpotFilterSize { get; set; }


		/// <summary>
		/// UseUE4LightingModel
		/// </summary>
		[Category("Debugging")]
		public bool UseUE4LightingModel { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		[Category("Debugging")]
		public bool SkipShadows { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		[Category("Debugging")]
		public bool SkipDirectLight { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		[Category("Debugging")]
		public bool SkipOmniLights { get; set; }

		/// <summary>
		/// Skips CSM
		/// </summary>
		[Category("Debugging")]
		public bool SkipSpotLights { get; set; }

		/// <summary>
		/// Show CSM's splits
		/// </summary>
		[Category("Debugging")]
		public bool ShowCSMSplits { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Category("Debugging")]
		public bool ShowOmniLightExtents { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Category("Debugging")]
		public bool ShowSpotLightExtents { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Category("Debugging")]
		public bool ShowOmniLightTileLoad { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Category("Debugging")]
		public bool ShowSpotLightTileLoad { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Category("Debugging")]
		public bool ShowOmniLights { get; set; }

		/// <summary>
		/// Show omni-light extents
		/// </summary>
		[Category("Debugging")]
		public bool ShowSpotLights { get; set; }


		public LightRendererConfig ()
		{
			CSMDepth		=	1024;
			CSMSize			=	1024;
			SplitSize		=	10;
			SplitFactor		=	2.5f;
			CSMSlopeBias	=	2;
			CSMDepthBias	=	0.0001f;
			CSMFilterSize	=	2;

			SpotShadowSize	=	512;
			SpotSlopeBias	=	2;
			SpotDepthBias	=	0.0001f;
			SpotFilterSize	=	2;
		}
	}
}

