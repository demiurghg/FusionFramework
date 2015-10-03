namespace Fusion.Core.Development {
	partial class ExceptionDialog {
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
			this.buttonTerminate = new System.Windows.Forms.Button();
			this.textBoxStack = new System.Windows.Forms.TextBox();
			this.textLabel = new System.Windows.Forms.Label();
			this.textBoxMessage = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.labelExceptionType = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// buttonTerminate
			// 
			this.buttonTerminate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonTerminate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonTerminate.Location = new System.Drawing.Point(331, 276);
			this.buttonTerminate.Name = "buttonTerminate";
			this.buttonTerminate.Size = new System.Drawing.Size(91, 23);
			this.buttonTerminate.TabIndex = 0;
			this.buttonTerminate.Text = "Terminate";
			this.buttonTerminate.UseVisualStyleBackColor = true;
			this.buttonTerminate.Click += new System.EventHandler(this.buttonTerminate_Click);
			// 
			// textBoxStack
			// 
			this.textBoxStack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxStack.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxStack.Location = new System.Drawing.Point(12, 157);
			this.textBoxStack.Multiline = true;
			this.textBoxStack.Name = "textBoxStack";
			this.textBoxStack.ReadOnly = true;
			this.textBoxStack.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxStack.Size = new System.Drawing.Size(410, 113);
			this.textBoxStack.TabIndex = 1;
			this.textBoxStack.WordWrap = false;
			// 
			// textLabel
			// 
			this.textLabel.AutoSize = true;
			this.textLabel.Location = new System.Drawing.Point(12, 9);
			this.textLabel.Name = "textLabel";
			this.textLabel.Size = new System.Drawing.Size(291, 39);
			this.textLabel.TabIndex = 2;
			this.textLabel.Text = "Unhandled exception has occurred in your game.\r\nIf you click Terminate applicatio" +
    "n will immediately terminated.\r\n\r\n";
			// 
			// textBoxMessage
			// 
			this.textBoxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxMessage.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBoxMessage.Location = new System.Drawing.Point(12, 90);
			this.textBoxMessage.Multiline = true;
			this.textBoxMessage.Name = "textBoxMessage";
			this.textBoxMessage.ReadOnly = true;
			this.textBoxMessage.Size = new System.Drawing.Size(410, 48);
			this.textBoxMessage.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 141);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Stack trace:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 74);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Message:";
			// 
			// labelExceptionType
			// 
			this.labelExceptionType.AutoSize = true;
			this.labelExceptionType.Location = new System.Drawing.Point(12, 48);
			this.labelExceptionType.Name = "labelExceptionType";
			this.labelExceptionType.Size = new System.Drawing.Size(35, 26);
			this.labelExceptionType.TabIndex = 7;
			this.labelExceptionType.Text = "label3\r\n\r\n";
			// 
			// ExceptionDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(434, 311);
			this.Controls.Add(this.labelExceptionType);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxMessage);
			this.Controls.Add(this.textLabel);
			this.Controls.Add(this.textBoxStack);
			this.Controls.Add(this.buttonTerminate);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(450, 350);
			this.Name = "ExceptionDialog";
			this.ShowIcon = false;
			this.Text = "Unhandled Exception";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExceptionDialog_FormClosed);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonTerminate;
		private System.Windows.Forms.TextBox textBoxStack;
		private System.Windows.Forms.Label textLabel;
		private System.Windows.Forms.TextBox textBoxMessage;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label labelExceptionType;
	}
}