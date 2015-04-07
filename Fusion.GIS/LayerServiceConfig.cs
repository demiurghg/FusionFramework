using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.LayerSpace.Layers;

namespace Fusion.GIS
{
	public partial class LayerServiceConfig
	{
		[Category("Map Layer")] public MapLayer.MapSource	MapSource	{ get; set; }
		[Category("Map Layer")] public bool					ShowFrames	{ get; set; }


		[Category("Municipal Divisions")] public bool	ShowMunicipalDivisions			{ get; set; }
		[Category("Municipal Divisions")] public bool	ShowEdges						{ get; set; }
		[Category("Municipal Divisions")] public float	MunicipalDivisionTransparency	{ get; set; }

		[Category("Urban")] public bool	ShowPeople				{ get; set; }
		[Category("Urban")] public bool	ShowInfectionPoints		{ get; set; }
		[Category("Urban")] public bool	ShowRoads				{ get; set; }
		[Category("Urban")] public bool	ShowBuildingsContours	{ get; set; }
		[Category("Urban")] public bool	ShowPOI					{ get; set; }

		[Category("Airlines")]		public bool	ShowAirLines	{ get; set; }
		[Category("Railroads")]		public bool	ShowRailroads	{ get; set; }

		[Category("Atmosphere")]	public bool		ShowAtmosphere	{ get; set; }
		[Category("Atmosphere")]	public float	ArrowsScale		{ get; set; }


		[Category("HeatMap")] public int	MaxHeatMapLevel		{ get; set; }
		[Category("HeatMap")] public float	HeatMapTransparency { get; set; }
		[Category("HeatMap")] public bool	ShowHeatMap			{ get; set; }

		[Category("HeatMap")] public int	MaxInfectLevel		{ get; set; }
		[Category("HeatMap")] public bool	ShowInfectHeatMap	{ get; set; }



		public double earthRadius		= 6378.137;
		public const double maxCameraDistance = 100000.0;
		public double cameraDistance	= 6421;
		
		[Category("Globe Layer")]
		public double CameraDistance {
			get { return cameraDistance; }
			set {
				cameraDistance = value;
				if (cameraDistance - earthRadius < 0.35)	cameraDistance = earthRadius + 0.35;
				if (cameraDistance > maxCameraDistance)		cameraDistance = maxCameraDistance;
			}
		}

		public LayerServiceConfig()
		{
			MapSource	= MapLayer.MapSource.OpenStreetMap;
			ShowFrames	= false;

			ShowPeople						= true;
			ShowMunicipalDivisions			= true;
			ShowEdges						= true;
			ShowAirLines					= true;
			ShowRailroads					= true;
			ShowRoads						= true;
			ShowPOI							= true;
			ShowAtmosphere					= false;
			ShowInfectionPoints				= true;
			ShowBuildingsContours			= true;
			MunicipalDivisionTransparency	= 0.7f;
			ArrowsScale						= 25;

			ShowHeatMap			= true;
			HeatMapTransparency = 0.6f;
			MaxHeatMapLevel		= 150;

			ShowInfectHeatMap = false;
			MaxInfectLevel = 50;
		}

	}
}
