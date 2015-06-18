using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Fusion;
using Fusion.Mathematics;
using System.Reflection;
using Fusion.Content;
using Fusion.Shell;
using System.Threading;
using System.Runtime.InteropServices;


namespace Fusion.Development {
	public partial class DevConForm : Form {

		static public DevConForm Instance;

		Game			game;
		Thread			uiThread;

		internal ContentBuilder	contentProject;


		public AssetCollection Assets { get { return contentProject.Assets; } }


		public static string FusionBinary { get { return Environment.GetEnvironmentVariable("FUSION_BIN"); } }
		public static string FusionContent { get { return Environment.GetEnvironmentVariable("FUSION_CONTENT"); } }


		class FormTraceListener : TraceListener {

			DevConForm devConForm;
			
			public FormTraceListener( DevConForm devConForm )
			{
				this.devConForm	=	devConForm;
			}


			public override void Write ( string message )
			{
				devConForm.messageTextBox.Text += message;
				devConForm.messageTextBox.SelectionStart = 999999;
				devConForm.messageTextBox.ScrollToCaret();
			}

			public override void WriteLine ( string message )
			{
				devConForm.messageTextBox.Text += message + "\r\n";
				devConForm.messageTextBox.SelectionStart = 999999;
				devConForm.messageTextBox.ScrollToCaret();
			}
		}


		FormTraceListener formTraceListener;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		/// <param name="modal"></param>
		/// <param name="buttonText"></param>
		/// <param name="messageText"></param>
		internal DevConForm ( Game game, string contentPath, string targetDirectory, bool modal )
		{
			formTraceListener = new FormTraceListener( this );
			Trace.Listeners.Add( formTraceListener );

			Instance	=	this;
			uiThread	=	Thread.CurrentThread;
			this.game	=	game;

			game.Reloading += game_Reloading;

			InitializeComponent();

			mainPropertyGrid.PropertySort		=	PropertySort.Categorized;

			mainTreeView.PathSeparator	=	"/";

			
			//
			//	setup content project:
			//
			contentProject	=	new ContentBuilder( contentPath, targetDirectory, this, game );
			contentProject.Load();

			RefreshConsole(true);


			if (modal) {
				launchButton.Text		=	"Launch";
				launchButton.Click		+=	launchButton_ClickLaunch;
				//messageTextBox.Text		=	MakeLaunchString();
				Activated += (s,e) => launchButton.Focus();
			} else {
				launchButton.Visible	=	false;
				//messageTextBox.Text		=	MakeLaunchString();
				Activated += (s,e) => textBox1.Focus();
			}

			this.FormClosed += (s,e) => Trace.Listeners.Remove( formTraceListener );
		}


		



		private void launchButton_ClickLaunch ( object sender, EventArgs e )
		{
			contentProject.Save();

			if (contentProject.BuildContent(false, null)) {
				this.DialogResult = DialogResult.OK;
				Close();
			}
		}



		void game_Reloading ( object sender, EventArgs e )
		{
			contentProject.BuildContent();
		}



		string MakeLaunchString ()
		{
			var	appName		=	Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
			var platform	=	(Marshal.SizeOf(typeof(IntPtr))==8) ? "x64" : "x86";
			var cmdline		=	Environment.CommandLine;
			var fusVer		=	string.Format("{0} {1}", Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly().GetName().Version);

			#if DEBUG
			var cfg	= "Debug";
			#else
			var cfg	= "Release";
			#endif

			return	  fusVer + "\r\n"
					+ appName + " "+ platform + " " + cfg + "\r\n" 
					+ cmdline + "";
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		string MakeCaptionString ()
		{
			var	appName		=	Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);
			var platform	=	(Marshal.SizeOf(typeof(IntPtr))==8) ? "x64" : "x86";

			#if DEBUG
			var cfg	= "Debug";
			#else
			var cfg	= "Release";
			#endif

			return	appName + " - Developer Console " + " (" + cfg + " " + platform + ")" + (contentProject.Modified ? "*" : "");
		}



		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Console 
		 * 
		-----------------------------------------------------------------------------------------*/
		#if false
		System.Drawing.Color GetMessageColor ( Log.MessageType type )
		{
			//return System.Drawing.Color.Black;
			if ( type==Log.MessageType.Debug		) { return System.Drawing.Color.Gray;	}
			if ( type==Log.MessageType.Message		) { return System.Drawing.Color.Black;	}
			if ( type==Log.MessageType.Information	) { return System.Drawing.Color.Blue;	}
			if ( type==Log.MessageType.Warning		) { return System.Drawing.Color.DarkOrange;	}
			if ( type==Log.MessageType.Error		) { return System.Drawing.Color.Red;	}
			return System.Drawing.Color.Black;
		}



