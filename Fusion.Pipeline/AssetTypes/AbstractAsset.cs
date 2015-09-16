using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;


namespace Fusion.Pipeline {

	public class AbstractAsset : Asset {

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetPath"></param>
		public AbstractAsset ( string assetPath ) : base( assetPath )
		{
		}



		/// <summary>
		/// 
		/// </summary>
		public override string[] Dependencies
		{
			get { return new string[0]; }
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="buildContext"></param>
		public override void Build ( BuildContext buildContext )
		{
			Misc.SaveObjectToXml( this, GetType(), buildContext.OpenTargetStream( this ) );
		}
	}
}
