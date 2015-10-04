using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;


namespace Fusion.Pipeline {
	
	[Asset("Content", "Assembled Cube Texture")]
	public class AssembledCubeTextureAsset : Asset {


		public string FacePosX { get; set; }
		public string FacePosY { get; set; }
		public string FacePosZ { get; set; }
		public string FaceNegX { get; set; }
		public string FaceNegY { get; set; }
		public string FaceNegZ { get; set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetPath"></param>
		public AssembledCubeTextureAsset ( string assetPath ) : base( assetPath )
		{
		}



		public override string[] Dependencies
		{
			get { return new[]{ FacePosX, FacePosY, FacePosZ, FaceNegX, FaceNegY, FaceNegZ }; }
		}



		public override void Build ( BuildContext buildContext )
		{
			throw new NotImplementedException();

			/*
			string tempFileName = Path.GetTempFileName();

			var dir		=	Path.GetDirectoryName( item.ResolvedPath );
			var names	=	File.ReadAllLines( item.ResolvedPath );

			names		=	names.Select( n => Path.Combine( dir, n ) ).ToArray();

			//	Launch 'nvassemble.exe' with temporary output file :
			ContentProject.Instance.RunExternalTool( @"NVTT\nvassemble.exe", "-cube " + string.Join( " ", names ) );

			File.Move( "output.dds", item.TargetPath );
			*/
		}
	} 
}
