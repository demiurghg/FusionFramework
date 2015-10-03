using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using Fusion.Core.Mathematics;


namespace Fusion.Pipeline.AssetTypes {

	[Asset("Content", "WAV File Sound Effect", "*.wav")]
	public class WaveFileSoundEffectAsset : Asset {


		/// <summary>
		/// Source file
		/// </summary>
		public string SourceFile { get; set; }



		public WaveFileSoundEffectAsset( string path ) : base(path)
		{
		}



		/// <summary>
		/// Dependencies
		/// </summary>
		public override string[] Dependencies
		{
			get { return new[]{ SourceFile }; }
		}


		/// <summary>
		/// Builds asset
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
