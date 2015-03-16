using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.GIS.GlobeMath;
using TriangleNet;
using TriangleNet.Geometry;

namespace Fusion.GIS.LayerSpace.Layers
{
	public partial class GeoObjectsLayer
	{
		[Serializable]
		public class MunicipalDivision
		{
			public string			Name;
			public List<DVector2>	Contour;
		}

		public List<MunicipalDivision> MunicipalDivisions = new List<MunicipalDivision>();



		/// <summary>
		/// 
		/// </summary>
		void LoadMunicipalDivisions()
		{
			if (Directory.Exists("cache/MOs")) {
				var files = Directory.GetFiles("cache/MOs");
				if (files.Length != 0) {
					LoadMunicipalDivisionsFromCahce(files);
					return;
				}
			} else {
				Directory.CreateDirectory("cache/MOs");
			}

			if (!File.Exists("MO.txt")) return;

			var sr		= new StreamReader(File.OpenRead("MO.txt"));
			var mo		= new MunicipalDivision();
			mo.Contour	= new List<DVector2>();

			while (!sr.EndOfStream) {

				var line = sr.ReadLine();

				if (line == "") {
					MunicipalDivisions.Add(mo);
				} 
				else if (Char.IsDigit(line[0])) {
					var		strs	= line.Split('	');
					float	lat		= float.Parse(strs[0]);
					float	lon		= float.Parse(strs[1]);

					mo.Contour.Add(new DVector2(lon, lat));
				} else {
					mo			= new MunicipalDivision();
					mo.Name		= line;
					mo.Contour	= new List<DVector2>();
				}
			}

			sr.Close();

			foreach (var division in MunicipalDivisions) {
				var fileStream = File.Create("cache/MOs/" + division.Name + ".bin");
				var serializer = new BinaryFormatter();
				serializer.Serialize(fileStream, division);
				fileStream.Close();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="files"></param>
		void LoadMunicipalDivisionsFromCahce(string[] files)
		{
			foreach (var file in files) {
				Stream fileStream = File.OpenRead(file);
				var deserializer = new BinaryFormatter();
				var division = (MunicipalDivision)deserializer.Deserialize(fileStream);
				fileStream.Close();

				MunicipalDivisions.Add(division);
			}
		}

	}
}
