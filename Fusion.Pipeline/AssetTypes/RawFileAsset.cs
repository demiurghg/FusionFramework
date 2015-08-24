using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using Fusion.Mathematics;


namespace Fusion.Pipeline.AssetTypes {

	[Asset("Content", "Raw File", "*.dds;*.*")]
	public class RawFileAsset : Asset, IFileDerivable {


		/// <summary>
		/// 
		/// </summary>
		public string SourceFile { get; set; }



		/// <summary>
		/// 
		/// </summary>
		public override string[] Dependencies
		{
			get { return new[]{ SourceFile }; }
		}



		/// <summary>
		/// Inits asset from file
		/// </summary>
		/// <param name="path"></param>
		public void InitFromFile( string path )
		{
			AssetPath	=	path;
			SourceFile	=	path;	
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Build ( BuildContext buildContext )
		{
			using ( var target = buildContext.OpenTargetStream( this ) ) {
				buildContext.CopyTo( SourceFile, target );
			}
		}
	}
}
