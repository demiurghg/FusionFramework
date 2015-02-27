namespace Fusion.Development {
	partial class DevConForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.components = new System.ComponentModel.Container();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.gameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.qToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.wToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitrToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.gridContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.gridCommandFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.exitButton = new System.Windows.Forms.Button();
			this.launchButton = new System.Windows.Forms.Button();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.mainTreeView = new System.Windows.Forms.TreeView();
			this.selectionBox = new System.Windows.Forms.ListBox();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.mainPropertyGrid = new System.Windows.Forms.PropertyGrid();
			this.messageTextBox = new System.Windows.Forms.TextBox();
			this.mainMenuStrip.SuspendLayout();
			this.gridContextMenu.SuspendLayout();
			this.gridCommandFlowPanel.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gameToolStripMenuItem});
			this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
			this.mainMenuStrip.Size = new System.Drawing.Size(525, 24);
			this.mainMenuStrip.TabIndex = 3;
			this.mainMenuStrip.Text = "menuStrip1";
			// 
			// gameToolStripMenuItem
			// 
			this.gameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.exitrToolStripMenuItem});
			this.gameToolStripMenuItem.Name = "gameToolStripMenuItem";
			this.gameToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
			this.gameToolStripMenuItem.Text = "Game";
			// 
			// addToolStripMenuItem
			// 
			this.addToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.qToolStripMenuItem,
            this.wToolStripMenuItem});
			this.addToolStripMenuItem.Name = "addToolStripMenuItem";
			this.addToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
			this.addToolStripMenuItem.Text = "Add";
			// 
			// qToolStripMenuItem
			// 
			this.qToolStripMenuItem.Name = "qToolStripMenuItem";
			this.qToolStripMenuItem.Size = new System.Drawing.Size(85, 22);
			this.qToolStripMenuItem.Text = "Q";
			// 
			// wToolStripMenuItem
			// 
			this.wToolStripMenuItem.Name = "wToolStripMenuItem";
			this.wToolStripMenuItem.Size = new System.Drawing.Size(85, 22);
			this.wToolStripMenuItem.Text = "W";
			// 
			// exitrToolStripMenuItem
			// 
			this.exitrToolStripMenuItem.Name = "exitrToolStripMenuItem";
			this.exitrToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
			this.exitrToolStripMenuItem.Text = "Exitr";
			// 
			// gridContextMenu
			// 
			this.gridContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1});
			this.gridContextMenu.Name = "gridContextMenu";
			this.gridContextMenu.Size = new System.Drawing.Size(61, 10);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(57, 6);
			// 
			// gridCommandFlowPanel
			// 
			this.gridCommandFlowPanel.AutoSize = true;
			this.gridCommandFlowPanel.BackColor = System.Drawing.SystemColors.Control;
			this.gridCommandFlowPanel.Controls.Add(this.flowLayoutPanel1);
			this.gridCommandFlowPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.gridCommandFlowPanel.Location = new System.Drawing.Point(198, 448);
			this.gridCommandFlowPanel.Margin = new System.Windows.Forms.Padding(0);
			this.gridCommandFlowPanel.Name = "gridCommandFlowPanel";
			this.gridCommandFlowPanel.Size = new System.Drawing.Size(327, 0);
			this.gridCommandFlowPanel.TabIndex = 10;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
			this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel2);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(0, 0);
			this.flowLayoutPanel1.TabIndex = 11;
			// 
			// flowLayoutPanel2
			// 
			this.flowLayoutPanel2.AutoSize = true;
			this.flowLayoutPanel2.BackColor = System.Drawing.SystemColors.Control;
			this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel2.Name = "flowLayoutPanel2";
			this.flowLayoutPanel2.Size = new System.Drawing.Size(0, 0);
			this.flowLayoutPanel2.TabIndex = 11;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.exitButton);
			this.panel1.Controls.Add(this.launchButton);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 614);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(525, 33);
			this.panel1.TabIndex = 12;
			// 
			// exitButton
			// 
			this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.exitButton.Location = new System.Drawing.Point(2, 2);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = new System.Drawing.Size(75, 29);
			this.exitButton.TabIndex = 1;
			this.exitButton.Text = "Terminate";
			this.exitButton.UseVisualStyleBackColor = true;
			this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
			// 
			// launchButton
			// 
			this.launchButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.launchButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
			this.launchButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.launchButton.Location = new System.Drawing.Point(419, 2);
			this.launchButton.Name = "launchButton";
			this.launchButton.Size = new System.Drawing.Size(104, 29);
			this.launchButton.TabIndex = 0;
			this.launchButton.Text = "Launch";
			this.launchButton.UseVisualStyleBackColor = true;
			// 
			// splitter3
			// 
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter3.Location = new System.Drawing.Point(0, 531);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new System.Drawing.Size(525, 3);
			this.splitter3.TabIndex = 14;
			this.splitter3.TabStop = false;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(195, 24);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 507);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			// 
			// mainTreeView
			// 
			this.mainTreeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.mainTreeView.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.mainTreeView.Location = new System.Drawing.Point(0, 24);
			this.mainTreeView.Name = "mainTreeView";
			this.mainTreeView.Size = new System.Drawing.Size(195, 507);
			this.mainTreeView.TabIndex = 0;
			this.mainTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.mainTreeView_BeforeSelect);
			this.mainTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.mainTreeView_AfterSelect);
			// 
			// selectionBox
			// 
			this.selectionBox.BackColor = System.Drawing.SystemColors.MenuBar;
			this.selectionBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.selectionBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.selectionBox.Font = new System.Drawing.Font("Segoe UI", 8F);
			this.selectionBox.FormattingEnabled = true;
			this.selectionBox.Location = new System.Drawing.Point(198, 451);
			this.selectionBox.Name = "selectionBox";
			this.selectionBox.Size = new System.Drawing.Size(327, 80);
			this.selectionBox.TabIndex = 4;
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter2.Location = new System.Drawing.Point(198, 448);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(327, 3);
			this.splitter2.TabIndex = 5;
			this.splitter2.TabStop = false;
			// 
			// mainPropertyGrid
			// 
			this.mainPropertyGrid.ContextMenuStrip = this.gridContextMenu;
			this.mainPropertyGrid.Cursor = System.Windows.Forms.Cursors.Hand;
			this.mainPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainPropertyGrid.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.mainPropertyGrid.Location = new System.Drawing.Point(198, 24);
			this.mainPropertyGrid.Name = "mainPropertyGrid";
			this.mainPropertyGrid.Size = new System.Drawing.Size(327, 424);
			this.mainPropertyGrid.TabIndex = 7;
			this.mainPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.mainPropertyGrid_PropertyValueChanged);
			// 
			// messageTextBox
			// 
			this.messageTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.messageTextBox.Font = new System.Drawing.Font("Consolas", 8F);
			this.messageTextBox.Location = new System.Drawing.Point(0, 534);
			this.messageTextBox.Multiline = true;
			this.messageTextBox.Name = "messageTextBox";
			this.messageTextBox.ReadOnly = true;
			this.messageTextBox.Size = new System.Drawing.Size(525, 80);
			this.messageTextBox.TabIndex = 15;
			// 
			// DevConForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(525, 647);
			this.Controls.Add(this.mainPropertyGrid);
			this.Controls.Add(this.gridCommandFlowPanel);
			this.Controls.Add(this.splitter2);
			this.Controls.Add(this.selectionBox);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.mainTreeView);
			this.Controls.Add(this.mainMenuStrip);
			this.Controls.Add(this.splitter3);
			this.Controls.Add(this.messageTextBox);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.MainMenuStrip = this.mainMenuStrip;
			this.Name = "DevConForm";
			this.Text = "Developer Console";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DevConForm_FormClosed);
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			this.gridContextMenu.ResumeLayout(false);
			this.gridCommandFlowPanel.ResumeLayout(false);
			this.gridCommandFlowPanel.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem gameToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitrToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem qToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem wToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip gridContextMenu;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.FlowLayoutPanel gridCommandFlowPanel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button launchButton;
		private System.Windows.Forms.Button exitButton;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
		private System.Windows.Forms.Splitter splitter3;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.TreeView mainTreeView;
		private System.Windows.Forms.ListBox selectionBox;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.PropertyGrid mainPropertyGrid;
		private System.Windows.Forms.TextBox messageTextBox;
	}
}