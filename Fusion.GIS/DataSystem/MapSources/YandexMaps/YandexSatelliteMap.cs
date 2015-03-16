using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.GIS.DataSystem.MapSources.YandexMaps
{
	public class YandexSatelliteMap : BaseYandexMap
	{
		public override string Name
		{
			get { return "YandexSatelliteMap"; }
		}

		public override string ShortName
		{
			get { return "YSM"; }
		}

		protected override string RefererUrl
		{
			get { return "http://maps.yandex.ru/"; }
		}

		public YandexSatelliteMap(Game game) : base(game)
		{
			Version = "3.185.0";
			MaxZoom = 18;
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, UrlServer, GetServerNum(x, y, 4) + 1, Version, x, y, zoom);
		}


		static readonly string UrlServer = "sat";
		static readonly string UrlFormat = "http://{0}0{1}.maps.yandex.net/tiles?l=sat&v={2}&x={3}&y={4}&z={5}";
	}
}
