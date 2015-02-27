using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Development;

namespace Fusion.Development {
	public static class DevCon {

		static bool prepared = false;
		static string savedContentProjectPath = "";
		static string savedTargetDirectory	= "";

		static DevConForm	devcon = null;

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

			var devcon = new DevConForm( game, contentProjectPath, targetDirectory, "", true );
			var r = devcon.ShowDialog();

			game.Exiting += game_Exiting;

			if ( r == System.Windows.Forms.DialogResult.OK ) {
				return true;
			} else {
				return false;
			}
		}



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
				devcon = new DevConForm( game, savedContentProjectPath, savedTargetDirectory, "", false );
			}

			devcon.Show();
			devcon.BringToFront();
			devcon.Focus();
		}

	}
}
