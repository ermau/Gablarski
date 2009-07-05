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
			this.pushLabel = new System.Windows.Forms.Label ();
			this.inPTT = new Gablarski.Clients.Windows.PushToTalkTextBox ();
			this.btnOk = new System.Windows.Forms.Button ();
			this.btnCancel = new System.Windows.Forms.Button ();
			this.SuspendLayout ();
			// 
			// pushLabel
			// 
			this.pushLabel.AutoSize = true;
			this.pushLabel.Location = new System.Drawing.Point (12, 9);
			this.pushLabel.Name = "pushLabel";
			this.pushLabel.Size = new System.Drawing.Size (70, 13);
			this.pushLabel.TabIndex = 0;
			this.pushLabel.Text = "Push to Talk:";
			// 
			// inPTT
			// 
			this.inPTT.Location = new System.Drawing.Point (88, 6);
			this.inPTT.Name = "inPTT";
			this.inPTT.Size = new System.Drawing.Size (156, 20);
			this.inPTT.TabIndex = 1;
			this.inPTT.Leave += new System.EventHandler (this.inPTT_Leave);
			this.inPTT.Enter += new System.EventHandler (this.inPTT_Enter);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point (88, 32);
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
			this.btnCancel.Location = new System.Drawing.Point (169, 32);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size (75, 23);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size (262, 64);
			this.Controls.Add (this.btnCancel);
			this.Controls.Add (this.btnOk);
			this.Controls.Add (this.inPTT);
			this.Controls.Add (this.pushLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "SettingsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			this.Load += new System.EventHandler (this.SettingsForm_Load);
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.Label pushLabel;
		private Gablarski.Clients.Windows.PushToTalkTextBox inPTT;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
	}
}