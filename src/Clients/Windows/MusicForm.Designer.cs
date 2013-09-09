namespace Gablarski.Clients.Windows
{
	partial class MusicForm
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
			this.captureMusic = new System.Windows.Forms.RadioButton ();
			this.fileMusic = new System.Windows.Forms.RadioButton ();
			this.filename = new System.Windows.Forms.TextBox ();
			this.browseButton = new System.Windows.Forms.Button ();
			this.label3 = new System.Windows.Forms.Label ();
			this.okButton = new System.Windows.Forms.Button ();
			this.cancelButton = new System.Windows.Forms.Button ();
			this.deviceSelector = new Gablarski.DeviceSelector ();
			this.SuspendLayout ();
			// 
			// captureMusic
			// 
			this.captureMusic.AutoSize = true;
			this.captureMusic.Location = new System.Drawing.Point (12, 12);
			this.captureMusic.Name = "captureMusic";
			this.captureMusic.Size = new System.Drawing.Size (62, 17);
			this.captureMusic.TabIndex = 0;
			this.captureMusic.TabStop = true;
			this.captureMusic.Text = "Capture";
			this.captureMusic.UseVisualStyleBackColor = true;
			// 
			// fileMusic
			// 
			this.fileMusic.AutoSize = true;
			this.fileMusic.Location = new System.Drawing.Point (12, 102);
			this.fileMusic.Name = "fileMusic";
			this.fileMusic.Size = new System.Drawing.Size (64, 17);
			this.fileMusic.TabIndex = 1;
			this.fileMusic.TabStop = true;
			this.fileMusic.Text = "From file";
			this.fileMusic.UseVisualStyleBackColor = true;
			// 
			// filename
			// 
			this.filename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.filename.Location = new System.Drawing.Point (120, 125);
			this.filename.Name = "filename";
			this.filename.Size = new System.Drawing.Size (153, 20);
			this.filename.TabIndex = 6;
			this.filename.TextChanged += new System.EventHandler (this.filename_TextChanged);
			// 
			// browseButton
			// 
			this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.browseButton.Location = new System.Drawing.Point (279, 123);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size (61, 23);
			this.browseButton.TabIndex = 7;
			this.browseButton.Text = "Browse...";
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler (this.browseButton_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point (9, 128);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size (26, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "File:";
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point (120, 162);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size (75, 23);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler (this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point (201, 162);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size (75, 23);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// deviceSelector
			// 
			this.deviceSelector.DeviceLabel = "Device:";
			this.deviceSelector.Location = new System.Drawing.Point (12, 35);
			this.deviceSelector.Name = "deviceSelector";
			this.deviceSelector.ProviderLabel = "Provider:";
			this.deviceSelector.Size = new System.Drawing.Size (328, 50);
			this.deviceSelector.TabIndex = 11;
			this.deviceSelector.Enter += new System.EventHandler (this.deviceSelector_Enter);
			// 
			// MusicForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size (352, 197);
			this.Controls.Add (this.deviceSelector);
			this.Controls.Add (this.cancelButton);
			this.Controls.Add (this.okButton);
			this.Controls.Add (this.label3);
			this.Controls.Add (this.browseButton);
			this.Controls.Add (this.filename);
			this.Controls.Add (this.fileMusic);
			this.Controls.Add (this.captureMusic);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MusicForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "MusicForm";
			this.Load += new System.EventHandler (this.MusicForm_Load);
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.RadioButton captureMusic;
		private System.Windows.Forms.RadioButton fileMusic;
		private System.Windows.Forms.TextBox filename;
		private System.Windows.Forms.Button browseButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private DeviceSelector deviceSelector;
	}
}