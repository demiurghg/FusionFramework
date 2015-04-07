using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.GIS.DataSystem.MapSources.Projections;
using Fusion.GIS.MapSources;

namespace Fusion.GIS.DataSystem.MapSources.OpenStreetMaps
{
	public abstract class BaseOpenStreetMapNodes : BaseMapSource
	{
		protected BaseOpenStreetMapNodes(Game game) : base(game)
		{
		}

		public override MapProjection Projection { get { return MercatorProjection.Instance; } }

	}

	public class OpenStreetMapNodes : BaseOpenStreetMapNodes
	{
		static readonly string UrlFormat = "http://api.openstreetmap.org/api/0.6/map?bbox={0},{1},{2},{3}";

		public override string Name
		{
			get { return "OpenStreetMapNodes"; }

		}

		public override string ShortName
		{
			get { return "OSMN"; }
		}

		protected override string RefererUrl
		{
			get { return "http://www.openstreetmap.org/"; }
		}


		float longitudeStep	= 0.05f;
		float latitudeStep	= 0.05f;

		public OpenStreetMapNodes(Game game) : base(game)
		{
		}

		public override string GenerateUrl(int x, int y, int zoom)
		{
			float longitudeWest	= (float)x - longitudeStep / 2;
			float longitudeEast	= (float)x + longitudeStep / 2;
			float latitudeNorth	= (float)y + latitudeStep / 2;
			float latitudeSouth	= (float)y - latitudeStep / 2;

			return String.Format( UrlFormat, longitudeWest, latitudeSouth, longitudeEast, latitudeNorth );
		}

	}
}
