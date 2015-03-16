using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;

namespace Fusion.GIS
{
	public class HeatMap
	{
#if false
		Game game;

		Vector2 leftBottomMerc, rightTopMerc;

		public int Width, Height;
		public Vector2 CellStep;
		public int Capacity		= 0;
		public int MaxAgents	= 10;
		public int MinAgents	= 0;
		public float Transparency = 1.0f;

		Ubershader computeShader;
		ConstantBuffer<HeatMapCData> cBuffer;

		[StructLayout(LayoutKind.Explicit)]
		public struct HeatMapCData {
			[FieldOffset(0)]	public Vector2	LeftBottom;
			[FieldOffset(8)]	public Vector2	RightTop;
			[FieldOffset(16)]	public Vector2	Offset;
			[FieldOffset(24)]	public float	Zoom;
			[FieldOffset(28)]	public uint		AgentsCount;
			[FieldOffset(32)]	public Vector2	CellStep;
			[FieldOffset(40)]	public int		Width;
			[FieldOffset(44)]	public int		Height;
			[FieldOffset(48)]	public int		MaxAgents;
			[FieldOffset(52)]	public int		MinAgents;
			[FieldOffset(56)]	public float	Transparency;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Element
		{
			[FieldOffset(0)]
			public Vector2 Position;
		}

		Buffer		dataBuffer;
		Buffer		stagingBuffer;
		Buffer		rwBuffer;
		Texture2D	finalTexture;
		Texture2D	tempTexture;

		Buffer readBuffer;

		UnorderedAccessView			rwUAV;
		UnorderedAccessView			finalTextureUAV;
		UnorderedAccessView			tempUAV;
		ShaderResourceView			dataSRV;
		ShaderResourceView			rwSRV;
		ShaderResourceView			tempSRV;
		public ShaderResourceView	FinalSRV;


		Texture2D pallete;

		public bool Visible = true;
		public bool Blur	= true;

		[Flags]
		public enum HeatMapFlags : uint
		{
			HM_ADD			= 0x0001,
			HM_DRAW			= 0x0002,
			HM_GAUS_BLUR	= 0x0004,

			BLUR_HORIZ		= 0x1000,
			BLUR_VERT		= 0x2000,
		}

