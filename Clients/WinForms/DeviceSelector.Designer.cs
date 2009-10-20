namespace Gablarski
{
	partial class DeviceSelector
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent ()
		{
			this.label2 = new System.Windows.Forms.Label ();
			this.label1 = new System.Windows.Forms.Label ();
			this.device = new System.Windows.Forms.ComboBox ();
			this.provider = new System.Windows.Forms.ComboBox ();
			this.SuspendLayout ();
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point (-3, 30);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size (44, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Device:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point (-2, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size (49, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "Provider:";
			// 
			// device
			// 
			this.device.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.device.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.device.Enabled = false;
			this.device.FormattingEnabled = true;
			this.device.Location = new System.Drawing.Point (107, 27);
			this.device.Name = "device";
			this.device.Size = new System.Drawing.Size (244, 21);
			this.device.TabIndex = 7;
			// 
			// provider
			// 
			this.provider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.provider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.provider.FormattingEnabled = true;
			this.provider.Location = new System.Drawing.Point (107, 0);
			this.provider.Name = "provider";
			this.provider.Size = new System.Drawing.Size (244, 21);
			this.provider.TabIndex = 6;
			this.provider.SelectedIndexChanged += new System.EventHandler (this.provider_SelectedIndexChanged);
			// 
			// DeviceSelector
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add (this.label2);
			this.Controls.Add (this.label1);
			this.Controls.Add (this.device);
			this.Controls.Add (this.provider);
			this.Name = "DeviceSelector";
			this.Size = new System.Drawing.Size (354, 50);
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox device;
		private System.Windows.Forms.ComboBox provider;
	}
}
