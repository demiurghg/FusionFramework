using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;

namespace Fusion.GIS.LayerSpace.Layers
{
	public class ElevationLayer : BaseLayer
	{
		class ElevationTile
		{
			public string	Name;
			public short[]	Data;
			public bool		IsExist;
		}


		Dictionary<string, ElevationTile> ramCache = new Dictionary<string, ElevationTile>();

		bool IsElevationDataAvailable = true;



		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		/// <param name="config"></param>
		public ElevationLayer(Game game, LayerServiceConfig config) : base(game, config)
		{
			if (!Directory.Exists("cache/Elevation")) {
				IsElevationDataAvailable = false;
			}
		}


#if true

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lon"></param>
		/// <param name="lat"></param>
		/// <returns></returns>
		public float GetElevation(double lon, double lat)
		{
			if (!IsElevationDataAvailable) return 0;

			var name = GenerateFileName((int)lon, (int)lat);

			if (!ramCache.ContainsKey(name)) {
				LoadElevationTile(name);
			}

			var tile = ramCache[name];

			if (!tile.IsExist) return 0;

			double fracX = lon - (int)lon;
			double fracY = lat - (int)lat;

			int xInd = (int)(1201.0 * fracX);
			int yInd = 1200 - (int)(1201.0 * fracY);
			
			//int xNext = xInd + 1;
			//int yNext = yInd + 1;
			//
			//xNext = xNext >= 1201 ? 1200 : xNext;
			//yNext = yNext >= 1201 ? 1200 : yNext;

			float value = tile.Data[yInd*1201 + xInd];

			return value;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="lon"></param>
		/// <param name="lat"></param>
		/// <returns></returns>
		string GenerateFileName(int lon, int lat)
		{
			var s = String.Format("N{0:00}E{1:000}", lat, lon);
			return s;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		void LoadElevationTile(string fileName)
		{
			string dir		= "cache/Elevation/";
			string ext		= ".hgt";
			string filePath = String.Format("{0}{1}{2}", dir, fileName, ext);

			if (!File.Exists(filePath)) {
				ramCache.Add(fileName, new ElevationTile {
						Name	= fileName,
						Data	= null,
						IsExist = false
					});
				return;
			}

			var stream	= File.OpenRead(filePath);
			var br		= new BinaryReader(stream);

			var sData = new short[1201 * 1201];
			//short max = short.MinValue;
			//short min = short.MaxValue;

			for (int i = 0; i < 1201; i++) {
				for (int j = 0; j < 1201; j++) {
					var val = ReverseBytes(br.ReadInt16());

					if (val == -32768) val = 0;

					sData[i * 1201 + j] = val;

					//if (val > max) max = val;
					//if (val < min) min = val;
				}
			}

			br.Close();

			ramCache.Add(fileName, new ElevationTile {
				Name	= fileName,
				Data	= sData,
				IsExist = true
			});
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		short ReverseBytes(short value)
		{
			return (short)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
		}

#endif
	}
}
