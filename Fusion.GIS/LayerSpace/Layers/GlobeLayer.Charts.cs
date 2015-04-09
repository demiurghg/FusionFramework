using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.DataSystem.GeoObjectsSources;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;

namespace Fusion.GIS.LayerSpace.Layers
{
    public partial class GlobeLayer
    {
	    List<GeoVert> chartsCPU;
		VertexBuffer chartsVB;


		void UpdateCharts(List<GeoVert> charts)
		{
			chartsCPU = charts;

			if (chartsVB == null) {
				chartsVB = new VertexBuffer(Game.GraphicsDevice, typeof (GeoVert), chartsCPU.Count);
			}

			if (chartsVB.Capacity != chartsCPU.Count) {
				chartsVB.Dispose();
				chartsVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), chartsCPU.Count);
			}

			chartsVB.SetData(chartsCPU.ToArray(), 0, chartsCPU.Count);
		}


		void DrawCharts()
		{
			//#if false
			if (chartsCPU == null) return;

			//string signature;
			//shader.SetVertexShader((int)(GlobeFlags.DRAW_LINES | GlobeFlags.VERTEX_SHADER | GlobeFlags.USE_GEOCOORDS), out signature);
			//shader.SetPixelShader((int)GlobeFlags.DRAW_CHARTS);
			//shader.SetGeometryShader((int)GlobeFlags.DRAW_CHARTS);

			//Game.GraphicsDevice.PipelineState = factory[(int)(GlobeFlags.DRAW_LINES | GlobeFlags.VERTEX_SHADER | GlobeFlags.USE_GEOCOORDS | GlobeFlags.DRAW_CHARTS)];
			//Game.GraphicsDevice.PipelineState.BlendState		= BlendState.Opaque;
			//Game.GraphicsDevice.DepthStencilState				= DepthStencilState.Default;
			//Game.GraphicsDevice.PipelineState.RasterizerState	= RasterizerState.CullCCW;


			//Game.GraphicsDevice.SetBlendState(BlendState.Opaque);
			//Game.GraphicsDevice.SetDepthStencilState(DepthStencilState.Default);

			//dotsBuf.SetCBufferVS(1);
			//dotsBuf.SetCBufferGS(1);

			Game.GraphicsDevice.VertexShaderConstants[1]	= dotsBuf;
			Game.GraphicsDevice.GeometryShaderConstants[1]	= dotsBuf;


			//Game.GraphicsDevice.SetRasterizerState(RasterizerState.CullCCW);

			Game.GraphicsDevice.PipelineState = myMiniFactory.ProducePipelineState(
					GlobeFlags.DRAW_VERTEX_LINES | GlobeFlags.VERTEX_SHADER | GlobeFlags.USE_GEOCOORDS | GlobeFlags.DRAW_CHARTS,
					Primitive.PointList,
					BlendState.Opaque,
					RasterizerState.CullCCW,
					DepthStencilState.Default);


			Game.GraphicsDevice.SetupVertexInput(chartsVB, null);
			Game.GraphicsDevice.Draw(/*Primitive.PointList,*/ chartsVB.Capacity, 0);

			//shader.ResetGeometryShader();
			//#endif
		}

    }
}
