using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using Fusion.Content;
using Fusion.Development;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Runtime.InteropServices;
using System.Reflection;


namespace Fusion.Development {
	internal class ContentBuilder {

		readonly string			contentProject;	
		readonly string			sourceDirectory;
		readonly string			targetDirectory;

		AssetCollection			descriptors;

		DevConForm				developerConsole;
		ContentBuilder			contentBuilder { get { return this; } }
		Game					game;

		public bool		Modified { get; private set; }
		


		/// <summary>
		/// 
		/// </summary>
		public ContentBuilder ( string contentProject, string targetDirectory, DevConForm devcon, Game game )
		{
			Modified				=	false;
			this.developerConsole	=	devcon;
			this.game				=	game;

			this.contentProject		=	Path.GetFullPath( contentProject );
			this.sourceDirectory	=	Path.GetFullPath( Path.GetDirectoryName( contentProject ) );
			this.targetDirectory	=	targetDirectory;

			descriptors				=	new AssetCollection();
		}



		/// <summary>
		/// Creates asset from file.
		/// This method creates	Asset from given type or assigns first acceptable Asset type.
		/// After creation it searchs for SourceFile or InputFile string property and sets path
		/// </summary>
		/// <param name="path"></param>
		public void CreateAssetFromFile ( string path, Type assetType )
		{
			var asset = (Asset)Activator.CreateInstance( assetType );
			asset.AssetPath	=	path;

			if (	!Misc.SetProperty( asset, "SourceFile", path ) 
				&&	!Misc.SetProperty( asset, "InputFile", path ) 
				&&	!Misc.SetProperty( asset, "SourcePath", path ) 
				&&	!Misc.SetProperty( asset, "InputPath", path ) 
				&&	!Misc.SetProperty( asset, "File", path ) 
				)
				{
					throw new Exception("Could not assign property (SourceFile, InputFile, SourcePath, InputPath or File) of created asset type for '" + path + "'");
				}

			AddAsset( asset );
		}



