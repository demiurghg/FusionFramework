using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Fusion.Mathematics;


namespace Fusion.Graphics
{
	[Obsolete]
	public class DynamicTexture : ShaderResource
	{

		public DynamicTexture(GraphicsDevice rs, int width, int height, ColorFormat format) : base(rs)
		{
			Width		=	width;
			Height		=	height;
			Depth		=	1;
			
			var tex = new D3D.Texture2D(rs.Device, new Texture2DDescription {
				ArraySize			= 1,
				BindFlags			= BindFlags.ShaderResource,
				CpuAccessFlags		= CpuAccessFlags.Write,
				Format				= Converter.Convert(format),
				Width				= width,
				Height				= height,
				MipLevels			= 1,
				OptionFlags			= ResourceOptionFlags.None,
				Usage				= ResourceUsage.Dynamic,
				SampleDescription	= new SampleDescription(1, 0)
			});

			SRV = new ShaderResourceView(rs.Device, tex);
		}


		public void WriteData(float[] data)
		{
			var dp = device.DeviceContext.MapSubresource(SRV.Resource, 0, MapMode.WriteDiscard, MapFlags.None);

			var mappedData = dp.DataPointer;
			int dataOffset = 0;
			for (int i = 0; i < Height; i++) {
				SharpDX.Utilities.Write(mappedData, data, dataOffset, Width);
				mappedData += dp.RowPitch;
				dataOffset += Width;
			}
			
			device.DeviceContext.UnmapSubresource(SRV.Resource, 0);
			//rs.DeviceContext.UpdateSubresource(data, SRV.Resource);
		}


		public void WriteData(Vector3[] data)
		{
		
			var dp = device.DeviceContext.MapSubresource(SRV.Resource, 0, MapMode.WriteDiscard, MapFlags.None);

			var mappedData = dp.DataPointer;
			int dataOffset = 0;
			for (int i = 0; i < Height; i++) {
				SharpDX.Utilities.Write(mappedData, data, dataOffset, Width);
				mappedData += dp.RowPitch;
				dataOffset += Width;
			}
			
			device.DeviceContext.UnmapSubresource(SRV.Resource, 0);

		}

	}
}
