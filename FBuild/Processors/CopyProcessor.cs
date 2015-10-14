using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Shell;

namespace FBuild.Processors {
	public class CopyProcessor : AssetProcessor {
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceStream"></param>
		/// <param name="targetStream"></param>
		public override void Process ( AssetFile assetFile )
		{
			using ( var sourceStream = assetFile.OpenSourceStream() ) {
				using ( var targetStream = assetFile.OpenTargetStream() ) {
					sourceStream.CopyTo( targetStream );
				}
			}
		}
	}
}
