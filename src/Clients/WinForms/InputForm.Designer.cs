namespace Gablarski.Clients.Windows
{
	partial class InputForm
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
			this.Label = new System.Windows.Forms.Label ();
			this.cancelButton = new System.Windows.Forms.Button ();
			this.okButton = new System.Windows.Forms.Button ();
			this.Input = new Gablarski.Clients.Windows.RequiredTextBox ();
			this.SuspendLayout ();
			// 
			// Label
			// 
			this.Label.AutoSize = true;
			this.Label.Location = new System.Drawing.Point (12, 9);
			this.Label.Name = "Label";
			this.Label.Size = new System.Drawing.Size (58, 13);
			this.Label.TabIndex = 0;
			this.Label.Text = "Nickname:";
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point (184, 32);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size (79, 23);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point (113, 32);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size (65, 23);
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// Input
			// 
			this.Input.CausesValidation = false;
			this.Input.Location = new System.Drawing.Point (113, 6);
			this.Input.Name = "Input";
			this.Input.Size = new System.Drawing.Size (150, 20);
			this.Input.TabIndex = 0;
			// 
			// InputForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size (275, 62);
			this.Controls.Add (this.Input);
			this.Controls.Add (this.okButton);
			this.Controls.Add (this.cancelButton);
			this.Controls.Add (this.Label);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "InputForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Enter Nickname";
			this.ResumeLayout (false);
			this.PerformLayout ();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		public RequiredTextBox Input;
		public System.Windows.Forms.Label Label;
	}
}