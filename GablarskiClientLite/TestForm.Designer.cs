namespace GablarskiClientLite
{
	partial class TestForm
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
			this.label1 = new System.Windows.Forms.Label ();
			this.ServerHost = new System.Windows.Forms.TextBox ();
			this.connectButton = new System.Windows.Forms.Button ();
			this.log = new System.Windows.Forms.TextBox ();
			this.label2 = new System.Windows.Forms.Label ();
			this.inputSelect = new System.Windows.Forms.ComboBox ();
			this.label3 = new System.Windows.Forms.Label ();
			this.playbackProviderSelect = new System.Windows.Forms.ComboBox ();
			this.captureProviderSelect = new System.Windows.Forms.ComboBox ();
			this.label4 = new System.Windows.Forms.Label ();
			this.label5 = new System.Windows.Forms.Label ();
			this.outputSelect = new System.Windows.Forms.ComboBox ();
			this.startServerButton = new System.Windows.Forms.Button ();
			this.label6 = new System.Windows.Forms.Label ();
			this.ServerName = new System.Windows.Forms.TextBox ();
			this.sourceRequestSelect = new System.Windows.Forms.ComboBox ();
			this.requestSource = new System.Windows.Forms.Button ();
			this.transmit = new System.Windows.Forms.Button ();
			this.sourceSelect = new System.Windows.Forms.ComboBox ();
			this.playerList = new System.Windows.Forms.TreeView ();
			this.label7 = new System.Windows.Forms.Label ();
			this.label8 = new System.Windows.Forms.Label ();
			this.nickname = new System.Windows.Forms.TextBox ();
			this.login = new System.Windows.Forms.Button ();
			this.userProviderSelect = new System.Windows.Forms.ComboBox ();
			this.label9 = new System.Windows.Forms.Label ();
			this.SuspendLayout ();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point (9, 287);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size (41, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Server:";
			// 
			// ServerHost
			// 
			this.ServerHost.Location = new System.Drawing.Point (111, 284);
			this.ServerHost.Name = "ServerHost";
			this.ServerHost.Size = new System.Drawing.Size (189, 20);
			this.ServerHost.TabIndex = 6;
			this.ServerHost.Text = "localhost";
			// 
			// connectButton
			// 
			this.connectButton.Location = new System.Drawing.Point (306, 282);
			this.connectButton.Name = "connectButton";
			this.connectButton.Size = new System.Drawing.Size (66, 23);
			this.connectButton.TabIndex = 7;
			this.connectButton.Text = "Connect";
			this.connectButton.UseVisualStyleBackColor = true;
			this.connectButton.Click += new System.EventHandler (this.connectButton_Click);
			// 
			// log
			// 
			this.log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.log.Location = new System.Drawing.Point (12, 391);
			this.log.Multiline = true;
			this.log.Name = "log";
			this.log.ReadOnly = true;
			this.log.Size = new System.Drawing.Size (566, 143);
			this.log.TabIndex = 8;
			this.log.TabStop = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point (9, 258);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size (34, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Input:";
			// 
			// inputSelect
			// 
			this.inputSelect.DisplayMember = "Name";
			this.inputSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.inputSelect.FormattingEnabled = true;
			this.inputSelect.Location = new System.Drawing.Point (111, 255);
			this.inputSelect.Name = "inputSelect";
			this.inputSelect.Size = new System.Drawing.Size (261, 21);
			this.inputSelect.TabIndex = 5;
			this.inputSelect.SelectedIndexChanged += new System.EventHandler (this.inputSelect_SelectedIndexChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point (9, 171);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size (96, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Playback Provider:";
			// 
			// playbackProviderSelect
			// 
			this.playbackProviderSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.playbackProviderSelect.Enabled = false;
			this.playbackProviderSelect.FormattingEnabled = true;
			this.playbackProviderSelect.Location = new System.Drawing.Point (111, 168);
			this.playbackProviderSelect.Name = "playbackProviderSelect";
			this.playbackProviderSelect.Size = new System.Drawing.Size (261, 21);
			this.playbackProviderSelect.TabIndex = 2;
			this.playbackProviderSelect.SelectedIndexChanged += new System.EventHandler (this.playbackProviderSelect_SelectedIndexChanged);
			// 
			// captureProviderSelect
			// 
			this.captureProviderSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.captureProviderSelect.Enabled = false;
			this.captureProviderSelect.FormattingEnabled = true;
			this.captureProviderSelect.Location = new System.Drawing.Point (111, 226);
			this.captureProviderSelect.Name = "captureProviderSelect";
			this.captureProviderSelect.Size = new System.Drawing.Size (261, 21);
			this.captureProviderSelect.TabIndex = 4;
			this.captureProviderSelect.SelectedIndexChanged += new System.EventHandler (this.captureProviderSelect_SelectedIndexChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point (9, 229);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size (89, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Capture Provider:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point (9, 200);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size (42, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "Output:";
			// 
			// outputSelect
			// 
			this.outputSelect.DisplayMember = "Name";
			this.outputSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.outputSelect.FormattingEnabled = true;
			this.outputSelect.Location = new System.Drawing.Point (111, 197);
			this.outputSelect.Name = "outputSelect";
			this.outputSelect.Size = new System.Drawing.Size (261, 21);
			this.outputSelect.TabIndex = 3;
			this.outputSelect.SelectedIndexChanged += new System.EventHandler (this.outputSelect_SelectedIndexChanged);
			// 
			// startServerButton
			// 
			this.startServerButton.Location = new System.Drawing.Point (299, 35);
			this.startServerButton.Name = "startServerButton";
			this.startServerButton.Size = new System.Drawing.Size (75, 23);
			this.startServerButton.TabIndex = 1;
			this.startServerButton.Text = "Start";
			this.startServerButton.UseVisualStyleBackColor = true;
			this.startServerButton.Click += new System.EventHandler (this.startServerButton_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point (12, 40);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size (70, 13);
			this.label6.TabIndex = 13;
			this.label6.Text = "Server name:";
			// 
			// ServerName
			// 
			this.ServerName.Location = new System.Drawing.Point (113, 37);
			this.ServerName.Name = "ServerName";
			this.ServerName.Size = new System.Drawing.Size (167, 20);
			this.ServerName.TabIndex = 0;
			this.ServerName.Text = "ohai";
			// 
			// sourceRequestSelect
			// 
			this.sourceRequestSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.sourceRequestSelect.Enabled = false;
			this.sourceRequestSelect.FormattingEnabled = true;
			this.sourceRequestSelect.Location = new System.Drawing.Point (12, 337);
			this.sourceRequestSelect.Name = "sourceRequestSelect";
			this.sourceRequestSelect.Size = new System.Drawing.Size (262, 21);
			this.sourceRequestSelect.TabIndex = 14;
			this.sourceRequestSelect.SelectedIndexChanged += new System.EventHandler (this.sourceRequestSelect_SelectedIndexChanged);
			// 
			// requestSource
			// 
			this.requestSource.Enabled = false;
			this.requestSource.Location = new System.Drawing.Point (280, 335);
			this.requestSource.Name = "requestSource";
			this.requestSource.Size = new System.Drawing.Size (92, 23);
			this.requestSource.TabIndex = 15;
			this.requestSource.Text = "Request Source";
			this.requestSource.UseVisualStyleBackColor = true;
			this.requestSource.Click += new System.EventHandler (this.requestSource_Click);
			// 
			// transmit
			// 
			this.transmit.Enabled = false;
			this.transmit.Location = new System.Drawing.Point (280, 362);
			this.transmit.Name = "transmit";
			this.transmit.Size = new System.Drawing.Size (92, 23);
			this.transmit.TabIndex = 16;
			this.transmit.Text = "Transmit";
			this.transmit.UseVisualStyleBackColor = true;
			this.transmit.Click += new System.EventHandler (this.transmit_Click);
			// 
			// sourceSelect
			// 
			this.sourceSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.sourceSelect.Enabled = false;
			this.sourceSelect.FormattingEnabled = true;
			this.sourceSelect.Location = new System.Drawing.Point (12, 364);
			this.sourceSelect.Name = "sourceSelect";
			this.sourceSelect.Size = new System.Drawing.Size (262, 21);
			this.sourceSelect.TabIndex = 17;
			this.sourceSelect.SelectedIndexChanged += new System.EventHandler (this.sourceSelect_SelectedIndexChanged);
			// 
			// playerList
			// 
			this.playerList.Location = new System.Drawing.Point (381, 30);
			this.playerList.Name = "playerList";
			this.playerList.Size = new System.Drawing.Size (197, 355);
			this.playerList.TabIndex = 18;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point (378, 14);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size (97, 13);
			this.label7.TabIndex = 19;
			this.label7.Text = "Player/Source List:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point (9, 312);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size (58, 13);
			this.label8.TabIndex = 20;
			this.label8.Text = "Nickname:";
			// 
			// nickname
			// 
			this.nickname.Enabled = false;
			this.nickname.Location = new System.Drawing.Point (111, 309);
			this.nickname.Name = "nickname";
			this.nickname.Size = new System.Drawing.Size (189, 20);
			this.nickname.TabIndex = 21;
			this.nickname.Text = "asdf";
			// 
			// login
			// 
			this.login.Enabled = false;
			this.login.Location = new System.Drawing.Point (306, 307);
			this.login.Name = "login";
			this.login.Size = new System.Drawing.Size (66, 23);
			this.login.TabIndex = 22;
			this.login.Text = "Login";
			this.login.UseVisualStyleBackColor = true;
			this.login.Click += new System.EventHandler (this.login_Click);
			// 
			// userProviderSelect
			// 
			this.userProviderSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.userProviderSelect.FormattingEnabled = true;
			this.userProviderSelect.Location = new System.Drawing.Point (113, 8);
			this.userProviderSelect.Name = "userProviderSelect";
			this.userProviderSelect.Size = new System.Drawing.Size (259, 21);
			this.userProviderSelect.TabIndex = 23;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point (12, 11);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size (74, 13);
			this.label9.TabIndex = 24;
			this.label9.Text = "User Provider:";
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size (590, 546);
			this.Controls.Add (this.label9);
			this.Controls.Add (this.userProviderSelect);
			this.Controls.Add (this.login);
			this.Controls.Add (this.nickname);
			this.Controls.Add (this.label8);
			this.Controls.Add (this.label7);
			this.Controls.Add (this.playerList);
			this.Controls.Add (this.sourceSelect);
			this.Controls.Add (this.transmit);
			this.Controls.Add (this.requestSource);
			this.Controls.Add (this.sourceRequestSelect);
			this.Controls.Add (this.ServerName);
			this.Controls.Add (this.label6);
			this.Controls.Add (this.startServerButton);
			this.Controls.Add (this.outputSelect);
			this.Controls.Add (this.label5);
			this.Controls.Add (this.label4);
			this.Controls.Add (this.captureProviderSelect);
			this.Controls.Add (this.playbackProviderSelect);
			this.Controls.Add (this.label3);
			this.Controls.Add (this.inputSelect);
			this.Controls.Add (this.label2);
			this.Controls.Add (this.log);
			this.Controls.Add (this.connectButton);
			this.Controls.Add (this.ServerHost);
			this.Controls.Add (this.label1);
			this.Name = "TestForm";
			this.Text = "Gablarski Test";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler (this.TestForm_FormClosed);
			this.Load += new System.EventHandler (this.TestForm_Load);
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox ServerHost;
		private System.Windows.Forms.Button connectButton;
		private System.Windows.Forms.TextBox log;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox inputSelect;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox playbackProviderSelect;
		private System.Windows.Forms.ComboBox captureProviderSelect;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox outputSelect;
		private System.Windows.Forms.Button startServerButton;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox ServerName;
		private System.Windows.Forms.ComboBox sourceRequestSelect;
		private System.Windows.Forms.Button requestSource;
		private System.Windows.Forms.Button transmit;
		private System.Windows.Forms.ComboBox sourceSelect;
		private System.Windows.Forms.TreeView playerList;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox nickname;
		private System.Windows.Forms.Button login;
		private System.Windows.Forms.ComboBox userProviderSelect;
		private System.Windows.Forms.Label label9;
	}
}