		System.Drawing.Color GetMessageBackColor ( Log.MessageType type )
		{
			//return System.Drawing.Color.White;
			if ( type==Log.MessageType.Debug		) { return System.Drawing.Color.White;	/*.White;	*/}
			if ( type==Log.MessageType.Message		) { return System.Drawing.Color.White;	/*.White;	*/}
			if ( type==Log.MessageType.Information	) { return System.Drawing.Color.White;	/*.White;	*/}
			if ( type==Log.MessageType.Warning		) { return System.Drawing.Color.White; /*.Yellow;  */}
			if ( type==Log.MessageType.Error		) { return System.Drawing.Color.White;	/*.Red;	    */}
			return System.Drawing.Color.Black;
		}


		public void AppendText( string text, System.Drawing.Color color, System.Drawing.Color backColor )
		{
			text	=	text /*+ new string(' ', Math.Max(0, 120 - text.Length))*/ +"\r\n";

			messageTextBox.SelectionStart = messageTextBox.TextLength;
			messageTextBox.SelectionLength = 0;

			messageTextBox.SelectionColor = color;
			messageTextBox.SelectionBackColor = backColor;
			messageTextBox.AppendText(text);
			messageTextBox.SelectionColor = messageTextBox.ForeColor;
			messageTextBox.ScrollToCaret();
		}



		void MessageCallback (Log.MessageType type, string msg)
		{
			if (!messageTextBox.IsDisposed) {
				if (uiThread==Thread.CurrentThread) {
					AppendText( msg, GetMessageColor(type), GetMessageBackColor(type) );
				} else {
					MethodInvoker action = delegate { AppendText( msg, GetMessageColor(type), GetMessageBackColor(type) ); };
					messageTextBox.BeginInvoke(action);
					messageTextBox.Invalidate();
				}
			}
		}



		void MessageCallbackOld (Log.MessageType type, string msg)
		{
			if (!messageTextBox.IsDisposed) {
				AppendText( msg, GetMessageColor(type), GetMessageBackColor(type) );
			}
		}
		#endif

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Console 
		 * 
		-----------------------------------------------------------------------------------------*/

		/// <summary>
		/// Refresh all editor stuff 
		/// </summary>
		public void RefreshConsole (bool force)
		{
			if (force) {
				mainTreeView.Nodes.Clear();
			}

			Text = MakeCaptionString();

			AddNode( "Parameters", game.Parameters );
			AddBranch( "Configuration", new[]{'.'}, game.GetConfiguration(), true );

			if (contentProject!=null) {
				contentProject.Outline(this);
			}

			mainMenuStrip.Items.Clear();


			//
			//	Add game service commands :
			//
			List<object> objList = game.GetServiceList().Select( a => (object)a ).ToList();
			
			objList.Insert( 0, game );
			objList.Insert( 1, contentProject );


			foreach ( var svc in objList ) {

				var item = new ToolStripMenuItem( svc.GetType().Name );
				//var item = new ToolStripMenuItem( svc.ToString() );

				var methodDescs = svc.GetType()
					.GetMethods()
					.Where( m1 => m1.GetCustomAttribute<UICommandAttribute>() != null )
					.Select( m2 => new { Method = m2, Attr = m2.GetCustomAttribute<UICommandAttribute>() } )
					.OrderBy( m3 => m3.Attr.GroupIndex )
					.ToList();

				if (!methodDescs.Any()) {
					continue;
				}

				int groupId = methodDescs.First().Attr.GroupIndex;

				foreach ( var method in methodDescs ) {
					if ( groupId != method.Attr.GroupIndex ) {
						item.DropDownItems.Add( new ToolStripSeparator() );
					}

					groupId	= method.Attr.GroupIndex;

					item.DropDownItems.Add( method.Attr.Name ?? method.Method.Name, null, new EventHandler( (obj,e)=> method.Method.Invoke(svc,null) ) );
				}


				mainMenuStrip.Items.Add( item );
			}


			//
			//	Add command
			//
			/*var cmds = Command.GatherCommands();
			var cmdMenu = new ToolStripMenuItem("Shell Commands");

			foreach ( var cmdType in cmds ) {

				var niceName = cmdType.GetCustomAttribute<CommandAttribute>().NiceName;
				var cmd		 = cmdType.GetCustomAttribute<CommandAttribute>().Name;

				cmdMenu.DropDownItems.Add( niceName, null, new EventHandler( (obj,e) => { game.Invoker.Push(cmd); game.Invoker.Update(new GameTime()); } ) );

			}
			
			mainMenuStrip.Items.Add( cmdMenu );*/


			launchButton.Focus();
		}



		List<TreeNode>	nodeSelection = new List<TreeNode>();


