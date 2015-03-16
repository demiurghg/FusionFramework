using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.GIS.DataSystem.MapSources.MapBox
{
	public class SpaceStationMap : BaseMapBoxMap
	{
		public override string Name
		{
			get { return "SpaceStationMap"; }
		}

		public override string ShortName
		{
			get { return "SSM"; }
		}

		protected override string RefererUrl
		{
			get { return "https://www.mapbox.com/"; }
		}

		public SpaceStationMap(Game game) : base(game)
		{
			Example = "3hqcl3di";
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, ServerLetters[GetServerNum(x,y, 2)], Example, zoom, x, y, AcessToken);
		}
	}
}
