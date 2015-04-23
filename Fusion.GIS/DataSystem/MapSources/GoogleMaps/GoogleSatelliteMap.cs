using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.GIS.MapSources;

namespace Fusion.GIS.DataSystem.MapSources.GoogleMaps
{
	public class GoogleSatelliteMap : BaseGoogleMapSource
	{
		public override string Name
		{
			get { return "GoogleSatelliteMap"; }
		}

		public override string ShortName
		{
			get { return "GSM"; }
		}

		public GoogleSatelliteMap(Game game) : base(game)
		{
			UrlFormatServer		= "khm";
			UrlFormatRequest	= "kh";
			UrlFormat			= "http://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
			MapVersion			= "168";

			MaxZoom = 19;
		}


		public override string GenerateUrl(int x, int y, int zoom)
		{
			string sec1 = string.Empty; // after &x=...
			string sec2 = string.Empty; // after &zoom=...
			GetSecureWords(x, y, out sec1, out sec2);

			string res = String.Format(UrlFormat, UrlFormatServer, GetServerNum(zoom, y, 4), UrlFormatRequest, MapVersion, Language, x, sec1, y, zoom, sec2, Server);

			return res;
		}
	}
}
