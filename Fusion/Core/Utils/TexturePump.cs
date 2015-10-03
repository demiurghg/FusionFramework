using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using Fusion;
using Fusion.Graphics;
using Fusion.Pipeline;
using Fusion.Content;

namespace Fusion {
	public partial class TexturePump : GameService {

		[Config]
		public TexturePumpConfig	Config { get; set; }
		
		public float	SizeInMb	{ get { return (float)((double)sizeInBytes/(1024.0*1024.0)); } }
		
		long sizeInBytes = 0;

		Dictionary<string, Item>	items			= new Dictionary<string, Item>();
		ConcurrentQueue<Item>		downloadQueue	= new ConcurrentQueue<Item>();  

		protected string cachePath = "Cache/TexturePump/";

		bool TaskStopRequest = false;
		Task pumpTask;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="game"></param>
		public TexturePump ( Game game ) : base( game ) 
		{
			Config		=	new TexturePumpConfig();
			pumpTask	= Task.Factory.StartNew(LoadingFunction);

			if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);
		}



		/// <summary>
		/// Disposes stuff
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				TaskStopRequest = true;

				pumpTask.Wait();

				foreach (var item in items) {
					if (item.Value.Texture != null) {
						item.Value.Texture.Dispose();
					}
				}
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Initialize ()
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public override void Update ( GameTime gameTime )
		{
			if (Config.ShowStatistics) {

				var ds = Game.GetService<DebugStrings>();

				ds.Add("Texture pump statistics:");
				ds.Add("Textures ready     - " + items.Count(x => x.Value.Status == TexturePumpStatus.Ready));
				ds.Add("Textures loading   - " + items.Count(x => x.Value.Status == TexturePumpStatus.Loading));
				ds.Add("Textures failed    - " + items.Count(x => x.Value.Status == TexturePumpStatus.Failed));
				ds.Add("Size in memory, mb - " + SizeInMb);
			}


			if (SizeInMb > Config.MemoryCacheSize) {
				RemoveOldTextures();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		public override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="texture"></param>
		/// <returns></returns>
		public TexturePumpStatus Load ( string path, out Texture2D texture )
		{
			texture	=	null;

			if (items.ContainsKey(path)) {
				var item = items[path];
				if (item.Status == TexturePumpStatus.Ready) texture = item.Texture;
				
				// Update request time
				item.LastRequestTime = DateTime.Now;
				
				return item.Status;
			} else {
				var item = new Item(path);
				
				items.Add(path, item);

				downloadQueue.Enqueue(item);

				return TexturePumpStatus.Loading;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public void RemoveAllFailedTextures()
		{
			var failedList = items.Where(x => x.Value.Status == TexturePumpStatus.Failed).Select(x => x.Key).ToList();

			foreach (var key in failedList) {
				items.Remove(key);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		void LoadingFunction()
		{
			while (!TaskStopRequest) {
				Item item;

				if (!downloadQueue.TryDequeue(out item)) continue;

				try {
					string filePath = item.Path;

					// Download image if url was given
					if (item.Path.StartsWith("http", true, CultureInfo.InvariantCulture)) {
						var wds = Game.GetService<WebDownloaderService>();

						filePath = cachePath + ContentUtils.CalculateMD5Hash(item.Path);

						if (!File.Exists(filePath)) {
							wds.DownloadImage(item.Path, filePath, Config.NetworkTimeout);
						}
					}

					// Load texture
					using ( var stream = File.OpenRead( filePath ) ) {
						item.Texture		= new Texture2D(Game.GraphicsDevice, stream, false);
						item.SizeInBytes	= (int)(item.Texture.Width*item.Texture.Height*4 * 1.3f);
					}

					Interlocked.Add(ref sizeInBytes, (long)item.SizeInBytes);

					item.Status			= TexturePumpStatus.Ready;
				} catch (Exception e) {
					Log.Warning(e.Message);
					item.Status = TexturePumpStatus.Failed;
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		void RemoveOldTextures()
		{
			// Remove textures
			var keysToRemove = new List<string>();

			var lifeTime = new TimeSpan(0, 0, 0, Config.MaximumLifeTimeSeconds);

			foreach (var item in items) {
				if (item.Value.Status == TexturePumpStatus.Ready &&
					DateTime.Now - item.Value.LastRequestTime > lifeTime) {
					keysToRemove.Add(item.Key);
				}
			}

			// Remove old textures
			foreach (var key in keysToRemove) {

				var item = items[key];

				item.Texture.Dispose();

				Interlocked.Add(ref sizeInBytes, -item.SizeInBytes);

				items.Remove(key);
			}

		}

	}
}
