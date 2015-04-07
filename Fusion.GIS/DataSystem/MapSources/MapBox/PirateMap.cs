using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.GIS.DataSystem.MapSources.MapBox
{
	class PirateMap : BaseMapBoxMap
	{
		public override string Name
		{
			get { return "PirateMap"; }
		}

		public override string ShortName
		{
			get { return "PM"; }
		}

		protected override string RefererUrl
		{
			get { return "https://www.mapbox.com/"; }
		}

		public PirateMap(Game game) : base(game)
		{
			Example = "a3cad6da";
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, ServerLetters[GetServerNum(x,y, 2)], Example, zoom, x, y, AcessToken);
		}
	}
}
