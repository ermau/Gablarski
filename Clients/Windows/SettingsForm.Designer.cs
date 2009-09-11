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
			this.controlsTab = new System.Windows.Forms.TabPage ();
			this.pushLabel = new System.Windows.Forms.Label ();
			this.inPTT = new Gablarski.Clients.Windows.PushToTalkTextBox ();
			this.inDisplaySources = new System.Windows.Forms.CheckBox ();
			this.tabs.SuspendLayout ();
			this.displayTab.SuspendLayout ();
			this.controlsTab.SuspendLayout ();
			this.SuspendLayout ();
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point (97, 152);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size (75, 23);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "Ok";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler (this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point (178, 152);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size (75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// tabs
			// 
			this.tabs.Controls.Add (this.displayTab);
			this.tabs.Controls.Add (this.controlsTab);
			this.tabs.Dock = System.Windows.Forms.DockStyle.Top;
			this.tabs.Location = new System.Drawing.Point (0, 0);
			this.tabs.Name = "tabs";
			this.tabs.SelectedIndex = 0;
			this.tabs.Size = new System.Drawing.Size (265, 146);
			this.tabs.TabIndex = 4;
			// 
			// displayTab
			// 
			this.displayTab.Controls.Add (this.inDisplaySources);
			this.displayTab.Location = new System.Drawing.Point (4, 22);
			this.displayTab.Name = "displayTab";
			this.displayTab.Padding = new System.Windows.Forms.Padding (3);
			this.displayTab.Size = new System.Drawing.Size (257, 120);
			this.displayTab.TabIndex = 0;
			this.displayTab.Text = "Display";
			this.displayTab.UseVisualStyleBackColor = true;
			// 
			// controlsTab
			// 
			this.controlsTab.Controls.Add (this.pushLabel);
			this.controlsTab.Controls.Add (this.inPTT);
			this.controlsTab.Location = new System.Drawing.Point (4, 22);
			this.controlsTab.Name = "controlsTab";
			this.controlsTab.Padding = new System.Windows.Forms.Padding (3);
			this.controlsTab.Size = new System.Drawing.Size (257, 120);
			this.controlsTab.TabIndex = 1;
			this.controlsTab.Text = "Controls";
			this.controlsTab.UseVisualStyleBackColor = true;
			// 
			// pushLabel
			// 
			this.pushLabel.AutoSize = true;
			this.pushLabel.Location = new System.Drawing.Point (3, 9);
			this.pushLabel.Name = "pushLabel";
			this.pushLabel.Size = new System.Drawing.Size (70, 13);
			this.pushLabel.TabIndex = 2;
			this.pushLabel.Text = "Push to Talk:";
			// 
			// inPTT
			// 
			this.inPTT.Location = new System.Drawing.Point (93, 6);
			this.inPTT.Name = "inPTT";
			this.inPTT.Size = new System.Drawing.Size (156, 20);
			this.inPTT.TabIndex = 3;
			// 
			// inDisplaySources
			// 
			this.inDisplaySources.AutoSize = true;
			this.inDisplaySources.Location = new System.Drawing.Point (8, 7);
			this.inDisplaySources.Name = "inDisplaySources";
			this.inDisplaySources.Size = new System.Drawing.Size (129, 17);
			this.inDisplaySources.TabIndex = 0;
			this.inDisplaySources.Text = "Display audio sources";
			this.inDisplaySources.UseVisualStyleBackColor = true;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size (265, 187);
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
		private PushToTalkTextBox inPTT;
	}
}