using System;																							 
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using Fusion.Core.Mathematics;
using Fusion.Content;


namespace Fusion.Pipeline {
	public class AssetCollection : List<Asset> {

		public AssetCollection () : base() {
		}

		public AssetCollection ( IEnumerable<Asset> collection ) : base(collection) {
		}


		/// <summary>
		/// Adds an asset to the end of the collection.
		/// If item with the same name already exists exception will be thrown.
		/// </summary>
		/// <param name="item"></param>
		public new void Add ( Asset item ) 
		{ 
			foreach ( var existingAsset in this ) {
				if ( existingAsset.Hash == item.Hash ) {
					throw new ArgumentException( string.Format("Asset '{0}' will cause hash collision with '{1}'", item.AssetPath, existingAsset.AssetPath ) );
				}
			}

			base.Add( item );
		}



		/// <summary>
		/// Searchs for all possible hash collisions.
		/// Returns list of collided asset paths.
		/// </summary>
		/// <returns></returns>
		public string[] GetHashCollisions ()
		{
			return this
				.Select( a => a.Hash )
				.GroupBy( tp => tp )
				.Where( tpg => tpg.Count()>1 )
				.Select( tpg1 => tpg1.First() )
				.Distinct()
				.ToArray();
		}



		/// <summary>
		/// Builds assets.
		/// </summary>
		/// <param name="forced">Force asset to be rebuilt</param>
		/// <param name="sourceDirectory">Directory to get resources from</param>
		/// <param name="targetDirectory">Directory to put built assets to</param>
		/// <param name="message">Output message</param>
		/// <param name="selection">Can be null. List of assets to build.</param>
		/// <returns></returns>
		public bool Build ( bool forced, string sourceDirectory, string targetDirectory, out string message, IEnumerable<string> selection )
		{
		#if false
			int succeeded = 0;
			int failed = 0;

			///Log.Message("---------- Content Build Started ----------");


			HashSet<string> selectedNames = null;
			if ( selection!=null ) {
				selectedNames = new HashSet<string>( selection.Select( n => ContentUtils.GetHashedFileName( n, "") ) );
			}
			
			//
			//	Check hash collisions :
			//
			var collisions = GetHashCollisions();

			if (collisions.Any()) {
				Log.Warning("Detected asset hash collisions:");
				foreach ( var c in collisions ) {
					Log.Warning("  {0}", c);
				}
			} else {
				Log.Message("Hash collisions are not detected.");
			}

			//
			//	Start build :
			//	
			var errors = new StringBuilder("Build failed:\r\n");

			//	make directories :
			Directory.CreateDirectory( targetDirectory );

			//	create build context :
			//var buildContext = new BuildContext( sourceDirectory, targetDirectory, this );
			
			//	build items :
			for ( int i = 0; i<Count; i++ ) {

				var asset = this[i];

				if (selectedNames!=null) {
					if ( !selectedNames.Contains( asset.Hash ) ) {
						continue;
					}
				}


				try {
					
					if ( buildContext.IsOutOfDate( asset, forced ) ) {
						Log.Message("...building: {0}", asset.AssetPath );
						asset.Build( buildContext );
					}

					succeeded ++;
				} catch ( AggregateException ae ) {

					foreach ( var e in ae.InnerExceptions ) {

						Log.Error("{0}: {1}", asset.AssetPath, e.Message );

						errors.AppendFormat("{0}: {1}", asset.AssetPath, e.Message );
						errors.AppendFormat( "\r\n" );
					}

					failed++;

				} catch ( Exception e )	{

					Log.Error("{0}: {1}", asset.AssetPath, e.Message );

					errors.AppendFormat("{0}: {1}", asset.AssetPath, e.Message );
					errors.AppendFormat( "\r\n" );

					failed++;
				}
			}

			//Log.Message("---------- Build: {0} succeeded, {1} failed ----------", succeeded, failed);

			if (failed==0) {
				message = string.Format("Build: {0} succeeded, {1} failed", succeeded, failed);
				return true;
			} else {
				message = string.Format("Build: {0} succeeded, {1} failed\r\n", succeeded, failed) + errors.ToString();
				return false;
			}
		#else
			message = "";
			return false;
		#endif
		}


		
		/// <summary>
		/// Loads table from xml file
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static AssetCollection Load ( string path )
		{
			AssetCollection assets;
			Type[] assetTypes;

			assetTypes	=	Asset.GatherAssetTypes();
			assets		=	(AssetCollection)Misc.LoadObjectFromXml( typeof(AssetCollection), path, Asset.GatherAssetTypes() );

			return assets;
		}
		


		/// <summary>
		/// Saves descriptor table to xml file
		/// </summary>
		/// <param name="table"></param>
		/// <param name="path"></param>
		public static void Save ( AssetCollection table, string path, string domain = null )
		{
			Misc.SaveObjectToXml( table, typeof(AssetCollection), path, Asset.GatherAssetTypes() );
		}
	}
}
