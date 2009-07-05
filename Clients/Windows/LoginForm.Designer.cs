namespace Gablarski.Clients.Windows
{
	partial class LoginForm
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
			if (disposing && (components != null))
			{
				components.Dispose ();
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
			this.servers = new System.Windows.Forms.ListView ();
			this.pnlModServer = new System.Windows.Forms.Panel ();
			this.labelPassword = new System.Windows.Forms.Label ();
			this.labelUsername = new System.Windows.Forms.Label ();
			this.inPassword = new System.Windows.Forms.TextBox ();
			this.inPort = new Gablarski.Clients.Windows.NumericRequiredTextBox ();
			this.labelNickname = new System.Windows.Forms.Label ();
			this.labelServer = new System.Windows.Forms.Label ();
			this.labelPort = new System.Windows.Forms.Label ();
			this.labelName = new System.Windows.Forms.Label ();
			this.inUsername = new System.Windows.Forms.TextBox ();
			this.inNickname = new Gablarski.Clients.Windows.RequiredTextBox ();
			this.inServer = new Gablarski.Clients.Windows.RequiredTextBox ();
			this.inName = new Gablarski.Clients.Windows.RequiredTextBox ();
			this.btnAddServer = new System.Windows.Forms.Button ();
			this.btnConnect = new System.Windows.Forms.Button ();
			this.btnEditServer = new System.Windows.Forms.Button ();
			this.btnSaveServer = new System.Windows.Forms.Button ();
			this.btnCancel = new System.Windows.Forms.Button ();
			this.pnlModServer.SuspendLayout ();
			this.SuspendLayout ();
			// 
			// servers
			// 
			this.servers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.servers.Location = new System.Drawing.Point (12, 12);
			this.servers.Name = "servers";
			this.servers.Size = new System.Drawing.Size (278, 213);
			this.servers.TabIndex = 0;
			this.servers.UseCompatibleStateImageBehavior = false;
			this.servers.ItemActivate += new System.EventHandler (this.servers_ItemActivate);
			this.servers.SelectedIndexChanged += new System.EventHandler (this.servers_SelectedIndexChanged);
			// 
			// pnlModServer
			// 
			this.pnlModServer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlModServer.Controls.Add (this.labelPassword);
			this.pnlModServer.Controls.Add (this.labelUsername);
			this.pnlModServer.Controls.Add (this.inPassword);
			this.pnlModServer.Controls.Add (this.inPort);
			this.pnlModServer.Controls.Add (this.labelNickname);
			this.pnlModServer.Controls.Add (this.labelServer);
			this.pnlModServer.Controls.Add (this.labelPort);
			this.pnlModServer.Controls.Add (this.labelName);
			this.pnlModServer.Controls.Add (this.inUsername);
			this.pnlModServer.Controls.Add (this.inNickname);
			this.pnlModServer.Controls.Add (this.inServer);
			this.pnlModServer.Controls.Add (this.inName);
			this.pnlModServer.Location = new System.Drawing.Point (12, 12);
			this.pnlModServer.Name = "pnlModServer";
			this.pnlModServer.Size = new System.Drawing.Size (278, 213);
			this.pnlModServer.TabIndex = 3;
			this.pnlModServer.Visible = false;
			// 
			// labelPassword
			// 
			this.labelPassword.AutoSize = true;
			this.labelPassword.Location = new System.Drawing.Point (6, 176);
			this.labelPassword.Name = "labelPassword";
			this.labelPassword.Size = new System.Drawing.Size (56, 13);
			this.labelPassword.TabIndex = 11;
			this.labelPassword.Text = "Password:";
			// 
			// labelUsername
			// 
			this.labelUsername.AutoSize = true;
			this.labelUsername.Location = new System.Drawing.Point (6, 142);
			this.labelUsername.Name = "labelUsername";
			this.labelUsername.Size = new System.Drawing.Size (58, 13);
			this.labelUsername.TabIndex = 10;
			this.labelUsername.Text = "Username:";
			// 
			// inPassword
			// 
			this.inPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.inPassword.Location = new System.Drawing.Point (79, 173);
			this.inPassword.Name = "inPassword";
			this.inPassword.Size = new System.Drawing.Size (196, 20);
			this.inPassword.TabIndex = 9;
			this.inPassword.UseSystemPasswordChar = true;
			// 
			// inPort
			// 
			this.inPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.inPort.Location = new System.Drawing.Point (79, 71);
			this.inPort.Name = "inPort";
			this.inPort.Size = new System.Drawing.Size (196, 20);
			this.inPort.TabIndex = 8;
			this.inPort.Text = "6112";
			// 
			// labelNickname
			// 
			this.labelNickname.AutoSize = true;
			this.labelNickname.Location = new System.Drawing.Point (6, 108);
			this.labelNickname.Name = "labelNickname";
			this.labelNickname.Size = new System.Drawing.Size (58, 13);
			this.labelNickname.TabIndex = 7;
			this.labelNickname.Text = "Nickname:";
			// 
			// labelServer
			// 
			this.labelServer.AutoSize = true;
			this.labelServer.Location = new System.Drawing.Point (6, 40);
			this.labelServer.Name = "labelServer";
			this.labelServer.Size = new System.Drawing.Size (41, 13);
			this.labelServer.TabIndex = 6;
			this.labelServer.Text = "Server:";
			// 
			// labelPort
			// 
			this.labelPort.AutoSize = true;
			this.labelPort.Location = new System.Drawing.Point (6, 74);
			this.labelPort.Name = "labelPort";
			this.labelPort.Size = new System.Drawing.Size (29, 13);
			this.labelPort.TabIndex = 5;
			this.labelPort.Text = "Port:";
			// 
			// labelName
			// 
			this.labelName.AutoSize = true;
			this.labelName.Location = new System.Drawing.Point (6, 6);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size (38, 13);
			this.labelName.TabIndex = 4;
			this.labelName.Text = "Name:";
			// 
			// inUsername
			// 
			this.inUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.inUsername.Location = new System.Drawing.Point (79, 139);
			this.inUsername.Name = "inUsername";
			this.inUsername.Size = new System.Drawing.Size (196, 20);
			this.inUsername.TabIndex = 3;
			// 
			// inNickname
			// 
			this.inNickname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.inNickname.Location = new System.Drawing.Point (79, 105);
			this.inNickname.Name = "inNickname";
			this.inNickname.Size = new System.Drawing.Size (196, 20);
			this.inNickname.TabIndex = 2;
			// 
			// inServer
			// 
			this.inServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.inServer.Location = new System.Drawing.Point (79, 37);
			this.inServer.Name = "inServer";
			this.inServer.Size = new System.Drawing.Size (196, 20);
			this.inServer.TabIndex = 1;
			// 
			// inName
			// 
			this.inName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.inName.Location = new System.Drawing.Point (79, 3);
			this.inName.Name = "inName";
			this.inName.Size = new System.Drawing.Size (196, 20);
			this.inName.TabIndex = 0;
			// 
			// btnAddServer
			// 
			this.btnAddServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAddServer.Image = global::Gablarski.Clients.Windows.Properties.Resources.ServerAddImage;
			this.btnAddServer.Location = new System.Drawing.Point (122, 231);
			this.btnAddServer.Name = "btnAddServer";
			this.btnAddServer.Size = new System.Drawing.Size (93, 24);
			this.btnAddServer.TabIndex = 2;
			this.btnAddServer.Text = "Add Server";
			this.btnAddServer.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.btnAddServer.UseVisualStyleBackColor = true;
			this.btnAddServer.Click += new System.EventHandler (this.btnAddServer_Click);
			// 
			// btnConnect
			// 
			this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnConnect.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnConnect.Enabled = false;
			this.btnConnect.Image = global::Gablarski.Clients.Windows.Properties.Resources.ServerConnectImage;
			this.btnConnect.Location = new System.Drawing.Point (12, 231);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size (75, 24);
			this.btnConnect.TabIndex = 1;
			this.btnConnect.Text = "Connect";
			this.btnConnect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.btnConnect.UseVisualStyleBackColor = true;
			// 
			// btnEditServer
			// 
			this.btnEditServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnEditServer.Image = global::Gablarski.Clients.Windows.Properties.Resources.ServerEditImage;
			this.btnEditServer.Location = new System.Drawing.Point (122, 231);
			this.btnEditServer.Name = "btnEditServer";
			this.btnEditServer.Size = new System.Drawing.Size (93, 24);
			this.btnEditServer.TabIndex = 4;
			this.btnEditServer.Text = "Edit Server";
			this.btnEditServer.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.btnEditServer.UseVisualStyleBackColor = true;
			this.btnEditServer.Visible = false;
			this.btnEditServer.Click += new System.EventHandler (this.btnEditServer_Click);
			// 
			// btnSaveServer
			// 
			this.btnSaveServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSaveServer.Image = global::Gablarski.Clients.Windows.Properties.Resources.SaveImage;
			this.btnSaveServer.Location = new System.Drawing.Point (122, 231);
			this.btnSaveServer.Name = "btnSaveServer";
			this.btnSaveServer.Size = new System.Drawing.Size (93, 24);
			this.btnSaveServer.TabIndex = 5;
			this.btnSaveServer.Text = "Save Server";
			this.btnSaveServer.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.btnSaveServer.UseVisualStyleBackColor = true;
			this.btnSaveServer.Visible = false;
			this.btnSaveServer.Click += new System.EventHandler (this.btnSaveServer_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Location = new System.Drawing.Point (221, 231);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size (66, 24);
			this.btnCancel.TabIndex = 6;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler (this.btnCancel_Click);
			// 
			// LoginForm
			// 
			this.AcceptButton = this.btnConnect;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size (302, 262);
			this.Controls.Add (this.btnCancel);
			this.Controls.Add (this.btnSaveServer);
			this.Controls.Add (this.btnEditServer);
			this.Controls.Add (this.pnlModServer);
			this.Controls.Add (this.btnAddServer);
			this.Controls.Add (this.btnConnect);
			this.Controls.Add (this.servers);
			this.MinimumSize = new System.Drawing.Size (285, 300);
			this.Name = "LoginForm";
			this.Text = "Gablarski Login";
			this.Load += new System.EventHandler (this.LoginForm_Load);
			this.pnlModServer.ResumeLayout (false);
			this.pnlModServer.PerformLayout ();
			this.ResumeLayout (false);

		}

		#endregion

		private System.Windows.Forms.ListView servers;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Button btnAddServer;
		private System.Windows.Forms.Panel pnlModServer;
		private System.Windows.Forms.TextBox inPassword;
		private Gablarski.Clients.Windows.NumericRequiredTextBox inPort;
		private System.Windows.Forms.Label labelNickname;
		private System.Windows.Forms.Label labelServer;
		private System.Windows.Forms.Label labelPort;
		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.TextBox inUsername;
		private Gablarski.Clients.Windows.RequiredTextBox inNickname;
		private Gablarski.Clients.Windows.RequiredTextBox inServer;
		private Gablarski.Clients.Windows.RequiredTextBox inName;
		private System.Windows.Forms.Label labelPassword;
		private System.Windows.Forms.Label labelUsername;
		private System.Windows.Forms.Button btnEditServer;
		private System.Windows.Forms.Button btnSaveServer;
		private System.Windows.Forms.Button btnCancel;
	}
}