using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.GlobeMath;
using Fusion.Mathematics;

namespace Fusion.GIS
{
	public static class GeoHelper
	{
		public static float EarthRadius = 6371;

		
		static public DVector2 WorldToTilePos(double lon, double lat, int zoom)
		{
			DVector2 p = new DVector2();
			p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
			p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

			return p;
		}


		static public Vector2 WorldToTilePos(double lon, double lat)
		{
			Vector2 p = new Vector2();
			p.X = (float)((lon + 180.0) / 360.0 * (1 << 0));
			p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << 0));

			return p;
		}


		static public DVector2 WorldToTilePos(DVector2 lonLat)
		{
			DVector2 p = new DVector2();
			p.X = (lonLat.X + 180.0) / 360.0 * (1 << 0);
			p.Y = (1.0 - Math.Log(Math.Tan(lonLat.Y * Math.PI / 180.0) + 1.0 / Math.Cos(lonLat.Y * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << 0);

			return p;
		}


		static public Vector2 WorldToTilePos(Vector2 pos)
		{
			return WorldToTilePos(pos.X, pos.Y);
		}


		static public Vector2 TileToWorldPos(double tile_x, double tile_y, int zoom)
		{
			Vector2 p = new Vector2();
			double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));

			p.X = (float)((tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
			p.Y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

			return p;
		}


		static public void TileToWorldPos(double tile_x, double tile_y, int zoom, out double lon, out double lat)
		{
			double n = Math.PI - ((2.0 * Math.PI * tile_y) / (1<<zoom));

			lon = (tile_x / (1<<zoom) * 360.0) - 180.0;
			lat = 180.0 / Math.PI * Math.Atan(Math.Sinh(n));

		}


		static public Vector2 GetNewPointWithDistance(Vector2 startPoint, float distanceMeters, float bearingRadians)
		{
			float lat1 = MathUtil.Rad(startPoint.Y);
			float lon1 = MathUtil.Rad(startPoint.X);
			
			float dist = distanceMeters / 1000.0f;

			var lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(dist / EarthRadius) + Math.Cos(lat1) * Math.Sin(dist / EarthRadius) * Math.Cos(bearingRadians));
			var lon2 = lon1 + Math.Atan2(Math.Sin(bearingRadians) * Math.Sin(dist / EarthRadius) * Math.Cos(lat1), Math.Cos(dist / EarthRadius) - Math.Sin(lat1) * Math.Sin(lat2));

			return new Vector2(MathUtil.Deg((float)lon2), MathUtil.Deg((float)lat2));
		}


		public static bool IsPointInPolygon(List<Vector2> boundaries, Vector2 pos)
		{
			int	i = 0;
			int j = boundaries.Count - 1;
			bool oddNodes = false;

			for (i = 0; i < boundaries.Count; i++) {
				if ((boundaries[i].Y < pos.Y && boundaries[j].Y >= pos.Y || boundaries[j].Y < pos.Y && boundaries[i].Y >= pos.Y) && (boundaries[i].X <= pos.X || boundaries[j].X <= pos.X)) {
					oddNodes ^= (boundaries[i].X + (pos.Y - boundaries[i].Y) / (boundaries[j].Y - boundaries[i].Y) * (boundaries[j].X - boundaries[i].X) < pos.X);
				}
				j = i;
			}

			return oddNodes;
		}


		public static bool IsPointInPolygon(List<DVector2> boundaries, DVector2 pos)
		{
			int	i = 0;
			int j = boundaries.Count - 1;
			bool oddNodes = false;

			for (i = 0; i < boundaries.Count; i++) {
				if ((boundaries[i].Y < pos.Y && boundaries[j].Y >= pos.Y || boundaries[j].Y < pos.Y && boundaries[i].Y >= pos.Y) && (boundaries[i].X <= pos.X || boundaries[j].X <= pos.X)) {
					oddNodes ^= (boundaries[i].X + (pos.Y - boundaries[i].Y) / (boundaries[j].Y - boundaries[i].Y) * (boundaries[j].X - boundaries[i].X) < pos.X);
				}
				j = i;
			}

			return oddNodes;
		}
	}
}
