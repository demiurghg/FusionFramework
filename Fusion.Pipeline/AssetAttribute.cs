using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;


namespace Fusion.Pipeline {

	/// <summary>
	/// Asset attribute.
	/// All asset types should me declared with AssetAttribute
	/// </summary>
	public sealed class AssetAttribute : Attribute {

		/// <summary>
		/// Asset's type category, e.g. "Content", "Rendering", "Game Assets"
		/// </summary>
		public string Category {
			get; private set;
		}


		/// <summary>
		/// Asset's type nice name, e.g. "Texture Asset" or "Monster Description"
		/// </summary>
		public string NiceName {
			get; private set;
		}


		/// <summary>
		/// Associated content source file extension
		/// </summary>
		public string Extensions {
			get; private set;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="domain">Asset's domain</param>
		/// <param name="nicename">Asset's type nice name, e.g. "Texture Asset" or "Monster Description"</param>
		public AssetAttribute ( string category, string nicename )
		{
			Category	=	category;
			NiceName	=	nicename;
			Extensions	=	"";
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="domain">Asset's domain</param>
		/// <param name="nicename">Asset's type nice name, e.g. "Texture Asset" or "Monster Description"</param>
		/// <param name="extensions">Source file extensions: "*.tga;*.jpg;*.bmp"</param>
		public AssetAttribute ( string category, string nicename, string extensions )
		{
			Category	=	category;
			NiceName	=	nicename;
			Extensions	=	extensions;
		}



		/// <summary>
		/// Checks whether file extension meets provided extensions.
		/// </summary>
		/// <param name="pathToFile"></param>
		/// <returns></returns>
		public bool AcceptFile ( string pathToFile )
		{
			var extList = Extensions.Split( new[]{';'}, StringSplitOptions.RemoveEmptyEntries );
						
			foreach ( var ext in extList ) {
				if (Wildcard.Match(pathToFile, ext, false)) {
					return true;
				}
			}
			return false;
		}

	}
}
