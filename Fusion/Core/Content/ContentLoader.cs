using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;


namespace Fusion.Core.Content {
	public abstract class ContentLoader {

		/// <summary>
		/// Loads asset.
		/// </summary>
		/// <param name="game">GameEngine</param>
		/// <param name="stream">Stream to read asset from.</param>
		/// <param name="requestedType">Requested asset type. Type specified by ContentLoaderAttribute and requested type may differ.</param>
		/// <param name="assetPath">Path to asset. This is requested asset path but not actual path to asset's file.</param>
		/// <returns></returns>
		public abstract object	Load ( GameEngine game, Stream stream, Type requestedType, string assetPath );


		public Type TargetType {
			get {
				return (GetType()
					.GetCustomAttributes( typeof(ContentLoaderAttribute), true )
					.First() as ContentLoaderAttribute )
					.TargetType;
			}
		}

		
		/// <summary>
		/// Gathers all content loaders
		/// </summary>
		/// <returns></returns>
		public static Type[] GatherContentLoaders ()
		{
			return Misc.GetAllSubclassedOf( typeof(ContentLoader) )
				.Where( t => t.HasAttribute<ContentLoaderAttribute>() )
				.ToArray();
		}
	}



	/// <summary>
	/// Attribute to mark content loader classes and make them visible for ContentLoader.GatherContentLoaders.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ContentLoaderAttribute : Attribute {

		public readonly Type TargetType;

		public ContentLoaderAttribute ( Type targetType )
		{
			this.TargetType = targetType;
		}
	}
}
