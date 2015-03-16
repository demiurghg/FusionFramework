using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Fusion.Mathematics;

namespace Fusion.GIS.DataSystem.GeoObjectsSources
{
	public partial class WikiMapiaSource
	{
		WebDownloader downloader = new WebDownloader("http://api.wikimapia.org/", @"cache\WikiMapia");


		string Format = "http://api.wikimapia.org/?key={0}&function={1}&language={2}&page={3}&count={4}";

		//string key;
		//string function;
		//string coordsBy;
		//string language;
		//string page;
		//string count; // 100 max


		readonly string coordsByXYZ		= "xyz&x={0}&y={1}&z={2}";
		//readonly string coordsByLatLon	= "latlon&lon_min={0}&lat_min={1}&lon_max={2}&lat_max={3}";
		const string CoordsByBBox = "bbox&bbox={0},{1},{2},{3}";

		const string PathFormat = "cache/WikiMapia/wmt{0:00000}{1:00000}{2:00}_{3:0000}.xml";
		const string PathFormatByID = "cache/WikiMapia/id_{0}.xml";

		const string FunctionPlaceGetByArea = "place.getbyarea&coordsby={0}";
		const string FunctionPlaceGetByID	= "place.getbyid&id={0}&data_blocks=main,geometry,location";


		public void GetByArea(int x, int y, int zoom, bool allPages = false, string language = "ru")
		{
			int page	= 1;
			int count	= 100;

			string key = GetKey();

			string coords	= String.Format(coordsByXYZ, x, y, zoom);
			string func		= String.Format(FunctionPlaceGetByArea, coords);

			string url	= string.Format(Format, key, func, language, page, count);
			string path = String.Format(PathFormat, x, y, zoom, page);

			var xmlDoc = LoadWikiDocument(url, path);

			int elementsCount;
			ParseXmlDocument(xmlDoc, out elementsCount);

			if (allPages) {
				int pagesCount = (int)Math.Ceiling((float)elementsCount/count);

				for (int i = 2; i < pagesCount/2; i++) {
					url		= string.Format(Format, key, func, language, i, count);
					path	= String.Format(PathFormat, x, y, zoom, i);

					xmlDoc = LoadWikiDocument(url, path);

					if (xmlDoc == null) {
						Console.WriteLine(url);
					} else {
						ParseXmlDocument(xmlDoc, out elementsCount);
					}
				}
			}

			//pl = RamCache[0];
			
			//Console.WriteLine();
		}

		//WikiMapiaPlace pl;


		XmlDocument LoadWikiDocument(string url, string path)
		{
			XmlDocument xmlDoc;
			if (File.Exists(path)) {
				xmlDoc = new XmlDocument();
				xmlDoc.Load(path);
			} else {
				xmlDoc = downloader.DownloadXml(url, path);
			}
			return xmlDoc;
		}

		public void GetByArea(float lonMin, float latMin, float lonMax, float latMax, int page, int count = 100, string language = "ru")
		{
			string key = GetKey();

			string coords	= String.Format(CoordsByBBox, lonMin, latMin, lonMax, latMax);
			string func		= String.Format(FunctionPlaceGetByArea, coords);

			string url		= string.Format(Format, key, func, language, page, count) + "&category=44786";

			string path = "cache/WikiMapia/area.xml";

			var xmlDoc = LoadWikiDocument(url, path);

			int elementsCount;
			ParseXmlDocument(xmlDoc, out elementsCount);
		}


		public WikiMapiaPlace GetByID(int id)
		{
			string key = GetKey();

			string func = String.Format(FunctionPlaceGetByID, id);
			string url	= string.Format(Format, key, func, "en", "", "");
			string path = String.Format(PathFormatByID, id);

			var		xmlDoc		= LoadWikiDocument(url, path);
			XmlNode mainNode	= xmlDoc.SelectSingleNode("wm");

			var	p = ParseWikiPlace(mainNode);

			RamCache.Add(p.Id, p);

			return p;
		}
		


		void ParseXmlDocument(XmlDocument xmlDoc, out int elementsFound)
		{
			XmlNode mainNode	= xmlDoc.SelectSingleNode("wm");
			XmlNode places		= mainNode.SelectSingleNode("places");

			string	lang			= mainNode.SelectSingleNode("language").InnerText;
			elementsFound = int.Parse(mainNode.SelectSingleNode("found").InnerText); // TryParse
			int		pageNumber		= int.Parse(mainNode.SelectSingleNode("page").InnerText);
			int		elementsCount	= int.Parse(mainNode.SelectSingleNode("count").InnerText);

			foreach (XmlNode place in places) {

				var p = ParseWikiPlace(place);

				if (!RamCache.ContainsKey(p.Id)) {
					RamCache.Add(p.Id, p);
				}
			}
		}


		WikiMapiaPlace ParseWikiPlace(XmlNode place)
		{
			string id = place.SelectSingleNode("id").InnerText;

			XmlNode				loc			= place.SelectSingleNode("location");
			WikiMapiaLocation	location	= new WikiMapiaLocation {
					Lon				= float.Parse(loc.SelectSingleNode("lon").InnerText),
					Lat				= float.Parse(loc.SelectSingleNode("lat").InnerText),
					North			= float.Parse(loc.SelectSingleNode("north").InnerText),
					South			= float.Parse(loc.SelectSingleNode("south").InnerText),
					East			= float.Parse(loc.SelectSingleNode("east").InnerText),
					West			= float.Parse(loc.SelectSingleNode("west").InnerText),
					Country			= loc.SelectSingleNode("country").InnerText,
					State			= loc.SelectSingleNode("state").InnerText,
					Place			= loc.SelectSingleNode("place").InnerText,
					CountryAdmId	= int.Parse(loc.SelectSingleNode("country_adm_id").InnerText)
				};

			var title = place.SelectSingleNode("title");

			WikiMapiaPlace p = new WikiMapiaPlace {
					Id			= int.Parse(id),
					Title		= title!=null ? title.InnerText : "No Title",
					Location	= location,
					Polygon		= new List<Vector2>()
				};

			XmlNode poly = place.SelectSingleNode("polygon");
			foreach (XmlNode child in poly.ChildNodes) {
				float x = float.Parse(child.SelectSingleNode("x").InnerText);
				float y = float.Parse(child.SelectSingleNode("y").InnerText);

				p.Polygon.Add(new Vector2(x, y));
			}


			p.Tags = new Dictionary<string, string>();

			var tags = place.SelectSingleNode("tags");
			foreach (XmlNode child in tags.ChildNodes) {
				p.Tags.Add(child.SelectSingleNode("id").InnerText, child.SelectSingleNode("title").InnerText);
			}

			return p;
		}

	}
}
