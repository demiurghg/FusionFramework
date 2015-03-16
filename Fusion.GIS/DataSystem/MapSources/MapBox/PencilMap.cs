using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.GIS.DataSystem.MapSources.MapBox
{
	class PencilMap : BaseMapBoxMap
	{
		public override string Name
		{
			get { return "PencilMap"; }
		}

		public override string ShortName
		{
			get { return "PenM"; }
		}

		protected override string RefererUrl
		{
			get { return "https://www.mapbox.com/"; }
		}

		public PencilMap(Game game) : base(game)
		{
			Example = "a4c252ab";
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			return String.Format(UrlFormat, ServerLetters[GetServerNum(x, y, 2)], Example, zoom, x, y, AcessToken);
		}
	}
}
