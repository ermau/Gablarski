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
			this.inInputProvider = new System.Windows.Forms.ComboBox ();
			this.lblInputProvider = new System.Windows.Forms.Label ();
			this.pushLabel = new System.Windows.Forms.Label ();
			this.dispInput = new System.Windows.Forms.Label ();
			this.linkSet = new System.Windows.Forms.LinkLabel ();
			this.linkClear = new System.Windows.Forms.LinkLabel ();
			this.voiceTab = new System.Windows.Forms.TabPage ();
			this.tabs.SuspendLayout ();
			this.displayTab.SuspendLayout ();
			this.controlsTab.SuspendLayout ();
			this.SuspendLayout ();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point (151, 152);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size (75, 23);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "Ok";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler (this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point (232, 152);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size (75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler (this.btnCancel_Click);
			// 
			// tabs
			// 
			this.tabs.Controls.Add (this.displayTab);
			this.tabs.Controls.Add (this.controlsTab);
			this.tabs.Controls.Add (this.voiceTab);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Top;
			this.tabs.Location = new System.Drawing.Point (0, 0);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size (319, 146);
			this.tabs.TabIndex = 4;
			// 
			// displayTab
			// 
			this.displayTab.Controls.Add (this.inConnectOnStart);
			this.displayTab.Controls.Add (this.inDisplaySources);
			this.displayTab.Location = new System.Drawing.Point (4, 22);
			this.displayTab.Name = "displayTab";
			this.displayTab.Padding = new System.Windows.Forms.Padding (3);
			this.displayTab.Size = new System.Drawing.Size (311, 120);
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
			this.controlsTab.Controls.Add (this.linkClear);
			this.controlsTab.Controls.Add (this.linkSet);
			this.controlsTab.Controls.Add (this.dispInput);
			this.controlsTab.Controls.Add (this.inInputProvider);
			this.controlsTab.Controls.Add (this.lblInputProvider);
			this.controlsTab.Controls.Add (this.pushLabel);
			this.controlsTab.Location = new System.Drawing.Point (4, 22);
			this.controlsTab.Name = "controlsTab";
			this.controlsTab.Padding = new System.Windows.Forms.Padding (3);
			this.controlsTab.Size = new System.Drawing.Size (311, 120);
			this.controlsTab.TabIndex = 1;
			this.controlsTab.Text = "Controls";
			this.controlsTab.UseVisualStyleBackColor = true;
			// 
			// inInputProvider
			// 
			this.inInputProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.inInputProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.inInputProvider.FormattingEnabled = true;
			this.inInputProvider.Location = new System.Drawing.Point (95, 7);
			this.inInputProvider.Name = "inInputProvider";
			this.inInputProvider.Size = new System.Drawing.Size (208, 21);
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
			// pushLabel
			// 
			this.pushLabel.AutoSize = true;
			this.pushLabel.Location = new System.Drawing.Point (8, 43);
			this.pushLabel.Name = "pushLabel";
			this.pushLabel.Size = new System.Drawing.Size (70, 13);
			this.pushLabel.TabIndex = 2;
			this.pushLabel.Text = "Push to Talk:";
			// 
			// dispInput
			// 
			this.dispInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.dispInput.Location = new System.Drawing.Point (158, 43);
			this.dispInput.Name = "dispInput";
			this.dispInput.Size = new System.Drawing.Size (145, 13);
			this.dispInput.TabIndex = 8;
			// 
			// linkSet
			// 
			this.linkSet.AutoSize = true;
			this.linkSet.Location = new System.Drawing.Point (92, 43);
			this.linkSet.Name = "linkSet";
			this.linkSet.Size = new System.Drawing.Size (23, 13);
			this.linkSet.TabIndex = 9;
			this.linkSet.TabStop = true;
			this.linkSet.Text = "Set";
			this.linkSet.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler (this.linkSet_LinkClicked);
			// 
			// linkClear
			// 
			this.linkClear.AutoSize = true;
			this.linkClear.Location = new System.Drawing.Point (121, 43);
			this.linkClear.Name = "linkClear";
			this.linkClear.Size = new System.Drawing.Size (31, 13);
			this.linkClear.TabIndex = 10;
			this.linkClear.TabStop = true;
			this.linkClear.Text = "Clear";
			this.linkClear.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler (this.linkClear_LinkClicked);
			// 
			// voiceTab
			// 
			this.voiceTab.Location = new System.Drawing.Point (4, 22);
			this.voiceTab.Name = "voiceTab";
			this.voiceTab.Padding = new System.Windows.Forms.Padding (3);
			this.voiceTab.Size = new System.Drawing.Size (311, 120);
			this.voiceTab.TabIndex = 2;
			this.voiceTab.Text = "Voice";
			this.voiceTab.UseVisualStyleBackColor = true;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size (319, 187);
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
			this.ResumeLayout (false);

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage displayTab;
		private System.Windows.Forms.CheckBox inDisplaySources;
		private System.Windows.Forms.TabPage controlsTab;
		private System.Windows.Forms.Label pushLabel;
		private System.Windows.Forms.ComboBox inInputProvider;
		private System.Windows.Forms.Label lblInputProvider;
		private System.Windows.Forms.CheckBox inConnectOnStart;
		private System.Windows.Forms.LinkLabel linkClear;
		private System.Windows.Forms.LinkLabel linkSet;
		private System.Windows.Forms.Label dispInput;
		private System.Windows.Forms.TabPage voiceTab;
	}
}