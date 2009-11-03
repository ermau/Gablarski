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
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator ();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator ();
			this.tools = new System.Windows.Forms.ToolStrip ();
			this.btnConnect = new System.Windows.Forms.ToolStripButton ();
			this.btnRegister = new System.Windows.Forms.ToolStripButton ();
			this.btnMute = new System.Windows.Forms.ToolStripButton ();
			this.musicButton = new System.Windows.Forms.ToolStripButton ();
			this.btnSettings = new System.Windows.Forms.ToolStripButton ();
			this.users = new Gablarski.Clients.Windows.UserTreeView ();
			this.tools.SuspendLayout ();
			this.SuspendLayout ();
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size (6, 25);
			this.toolStripSeparator1.Visible = false;
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size (6, 25);
			// 
			// tools
			// 
			this.tools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tools.Items.AddRange (new System.Windows.Forms.ToolStripItem[] {
            this.btnConnect,
            this.btnRegister,
            this.toolStripSeparator1,
            this.btnMute,
            this.musicButton,
            this.toolStripSeparator2,
            this.btnSettings});
			this.tools.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.tools.Location = new System.Drawing.Point (0, 0);
			this.tools.Name = "tools";
			this.tools.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.tools.Size = new System.Drawing.Size (229, 25);
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
			// btnRegister
			// 
			this.btnRegister.Image = global::Gablarski.Clients.Windows.Properties.Resources.ChannelAddImage;
			this.btnRegister.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnRegister.Name = "btnRegister";
			this.btnRegister.Size = new System.Drawing.Size (69, 22);
			this.btnRegister.Text = "Register";
			this.btnRegister.Visible = false;
			// 
			// btnMute
			// 
			this.btnMute.CheckOnClick = true;
			this.btnMute.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnMute.Image = global::Gablarski.Clients.Windows.Properties.Resources.SoundMuteImage;
			this.btnMute.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnMute.Name = "btnMute";
			this.btnMute.Size = new System.Drawing.Size (23, 22);
			this.btnMute.Text = "Mute";
			this.btnMute.Visible = false;
			// 
			// musicButton
			// 
			this.musicButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.musicButton.Image = global::Gablarski.Clients.Windows.Properties.Resources.MusicImage;
			this.musicButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.musicButton.Name = "musicButton";
			this.musicButton.Size = new System.Drawing.Size (23, 22);
			this.musicButton.Text = "Play Music";
			this.musicButton.Visible = false;
			this.musicButton.Click += new System.EventHandler (this.musicButton_Click);
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
			// users
			// 
			this.users.AllowDrop = true;
			this.users.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.users.ImageIndex = 0;
			this.users.Location = new System.Drawing.Point (-1, 25);
			this.users.Name = "users";
			this.users.SelectedImageIndex = 0;
			this.users.Size = new System.Drawing.Size (231, 358);
			this.users.TabIndex = 1;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size (229, 382);
			this.Controls.Add (this.tools);
			this.Controls.Add (this.users);
			this.MinimumSize = new System.Drawing.Size (192, 161);
			this.Name = "MainForm";
			this.Text = "Gablarski";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler (this.MainForm_FormClosing);
			this.Load += new System.EventHandler (this.MainForm_Load);
			this.tools.ResumeLayout (false);
			this.tools.PerformLayout ();
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.ToolStrip tools;
		private UserTreeView users;
		private System.Windows.Forms.ToolStripButton btnConnect;
		private System.Windows.Forms.ToolStripButton btnSettings;
		private System.Windows.Forms.ToolStripButton musicButton;
		private System.Windows.Forms.ToolStripButton btnRegister;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton btnMute;
	}
}