using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.GlobeMath;
using Fusion.Graphics;
using Fusion.Mathematics;

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GlobeLayer
	{
		VertexBuffer	atmosVB;
		IndexBuffer		atmosIB;

		Texture2D atmosTexture;
		Texture2D atmosNextTexture;

		Texture2D arrowTex;

		Vector3[][] atmosData;

		float	atmosTime	= 0.0f;
		int		atmosMapInd = 0;

		void InitAtmosphere()
		{
			if(!Directory.Exists("cache/Atmosphere")) return;

			var tindexes = new List<int>();

			int rowsCount		= 25;
			int columnsCount	= 25;
			for (int row = 0; row < rowsCount - 1; row++) {
				for (int col = 0; col < columnsCount - 1; col++) {
					tindexes.Add(col + row			* columnsCount);
					tindexes.Add(col + (row + 1)	* columnsCount);
					tindexes.Add(col + 1 + row		* columnsCount);

					tindexes.Add(col + 1 +	row			* columnsCount);
					tindexes.Add(col +		(row + 1)	* columnsCount);
					tindexes.Add(col + 1 +	(row + 1)	* columnsCount);
				}
			}

			atmosIB = new IndexBuffer(Game.GraphicsDevice, tindexes.Count);
			atmosIB.SetData(tindexes.ToArray(), 0, tindexes.Count);

			atmosVB = new VertexBuffer(Game.GraphicsDevice, typeof(GeoVert), rowsCount * columnsCount);

			atmosTexture		= new Texture2D(Game.GraphicsDevice, columnsCount, rowsCount, ColorFormat.Rgb32F, false);
			atmosNextTexture	= new Texture2D(Game.GraphicsDevice, columnsCount, rowsCount, ColorFormat.Rgb32F, false);


			arrowTex = Game.Content.Load<Texture2D>("arrow.tga");

			var verts = LoadGrid(Directory.GetFiles("cache/Atmosphere/Temperature")[0]);

			atmosVB.SetData(verts.ToArray(), 0, verts.Count);


			LoadWindAndTempFromFolder();

			
			atmosTexture.SetData(atmosData[0]);
			atmosNextTexture.SetData(atmosData[1]);
		}



		void UpdateAtmosphere(GameTime gameTime)
		{
			if (atmosData == null) return;

			atmosTime += gameTime.ElapsedSec;
			if (atmosTime >= 1.0f) {
				atmosMapInd++;
				if (atmosMapInd >= atmosData.Length) atmosMapInd = 0;
				atmosTime = atmosTime - (int)atmosTime;

				atmosTexture.SetData(atmosData[atmosMapInd]);

				int nextInd = atmosMapInd + 1;
				if (nextInd >= atmosData.Length) nextInd = 0;
				atmosNextTexture.SetData(atmosData[nextInd]);
			}
		}


		void LoadWindAndTempFromFolder()
		{
			var tempFiles = Directory.GetFiles("cache/Atmosphere/Temperature");

			atmosData = new Vector3[tempFiles.Length][];

			// Temperature
			for (int i = 0; i < tempFiles.Length; i++) {
				var sr = new StreamReader(tempFiles[i]);

				var temps = new Vector3[25 * 25];
				int ind = 0;
				while (!sr.EndOfStream) {
					var str = sr.ReadLine();
					if (str == "") {
						continue;
					}
					var strs = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

					temps[ind] = new Vector3(float.Parse(strs[2]), 0.0f, 0.0f);

					ind++;
				}

				sr.Close();

				atmosData[i] = temps;
			}


			var windFiles = Directory.GetFiles("cache/Atmosphere/Wind");

			// Wind
			for (int i = 0; i < windFiles.Length; i++) {
				var sr = new StreamReader(windFiles[i]);

				int ind = 0;
				while (!sr.EndOfStream) {
					var str = sr.ReadLine();
					if (str == "") {
						continue;
					}
					var strs = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

					atmosData[i][ind].Y = float.Parse(strs[2]);
					atmosData[i][ind].Z = MathUtil.Rad(float.Parse(strs[3]));

					ind++;
				}

				sr.Close();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		List<GeoVert> LoadGrid(string fileName)
		{
			var verts = new List<GeoVert>();

			var sr = new StreamReader(fileName);

			int i = 0, j = 0;

			float step = 1.0f/24.0f;

			while (!sr.EndOfStream) {
				var str = sr.ReadLine();
				if (str == "") {
					i++;
					j = 0; 
					continue;
				}
				var strs = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

				verts.Add(new GeoVert {
					Lon			= DMathUtil.DegreesToRadians(double.Parse(strs[0])),
					Lat			= DMathUtil.DegreesToRadians(double.Parse(strs[1])),
					Color		= Color.White,
					Position	= Vector3.Zero,
					Tex			= new Vector4(step*j, step*i, 0, 0)
				});

				j++;
			}

			sr.Close();

			return verts;
		}

	}
}
