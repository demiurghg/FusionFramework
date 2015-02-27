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


	public class Config
	{
		[Category("Rotation")]
		public float Yaw { get; set; }

		[Category("Rotation")]
		public float Pitch { get; set; }

		[Category("Rotation")]
		public float Roll { get; set; }

		public SkyType	Sky { get; set; }
		public float	SkyIntensity { get; set; }
		public Vector3	SunDirection { get; set; }
		public float	SunGlowIntensity { get; set; }
		public float	SunLightIntensity { get; set; }
		public int		SunTemperature { get; set; }
		public float	SkyTurbidity { get; set; }
		public float	SkySphereSize { get; set; }
		public float	AerialFogDensity { get; set; }
		public float	ScatteringLevel { get; set; }

		//[Editor(typeof(SurfaceShader.FileLocationEditor), typeof(UITypeEditor))]
		public string	TexturePath { get; set; }

		public Config()
		{
			TexturePath = "uffizi_cross.dds";
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
