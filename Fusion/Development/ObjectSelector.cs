using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion;
using Fusion.Mathematics;
using SharpDX;

namespace Fusion.Development {
	internal partial class ObjectSelector : Form {


		public static bool Show<T> ( Form owner, string text, string caption, IEnumerable<KeyValuePair<string,T>> list, out T result )
		{
			var objSel = new ObjectSelector();

			objSel.textLabel.Text	=	text;
			objSel.Text				=	caption;

			objSel.listBox.Items.AddRange( list.Select( a => a.Key ).ToArray() );
			objSel.listBox.SelectedIndex = 0;

			var r = objSel.ShowDialog( owner );

			if (r==DialogResult.OK) {
				result = list.ElementAt( objSel.listBox.SelectedIndex ).Value;
				return true;
			}


			result = default(T);
			return false;
		}
		

		ObjectSelector ()
		{
			InitializeComponent();
		}


		private void cancelButton_Click ( object sender, EventArgs e )
		{
			this.DialogResult	=	DialogResult.Cancel;
		}


		private void okButton_Click ( object sender, EventArgs e )
		{
			this.DialogResult	=	DialogResult.OK;
		}
	}
}
