using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Fusion.Core.Mathematics;


namespace Fusion.Pipeline.AssetTypes {

	[Asset("Content", "FBX File Scene", "*.fbx")]
	public class FbxFileSceneAsset : Asset {

		/// <summary>
		/// 
		/// </summary>
		[Category("Import Parameters")]
		[Description("Source FBX file.")]
		public string SourceFile { get; set; }

		/// <summary>
		/// Vertex merge tolerance
		/// </summary>
		[Category("Import Parameters")]
		[Description("Vertex merge tolerance.")]
		public float MergeTolerance { get; set; }

		/// <summary>
		/// Evaluates transform
		/// </summary>
		[Category("Import Parameters")]
		[Description("Import animation.")]
		public bool ImportAnimation { get; set; }

		/// <summary>
		/// Evaluates transform
		/// </summary>
		[Category("Import Parameters")]
		[Description("Import geometry.")]
		public bool ImportGeometry { get; set; }



		/// <summary>
		/// Dependencies
		/// </summary>
		public override string[] Dependencies
		{
			get { return new[]{ SourceFile }; }
		}



		/// <summary>
		/// Ctor
		/// </summary>
		public FbxFileSceneAsset(string path) : base(path)
		{
			ImportGeometry	=	true;
		}




		/// <summary>
		/// Builds asset
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Build ( BuildContext buildContext )
		{
			var resolvedPath	=	buildContext.Resolve( SourceFile );
			var destPath		=	buildContext.GetTempFileName( Hash, ".scene" );
			var cmdLine			=	string.Format("\"{0}\" /out:\"{1}\" /merge:{2} {4} {5}", 
				resolvedPath, destPath, 
				MergeTolerance, 
				null, 
				ImportAnimation ? "/anim":"", 
				ImportGeometry ? "/geom":"" 
			);

			buildContext.RunTool( "Native.Fbx.exe", cmdLine );

			using ( var target = buildContext.OpenTargetStream( this ) ) {
				buildContext.CopyTo( destPath, target );
			}
		}
	}
}
