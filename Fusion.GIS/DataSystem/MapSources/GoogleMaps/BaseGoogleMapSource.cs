using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.GIS.DataSystem.MapSources.Projections;
using Fusion.GIS.MapSources;

namespace Fusion.GIS.DataSystem.MapSources.GoogleMaps
{
	public abstract class BaseGoogleMapSource : BaseMapSource
	{
		protected string SecureWord			= "Galileo";
		protected string Sec1				= "&s=";
		protected string UrlFormat			= "http://{0}{1}.{10}/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
		protected string UrlFormatServer	= "mt";
		protected string UrlFormatRequest	= "vt";
		protected string MapVersion			= "m@249000000";
		protected string Language			= "en";
		protected string Server				= "google.com";

		public override MapProjection Projection { get { return MercatorProjection.Instance; } }

		protected void GetSecureWords(int x, int y, out string sec1, out string sec2)
		{
			sec1 = string.Empty; // after &x=...
			sec2 = string.Empty; // after &zoom=...
			
			int seclen	= ((x * 3) + y) % 8;
				sec2	= SecureWord.Substring(0, seclen);
			if (y >= 10000 && y < 100000) {
				sec1 = Sec1;
			}
		}


		protected BaseGoogleMapSource(Game game) : base(game)
		{
		}

		protected int GetServerNum(int x, int y, int max)
		{
			return (x + 2 * y) % max;
		}


		protected override string RefererUrl
		{
			get { return "http://maps.google.com/"; }
		}
	}


	public class GoogleMap : BaseGoogleMapSource
	{
		public override string Name
		{
			get { return "GoogleMap"; }
		}

		public override string ShortName
		{
			get { return "GM"; }
		}

		public GoogleMap(Game game) : base(game)
		{
		}



		public override string GenerateUrl(int x, int y, int zoom)
		{
			string sec1 = string.Empty; // after &x=...
            string sec2 = string.Empty; // after &zoom=...
            GetSecureWords(x, y, out sec1, out sec2);


			return String.Format(UrlFormat, UrlFormatServer, GetServerNum(zoom,y,4), UrlFormatRequest, MapVersion, Language, x, sec1, y, zoom, sec2, Server);
		}
		
	}
}
