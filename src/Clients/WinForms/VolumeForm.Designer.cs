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
			this.label3 = new System.Windows.Forms.Label();
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
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(49, 29);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(33, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "100%";
			// 
			// VolumeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 45);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(label1);
			this.Controls.Add(this.volume);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "VolumeForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "VolumeForm";
			((System.ComponentModel.ISupportInitialize)(this.volume)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TrackBar volume;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
	}
}