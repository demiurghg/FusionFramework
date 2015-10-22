using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Shell;
using Fusion;
using Fusion.Core;
using Fusion.Core.Content;

namespace Fusion.Build.Processors {

	public class AssetProcessorBinding {

		string	name;
		Type	type;

		/// <summary>
		/// Processor name
		/// </summary>
		public string Name {
			get {
				return name;
			}
		}


		/// <summary>
		/// Processor type
		/// </summary>
		public Type Type {
			get {
				return type;
			}
		}

		
		/// <summary>
		/// 
		/// </summary>
		public AssetProcessorBinding ( string assetProcessorName, Type assetProcessorType )
		{
			this.name	=	assetProcessorName;
			this.type	=	assetProcessorType;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public AssetProcessor CreateAssetProcessor ()
		{
			return (AssetProcessor)Activator.CreateInstance( type );
		}


		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<AssetProcessorBinding> GatherAssetProcessors ()
		{
			return Misc.GetAllClassesWithAttribute<AssetProcessorAttribute>()
							.Where( t1 => t1.IsSubclassOf( typeof(AssetProcessor) ) )
							.Select( t2 => new AssetProcessorBinding( t2.GetCustomAttribute<AssetProcessorAttribute>().Name, t2 ) )
							.ToList();
		}
	}
}
