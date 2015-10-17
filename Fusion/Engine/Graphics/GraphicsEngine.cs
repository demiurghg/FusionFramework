using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Common;

namespace Fusion.Engine.Graphics {

	class GraphicsEngine {

		/// <summary>
		/// 
		/// </summary>
		/// <param name="engine"></param>
		public GraphicsEngine ( GameEngine engine )
		{
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
		public void Clear ()
		{
		}


		/// <summary>
		/// Mesh instances
		/// </summary>
		ICollection<MeshInstance> Instances { get; set; }


		/// <summary>
		/// Sprites 
		/// </summary>
		ICollection<Sprite> Sprites { get; set; }


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
