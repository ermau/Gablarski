using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Clients.Windows.Properties;

namespace Gablarski.Clients.Windows
{
	public partial class CommentForm : Form
	{
		public CommentForm (string currentComment)
		{
			InitializeComponent();
			this.comment.Text = currentComment;
		}

		public string Comment
		{
			get; private set;
		}

		private void CommentForm_Load (object sender, EventArgs e)
		{
			this.Icon = Resources.CommentImage.ToIcon();
		}

		private void CommentTextChanged(object sender, EventArgs e)
		{
			Comment = comment.Text;
		}

		private void OkButtonClick (object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		private void CancelButtonClick (object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}
