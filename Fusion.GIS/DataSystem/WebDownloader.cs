using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Fusion.GIS.DataSystem
{
	// http://www.openstreetmap.org/api/0.6/map?bbox=30.169,59.9158,30.3133,59.9677
	// http://www.overpass-api.de/api/map?bbox=30.169,59.9158,30.3133,59.9677
	// http://overpass.osm.rambler.ru/cgi/xapi_meta?*[bbox=30.169,59.9158,30.3133,59.9677]
	// 
	public class WebDownloader
	{
		int TimeoutMs			= 500000;
		string requestAccept	= "*/*";
		string RefererUrl;
		string UserAgent;
		string CacheFolder;

		Random r = new Random();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="refererUrl"></param>
		/// <param name="cacheFolder"></param>
		public WebDownloader(string refererUrl, string cacheFolder)
		{
			CacheFolder = cacheFolder;
			RefererUrl	= refererUrl;
			UserAgent	= string.Format("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:{0}.0) Gecko/{2}{3:00}{4:00} Firefox/{0}.0.{1}", r.Next(3, 14), r.Next(1, 10), r.Next(DateTime.Today.Year - 4, DateTime.Today.Year), r.Next(12), r.Next(30));
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="Url"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public XmlDocument DownloadXml(string Url, string path)
		{

			try {
				var response = GetResponseStream(Url);

				var doc = new XmlDocument();
				doc.Load(response);
				doc.Save(path);

				return doc;
			}
			catch (Exception e)
			{
#if DEBUG
				Log.Warning(e.Message);
#endif
				return null;
			}
		}


		Stream GetResponseStream(string Url)
		{
			try {
				var request = (HttpWebRequest) WebRequest.Create(Url);

				request.CachePolicy			= new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheIfAvailable);
				request.Timeout				= TimeoutMs;
				request.UserAgent			= UserAgent;
				request.ReadWriteTimeout	= TimeoutMs*6;
				request.Accept				= requestAccept;

				var response = (HttpWebResponse) request.GetResponse();

				return response.GetResponseStream();

			} catch (Exception e) {
                Console.WriteLine(e.Message);
				return null;
			}
		}

	}
}
