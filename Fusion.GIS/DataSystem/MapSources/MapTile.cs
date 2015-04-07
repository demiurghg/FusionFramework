using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Graphics;
//using SharpDX.Direct3D11;

namespace Fusion.GIS.DataSystem.MapSources
{
	public class MapTile
	{
		public Texture2D	Tile;

		public float		Lat;
		public float		Lon;
		
		public int			X;
		public int			Y;
		public int			Zoom;
		
		public string		Url;
		public string		Path;
		
		public float		Time;
		public int			LruIndex;

		public bool			IsLoaded = false;
	}
}
