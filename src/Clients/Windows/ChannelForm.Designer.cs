namespace Gablarski.Clients.Windows
{
	partial class ChannelForm
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
			this.lblName = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.inName = new Gablarski.Clients.Windows.RequiredTextBox();
			this.inDescription = new System.Windows.Forms.TextBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.inUserLimit = new System.Windows.Forms.NumericUpDown();
			this.lblUserLimit = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.inUserLimit)).BeginInit();
			this.SuspendLayout();
			// 
			// lblName
			// 
			this.lblName.AutoSize = true;
			this.lblName.Location = new System.Drawing.Point(12, 15);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(38, 13);
			this.lblName.TabIndex = 0;
			this.lblName.Text = "Name:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Description:";
			// 
			// inName
			// 
			this.inName.CausesValidation = false;
			this.inName.Location = new System.Drawing.Point(118, 12);
			this.inName.Name = "inName";
			this.inName.Size = new System.Drawing.Size(154, 20);
			this.inName.TabIndex = 0;
			// 
			// inDescription
			// 
			this.inDescription.CausesValidation = false;
			this.inDescription.Location = new System.Drawing.Point(118, 38);
			this.inDescription.Multiline = true;
			this.inDescription.Name = "inDescription";
			this.inDescription.Size = new System.Drawing.Size(154, 57);
			this.inDescription.TabIndex = 1;
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(118, 130);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 3;
			this.btnOk.Text = "Ok";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(199, 130);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// inUserLimit
			// 
			this.inUserLimit.CausesValidation = false;
			this.inUserLimit.Location = new System.Drawing.Point(118, 101);
			this.inUserLimit.Name = "inUserLimit";
			this.inUserLimit.Size = new System.Drawing.Size(120, 20);
			this.inUserLimit.TabIndex = 2;
			// 
			// lblUserLimit
			// 
			this.lblUserLimit.AutoSize = true;
			this.lblUserLimit.Location = new System.Drawing.Point(12, 103);
			this.lblUserLimit.Name = "lblUserLimit";
			this.lblUserLimit.Size = new System.Drawing.Size(52, 13);
			this.lblUserLimit.TabIndex = 7;
			this.lblUserLimit.Text = "User limit:";
			// 
			// ChannelForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(284, 165);
			this.Controls.Add(this.lblUserLimit);
			this.Controls.Add(this.inUserLimit);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.inDescription);
			this.Controls.Add(this.inName);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.lblName);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ChannelForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Channel";
			((System.ComponentModel.ISupportInitialize)(this.inUserLimit)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Label label2;
		private RequiredTextBox inName;
		private System.Windows.Forms.TextBox inDescription;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.NumericUpDown inUserLimit;
		private System.Windows.Forms.Label lblUserLimit;
	}
}