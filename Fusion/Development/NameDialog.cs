using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fusion.Development {
	internal partial class NameDialog : Form {


		/// <summary>
		/// 
		/// </summary>
		/// <param name="text"></param>
		/// <param name="caption"></param>
		/// <returns></returns>
		public static string Show( Form owner, string text, string caption, string suggestion = "" )
		{
			var dlg	=	new NameDialog();
			dlg.textLabel.Text	=	text;
			dlg.Text			=	caption;
			dlg.textBox.Text	=	suggestion;

			var dr = dlg.ShowDialog(owner);

			if (dr==DialogResult.OK) {
				return dlg.textBox.Text;
			} else {
				return null;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		NameDialog ()
		{
			InitializeComponent();
		}

		private void cancelButton_Click ( object sender, EventArgs e )
		{
			DialogResult	=	DialogResult.Cancel;
			Close();
		}

		private void okButton_Click ( object sender, EventArgs e )
		{
			DialogResult	=	DialogResult.OK;
			Close();
		}
	}
}
