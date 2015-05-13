using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Shell {
	public class Shell {

		/// <summary>
		/// Executes given string.
		/// </summary>
		/// <param name="command"></param>
		public void ExecuteString ( string command )
		{
			throw new NotImplementedException();
		}
		

		/// <summary>
		/// Gains access to object properties.
		/// Example:
		///		RegisterNamedObject( "Config:Rendering/Lighting", RenderSystem.Lighting.Config );
		///		RegisterNamedObject( "System:Rendering/Lighting", RenderSystem.Lighting );
		///		RegisterNamedObject( "System:Rendering/Lighting", RenderSystem.Lighting );
		///		
		///			Config:Rendering/Lighting/SkipShadows
		///			
		///			set Content:Textures/Walls/Wall01.tga Compression BC1
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="name"></param>
		/// <param name="obj"></param>
		public void RegisterNamedObject ( string path, object obj )
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Ungains access to object properties.
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="name"></param>
		/// <param name="obj"></param>
		public void UnregisterNamedObject ( object obj )
		{
			throw new NotImplementedException();
		}

	}
}
