using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.DataSystem.MapSources.Projections;
using Fusion.GIS.MapSources;

namespace Fusion.GIS.DataSystem.MapSources.YandexMaps
{
	public abstract class BaseYandexMap : BaseMapSource
	{
		protected BaseYandexMap(Game game) : base(game)
		{
		}

		protected int GetServerNum(int x, int y, int max)
		{
			return (x + 2 * y) % max;
		}

		protected string Version = "4.19.4";

		public override MapProjection Projection { get { return MercatorProjectionYandex.Instance; } }
	}

	public class YandexMap : BaseYandexMap
	{
		public override string Name
		{
			get { return "YandexMap"; }
		}

		public override string ShortName
		{
			get { return "YM"; }
		}

		protected override string RefererUrl
		{
			get { return "http://maps.yandex.ru/"; }
		}

		public YandexMap(Game game) : base(game)
		{
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, UrlServer, GetServerNum(x, y, 4) + 1, Version, x, y, zoom);
		}


		static readonly string UrlServer = "vec";
		static readonly string UrlFormat = "http://{0}0{1}.maps.yandex.net/tiles?l=map&v={2}&x={3}&y={4}&z={5}";
	}

}