		public void RemoveSelectedNodes ()
		{

			var temp = nodeSelection.ToList();
			nodeSelection.Clear();

			temp.ForEach( node => node.Remove() );
		}



		public void SelectNone ()
		{
			selectionBox.Items.Clear();
			gridCommandFlowPanel.Controls.Clear();
			mainPropertyGrid.SelectedObjects	=	null;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mainTreeView_AfterSelect ( object sender, TreeViewEventArgs e )
		{
			//	Add stuff to property grid :
			var selectedObjects	=	nodeSelection.Select( n => n.Tag ).Where( t => t != null ).ToArray();
			mainPropertyGrid.SelectedObjects	=	selectedObjects;
			mainPropertyGrid.PropertySort		=	PropertySort.Categorized;
			mainPropertyGrid.CommandsVisibleIfAvailable = true;

			//	Add stuff to selected box :
			selectionBox.Items.Clear();
			selectionBox.Items.AddRange( nodeSelection.Select( n => n.FullPath ).ToArray() );


			//
			//	Adds command :
			//
			gridCommandFlowPanel.Controls.Clear();


			List<MethodInfo> methodNames = new List<MethodInfo>();

			foreach ( var obj in selectedObjects ) {

				foreach ( var method in obj.GetType().GetMethods( BindingFlags.Public | BindingFlags.Instance ) ) {

					var ca = (UICommandAttribute)method.GetCustomAttributes( typeof(UICommandAttribute) ).FirstOrDefault();

					if (ca==null) {
						continue;
					}

					methodNames.Add( method );
				}
			}

			methodNames = methodNames
					.DistinctBy( m => m.Name )
					.OrderBy( m1 => ((UICommandAttribute)m1.GetCustomAttribute( typeof(UICommandAttribute) )).GroupIndex )
					.ToList();

			foreach ( var mn in methodNames ) {
				
				#if true
					var btn = new Button(){ Text = mn.Name, AutoSize = true, Padding = new Padding(0), Margin = new Padding(0,3,3,0), Height = 15/*23*/ };
				#else
					var btn = new Label(){ 
						Text = mn.Name, 
						AutoSize = true, 
						Padding = new Padding(1), 
						Margin = new Padding(0,3,0,0),
						Font	= new Font(FontFamily.GenericSansSerif, 8, FontStyle.Underline),
						ForeColor = System.Drawing.Color.Blue,
						Cursor = Cursors.Hand,
						//BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
					};

					btn.MouseEnter += (s,a) => { btn.ForeColor = System.Drawing.Color.BlueViolet; };
					btn.MouseLeave += (s,a) => { btn.ForeColor = System.Drawing.Color.Blue;		  };
					btn.MouseDown  += (s,a) => { btn.ForeColor = System.Drawing.Color.DarkBlue;	  };
					btn.MouseUp    += (s,a) => { btn.ForeColor = System.Drawing.Color.Blue;       };
				#endif
				
				btn.Click += (s,a) => {
					//btn.ForeColor = System.Drawing.Color.Blue;
					CallMethodOnSelectedItems( mn.Name );
				};

				gridCommandFlowPanel.Controls.Add( btn );
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="methodName"></param>
		void CallMethodOnSelectedItems ( string methodName )
		{
			var selectedObjects = mainPropertyGrid.SelectedObjects;

			foreach ( var obj in selectedObjects ) {

				foreach ( var method in obj.GetType().GetMethods( BindingFlags.Public | BindingFlags.Instance ) ) {

					var ca = (UICommandAttribute)method.GetCustomAttributes( typeof(UICommandAttribute) ).FirstOrDefault();

					if (ca==null) {
						continue;
					}

					if (method.Name!=methodName) {
						continue;
					}

					method.Invoke( obj, null );
				}
			}

			mainPropertyGrid.Refresh();
		}



		/// <summary>
		/// Multiple selection support
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void mainTreeView_BeforeSelect ( object sender, TreeViewCancelEventArgs e )
		{
			if ( ModifierKeys == Keys.Control ) {

				if (!nodeSelection.Contains(e.Node)) {
					nodeSelection.Add( e.Node );
				}

			} else {

				nodeSelection.Clear();
				nodeSelection.Add( e.Node );
			}
		}



		/// <summary>
		/// Get selected names
		/// </summary>
		/// <param name="rootNode"></param>
		/// <returns></returns>
		public IEnumerable<string> GetSelectedNames ( string rootNode )
		{
			return nodeSelection.Select( n => n.FullPath )
					.Where( n1 => n1.StartsWith( rootNode + "/" ) )
					.Select( n2 => n2.Replace( rootNode + "/", "" ) )
					.ToList();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerable<object> GetSelectedObjects ()
		{
			return nodeSelection.Select( n => n.Tag );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		/// <param name="e"></param>
		private void mainPropertyGrid_PropertyValueChanged ( object s, PropertyValueChangedEventArgs e )
		{
			//mainPropertyGrid.Refresh();

			var descs = mainPropertyGrid.SelectedObjects
						.Where( so => so is AbstractAsset )
						.Select( so1 => (AbstractAsset)so1 );

			foreach ( var desc in descs ) {
				try {
					Game.Instance.Content.PatchAbstractAsset( desc.AssetPath, desc );
				} catch ( Exception ex ) {
					Log.Warning("Patch: {0}", ex.Message);
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="obj"></param>
		public void AddNode ( string name, object obj )
		{
			TreeNode node = null;

			if (mainTreeView.Nodes.ContainsKey( name )) {
				node = mainTreeView.Nodes[ name ];
			} else {
				mainTreeView.Nodes.Add( name, name );
				node = mainTreeView.Nodes[ name ];
			}

			node.Tag = obj;
		}



		public class PathComparer : IComparer<string> {

			[DllImport("shlwapi.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
			static extern int StrCmpLogicalW(String x, String y);

			public int Compare(string x, string y) {

				var pathX = x.Split( new[]{'/', '\\'}, StringSplitOptions.RemoveEmptyEntries );
				var pathY = y.Split( new[]{'/', '\\'}, StringSplitOptions.RemoveEmptyEntries );

				var min	  = Math.Min( pathX.Length, pathY.Length );

				
				for ( int i = 0; i<min; i++ ) {

					if ( i == min-1 ) {
						
						var lenD = pathY.Length - pathX.Length;
						var extD = StrCmpLogicalW( Path.GetExtension(pathX[i]), Path.GetExtension(pathY[i]) );
						
						if (lenD!=0) {
							return lenD;
						} else if (extD!=0) {
							return extD;
						} else {
							return StrCmpLogicalW( pathX[i], pathY[i] );
						}
					}

					int cmp = StrCmpLogicalW( pathX[i], pathY[i] );

					if (cmp!=0) {
						return cmp;
					}
				}

				return 0;
			}

		}



		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rootName"></param>
		/// <param name="list"></param>
		public void AddBranch<T> ( string rootName, char[] separator, IEnumerable<KeyValuePair<string,T>> pairList, bool expand )
		{
			TreeNode rootNode = null;

			if (mainTreeView.Nodes.ContainsKey(rootName)) {
				rootNode = mainTreeView.Nodes[rootName];
			} else {
				mainTreeView.Nodes.Add(rootName,rootName);
				rootNode = mainTreeView.Nodes[rootName];
			}

			//
			//	Sort file names :
			//
			var pairList2 = pairList.OrderBy( p1 => p1.Key, new PathComparer() ).ToList();

			//
			//	Add all :
			//
			foreach ( var pair in pairList2 ) {
				
				var path	= 	pair.Key;
				var tokens	=	path.Split( separator, StringSplitOptions.RemoveEmptyEntries );
				var node	=	rootNode;

				if (expand) {
					node.ExpandAll();
				}

				foreach ( var tok in tokens ) {
					
					if (node.Nodes.ContainsKey(tok)) {
						node = node.Nodes[tok];
					} else {
						node.Nodes.Add(tok,tok);
						node = node.Nodes[tok];
					}

					node.ForeColor	=	System.Drawing.Color.Black;
				}

				node.Tag		=	pair.Value;
			}
		}


		private void exitButton_Click ( object sender, EventArgs e )
		{
			contentProject.Save();
			Process.GetCurrentProcess().Kill();
		}

		private void DevConForm_FormClosed ( object sender, FormClosedEventArgs e )
		{
			contentProject.Save();
		}

		private void textBox1_TextChanged ( object sender, EventArgs e )
		{

		}

		private void textBox1_KeyPress ( object sender, KeyPressEventArgs e )
		{

		}

		private void textBox1_KeyDown ( object sender, KeyEventArgs e )
		{
			if (e.KeyCode==Keys.Enter) {

				if (luaSelector.Checked) {

					Log.Message("Lua: {0}", textBox1.Text);

					try {

						game.Lua.DoString( textBox1.Text );
						textBox1.Text = "";

					} catch ( NLua.Exceptions.LuaException lex ) {
						Log.Error( lex.Message );
					}

				} else {
					try {
						Log.Message( textBox1.Text );
						game.Invoker.Push( textBox1.Text);
						textBox1.Text = "";
					} catch ( Exception ex ) {
						Log.Error( ex.Message );
					}
				}
			}
		}

		private void buttonClear_Click ( object sender, EventArgs e )
		{
			messageTextBox.Clear();
		}
	}
}
