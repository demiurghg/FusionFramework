using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;
using Fusion;



namespace Fusion.GIS.LayerSpace.Layers
{
	partial class GlobeLayer
	{
		public int HeatMapDim { protected set; get; }

		public float[] HeatMapData;
		public float[] InfectData;
		double heatMapLeft = 0, heatMapRight = 0, heatMapTop = 0, heatMapBottom = 0;

		VertexBuffer	heatVB;
		IndexBuffer		heatIB;

		Texture2D heatMap;
		Texture2D infectMap;
		Texture2D heatMapPalette;



		void InitHeatMap(int mapDim)
		{
			HeatMapDim	= mapDim;
			HeatMapData = new float[HeatMapDim*HeatMapDim];
			InfectData	= new float[HeatMapDim * HeatMapDim];

			heatMap		= new Texture2D(Game.GraphicsDevice, mapDim, mapDim, ColorFormat.R32F, false);
			infectMap	= new Texture2D(Game.GraphicsDevice, mapDim, mapDim, ColorFormat.R32F, false);
			//heatMap		= Game.GraphicsDevice.CreateDynamicTexture(mapDim, mapDim);
			//infectMap	= Game.GraphicsDevice.CreateDynamicTexture(mapDim, mapDim);

			//HeatMapData[0] = 1.0f;
			//HeatMapData[mapDim] = 0.5f;
			//for (int i = 0; i < HeatMapData.Length; i++) {
			//	HeatMapData[i] = (float)r.NextDouble();
			//}
		}


		public void SetHeatMapCoordinates(double left, double right, double top, double bottom)
		{
			if (heatMapLeft == left && heatMapRight == right && heatMapTop == top && heatMapBottom == bottom) return;

			heatMapLeft		= left;
			heatMapRight	= right;
			heatMapTop		= top;
			heatMapBottom	= bottom;

			if (heatVB != null) {
				heatVB.Dispose();
			}
			if (heatIB != null) {
				heatIB.Dispose();
			}

			var lt = GeoHelper.WorldToTilePos(new DVector2(left, top));
			var rb = GeoHelper.WorldToTilePos(new DVector2(right, bottom));

			GenerateTileGrid(10, out heatVB, out heatIB, lt.X, rb.X, lt.Y, rb.Y, 0);
		}

		public void UpdateHeatMapData()
		{
			heatMap.SetData(HeatMapData);

			for (int i = 0; i < HeatMapData.Length; i++) {
				HeatMapData[i] = 0.0f;
			}
			//var byteArray = new byte[HeatMapDim * HeatMapDim * 4];
			//
			//Buffer.BlockCopy(HeatMapData, 0, byteArray, 0, byteArray.Length);
		}


		public void UpdateInfectData()
		{
			infectMap.SetData(InfectData);
			
			for (int i = 0; i < InfectData.Length; i++) {
				InfectData[i] = 0.0f;
			}
		}


		public void HeatMapCoords(out double left, out double right, out double top, out double bottom)
		{
			left	= heatMapLeft;
			right	= heatMapRight;
			top		= heatMapTop;
			bottom	= heatMapBottom;
		}
	}
}
