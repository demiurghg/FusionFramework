using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.DataSystem.MapSources.Projections;
using Fusion.GIS.MapSources;

namespace Fusion.GIS.DataSystem.MapSources.MapBox
{
	public abstract class BaseMapBoxMap : BaseMapSource
	{
		protected BaseMapBoxMap(Game game) : base(game)
		{
		}

		public override MapProjection Projection { get { return MercatorProjection.Instance; } }

		protected int GetServerNum(int x, int y, int max) 
		{
			return (x + 2 * y) % max;
		}

		protected string ServerLetters	= "ab";
		protected string AcessToken		= "pk.eyJ1IjoibWFwYm94IiwiYSI6IlhHVkZmaW8ifQ.hAMX5hSW-QnTeRCMAy9A8Q";
		protected string Example		= "";
		protected string UrlFormat		= "http://{0}.tiles.mapbox.com/v4/examples.{1}/{2}/{3}/{4}.png?access_token={5}";
	}
}
