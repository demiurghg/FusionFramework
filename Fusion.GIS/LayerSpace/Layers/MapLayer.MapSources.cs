using System;
using System.Collections.Generic;
using Fusion.GIS.DataSystem.MapSources.GoogleMaps;
using Fusion.GIS.DataSystem.MapSources.MapBox;
using Fusion.GIS.DataSystem.MapSources.OpenStreetMaps;
using Fusion.GIS.DataSystem.MapSources.YandexMaps;
using Fusion.GIS.MapSources;

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class MapLayer
	{
		public static List<BaseMapSource> MapSources = new List<BaseMapSource>();

		public enum MapSource : int
		{
			OpenStreetMap		= 0,
			GoogleMap			= 1,
			GoogleSatteliteMap	= 2,
			Yandex				= 3,
			YandexSatellite		= 4,
			PencilMap			= 5,
			SpaceStationMap		= 6,
			PirateMap			= 7,
		}

		protected void RegisterMapSources()
		{
			MapSources.Add( new OpenStreetMap(Game)			);
			MapSources.Add( new GoogleMap(Game)				);
			MapSources.Add( new GoogleSatelliteMap(Game)	);
			MapSources.Add( new YandexMap(Game)				);
			MapSources.Add( new YandexSatelliteMap(Game)	);
			MapSources.Add( new PencilMap(Game)				);
			MapSources.Add( new SpaceStationMap(Game)		);
			MapSources.Add( new PirateMap(Game)				);


			var res = WGS84toGoogleBing(0.5, 0.5);
			var ret = GoogleBingtoWGS84Mercator(0.5, 0.5);
		}






		double[] WGS84toGoogleBing(double lon, double lat)
		{
			double x = lon * 20037508.34 / 180;
			double y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
			y = y * 20037508.34 / 180;
			return new double[] { x, y };
		}

		double[] GoogleBingtoWGS84Mercator(double x, double y)
		{
			double lon = (x / 20037508.34) * 180;
			double lat = (y / 20037508.34) * 180;

			lat = 180 / Math.PI * (2 * Math.Atan(Math.Exp(lat * Math.PI / 180)) - Math.PI / 2);
			return new double[] { lon, lat };
		}
	}
}
