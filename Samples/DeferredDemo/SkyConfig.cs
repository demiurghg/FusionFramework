using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Development;
using System.ComponentModel;

namespace DeferredDemo
{
	public enum SkyType
	{
		Procedural,
		Panoramic_Half,
		Panoramic_Full,
		CubeMap
	}



	public enum RgbSpace {
		sRGB,
		CIE_RGB,
	}


	public class Config
	{
		[Category("Sky Model")]
		public float	SkyIntensity { get; set; }
		[Category("Sky Model")]
		public Vector3	SunDirection { get; set; }
		[Category("Sky Model")]
		public float	SunGlowIntensity { get; set; }
		[Category("Sky Model")]
		public float	SunLightIntensity { get; set; }
		[Category("Sky Model")]
		public int		SunTemperature { get; set; }
		[Category("Sky Model")]
		public float	SkyTurbidity { get; set; }
		[Category("Sky Model")]
		public float	SkySphereSize { get; set; }
		[Category("Sky Model")]
		public float	AerialFogDensity { get; set; }
		[Category("Sky Model")]
		public float	ScatteringLevel { get; set; }
		[Category("Sky Model")]
		public RgbSpace	RgbSpace { get; set; }


		public Config()
		{
			RgbSpace	= RgbSpace.sRGB;
			AerialFogDensity = 0.001f;
			SkySphereSize = 5000.0f;
			SkyTurbidity = 4.0f;
			SunDirection = new Vector3( 1.0f, 0.5f, -1.0f );
			SunGlowIntensity = 1f;
			SunLightIntensity = 0.1f;
			SunTemperature = 5700;
			SkyIntensity = 1.0f;
			ScatteringLevel = 0.1f;
		}
	}
}
