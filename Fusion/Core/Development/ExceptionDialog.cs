using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Fusion.Core.Development {
	internal partial class ExceptionDialog : Form {
		private ExceptionDialog ( Exception exception )
		{
			InitializeComponent();

			this.labelExceptionType.Text	=	exception.GetType().ToString();
			this.textBoxMessage.Text = exception.Message;
			this.textBoxStack.Text = exception.StackTrace.ToString();
			this.AcceptButton = buttonTerminate;

			if (Debugger.IsAttached) {
				this.buttonTerminate.Text = "Break";
			}
		}


		public static void Show ( Exception exception )
		{
			var dlg = new ExceptionDialog(exception);
			dlg.ShowDialog();
		}

		private void buttonTerminate_Click ( object sender, EventArgs e )
		{
			if (Debugger.IsAttached) {
				Close();
			} else {
				Process.GetCurrentProcess().Kill();
			}
		}

		private void ExceptionDialog_FormClosed ( object sender, FormClosedEventArgs e )
		{
			if (Debugger.IsAttached) {
			} else {
				Process.GetCurrentProcess().Kill();
			}
		}
	}
}