		/// <summary>
		/// 
		/// </summary>
		public void DetectAssetHashCollisions ()
		{
			var collisions = descriptors.GetHashCollisions();

			if (collisions.Any()) {
				Log.Warning("Detected asset hash collisions:");
				foreach ( var c in collisions ) {
					Log.Warning("  {0}", c);
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public void AddAsset ( Asset desc )
		{
			descriptors.Add( desc );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldDesc"></param>
		/// <param name="newDesc"></param>
		public void Replace ( Asset oldDesc, Asset newDesc )
		{
			descriptors.Remove( oldDesc );
			descriptors.Add( newDesc );
		}



		/// <summary>
		/// 
		/// </summary>
		public void Outline ( DevConForm devcon )
		{
			var descList = descriptors
				.OrderBy( c0 => c0.AssetPath )
				.OrderByDescending( c1 => c1.AssetPath.Count( ch => ch=='\\' || ch=='/' ) )
				.ThenBy( c3 => c3.AssetPath )
				.Select( c => new KeyValuePair<string,Asset>( c.AssetPath, c ) )
				.ToList();

			devcon.AddBranch( "Content", new[]{'/', '\\'}, descList, false );
		}



		/// <summary>
		/// Loads all possible domains.
		/// </summary>
		public void LoadDescriptors ()
		{
			Log.Message("Loading content project...");

			try {
				descriptors	=	AssetCollection.Load( contentProject );
			} catch ( Exception e ) {
				Log.Error("Failed to load content:");
				Log.Error("{0}", e.Message);
				return;	
			}
			
			Modified = false;
			Log.Message("Content project successfully loaded.");
		}



		/// <summary>
		/// Saves all domains
		/// </summary>
		public void Save ()
		{
			Log.Message("Saving content project...");

			try {
				AssetCollection.Save( descriptors, contentProject );
			} catch ( Exception e ) {
				Log.Error("Failed to save content:");
				Log.Error("{0}", e.Message);
				return;	
			}

			Log.Message("Content project successfully saved.");
			Modified = false;
		}




		/// <summary>
		/// 
		/// </summary>
		/// <param name="forced"></param>
		public bool BuildContent ( bool forced, IEnumerable<string> selection )
		{
			string message;
			bool result =  descriptors.Build( forced, sourceDirectory, "Content", out message, selection );

			developerConsole.SetMessage( message );

			return result;
		}




		/// <summary>
		/// 
		/// </summary>
		public void RemoveDescriptors ( IEnumerable<string> nameList )
		{
			//Path.Conver
			int count = descriptors.RemoveAll( item => nameList.Contains( ContentUtils.BackslashesToSlashes( item.AssetPath ) ) );
			Log.Message("{0} content items removed", count);
		}

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	UI stuff :
		 * 
		-----------------------------------------------------------------------------------------*/
		
		/// <summary>
		/// Builds content
		/// </summary>
		[Command("Build Content", 0)]
		public void BuildContent ()
		{
			BuildContent(false, null);
		}


		/// <summary>
		/// Builds content
		/// </summary>
		[Command("Rebuild Content", 0)]
		public void RebuildContent ()
		{
			BuildContent(true, null);
		}


		/// <summary>
		/// Builds content
		/// </summary>
		[Command("Rebuild Selection", 0)]
		public void RebuildSelection ()
		{
			BuildContent(true, developerConsole.GetSelectedNames("Content") );
		}


		/// <summary>
		/// 
		/// </summary>
		[Command("Clean Content", 0)]
		public void CleanContent ()
		{
			var r = MessageBox.Show("Are you shure to clean target content folder?", "Clean Content", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning );
			
			if (r==DialogResult.OK) {
				Directory.Delete(targetDirectory,true);
			}
		}



		/// <summary>
		/// 
		/// </summary>
		[Command("Add Asset...", 1)]
		public void AddAsset ()
		{
			var dict = 
				Asset.GatherAssetTypes()
				.Select( type => 
					new KeyValuePair<string,Type>( type.GetCustomAttribute<AssetAttribute>().Category + ": " + type.GetCustomAttribute<AssetAttribute>().NiceName, type ) );

			if (!dict.Any()) {
				MessageBox.Show("There are not descriptor classes. Make sure that your classes are inherited from Descriptor class and has attribute DescriptorAttribute", 
								"Add Descriptor", MessageBoxButtons.OK, MessageBoxIcon.Information );
				return;
			}

			Type result;

			if (ObjectSelector.Show(developerConsole, "Select asset type", "Add Asset", dict, out result ) ) {
				
				string suggestion = result.GetCustomAttribute<AssetAttribute>().Category + @"\";

				var path = NameDialog.Show(developerConsole, "Enter asset name", "Add Asset", suggestion );

				if (path==null) {
					return;
				}

				var desc	=	(Asset)Activator.CreateInstance( result );
				desc.AssetPath	=	path;

				try {
					contentBuilder.AddAsset( desc );
				} catch ( ContentException e ) {
					MessageBox.Show(e.Message, "Add Asset", MessageBoxButtons.OK, MessageBoxIcon.Error );
				}

				Modified = true;
				developerConsole.RefreshConsole(false);
				//game.Reload();
			}
		}


		string ofdDirectory = null;



		/// <summary>
		/// Adds content using OpenFileDialog
		/// </summary>
		[Command("Create Asset From File...", 1)]
		public void CreateAssetFromFile ()
		{
			if (ofdDirectory==null) {
				ofdDirectory = Path.GetFullPath( sourceDirectory );
			}

			OpenFileDialog ofd = new OpenFileDialog();

			var assetTypeList =  Asset.GatherAssetTypes()
				.Where( at => !string.IsNullOrWhiteSpace(at.GetCustomAttribute<AssetAttribute>().Extensions) ).ToList();


			ofd.RestoreDirectory	=	true;
			ofd.InitialDirectory	=	ofdDirectory;
			ofd.Multiselect			=	true;
			ofd.Filter				=	assetTypeList
				.Select( at0 => at0.GetCustomAttribute<AssetAttribute>() )
				.Select( ata => ata.NiceName + " (" + ata.Extensions + ")|" + ata.Extensions )
				.Aggregate( (current, next) => next + "|" + current )
				//+ "|All files (*.*)|*.*"
				;


			var r = ofd.ShowDialog();

			//ofdDirectory	=	ofd.Di

			var fusionContent	=	 DevConForm.FusionContent;

			var names = ofd.FileNames;
			
			assetTypeList.Reverse();
			//assetTypeList.Add( null );
			var selectedAssetType = assetTypeList[ ofd.FilterIndex - 1 ];

			
			foreach ( var n in names ) {	
				string nn = n;

				var contentUri	=	new Uri( sourceDirectory + "\\" );
				var fileName	=	contentUri.MakeRelativeUri( new Uri(nn) ).ToString();

				try {
					CreateAssetFromFile( fileName, selectedAssetType );
					Log.Message( "Added : {1} : {0}", fileName, selectedAssetType.GetCustomAttribute<AssetAttribute>().NiceName );
				} catch ( Exception e ) {
					MessageBox.Show(developerConsole, string.Format("Could not add '{0}' : {1}", fileName, e.Message), "Create Asset From File", MessageBoxButtons.OK, MessageBoxIcon.Error );
				}
			}

			Modified = true;
			developerConsole.RefreshConsole(false);
		}



		/// <summary>
		/// 
		/// </summary>
		[Command("Remove Selected Assets", 1)]
		public void RemoveSelectedAssets ()
		{
			if (developerConsole!=null && !developerConsole.IsDisposed) {
				if (contentBuilder!=null) {
					
					var names = developerConsole.GetSelectedNames(targetDirectory);
				
					if ( names.Any() ) {

						var r = MessageBox.Show("Are you sure to remove selected items?", "Remove Content", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning );

						if ( r==DialogResult.OK ) {
							contentBuilder.RemoveDescriptors( names );
							developerConsole.SelectNone();
							Modified = true;
							developerConsole.RefreshConsole(true);
						}
					}
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		#if false
		[Command("Change Asset Type...", 1)]
		public void ChangeAssetType ()
		{
			var assetList = developerConsole.GetSelectedObjects().Where( obj => obj is Asset ).Select( obj => (Asset)obj );

			if (!items.Any()) {
				MessageBox.Show("No content item selected", "Assign Content Processor", MessageBoxButtons.OK, MessageBoxIcon.Information );
				return;
			}

			var list = Asset.GatherAssetTypes()
							.Select( at => at.GetCustomAttribute<AssetAttribute>().Category + ": " + at.GetCustomAttribute<AssetAttribute>().NiceName )
							.ToList();


			if ( ObjectSelector.Show( developerConsole, "Select content processor to assign to selection", "Assign Content Processor", list, out contentProcessor ) ) {

				foreach ( var item in items ) {

					if ( contentProcessor.MatchExt( item.Path ) ) {

						var newItem = contentProcessor.CreateContentItem( item.Path );

						contentProject.Replace( item, newItem );

						changed.Add( item.Path );

					} else {
						dontMatch.Add( item.Path );
					}
				}
			}
		}
		#endif




		/// <summary>
		/// Save content project
		/// </summary>
		[Command("Save Content", 5)]
		public void SaveContentProject ()
		{
			Save();
			developerConsole.RefreshConsole(false);
		}



		/// <summary>
		/// Save content project
		/// </summary>
		[Command("Refresh Console", 6)]
		public void RefreshConsole ()
		{
			developerConsole.RefreshConsole(true);
		}

	}

}
