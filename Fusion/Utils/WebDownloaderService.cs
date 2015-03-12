using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Fusion
{
	public class WebDownloaderService : GameService
	{
		string	requestAccept = "*/*";
		string	UserAgent;

		Random r = new Random();



		public WebDownloaderService(Game game) : base(game)
		{
			UserAgent	= string.Format("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:{0}.0) Gecko/{2}{3:00}{4:00} Firefox/{0}.0.{1}", r.Next(3, 14), r.Next(1, 10), r.Next(DateTime.Today.Year - 4, DateTime.Today.Year), r.Next(12), r.Next(30));
			
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="path"></param>
		/// <param name="timeout"></param>
		public void DownloadImage(string url, string path, int timeout = 50000)
		{
			var response = GetResponseStream(url, timeout);

			if(response == null) throw new Exception("Image not dowloaded, no response from : " + url);
			
			var bitmap = new System.Drawing.Bitmap(response);
			bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
		}


		public byte[] DownloadImageByte(string url, int timeout = 50000)
		{
			var response = GetResponseStream(url, timeout);

			if(response == null) throw new Exception("Image not dowloaded, no response from : " + url);

			return ReadFully(response);
		}


		public static byte[] ReadFully(Stream input)
		{
			byte[] buffer = new byte[16 * 1024];
			using (MemoryStream ms = new MemoryStream()) {
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public XmlDocument DownloadXml(string url, string path)
		{

			try {
				var response = GetResponseStream(url);

				var doc = new XmlDocument();
				doc.Load(response);
				doc.Save(path);

				return doc;
			} catch (Exception e) {
//#if DEBUG
				Log.Warning(e.Message);
//#endif
				return null;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="timeoutMs"></param>
		/// <returns></returns>
		Stream GetResponseStream(string url, int timeoutMs = 5000)
		{
			try {
				var request = (HttpWebRequest) WebRequest.Create(url);

				//request.CachePolicy			= new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
				request.Timeout				= timeoutMs;
				request.UserAgent			= UserAgent;
				request.ReadWriteTimeout	= timeoutMs * 6;
				request.Accept				= requestAccept;
				
				
				var response = (HttpWebResponse) request.GetResponse();

				return response.GetResponseStream();

			} catch (Exception e) {
                Log.Warning(e.Message);
				return null;
			}
		}
	}
}
