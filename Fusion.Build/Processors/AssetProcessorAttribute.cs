using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Shell;
using Fusion;

namespace Fusion.Build.Processors {

	public class AssetProcessorAttribute : Attribute {

		string	name;

		/// <summary>
		/// Processor name
		/// </summary>
		public string Name {
			get {
				return name;
			}
		}

		/// <summary>
		/// Processor name
		/// </summary>
		public string Description {
			get; private set;
		}

		
		/// <summary>
		/// 
		/// </summary>
		public AssetProcessorAttribute ( string assetProcessorName, string description )
		{
			this.name	=	assetProcessorName;
			Description	=	description;
		}
	}
}
