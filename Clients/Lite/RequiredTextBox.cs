using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Gablarski.Clients.Windows
{
	public class RequiredTextBox
		: TextBox
	{
		protected override void OnTextChanged (EventArgs e)
		{
			if (this.BackColor == Color.Red)
				this.BackColor = Color.Empty;

			base.OnTextChanged (e);
		}

		protected override void OnValidating (CancelEventArgs e)
		{
			if (String.IsNullOrEmpty (this.Text) || this.Text.Trim() == String.Empty)
			{
				e.Cancel = true;
				this.BackColor = Color.Red;
			}

			base.OnValidating (e);
		}
	}
}