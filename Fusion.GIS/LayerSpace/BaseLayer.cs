using System;

namespace Fusion.GIS.LayerSpace
{
	public class BaseLayer : IDisposable
	{
		protected Game Game;
		protected LayerServiceConfig Config;

		public BaseLayer(Game game, LayerServiceConfig config)
		{
			Game	= game;
			Config	= config;
		}

		public virtual void Update(GameTime gameTime)
		{
			
		}

		public virtual void Draw(GameTime gameTime, Fusion.Graphics.StereoEye stereoEye)
		{

		}

		public virtual void Dispose()
		{
			
		}
	}
}
