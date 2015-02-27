using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using Fusion.Mathematics;


namespace Fusion.Content {

	[Asset("Content", "Raw File", "*.dds;*.*")]
	public class RawFileAsset : Asset {


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
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Build ( BuildContext buildContext )
		{
			using ( var target = buildContext.TargetStream( this ) ) {
				buildContext.CopyTo( SourceFile, target );
			}
		}
	}
}
