using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.GIS;
using Fusion.Graphics;
using Fusion.Mathematics;

namespace Fusion.GIS.LayerSpace.Layers
{
#if false
	public class SimpleBuildings : BaseLayer
	{
		public struct SimpleBuilding
		{
			[Vertex("POSITION")]	public Vector2 Position;
			[Vertex("COLOR")]		public Vector4 Color;
			[Vertex("TEXCOORD", 0)]	public Vector4 WLHRot;
		}


		ConstBufferGeneric<ConstData>	constBuffer;
		Ubershader				drawShader;

		VertexBuffer	vertexBuffer;

		[Flags]
		public enum BuildingsFlags : int
		{
			BUILDINGS_DRAW = 0x0001,
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct ConstData 
		{
			[FieldOffset(0)]	public	Matrix 	ViewProj	;
			[FieldOffset(64)]	public	Vector4 Zoom		;
			[FieldOffset(80)]	public	Vector4 Offset		;
			[FieldOffset(96)]	public	Vector4 ViewPosition;
		}


		public SimpleBuildings(Game game, List<SimpleBuilding> buildings, LayerServiceConfig config) : base(game, config)
		{
			var rs = game.GraphicsDevice;
			var ml = Game.GetService<LayerService>().MapLayer;

			constBuffer = rs.CreateConstBuffer<ConstData>();
			drawShader	= rs.CreateUberShader("SimpleBuildings.hlsl", typeof(BuildingsFlags));


			vertexBuffer = rs.CreateVertexBuffer(typeof(SimpleBuilding), buildings.Count);
			vertexBuffer.SetData(buildings.ToArray(), 0, buildings.Count);

			//MeshInstance mi = new MeshInstance(rs, GenerateMesh(buildings), Matrix.Scaling(ml.Zoom)*Matrix.Translation(ml.Offset.X, 0.0f, ml.Offset.Y), "Ololo");
			//rs.Instances.Add(mi);
		}


		public override void Draw(GameTime gameTime)
		{
			var rs	= Game.GraphicsDevice;
			var cam = Game.GetService<Camera>();
			var ls	= Game.GetService<LayerService>();

			constBuffer.Data.ViewProj		= cam.ViewMatrix*cam.ProjMatrix;
			constBuffer.Data.Zoom			= new Vector4(ls.MapLayer.Zoom);
			constBuffer.Data.Offset			= new Vector4(ls.MapLayer.Offset, 0.0f, 0.0f);
			constBuffer.Data.ViewPosition	= new Vector4(cam.CameraMatrix.TranslationVector, 0.0f);
			constBuffer.UpdateCBuffer();


			string signature;
			drawShader.SetVertexShader((int)BuildingsFlags.BUILDINGS_DRAW, out signature);

			drawShader.SetGeometryShader(	(int)BuildingsFlags.BUILDINGS_DRAW);
			drawShader.SetPixelShader(		(int)BuildingsFlags.BUILDINGS_DRAW);
			constBuffer.SetCBufferVS(0);
			constBuffer.SetCBufferGS(0);
			constBuffer.SetCBufferPS(0);

			rs.SetupVertexInput(vertexBuffer, null, signature);

			rs.SetBlendState(BlendState.Opaque);
			rs.SetDepthStencilState(DepthStencilState.Default);
			rs.SetRasterizerState(RasterizerState.CullCCW);


			rs.Draw(Primitive.PointList, vertexBuffer.Capacity, 0);

		}


		public override void Update(GameTime gameTime)
		{
			//var rs = Game.GraphicsDevice;
			//var ml = Game.GetService<LayerService>().MapLayer;
		}

