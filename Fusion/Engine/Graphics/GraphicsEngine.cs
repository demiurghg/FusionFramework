using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;

namespace Fusion.Engine.Graphics {

	public class GraphicsEngine : DisposableBase {

		internal readonly GraphicsDevice Device;

		SpriteEngine	spriteEngine;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="engine"></param>
		public GraphicsEngine ( GameEngine engine )
		{
			this.Device	=	engine.GraphicsDevice;

			SpriteLayers	=	new List<SpriteLayer>();
		}



		/// <summary>
		/// Intializes graphics engine.
		/// </summary>
		public void Initialize ()
		{
			spriteEngine	=	new SpriteEngine( this );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref spriteEngine );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		internal void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			spriteEngine.DrawSprites( gameTime, stereoEye, SpriteLayers );
		}


		/// <summary>
		/// Renders current scene to given target and camera
		/// </summary>
		/// <param name="target"></param>
		/// <param name="camera"></param>
		public void RenderScene ( RenderFrame target, Camera camera )
		{
		}


		/// <summary>
		/// Renders all sprites to specified target
		/// </summary>
		/// <param name="target"></param>
		public void RenderSprites (	RenderFrame target )
		{
		}


		/// <summary>
		/// Clears all instances, sprites and lights.
		/// </summary>
		public void ClearAll ()
		{
			SpriteLayers.Clear();
		}


		/// <summary>
		/// Mesh instances
		/// </summary>
		ICollection<MeshInstance> Instances { get; set; }


		/// <summary>
		/// Sprites 
		/// </summary>
		public ICollection<SpriteLayer> SpriteLayers { get; set; }


		/// <summary>
		/// Omni-lights
		/// </summary>
		ICollection<OmniLight> OmniLights { get; set; }


		/// <summary>
		/// Spotlights
		/// </summary>
		ICollection<SpotLight> SpotLights { get; set; }
	}
}
