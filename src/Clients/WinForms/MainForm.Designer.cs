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
			System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tools = new System.Windows.Forms.ToolStrip();
			this.btnConnect = new System.Windows.Forms.ToolStripButton();
			this.btnRegister = new System.Windows.Forms.ToolStripButton();
			this.btnMute = new System.Windows.Forms.ToolStripButton();
			this.btnMuteMic = new System.Windows.Forms.ToolStripButton();
			this.btnAFK = new System.Windows.Forms.ToolStripButton();
			this.btnMusic = new System.Windows.Forms.ToolStripButton();
			this.btnComment = new System.Windows.Forms.ToolStripButton();
			this.btnSettings = new System.Windows.Forms.ToolStripButton();
			this.aboutButton = new System.Windows.Forms.ToolStripButton();
			this.users = new Gablarski.Clients.Windows.UserTreeView();
			toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.tools.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.Name = "toolStripSeparator3";
			toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// tools
			// 
			this.tools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnConnect,
            this.btnRegister,
            this.toolStripSeparator1,
            this.btnMute,
            this.btnMuteMic,
            this.btnAFK,
            this.btnMusic,
            this.toolStripSeparator2,
            this.btnComment,
            toolStripSeparator3,
            this.btnSettings,
            this.aboutButton});
			this.tools.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.tools.Location = new System.Drawing.Point(0, 0);
			this.tools.Name = "tools";
			this.tools.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.tools.Size = new System.Drawing.Size(251, 25);
			this.tools.TabIndex = 0;
			// 
			// btnConnect
			// 
			this.btnConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnConnect.Image = global::Gablarski.Clients.Windows.Properties.Resources.DisconnectImage;
			this.btnConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(23, 22);
			this.btnConnect.Text = "Disconnect";
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// btnRegister
			// 
			this.btnRegister.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnRegister.Image = global::Gablarski.Clients.Windows.Properties.Resources.ChannelAddImage;
			this.btnRegister.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnRegister.Name = "btnRegister";
			this.btnRegister.Size = new System.Drawing.Size(23, 22);
			this.btnRegister.Text = "Register";
			this.btnRegister.Visible = false;
			this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
			// 
			// btnMute
			// 
			this.btnMute.CheckOnClick = true;
			this.btnMute.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnMute.Enabled = false;
			this.btnMute.Image = global::Gablarski.Clients.Windows.Properties.Resources.SoundMuteImage;
			this.btnMute.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnMute.Name = "btnMute";
			this.btnMute.Size = new System.Drawing.Size(23, 22);
			this.btnMute.Text = "Mute Sound";
			this.btnMute.CheckedChanged += new System.EventHandler(this.btnMute_CheckedChanged);
			// 
			// btnMuteMic
			// 
			this.btnMuteMic.CheckOnClick = true;
			this.btnMuteMic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnMuteMic.Enabled = false;
			this.btnMuteMic.Image = global::Gablarski.Clients.Windows.Properties.Resources.CaptureMuteImage;
			this.btnMuteMic.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnMuteMic.Name = "btnMuteMic";
			this.btnMuteMic.Size = new System.Drawing.Size(23, 22);
			this.btnMuteMic.Text = "Mute Microphone";
			this.btnMuteMic.CheckStateChanged += new System.EventHandler(this.btnMuteMic_CheckStateChanged);
			// 
			// btnAFK
			// 
			this.btnAFK.CheckOnClick = true;
			this.btnAFK.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnAFK.Enabled = false;
			this.btnAFK.Image = global::Gablarski.Clients.Windows.Properties.Resources.UserAFKImage;
			this.btnAFK.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnAFK.Name = "btnAFK";
			this.btnAFK.Size = new System.Drawing.Size(23, 22);
			this.btnAFK.Text = "AFK";
			this.btnAFK.CheckedChanged += new System.EventHandler(this.btnAFK_CheckedChanged);
			// 
			// btnMusic
			// 
			this.btnMusic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnMusic.Image = global::Gablarski.Clients.Windows.Properties.Resources.MusicImage;
			this.btnMusic.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnMusic.Name = "btnMusic";
			this.btnMusic.Size = new System.Drawing.Size(23, 22);
			this.btnMusic.Text = "Play Music";
			this.btnMusic.Visible = false;
			this.btnMusic.Click += new System.EventHandler(this.musicButton_Click);
			// 
			// btnComment
			// 
			this.btnComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnComment.Enabled = false;
			this.btnComment.Image = global::Gablarski.Clients.Windows.Properties.Resources.CommentImage;
			this.btnComment.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnComment.Name = "btnComment";
			this.btnComment.Size = new System.Drawing.Size(23, 22);
			this.btnComment.Text = "Set Comment";
			this.btnComment.Click += new System.EventHandler(this.btnComment_Click);
			// 
			// btnSettings
			// 
			this.btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btnSettings.Image = global::Gablarski.Clients.Windows.Properties.Resources.SettingsImage;
			this.btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.btnSettings.Name = "btnSettings";
			this.btnSettings.Size = new System.Drawing.Size(23, 22);
			this.btnSettings.Text = "Settings";
			this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
			// 
			// aboutButton
			// 
			this.aboutButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.aboutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.aboutButton.Image = global::Gablarski.Clients.Windows.Properties.Resources.HelpImage;
			this.aboutButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.aboutButton.Name = "aboutButton";
			this.aboutButton.Size = new System.Drawing.Size(23, 22);
			this.aboutButton.Text = "About";
			this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
			// 
			// users
			// 
			this.users.AllowDrop = true;
			this.users.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.users.ImageIndex = 0;
			this.users.Location = new System.Drawing.Point(-1, 25);
			this.users.Name = "users";
			this.users.SelectedImageIndex = 0;
			this.users.Size = new System.Drawing.Size(253, 518);
			this.users.TabIndex = 1;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(251, 542);
			this.Controls.Add(this.tools);
			this.Controls.Add(this.users);
			this.MinimumSize = new System.Drawing.Size(192, 161);
			this.Name = "MainForm";
			this.Text = "Gablarski";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.tools.ResumeLayout(false);
			this.tools.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip tools;
		private UserTreeView users;
		private System.Windows.Forms.ToolStripButton btnConnect;
		private System.Windows.Forms.ToolStripButton btnSettings;
		private System.Windows.Forms.ToolStripButton btnMusic;
		private System.Windows.Forms.ToolStripButton btnRegister;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton btnMute;
		private System.Windows.Forms.ToolStripButton btnMuteMic;
		private System.Windows.Forms.ToolStripButton btnComment;
		private System.Windows.Forms.ToolStripButton btnAFK;
		private System.Windows.Forms.ToolStripButton aboutButton;
	}
}