		public void Init(Game game, int Capacity, Vector2 leftBottom, Vector2 rightTop, int Width, int Height)
		{
			this.game		= game;
			this.Capacity	= Capacity;
			this.Width		= Width;
			this.Height		= Height;

			var rs = game.GraphicsDevice;

			//////////////////////////// Init buffers ///////////////////////////////////////////
			var dataBufDesc = new BufferDescription {
				BindFlags			= BindFlags.ShaderResource,
				CpuAccessFlags		= CpuAccessFlags.None,
				OptionFlags			= ResourceOptionFlags.BufferStructured,
				Usage				= ResourceUsage.Default,
				SizeInBytes			= Capacity * Marshal.SizeOf(typeof(Element)),
				StructureByteStride = Marshal.SizeOf(typeof(Element))
			};

			dataBuffer = new Buffer(rs.Device, dataBufDesc);

			var stagingBufDesc = new BufferDescription {
				BindFlags			= BindFlags.None,
				CpuAccessFlags		= CpuAccessFlags.Write,
				OptionFlags			= ResourceOptionFlags.None,
				Usage				= ResourceUsage.Staging,
				SizeInBytes			= dataBufDesc.SizeInBytes,
				StructureByteStride = dataBufDesc.StructureByteStride
			};
			
			stagingBuffer = new Buffer(rs.Device, stagingBufDesc);
			
			stagingBufDesc.CpuAccessFlags = CpuAccessFlags.Read;
			readBuffer = new Buffer(rs.Device, stagingBufDesc);


			/////////////////////  ////////////////////////////////////
			var rwBufDesc = new BufferDescription {
				BindFlags			= BindFlags.ShaderResource | BindFlags.UnorderedAccess,
				CpuAccessFlags		= CpuAccessFlags.None,
				OptionFlags			= ResourceOptionFlags.BufferAllowRawViews,
				Usage				= ResourceUsage.Default,
				SizeInBytes			= Width * Height * Marshal.SizeOf(typeof(uint)),
				StructureByteStride = Marshal.SizeOf(typeof(uint))
			};

			rwBuffer = new Buffer(rs.Device, rwBufDesc);

			var textDesc = new Texture2DDescription
				{
					ArraySize			= 1,
					BindFlags			= BindFlags.ShaderResource | BindFlags.UnorderedAccess,
					CpuAccessFlags		= CpuAccessFlags.None,
					Format				= Format.R32G32B32A32_Float,
					Width				= Width,
					Height				= Height,
					MipLevels			= 1,
					OptionFlags			= ResourceOptionFlags.None,
					Usage				= ResourceUsage.Default,
					SampleDescription	= new SampleDescription{Count = 1}
				};


			finalTexture	= new Texture2D(rs.Device, textDesc);
			tempTexture		= new Texture2D(rs.Device, textDesc);

			///////////////////////////// Init UAV and SRV //////////////////////////////////////////
			ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription {
				Format		= Format.Unknown,
				Buffer		= { ElementCount = Capacity, FirstElement = 0 },
				Dimension	= ShaderResourceViewDimension.Buffer
			};

			dataSRV = new ShaderResourceView(rs.Device, dataBuffer, srvDesc);


			var rwUAVDesc = new UnorderedAccessViewDescription {
				Format		= Format.R32_Typeless,
				Dimension	= UnorderedAccessViewDimension.Buffer,
				Buffer		= new UnorderedAccessViewDescription.BufferResource {ElementCount = Width*Height, FirstElement = 0, Flags = UnorderedAccessViewBufferFlags.Raw}
			};

			rwUAV = new UnorderedAccessView(rs.Device, rwBuffer, rwUAVDesc);

			
			var uavDesc = new UnorderedAccessViewDescription
				{
					Format		= Format.R32G32B32A32_Float,
					Dimension	= UnorderedAccessViewDimension.Texture2D,
					Texture2D	= new UnorderedAccessViewDescription.Texture2DResource { MipSlice = 0 }
				};

			finalTextureUAV = new UnorderedAccessView(rs.Device, finalTexture, uavDesc);
			tempUAV			= new UnorderedAccessView(rs.Device, tempTexture, uavDesc);


			var rwSRVDesc = new ShaderResourceViewDescription {
				Format = Format.R32_UInt,
				Dimension	= ShaderResourceViewDimension.Buffer,
				Buffer		= new ShaderResourceViewDescription.BufferResource { ElementCount = Width*Height, FirstElement = 0}
			};

			rwSRV = new ShaderResourceView(rs.Device, rwBuffer, rwSRVDesc);
			

			var srvTexDesc = new ShaderResourceViewDescription
				{
					Format		= Format.R32G32B32A32_Float,
					Dimension	= ShaderResourceViewDimension.Texture2D,
					Texture2D	= new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 }
				};

			FinalSRV	= new ShaderResourceView(rs.Device, finalTexture,	srvTexDesc);
			tempSRV		= new ShaderResourceView(rs.Device, tempTexture,	srvTexDesc);

			computeShader = rs.CreateUberShader( "effects/HeatMap.hlsl", typeof(HeatMapFlags));
			cBuffer = rs.CreateConstBuffer<HeatMapCData>();


			rightTopMerc	= GeoHelper.WorldToTilePos(rightTop);
			leftBottomMerc	= GeoHelper.WorldToTilePos(leftBottom);

			CellStep	= rightTopMerc - leftBottomMerc;
			CellStep.X	= CellStep.X / Width;
			CellStep.Y	= CellStep.X;//CellStep.Y / Height;

			pallete = game.Content.Load<Texture2D>("textures/pallete.tga");
		}


