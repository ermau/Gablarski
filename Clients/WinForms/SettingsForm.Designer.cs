namespace Gablarski.Clients.Windows
{
	partial class SettingsForm
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
			this.btnOk = new System.Windows.Forms.Button ();
			this.btnCancel = new System.Windows.Forms.Button ();
			this.tabs = new System.Windows.Forms.TabControl ();
			this.generalTab = new System.Windows.Forms.TabPage ();
			this.gablarskiURLs = new System.Windows.Forms.CheckBox ();
			this.inConnectOnStart = new System.Windows.Forms.CheckBox ();
			this.inDisplaySources = new System.Windows.Forms.CheckBox ();
			this.voiceTab = new System.Windows.Forms.TabPage ();
			this.groupBox1 = new System.Windows.Forms.GroupBox ();
			this.voiceActivation = new System.Windows.Forms.CheckBox ();
			this.dispThreshold = new System.Windows.Forms.Label ();
			this.label3 = new System.Windows.Forms.Label ();
			this.lblTreshold = new System.Windows.Forms.Label ();
			this.threshold = new System.Windows.Forms.TrackBar ();
			this.label2 = new System.Windows.Forms.Label ();
			this.lblVoiceSensitivity = new System.Windows.Forms.Label ();
			this.vadSensitivity = new System.Windows.Forms.TrackBar ();
			this.voiceSelector = new Gablarski.DeviceSelector ();
			this.controlsTab = new System.Windows.Forms.TabPage ();
			this.label10 = new System.Windows.Forms.Label ();
			this.linkClear = new System.Windows.Forms.LinkLabel ();
			this.linkSet = new System.Windows.Forms.LinkLabel ();
			this.dispInput = new System.Windows.Forms.Label ();
			this.inInputProvider = new System.Windows.Forms.ComboBox ();
			this.lblInputProvider = new System.Windows.Forms.Label ();
			this.musicTab = new System.Windows.Forms.TabPage ();
			this.musicIgnoreYou = new System.Windows.Forms.CheckBox ();
			this.label9 = new System.Windows.Forms.Label ();
			this.label8 = new System.Windows.Forms.Label ();
			this.label7 = new System.Windows.Forms.Label ();
			this.label6 = new System.Windows.Forms.Label ();
			this.label5 = new System.Windows.Forms.Label ();
			this.label4 = new System.Windows.Forms.Label ();
			this.normalVolume = new System.Windows.Forms.TrackBar ();
			this.talkingVolume = new System.Windows.Forms.TrackBar ();
			this.volumeControl = new System.Windows.Forms.CheckBox ();
			this.musicPlayers = new System.Windows.Forms.CheckedListBox ();
			this.tabs.SuspendLayout ();
			this.generalTab.SuspendLayout ();
			this.voiceTab.SuspendLayout ();
			this.groupBox1.SuspendLayout ();
			this.controlsTab.SuspendLayout ();
			this.musicTab.SuspendLayout ();
			this.SuspendLayout ();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point (151, 295);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size (75, 23);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "Ok";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler (this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point (232, 295);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size (75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler (this.btnCancel_Click);
			// 
			// tabs
			// 
			this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tabs.Controls.Add (this.generalTab);
			this.tabs.Controls.Add (this.voiceTab);
			this.tabs.Controls.Add (this.controlsTab);
			this.tabs.Controls.Add (this.musicTab);
			this.tabs.Location = new System.Drawing.Point (0, 0);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size (319, 289);
			this.tabs.TabIndex = 4;
			// 
			// generalTab
			// 
			this.generalTab.Controls.Add (this.gablarskiURLs);
			this.generalTab.Controls.Add (this.inConnectOnStart);
			this.generalTab.Controls.Add (this.inDisplaySources);
			this.generalTab.Location = new System.Drawing.Point (4, 22);
			this.generalTab.Name = "generalTab";
			this.generalTab.Padding = new System.Windows.Forms.Padding (3);
			this.generalTab.Size = new System.Drawing.Size (311, 263);
			this.generalTab.TabIndex = 0;
			this.generalTab.Text = "General";
			this.generalTab.UseVisualStyleBackColor = true;
			// 
			// gablarskiURLs
			// 
			this.gablarskiURLs.AutoSize = true;
			this.gablarskiURLs.Enabled = false;
			this.gablarskiURLs.Location = new System.Drawing.Point (8, 52);
			this.gablarskiURLs.Name = "gablarskiURLs";
			this.gablarskiURLs.Size = new System.Drawing.Size (136, 17);
			this.gablarskiURLs.TabIndex = 2;
			this.gablarskiURLs.Text = "Enable Gablarski URLs";
			this.gablarskiURLs.UseVisualStyleBackColor = true;
			// 
			// inConnectOnStart
			// 
			this.inConnectOnStart.AutoSize = true;
			this.inConnectOnStart.Enabled = false;
			this.inConnectOnStart.Location = new System.Drawing.Point (8, 29);
			this.inConnectOnStart.Name = "inConnectOnStart";
			this.inConnectOnStart.Size = new System.Drawing.Size (146, 17);
			this.inConnectOnStart.TabIndex = 1;
			this.inConnectOnStart.Text = "Show Connect on startup";
			this.inConnectOnStart.UseVisualStyleBackColor = true;
			// 
			// inDisplaySources
			// 
			this.inDisplaySources.AutoSize = true;
			this.inDisplaySources.Location = new System.Drawing.Point (8, 6);
			this.inDisplaySources.Name = "inDisplaySources";
			this.inDisplaySources.Size = new System.Drawing.Size (129, 17);
			this.inDisplaySources.TabIndex = 0;
			this.inDisplaySources.Text = "Display audio sources";
			this.inDisplaySources.UseVisualStyleBackColor = true;
			// 
			// voiceTab
			// 
			this.voiceTab.Controls.Add (this.groupBox1);
			this.voiceTab.Controls.Add (this.voiceSelector);
			this.voiceTab.Location = new System.Drawing.Point (4, 22);
			this.voiceTab.Name = "voiceTab";
			this.voiceTab.Padding = new System.Windows.Forms.Padding (3);
			this.voiceTab.Size = new System.Drawing.Size (311, 263);
			this.voiceTab.TabIndex = 2;
			this.voiceTab.Text = "Voice";
			this.voiceTab.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add (this.voiceActivation);
			this.groupBox1.Controls.Add (this.dispThreshold);
			this.groupBox1.Controls.Add (this.label3);
			this.groupBox1.Controls.Add (this.lblTreshold);
			this.groupBox1.Controls.Add (this.threshold);
			this.groupBox1.Controls.Add (this.label2);
			this.groupBox1.Controls.Add (this.lblVoiceSensitivity);
			this.groupBox1.Controls.Add (this.vadSensitivity);
			this.groupBox1.Location = new System.Drawing.Point (8, 65);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size (295, 168);
			this.groupBox1.TabIndex = 8;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "                                ";
			// 
			// voiceActivation
			// 
			this.voiceActivation.AutoSize = true;
			this.voiceActivation.Location = new System.Drawing.Point (9, 1);
			this.voiceActivation.Name = "voiceActivation";
			this.voiceActivation.Size = new System.Drawing.Size (103, 17);
			this.voiceActivation.TabIndex = 6;
			this.voiceActivation.Text = "Voice Activation";
			this.voiceActivation.UseVisualStyleBackColor = true;
			// 
			// dispThreshold
			// 
			this.dispThreshold.Location = new System.Drawing.Point (260, 145);
			this.dispThreshold.Name = "dispThreshold";
			this.dispThreshold.Size = new System.Drawing.Size (35, 15);
			this.dispThreshold.TabIndex = 7;
			this.dispThreshold.Text = "0.6s";
			this.dispThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point (260, 69);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size (24, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Yell";
			// 
			// lblTreshold
			// 
			this.lblTreshold.AutoSize = true;
			this.lblTreshold.Location = new System.Drawing.Point (6, 99);
			this.lblTreshold.Name = "lblTreshold";
			this.lblTreshold.Size = new System.Drawing.Size (91, 13);
			this.lblTreshold.TabIndex = 6;
			this.lblTreshold.Text = "Silence threshold:";
			// 
			// threshold
			// 
			this.threshold.BackColor = System.Drawing.SystemColors.Window;
			this.threshold.Location = new System.Drawing.Point (6, 115);
			this.threshold.Maximum = 30;
			this.threshold.Name = "threshold";
			this.threshold.Size = new System.Drawing.Size (283, 45);
			this.threshold.TabIndex = 3;
			this.threshold.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
			this.threshold.Value = 6;
			this.threshold.Scroll += new System.EventHandler (this.threshold_Scroll);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point (6, 69);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size (46, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Whisper";
			// 
			// lblVoiceSensitivity
			// 
			this.lblVoiceSensitivity.AutoSize = true;
			this.lblVoiceSensitivity.Location = new System.Drawing.Point (6, 19);
			this.lblVoiceSensitivity.Name = "lblVoiceSensitivity";
			this.lblVoiceSensitivity.Size = new System.Drawing.Size (57, 13);
			this.lblVoiceSensitivity.TabIndex = 2;
			this.lblVoiceSensitivity.Text = "Sensitivity:";
			// 
			// vadSensitivity
			// 
			this.vadSensitivity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.vadSensitivity.BackColor = System.Drawing.SystemColors.Window;
			this.vadSensitivity.LargeChange = 500;
			this.vadSensitivity.Location = new System.Drawing.Point (6, 37);
			this.vadSensitivity.Maximum = 8000;
			this.vadSensitivity.Name = "vadSensitivity";
			this.vadSensitivity.Size = new System.Drawing.Size (283, 45);
			this.vadSensitivity.SmallChange = 100;
			this.vadSensitivity.TabIndex = 1;
			this.vadSensitivity.TickFrequency = 100;
			this.vadSensitivity.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
			this.vadSensitivity.Value = 2200;
			// 
			// voiceSelector
			// 
			this.voiceSelector.DeviceLabel = "Capture Device:";
			this.voiceSelector.Location = new System.Drawing.Point (8, 6);
			this.voiceSelector.Name = "voiceSelector";
			this.voiceSelector.ProviderLabel = "Capture Provider:";
			this.voiceSelector.Size = new System.Drawing.Size (295, 50);
			this.voiceSelector.TabIndex = 0;
			// 
			// controlsTab
			// 
			this.controlsTab.Controls.Add (this.label10);
			this.controlsTab.Controls.Add (this.linkClear);
			this.controlsTab.Controls.Add (this.linkSet);
			this.controlsTab.Controls.Add (this.dispInput);
			this.controlsTab.Controls.Add (this.inInputProvider);
			this.controlsTab.Controls.Add (this.lblInputProvider);
			this.controlsTab.Location = new System.Drawing.Point (4, 22);
			this.controlsTab.Name = "controlsTab";
			this.controlsTab.Padding = new System.Windows.Forms.Padding (3);
			this.controlsTab.Size = new System.Drawing.Size (311, 263);
			this.controlsTab.TabIndex = 1;
			this.controlsTab.Text = "Controls";
			this.controlsTab.UseVisualStyleBackColor = true;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point (8, 43);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size (70, 13);
			this.label10.TabIndex = 12;
			this.label10.Text = "Push to Talk:";
			// 
			// linkClear
			// 
			this.linkClear.AutoSize = true;
			this.linkClear.Location = new System.Drawing.Point (143, 43);
			this.linkClear.Name = "linkClear";
			this.linkClear.Size = new System.Drawing.Size (31, 13);
			this.linkClear.TabIndex = 10;
			this.linkClear.TabStop = true;
			this.linkClear.Text = "Clear";
			this.linkClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler (this.linkClear_LinkClicked);
			// 
			// linkSet
			// 
			this.linkSet.AutoSize = true;
			this.linkSet.Location = new System.Drawing.Point (106, 43);
			this.linkSet.Name = "linkSet";
			this.linkSet.Size = new System.Drawing.Size (23, 13);
			this.linkSet.TabIndex = 9;
			this.linkSet.TabStop = true;
			this.linkSet.Text = "Set";
			this.linkSet.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler (this.linkSet_LinkClicked);
			// 
			// dispInput
			// 
			this.dispInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.dispInput.Location = new System.Drawing.Point (180, 43);
			this.dispInput.Name = "dispInput";
			this.dispInput.Size = new System.Drawing.Size (123, 13);
			this.dispInput.TabIndex = 8;
			// 
			// inInputProvider
			// 
			this.inInputProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.inInputProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.inInputProvider.FormattingEnabled = true;
			this.inInputProvider.Location = new System.Drawing.Point (109, 7);
			this.inInputProvider.Name = "inInputProvider";
			this.inInputProvider.Size = new System.Drawing.Size (194, 21);
			this.inInputProvider.TabIndex = 5;
			this.inInputProvider.SelectedIndexChanged += new System.EventHandler (this.inInputProvider_SelectedIndexChanged);
			// 
			// lblInputProvider
			// 
			this.lblInputProvider.AutoSize = true;
			this.lblInputProvider.Location = new System.Drawing.Point (8, 10);
			this.lblInputProvider.Name = "lblInputProvider";
			this.lblInputProvider.Size = new System.Drawing.Size (76, 13);
			this.lblInputProvider.TabIndex = 4;
			this.lblInputProvider.Text = "Input Provider:";
			// 
			// musicTab
			// 
			this.musicTab.Controls.Add (this.musicIgnoreYou);
			this.musicTab.Controls.Add (this.label9);
			this.musicTab.Controls.Add (this.label8);
			this.musicTab.Controls.Add (this.label7);
			this.musicTab.Controls.Add (this.label6);
			this.musicTab.Controls.Add (this.label5);
			this.musicTab.Controls.Add (this.label4);
			this.musicTab.Controls.Add (this.normalVolume);
			this.musicTab.Controls.Add (this.talkingVolume);
			this.musicTab.Controls.Add (this.volumeControl);
			this.musicTab.Controls.Add (this.musicPlayers);
			this.musicTab.Location = new System.Drawing.Point (4, 22);
			this.musicTab.Name = "musicTab";
			this.musicTab.Padding = new System.Windows.Forms.Padding (3);
			this.musicTab.Size = new System.Drawing.Size (311, 263);
			this.musicTab.TabIndex = 3;
			this.musicTab.Text = "Music";
			this.musicTab.UseVisualStyleBackColor = true;
			// 
			// musicIgnoreYou
			// 
			this.musicIgnoreYou.AutoSize = true;
			this.musicIgnoreYou.Checked = true;
			this.musicIgnoreYou.CheckState = System.Windows.Forms.CheckState.Checked;
			this.musicIgnoreYou.Location = new System.Drawing.Point (189, 6);
			this.musicIgnoreYou.Name = "musicIgnoreYou";
			this.musicIgnoreYou.Size = new System.Drawing.Size (119, 17);
			this.musicIgnoreYou.TabIndex = 12;
			this.musicIgnoreYou.Text = "Ignore your sources";
			this.musicIgnoreYou.UseVisualStyleBackColor = true;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point (6, 181);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size (80, 13);
			this.label9.TabIndex = 11;
			this.label9.Text = "Normal volume:";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point (275, 229);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size (33, 13);
			this.label8.TabIndex = 10;
			this.label8.Text = "100%";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point (8, 229);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size (21, 13);
			this.label7.TabIndex = 9;
			this.label7.Text = "0%";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point (6, 109);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size (82, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "Talking volume:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point (275, 156);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size (33, 13);
			this.label5.TabIndex = 7;
			this.label5.Text = "100%";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point (8, 156);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size (21, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "0%";
			// 
			// normalVolume
			// 
			this.normalVolume.BackColor = System.Drawing.SystemColors.Window;
			this.normalVolume.Location = new System.Drawing.Point (9, 197);
			this.normalVolume.Maximum = 100;
			this.normalVolume.Name = "normalVolume";
			this.normalVolume.Size = new System.Drawing.Size (294, 45);
			this.normalVolume.TabIndex = 5;
			this.normalVolume.TickFrequency = 10;
			this.normalVolume.Value = 100;
			// 
			// talkingVolume
			// 
			this.talkingVolume.BackColor = System.Drawing.SystemColors.Window;
			this.talkingVolume.Location = new System.Drawing.Point (9, 124);
			this.talkingVolume.Maximum = 100;
			this.talkingVolume.Name = "talkingVolume";
			this.talkingVolume.Size = new System.Drawing.Size (294, 45);
			this.talkingVolume.TabIndex = 4;
			this.talkingVolume.TickFrequency = 10;
			this.talkingVolume.Value = 30;
			// 
			// volumeControl
			// 
			this.volumeControl.AutoSize = true;
			this.volumeControl.Checked = true;
			this.volumeControl.CheckState = System.Windows.Forms.CheckState.Checked;
			this.volumeControl.Location = new System.Drawing.Point (9, 6);
			this.volumeControl.Name = "volumeControl";
			this.volumeControl.Size = new System.Drawing.Size (131, 17);
			this.volumeControl.TabIndex = 3;
			this.volumeControl.Text = "Enable volume control";
			this.volumeControl.UseVisualStyleBackColor = true;
			// 
			// musicPlayers
			// 
			this.musicPlayers.FormattingEnabled = true;
			this.musicPlayers.Location = new System.Drawing.Point (9, 26);
			this.musicPlayers.Name = "musicPlayers";
			this.musicPlayers.Size = new System.Drawing.Size (294, 79);
			this.musicPlayers.TabIndex = 1;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size (319, 330);
			this.Controls.Add (this.tabs);
			this.Controls.Add (this.btnCancel);
			this.Controls.Add (this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "SettingsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			this.Load += new System.EventHandler (this.SettingsForm_Load);
			this.tabs.ResumeLayout (false);
			this.generalTab.ResumeLayout (false);
			this.generalTab.PerformLayout ();
			this.voiceTab.ResumeLayout (false);
			this.groupBox1.ResumeLayout (false);
			this.groupBox1.PerformLayout ();
			this.controlsTab.ResumeLayout (false);
			this.controlsTab.PerformLayout ();
			this.musicTab.ResumeLayout (false);
			this.musicTab.PerformLayout ();
			this.ResumeLayout (false);

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage generalTab;
		private System.Windows.Forms.CheckBox inDisplaySources;
		private System.Windows.Forms.TabPage controlsTab;
		private System.Windows.Forms.ComboBox inInputProvider;
		private System.Windows.Forms.Label lblInputProvider;
		private System.Windows.Forms.CheckBox inConnectOnStart;
		private System.Windows.Forms.LinkLabel linkClear;
		private System.Windows.Forms.LinkLabel linkSet;
		private System.Windows.Forms.Label dispInput;
		private System.Windows.Forms.TabPage voiceTab;
		private DeviceSelector voiceSelector;
		private System.Windows.Forms.TrackBar vadSensitivity;
		private System.Windows.Forms.Label lblVoiceSensitivity;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TrackBar threshold;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label dispThreshold;
		private System.Windows.Forms.Label lblTreshold;
		private System.Windows.Forms.TabPage musicTab;
		private System.Windows.Forms.CheckedListBox musicPlayers;
		private System.Windows.Forms.TrackBar normalVolume;
		private System.Windows.Forms.TrackBar talkingVolume;
		private System.Windows.Forms.CheckBox volumeControl;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox musicIgnoreYou;
		private System.Windows.Forms.CheckBox gablarskiURLs;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox voiceActivation;
		private System.Windows.Forms.Label label10;
	}
}