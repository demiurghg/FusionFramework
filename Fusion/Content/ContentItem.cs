using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Windows.Forms;



namespace Fusion.Content {

	[Asset("Content", "BUG BUG BUG")]
	public class ContentItem : Asset {

		[Category("Content Item")]
		[ReadOnly(true)]
		public List<string> Dependencies { get; set; }

		[Category("Content Item")]
		[ReadOnly(true)]
		[XmlIgnore]
		public string ResolvedPath { get; set; }

		[Category("Content Item")]
		[ReadOnly(true)]
		[XmlIgnore]
		public string TargetPath { get; set; }

		
		[Category("Content Item")]
		[ReadOnly(true)]
		public string ContentProcessor { get; set; }

		
		[Category("Content Item")]
		[Description("Newer content processor will ignore this content item.\r\nSetting this parameter can cause program failure\r\nbut reduce rebuild time especially on heavy files.")]
		public bool IgnoreToolChanges { get; set; }


		/// <summary>
		/// 
		/// </summary>
		public ContentItem ()
		{
			Path = "";
			ContentProcessor = "";
			Dependencies = new List<string>();
			IgnoreToolChanges = false;
		}



		public ContentItem ( string path, string processor )
		{
			Path = path;
			ContentProcessor = processor;
			Dependencies = new List<string>();
		}



		public ContentItem ( ContentItem other )
		{
			Path				=	other.Path;
			ContentProcessor	=	other.Path;
			Path	=	other.Path;
			Path	=	other.Path;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="depName"></param>
		public void AddDependency ( string depName )
		{
			if (Dependencies.Contains(depName)) {
				return;
			} else {
				Dependencies.Add( depName );
			}
		}


		/// <summary>
		/// 
		/// </summary>
		[Command]
		public void Rebuild ()
		{
			ContentProject.Instance.BuildContentItem( this, true );
			
			if (Game.Instance!=null) {
				Game.Instance.Reload();
			}
		}


		[Command]
		public void Open ()
		{
			try {
				var fileName = ContentProject.Instance.ResolveContentSourcePath( Path );
				Process.Start( fileName );
			} catch (Exception ex) {
				MessageBox.Show( ex.Message, "Open", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}


		[Command]
		public void Folder ()
		{
			try {
				var fileName = ContentProject.Instance.ResolveContentSourcePath( Path );

				var psi = new ProcessStartInfo( System.IO.Path.GetDirectoryName( fileName ) );
				psi.WindowStyle = ProcessWindowStyle.Normal;
				Process.Start( psi );

			} catch (Exception ex) {
				MessageBox.Show( ex.Message, "Folder", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}
	}




}
