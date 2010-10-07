namespace Gablarski.Clients.Windows
{
	partial class RegisterForm
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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.loading = new System.Windows.Forms.PictureBox();
			this.password2 = new Gablarski.Clients.Windows.RequiredTextBox();
			this.username = new Gablarski.Clients.Windows.RequiredTextBox();
			this.password = new Gablarski.Clients.Windows.RequiredTextBox();
			((System.ComponentModel.ISupportInitialize)(this.loading)).BeginInit();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.Location = new System.Drawing.Point(50, 97);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 4;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CausesValidation = false;
			this.cancelButton.Location = new System.Drawing.Point(131, 97);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Username:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 41);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Password:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 68);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(97, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "Reenter Password:";
			// 
			// loading
			// 
			this.loading.Image = global::Gablarski.Clients.Windows.Properties.Resources.LoadingImage;
			this.loading.Location = new System.Drawing.Point(28, 100);
			this.loading.Name = "loading";
			this.loading.Size = new System.Drawing.Size(16, 16);
			this.loading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.loading.TabIndex = 11;
			this.loading.TabStop = false;
			this.loading.Visible = false;
			// 
			// password2
			// 
			this.password2.Location = new System.Drawing.Point(115, 65);
			this.password2.Name = "password2";
			this.password2.Size = new System.Drawing.Size(130, 20);
			this.password2.TabIndex = 3;
			this.password2.UseSystemPasswordChar = true;
			// 
			// username
			// 
			this.username.Location = new System.Drawing.Point(115, 12);
			this.username.Name = "username";
			this.username.Size = new System.Drawing.Size(130, 20);
			this.username.TabIndex = 1;
			// 
			// password
			// 
			this.password.Location = new System.Drawing.Point(115, 38);
			this.password.Name = "password";
			this.password.Size = new System.Drawing.Size(130, 20);
			this.password.TabIndex = 2;
			this.password.UseSystemPasswordChar = true;
			// 
			// RegisterForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(257, 132);
			this.Controls.Add(this.loading);
			this.Controls.Add(this.password2);
			this.Controls.Add(this.username);
			this.Controls.Add(this.password);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "RegisterForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Register";
			((System.ComponentModel.ISupportInitialize)(this.loading)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private RequiredTextBox password;
		private RequiredTextBox username;
		private RequiredTextBox password2;
		private System.Windows.Forms.PictureBox loading;
	}
}