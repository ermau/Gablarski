namespace Gablarski.Clients.Windows
{
	partial class VolumeForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.Label label1;
			this.volume = new System.Windows.Forms.TrackBar();
			this.label2 = new System.Windows.Forms.Label();
			this.normal = new System.Windows.Forms.LinkLabel();
			label1 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.volume)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(0, 29);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(21, 13);
			label1.TabIndex = 1;
			label1.Text = "0%";
			// 
			// volume
			// 
			this.volume.Dock = System.Windows.Forms.DockStyle.Fill;
			this.volume.LargeChange = 20;
			this.volume.Location = new System.Drawing.Point(0, 0);
			this.volume.Maximum = 500;
			this.volume.Name = "volume";
			this.volume.Size = new System.Drawing.Size(284, 45);
			this.volume.TabIndex = 0;
			this.volume.TickFrequency = 20;
			this.volume.Value = 100;
			this.volume.Scroll += new System.EventHandler(this.volume_Scroll);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(254, 29);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(33, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "500%";
			// 
			// normal
			// 
			this.normal.AutoSize = true;
			this.normal.Location = new System.Drawing.Point(50, 29);
			this.normal.Name = "normal";
			this.normal.Size = new System.Drawing.Size(33, 13);
			this.normal.TabIndex = 3;
			this.normal.TabStop = true;
			this.normal.Text = "100%";
			this.normal.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.normal_LinkClicked);
			// 
			// VolumeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 45);
			this.Controls.Add(this.normal);
			this.Controls.Add(this.label2);
			this.Controls.Add(label1);
			this.Controls.Add(this.volume);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "VolumeForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Volume";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.VolumeForm_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.volume)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TrackBar volume;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.LinkLabel normal;
	}
}