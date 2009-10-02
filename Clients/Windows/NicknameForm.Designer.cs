namespace Gablarski.Clients.Windows
{
	partial class NicknameForm
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
			this.cancelButton = new System.Windows.Forms.Button ();
			this.okButton = new System.Windows.Forms.Button ();
			this.nickname = new Gablarski.Clients.Windows.RequiredTextBox ();
			this.SuspendLayout ();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point (12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size (58, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Nickname:";
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point (151, 32);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size (79, 23);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point (80, 32);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size (65, 23);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// nickname
			// 
			this.nickname.CausesValidation = false;
			this.nickname.Location = new System.Drawing.Point (80, 6);
			this.nickname.Name = "nickname";
			this.nickname.Size = new System.Drawing.Size (150, 20);
			this.nickname.TabIndex = 0;
			// 
			// NicknameForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size (242, 62);
			this.Controls.Add (this.nickname);
			this.Controls.Add (this.okButton);
			this.Controls.Add (this.cancelButton);
			this.Controls.Add (this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "NicknameForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Enter Nickname";
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		public RequiredTextBox nickname;
	}
}