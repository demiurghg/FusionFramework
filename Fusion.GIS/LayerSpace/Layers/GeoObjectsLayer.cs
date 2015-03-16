using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GeoObjectsLayer : BaseLayer
	{

		public GeoObjectsLayer(Game game, LayerServiceConfig config) : base(game, config)
		{
			LoadMunicipalDivisions();
		}
	}
}