		public void Update(Element[] newPositions)
		{
			var rs	= game.GraphicsDevice;
			var map = game.GetService<LayerService>().MapLayer;

			var dbox = rs.DeviceContext.MapSubresource(stagingBuffer, 0, MapMode.Write, MapFlags.None);
			Utilities.Write(dbox.DataPointer, newPositions, 0, newPositions.Length);
			rs.DeviceContext.UnmapSubresource(stagingBuffer, 0);

			rs.DeviceContext.ClearUnorderedAccessView(rwUAV, Int4.Zero);

			rs.DeviceContext.CopyResource(stagingBuffer, dataBuffer);

			cBuffer.Data.LeftBottom		= leftBottomMerc;
			cBuffer.Data.RightTop		= rightTopMerc;
			cBuffer.Data.Offset			= map.Offset;
			cBuffer.Data.Zoom			= map.Zoom;
			cBuffer.Data.AgentsCount	= (uint)Capacity;
			cBuffer.Data.CellStep		= CellStep;
			cBuffer.Data.Width			= Width;
			cBuffer.Data.Height			= Height;
			cBuffer.Data.MaxAgents		= MaxAgents;
			cBuffer.Data.MinAgents		= MinAgents;
			cBuffer.Data.Transparency	= Transparency;
			cBuffer.UpdateCBuffer();

			cBuffer.SetCBufferCS(0);

			computeShader.SetComputeShader((int)HeatMapFlags.HM_ADD);

			rs.DeviceContext.ComputeShader.SetUnorderedAccessViews(0, rwUAV);
			rs.DeviceContext.ComputeShader.SetShaderResource(0, dataSRV);

			rs.DeviceContext.Dispatch(Width/16, Height/16, 1);

			rs.DeviceContext.ComputeShader.SetUnorderedAccessView(0, null);
			rs.DeviceContext.ComputeShader.SetShaderResource(0, null);
			rs.DeviceContext.ComputeShader.SetConstantBuffer(0, null);


			cBuffer.SetCBufferCS(0);

			computeShader.SetComputeShader((int)HeatMapFlags.HM_DRAW);

			rs.DeviceContext.ComputeShader.SetUnorderedAccessViews(0, finalTextureUAV);
			rs.DeviceContext.ComputeShader.SetShaderResource(0, rwSRV);
			//rs.DeviceContext.ComputeShader.SetShaderResource(1, pallete);
			pallete.SetCS(1);

			rs.DeviceContext.Dispatch(Width/16, Height/16, 1);

			rs.DeviceContext.ComputeShader.SetUnorderedAccessView(0, null);
			rs.DeviceContext.ComputeShader.SetShaderResource(0, null);
			rs.DeviceContext.ComputeShader.SetShaderResource(1, null);
			rs.DeviceContext.ComputeShader.SetConstantBuffer(0, null);


			// Guassian Blur
			if (Blur) {
				//Horizontal Blur
				computeShader.SetComputeShader((int) HeatMapFlags.HM_GAUS_BLUR | (int) HeatMapFlags.BLUR_HORIZ);

				rs.DeviceContext.ComputeShader.SetUnorderedAccessViews(0, tempUAV);
				rs.DeviceContext.ComputeShader.SetShaderResource(0, FinalSRV);

				rs.DeviceContext.Dispatch(1, Height, 1);

				rs.DeviceContext.ComputeShader.SetUnorderedAccessView(0, null);
				rs.DeviceContext.ComputeShader.SetShaderResource(0, null);

				//Vertical Blur
				computeShader.SetComputeShader((int) HeatMapFlags.HM_GAUS_BLUR | (int) HeatMapFlags.BLUR_VERT);

				rs.DeviceContext.ComputeShader.SetUnorderedAccessViews(0, finalTextureUAV);
				rs.DeviceContext.ComputeShader.SetShaderResource(0, tempSRV);

				rs.DeviceContext.Dispatch(1, Height, 1);

				rs.DeviceContext.ComputeShader.SetUnorderedAccessView(0, null);
				rs.DeviceContext.ComputeShader.SetShaderResource(0, null);

				//////////////////
			}

			//rs.DeviceContext.CopyResource(dataBuffer, readBuffer);
			//
			//var dPointer = rs.DeviceContext.MapSubresource(readBuffer, 0, MapMode.Read, MapFlags.None);
			//
			//Vector2[] vecs = new Vector2[10000];
			//unsafe {
			//	Vector2* p = (Vector2*) dPointer.DataPointer;
			//	for (int i = 0; i < 10000; i++) {
			//		vecs[i] = p[i];
			//	}
			//}
			//
			//rs.DeviceContext.UnmapSubresource(readBuffer, 0);
		}
#endif
	}
}
