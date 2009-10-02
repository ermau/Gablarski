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
			this.displayTab = new System.Windows.Forms.TabPage ();
			this.inConnectOnStart = new System.Windows.Forms.CheckBox ();
			this.inDisplaySources = new System.Windows.Forms.CheckBox ();
			this.controlsTab = new System.Windows.Forms.TabPage ();
			this.ptt = new System.Windows.Forms.CheckBox ();
			this.linkClear = new System.Windows.Forms.LinkLabel ();
			this.linkSet = new System.Windows.Forms.LinkLabel ();
			this.dispInput = new System.Windows.Forms.Label ();
			this.inInputProvider = new System.Windows.Forms.ComboBox ();
			this.lblInputProvider = new System.Windows.Forms.Label ();
			this.voiceTab = new System.Windows.Forms.TabPage ();
			this.dispThreshold = new System.Windows.Forms.Label ();
			this.lblTreshold = new System.Windows.Forms.Label ();
			this.label3 = new System.Windows.Forms.Label ();
			this.label2 = new System.Windows.Forms.Label ();
			this.threshold = new System.Windows.Forms.TrackBar ();
			this.lblVoiceSensitivity = new System.Windows.Forms.Label ();
			this.vadSensitivity = new System.Windows.Forms.TrackBar ();
			this.voiceSelector = new Gablarski.DeviceSelector ();
			this.tabs.SuspendLayout ();
			this.displayTab.SuspendLayout ();
			this.controlsTab.SuspendLayout ();
			this.voiceTab.SuspendLayout ();
			((System.ComponentModel.ISupportInitialize)(this.threshold)).BeginInit ();
			((System.ComponentModel.ISupportInitialize)(this.vadSensitivity)).BeginInit ();
			this.SuspendLayout ();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point (151, 242);
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
			this.btnCancel.Location = new System.Drawing.Point (232, 242);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size (75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler (this.btnCancel_Click);
			// 
			// tabs
			// 
			this.tabs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.tabs.Controls.Add (this.displayTab);
			this.tabs.Controls.Add (this.controlsTab);
			this.tabs.Controls.Add (this.voiceTab);
			this.tabs.Location = new System.Drawing.Point (0, 0);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size (319, 236);
			this.tabs.TabIndex = 4;
			// 
			// displayTab
			// 
			this.displayTab.Controls.Add (this.inConnectOnStart);
			this.displayTab.Controls.Add (this.inDisplaySources);
			this.displayTab.Location = new System.Drawing.Point (4, 22);
			this.displayTab.Name = "displayTab";
			this.displayTab.Padding = new System.Windows.Forms.Padding (3);
			this.displayTab.Size = new System.Drawing.Size (311, 210);
			this.displayTab.TabIndex = 0;
			this.displayTab.Text = "Display";
			this.displayTab.UseVisualStyleBackColor = true;
			// 
			// inConnectOnStart
			// 
			this.inConnectOnStart.AutoSize = true;
			this.inConnectOnStart.Location = new System.Drawing.Point (8, 33);
			this.inConnectOnStart.Name = "inConnectOnStart";
			this.inConnectOnStart.Size = new System.Drawing.Size (146, 17);
			this.inConnectOnStart.TabIndex = 1;
			this.inConnectOnStart.Text = "Show Connect on startup";
			this.inConnectOnStart.UseVisualStyleBackColor = true;
			// 
			// inDisplaySources
			// 
			this.inDisplaySources.AutoSize = true;
			this.inDisplaySources.Location = new System.Drawing.Point (8, 10);
			this.inDisplaySources.Name = "inDisplaySources";
			this.inDisplaySources.Size = new System.Drawing.Size (129, 17);
			this.inDisplaySources.TabIndex = 0;
			this.inDisplaySources.Text = "Display audio sources";
			this.inDisplaySources.UseVisualStyleBackColor = true;
			// 
			// controlsTab
			// 
			this.controlsTab.Controls.Add (this.ptt);
			this.controlsTab.Controls.Add (this.linkClear);
			this.controlsTab.Controls.Add (this.linkSet);
			this.controlsTab.Controls.Add (this.dispInput);
			this.controlsTab.Controls.Add (this.inInputProvider);
			this.controlsTab.Controls.Add (this.lblInputProvider);
			this.controlsTab.Location = new System.Drawing.Point (4, 22);
			this.controlsTab.Name = "controlsTab";
			this.controlsTab.Padding = new System.Windows.Forms.Padding (3);
			this.controlsTab.Size = new System.Drawing.Size (311, 210);
			this.controlsTab.TabIndex = 1;
			this.controlsTab.Text = "Controls";
			this.controlsTab.UseVisualStyleBackColor = true;
			// 
			// ptt
			// 
			this.ptt.AutoSize = true;
			this.ptt.Location = new System.Drawing.Point (11, 42);
			this.ptt.Name = "ptt";
			this.ptt.Size = new System.Drawing.Size (89, 17);
			this.ptt.TabIndex = 11;
			this.ptt.Text = "Push to Talk:";
			this.ptt.UseVisualStyleBackColor = true;
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
			// voiceTab
			// 
			this.voiceTab.Controls.Add (this.dispThreshold);
			this.voiceTab.Controls.Add (this.lblTreshold);
			this.voiceTab.Controls.Add (this.label3);
			this.voiceTab.Controls.Add (this.label2);
			this.voiceTab.Controls.Add (this.threshold);
			this.voiceTab.Controls.Add (this.lblVoiceSensitivity);
			this.voiceTab.Controls.Add (this.vadSensitivity);
			this.voiceTab.Controls.Add (this.voiceSelector);
			this.voiceTab.Location = new System.Drawing.Point (4, 22);
			this.voiceTab.Name = "voiceTab";
			this.voiceTab.Padding = new System.Windows.Forms.Padding (3);
			this.voiceTab.Size = new System.Drawing.Size (311, 210);
			this.voiceTab.TabIndex = 2;
			this.voiceTab.Text = "Voice";
			this.voiceTab.UseVisualStyleBackColor = true;
			// 
			// dispThreshold
			// 
			this.dispThreshold.Location = new System.Drawing.Point (268, 189);
			this.dispThreshold.Name = "dispThreshold";
			this.dispThreshold.Size = new System.Drawing.Size (35, 15);
			this.dispThreshold.TabIndex = 7;
			this.dispThreshold.Text = "0.6s";
			this.dispThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lblTreshold
			// 
			this.lblTreshold.AutoSize = true;
			this.lblTreshold.Location = new System.Drawing.Point (8, 143);
			this.lblTreshold.Name = "lblTreshold";
			this.lblTreshold.Size = new System.Drawing.Size (91, 13);
			this.lblTreshold.TabIndex = 6;
			this.lblTreshold.Text = "Silence threshold:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point (279, 113);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size (24, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Yell";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point (8, 113);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size (46, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Whisper";
			// 
			// threshold
			// 
			this.threshold.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.threshold.Location = new System.Drawing.Point (11, 159);
			this.threshold.Maximum = 30;
			this.threshold.Name = "threshold";
			this.threshold.Size = new System.Drawing.Size (292, 45);
			this.threshold.TabIndex = 3;
			this.threshold.Value = 6;
			this.threshold.Scroll += new System.EventHandler (this.threshold_Scroll);
			// 
			// lblVoiceSensitivity
			// 
			this.lblVoiceSensitivity.AutoSize = true;
			this.lblVoiceSensitivity.Location = new System.Drawing.Point (8, 63);
			this.lblVoiceSensitivity.Name = "lblVoiceSensitivity";
			this.lblVoiceSensitivity.Size = new System.Drawing.Size (85, 13);
			this.lblVoiceSensitivity.TabIndex = 2;
			this.lblVoiceSensitivity.Text = "Voice sensitivity:";
			// 
			// vadSensitivity
			// 
			this.vadSensitivity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.vadSensitivity.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.vadSensitivity.LargeChange = 500;
			this.vadSensitivity.Location = new System.Drawing.Point (8, 81);
			this.vadSensitivity.Maximum = 8000;
			this.vadSensitivity.Name = "vadSensitivity";
			this.vadSensitivity.Size = new System.Drawing.Size (295, 45);
			this.vadSensitivity.SmallChange = 100;
			this.vadSensitivity.TabIndex = 1;
			this.vadSensitivity.TickFrequency = 100;
			this.vadSensitivity.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
			this.vadSensitivity.Value = 2200;
			// 
			// voiceSelector
			// 
			this.voiceSelector.Location = new System.Drawing.Point (8, 6);
			this.voiceSelector.Name = "voiceSelector";
			this.voiceSelector.Size = new System.Drawing.Size (295, 50);
			this.voiceSelector.TabIndex = 0;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size (319, 277);
			this.Controls.Add (this.tabs);
			this.Controls.Add (this.btnCancel);
			this.Controls.Add (this.btnOk);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "SettingsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			this.Load += new System.EventHandler (this.SettingsForm_Load);
			this.tabs.ResumeLayout (false);
			this.displayTab.ResumeLayout (false);
			this.displayTab.PerformLayout ();
			this.controlsTab.ResumeLayout (false);
			this.controlsTab.PerformLayout ();
			this.voiceTab.ResumeLayout (false);
			this.voiceTab.PerformLayout ();
			((System.ComponentModel.ISupportInitialize)(this.threshold)).EndInit ();
			((System.ComponentModel.ISupportInitialize)(this.vadSensitivity)).EndInit ();
			this.ResumeLayout (false);

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage displayTab;
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
		private System.Windows.Forms.CheckBox ptt;
		private System.Windows.Forms.TrackBar vadSensitivity;
		private System.Windows.Forms.Label lblVoiceSensitivity;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TrackBar threshold;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label dispThreshold;
		private System.Windows.Forms.Label lblTreshold;
	}
}