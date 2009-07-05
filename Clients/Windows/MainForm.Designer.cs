namespace Gablarski.Clients.Windows
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose (bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose ();
			}
			base.Dispose (disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.components = new System.ComponentModel.Container ();
			this.tools = new System.Windows.Forms.ToolStrip ();
			this.btnConnect = new System.Windows.Forms.ToolStripButton ();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator ();
			this.btnSettings = new System.Windows.Forms.ToolStripButton ();
			this.players = new Gablarski.Clients.Windows.PlayerTreeView ();
			this.tools.SuspendLayout ();
			this.SuspendLayout ();
			// 
			// tools
			// 
			this.tools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tools.Items.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.btnConnect,
            this.toolStripSeparator1,
            this.btnSettings});
			this.tools.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.tools.Location = new System.Drawing.Point (0, 0);
			this.tools.Name = "tools";
			this.tools.Size = new System.Drawing.Size (187, 25);
			this.tools.TabIndex = 0;
			// 
			// btnConnect
			// 
			this.btnConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnConnect.Enabled = false;
			this.btnConnect.Image = global::Gablarski.Clients.Windows.Properties.Resources.DisconnectImage;
			this.btnConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size (23, 22);
			this.btnConnect.Text = "Disconnect";
			this.btnConnect.Click += new System.EventHandler (this.btnConnect_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size (6, 25);
			// 
			// btnSettings
			// 
			this.btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnSettings.Image = global::Gablarski.Clients.Windows.Properties.Resources.SettingsImage;
			this.btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnSettings.Name = "btnSettings";
			this.btnSettings.Size = new System.Drawing.Size (23, 22);
			this.btnSettings.Text = "Settings";
			this.btnSettings.Click += new System.EventHandler (this.btnSettings_Click);
			// 
			// players
			// 
			this.players.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.players.ImageIndex = 0;
			this.players.Location = new System.Drawing.Point (0, 25);
			this.players.Name = "players";
			this.players.SelectedImageIndex = 0;
			this.players.Size = new System.Drawing.Size (187, 266);
			this.players.TabIndex = 1;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size (187, 291);
			this.Controls.Add (this.players);
			this.Controls.Add (this.tools);
			this.MinimumSize = new System.Drawing.Size (192, 161);
			this.Name = "MainForm";
			this.Text = "Gablarski";
			this.Load += new System.EventHandler (this.MainForm_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler (this.MainForm_FormClosing);
			this.tools.ResumeLayout (false);
			this.tools.PerformLayout ();
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.ToolStrip tools;
		private PlayerTreeView players;
		private System.Windows.Forms.ToolStripButton btnConnect;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton btnSettings;
	}
}