		public Mesh GenerateMesh(List<SimpleBuilding> buildings)
		{
			Mesh mesh = new Mesh();
			//mesh.AddMaterial("");
			mesh.ShadingGroups.Add(new Mesh.ShadingGroup() { MaterialIndex = 0, PrimitiveCount = 12*buildings.Count, StartPrimitive = 0});
			mesh.AddMaterial("default");
			
			//var cub = Game.Content.Load<Scene>("scenes/Cube.fbx").Nodes[1];
			//cub.Mesh.

			Vector3[] cube = new Vector3[8];
			cube[0] = new Vector3(0, 0, 0);
			cube[1] = new Vector3(1, 0, 0);
			cube[2] = new Vector3(0, 0, 1);
			cube[3] = new Vector3(1, 0, 1);
			cube[4] = new Vector3(0, 1, 0);
			cube[5] = new Vector3(1, 1, 0);
			cube[6] = new Vector3(0, 1, 1);
			cube[7] = new Vector3(1, 1, 1);

			int primitivesCount = 0;
			float scale			= 0.0000001f;
			foreach (var simB in buildings) {
				float width = simB.WLHRot.X*scale;
				float height = simB.WLHRot.Z*scale;
				float length = simB.WLHRot.Y*scale;

				var mat = Matrix.Scaling(width, height, length) * Matrix.Translation(-width/2, 0.0f, -length/2) * Matrix.Translation(simB.Position.X, 0.0f, simB.Position.Y);

				var newPoints = new Vector3[8];
				for (int i = 0; i < 8; i++) {
					var res			= Vector3.Transform(cube[i], mat);
					newPoints[i]	= new Vector3(res.X, res.Y, res.Z);
				}


				// Bottom
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[0], Normal = new Vector3(0, -1, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[1], Normal = new Vector3(0, -1, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[2], Normal = new Vector3(0, -1, 0), Color0 = Color.Gray });

				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[1], Normal = new Vector3(0, -1, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[3], Normal = new Vector3(0, -1, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[2], Normal = new Vector3(0, -1, 0), Color0 = Color.Gray });
				
				// Top
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[4], Normal = new Vector3(0, 1, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[6], Normal = new Vector3(0, 1, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[5], Normal = new Vector3(0, 1, 0), Color0 = Color.Gray });
				
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[5], Normal = new Vector3(0, 1, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[6], Normal = new Vector3(0, 1, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[7], Normal = new Vector3(0, 1, 0), Color0 = Color.Gray });
				
				// Back
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[1], Normal = new Vector3(0, 0, -1), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[0], Normal = new Vector3(0, 0, -1), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[4], Normal = new Vector3(0, 0, -1), Color0 = Color.Gray });
				
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[1], Normal = new Vector3(0, 0, -1), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[4], Normal = new Vector3(0, 0, -1), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[5], Normal = new Vector3(0, 0, -1), Color0 = Color.Gray });
				
				// Front
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[3], Normal = new Vector3(0, 0, 1), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[7], Normal = new Vector3(0, 0, 1), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[2], Normal = new Vector3(0, 0, 1), Color0 = Color.Gray });
				
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[2], Normal = new Vector3(0, 0, 1), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[7], Normal = new Vector3(0, 0, 1), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[6], Normal = new Vector3(0, 0, 1), Color0 = Color.Gray });
				
				// Right
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[0], Normal = new Vector3(-1, 0, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[2], Normal = new Vector3(-1, 0, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[4], Normal = new Vector3(-1, 0, 0), Color0 = Color.Gray });
				
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[2], Normal = new Vector3(-1, 0, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[6], Normal = new Vector3(-1, 0, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[4], Normal = new Vector3(-1, 0, 0), Color0 = Color.Gray });
				
				// Left
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[1], Normal = new Vector3(1, 0, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[5], Normal = new Vector3(1, 0, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[3], Normal = new Vector3(1, 0, 0), Color0 = Color.Gray });
				
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[5], Normal = new Vector3(1, 0, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[7], Normal = new Vector3(1, 0, 0), Color0 = Color.Gray });
				mesh.AddVertex(new Mesh.Vertex { Position = newPoints[3], Normal = new Vector3(1, 0, 0), Color0 = Color.Gray });


				for (int i = 0; i < 12*3; i+=3) {
					int ind0 = primitivesCount++;
					int ind1 = primitivesCount++;
					int ind2 = primitivesCount++;
					mesh.Triangles.Add(new Mesh.Triangle(ind0, ind1, ind2));
				}
			}

			//var rs = Game.GetService<RenderSystem>();
			//mesh.Tag = new RendMesh(rs, mesh);
			//

			return mesh;
		}
	}
#endif
}
