using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Content;
using Fusion.Development;
using System.IO;

namespace Fusion.Development {
	public static class DevCon {

		static bool prepared = false;
		static string savedContentProjectPath = "";
		static string savedTargetDirectory	= "";
		static string sourceDirectory = "";

		static DevConForm	devcon = null;

		static public AssetCollection Assets { get { return devcon.Assets; } }

		//static ContentBuilder builder;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="contentPath"></param>
		/// <returns></returns>
		public static bool Prepare ( Game game, string contentProjectPath, string targetDirectory )
		{
			prepared				=	true;
			savedContentProjectPath	=	contentProjectPath;
			savedTargetDirectory	=	targetDirectory;

			sourceDirectory			=	Path.GetFullPath( Path.GetDirectoryName( contentProjectPath ) );


			var devcon = new DevConForm( game, contentProjectPath, targetDirectory, true );
			var r = devcon.ShowDialog();

			game.Exiting += game_Exiting;

			if ( r == System.Windows.Forms.DialogResult.OK ) {
				return true;
			} else {
				return false;
			}
		}



		/// <summary>
		/// Makes relative path to current content project.
		/// </summary>
		/// <param name="absolutePath"></param>
		/// <returns></returns>
		static public string MakeRelativePath ( string absolutePath )
		{
			var contentUri	=	new Uri( sourceDirectory + "\\" );
			var fileName	=	contentUri.MakeRelativeUri( new Uri(absolutePath) ).ToString();

			return fileName;
		}



		static bool ContentSourceFileExists ( string path )
		{
			return File.Exists( Path.Combine( sourceDirectory, path ) );
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void game_Exiting ( object sender, EventArgs e )
		{
			if (devcon!=null && !devcon.IsDisposed) {
				devcon.Close();
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		public static void Show ( Game game )
		{
			if (!prepared) {
				Log.Warning("DevCon.Prepare() required.");
				return;
			}

			if (devcon==null || devcon.IsDisposed) {
				devcon = new DevConForm( game, savedContentProjectPath, savedTargetDirectory, false );
			}

			devcon.Show();
			devcon.BringToFront();
			devcon.Focus();
		}

	}
}
