using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Shell;
using Fusion.Drivers.Graphics;

namespace Fusion.Build.Processors {

	[AssetProcessor("Scenes", "Converts FBX files to scene")]
	public class SceneProcessor : AssetProcessor {

		/// <summary>
		/// Vertex merge tolerance
		/// </summary>
		[CommandLineParser.Name("merge", "merge tolerance (default=0)")]
		public float MergeTolerance { get; set; }

		/// <summary>
		/// Evaluates transform
		/// </summary>
		[CommandLineParser.Name("anim", "import animation")]
		public bool ImportAnimation { get; set; }

		/// <summary>
		/// Evaluates transform
		/// </summary>
		[CommandLineParser.Name("geom", "import geometry")]
		public bool ImportGeometry { get; set; }


		
		/// <summary>
		/// 
		/// </summary>
		public SceneProcessor ()
		{
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceStream"></param>
		/// <param name="targetStream"></param>
		public override void Process ( AssetFile assetFile, BuildContext context )
		{
			var resolvedPath	=	assetFile.FullSourcePath;
			var destPath		=	context.GetTempFileName( assetFile.KeyPath, ".scene" );
			var cmdLine			=	string.Format("\"{0}\" /out:\"{1}\" /merge:{2} {3} {4}", 
				resolvedPath, destPath, 
				MergeTolerance, 
				ImportAnimation ? "/anim":"", 
				ImportGeometry ? "/geom":"" 
			);

			context.RunTool( "FScene.exe", cmdLine );

			using ( var target = assetFile.OpenTargetStream() ) {
				context.CopyFileTo( destPath, target );
			}
		}
	}
}
