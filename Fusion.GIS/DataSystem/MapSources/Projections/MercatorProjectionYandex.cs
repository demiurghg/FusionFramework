using System;
using Fusion.GIS.GlobeMath;

namespace Fusion.GIS.DataSystem.MapSources.Projections
{
	class MercatorProjectionYandex : MapProjection
	{
		public static readonly MercatorProjectionYandex Instance = new MercatorProjectionYandex();
		
		static readonly double MinLatitude	= -85.05112878f;
		static readonly double MaxLatitude	= 85.05112878f;
		static readonly double MinLongitude	= -177;
		static readonly double MaxLongitude	= 177;
		
		static readonly double RAD_DEG = 180 / Math.PI;
		static readonly double DEG_RAD = Math.PI / 180;
		static readonly double MathPiDiv4 = Math.PI / 4;
		
		public double Axis
		{
		   get
		   {
		      return 6356752.3142;
		   }
		}
		
		public double Flattening
		{
		   get
		   {
		      return (1.0 / 298.257223563);
		   }
		}

		public override DVector2 WorldToTilePos(double lon, double lat, int zoom)
		{
			lat = DMathUtil.Clamp(lat, MinLatitude, MaxLatitude);
			lon = DMathUtil.Clamp(lon, MinLongitude, MaxLongitude);
			
			lat = lat < MinLatitude ? MinLatitude : lat > MaxLatitude ? MaxLatitude : lat;
			lon = lon < MinLongitude ? MinLongitude : lon > MaxLongitude ? MaxLongitude : lon;
			
			double rLon = lon * DEG_RAD; // Math.PI / 180; 
			double rLat = lat * DEG_RAD; // Math.PI / 180; 
			
			double a = 6378137;
			double k = 0.0818191908426;
			
			double z	= Math.Tan(MathPiDiv4 + rLat / 2) / Math.Pow((Math.Tan(MathPiDiv4 + Math.Asin(k * Math.Sin(rLat)) / 2)), k);
			double z1	= Math.Pow(2, 23 - zoom);
			
			double DX = ((20037508.342789 + a * rLon) * 53.5865938 / z1) / 256.0;
			double DY = ((20037508.342789 - a * Math.Log(z)) * 53.5865938 / z1) / 256.0;
			
			DVector2 ret = DVector2.Zero;
			ret.X = DX;
			ret.Y = DY;
			
			return ret;
		}

		public override DVector2 TileToWorldPos(double x, double y, int zoom)
		{
			x = x*256.0;
			y = y*256.0;
			double a = 6378137;
			double c1 = 0.00335655146887969;
			double c2 = 0.00000657187271079536;
			double c3 = 0.00000001764564338702;
			double c4 = 0.00000000005328478445;
			double z1 = (23 - zoom);
			double mercX = (x * Math.Pow(2, z1)) / 53.5865938 - 20037508.342789;
			double mercY = 20037508.342789 - (y * Math.Pow(2, z1)) / 53.5865938;
			
			double g = Math.PI / 2 - 2 * Math.Atan(1 / Math.Exp(mercY / a));
			double z = g + c1 * Math.Sin(2 * g) + c2 * Math.Sin(4 * g) + c3 * Math.Sin(6 * g) + c4 * Math.Sin(8 * g);
			
			DVector2 ret = DVector2.Zero;
			ret.Y =  z * RAD_DEG;
			ret.X =  mercX / a * RAD_DEG;
			
			return ret;

			//double j = Math.PI, f = j / 2, i = 6378137, n = 0.003356551468879694, k = 0.00000657187271079536, h = 1.764564338702e-8, m = 5.328478445e-11;
			//double g = f - 2 * Math.Atan(1 / Math.Exp(y / i));
			//double l = g + n * Math.Sin(2 * g) + k * Math.Sin(4 * g) + h * Math.Sin(6 * g) + m * Math.Sin(8 * g);
			//double d = x / i;
			//return new DVector2(d * 180 / Math.PI, l * 180 / Math.PI);
		}

	}
}
