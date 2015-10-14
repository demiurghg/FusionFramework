using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Shell;

namespace FBuild.Processors {

	public abstract class AssetProcessor {
		
		/// <summary>
		/// 
		/// </summary>
		public AssetProcessor ()
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceStream"></param>
		/// <param name="targetStream"></param>
		public abstract void Process ( AssetFile assetFile );
	}
}
