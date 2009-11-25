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

	public class NumericRequiredTextBox
		: TextBox
	{
		public decimal Value
		{
			get
			{
				decimal v;
				return Decimal.TryParse (this.Text, out v) ? v : 0M;
			}
		}

		protected override void OnTextChanged (EventArgs e)
		{
			if (this.BackColor == Color.Red)
				this.BackColor = Color.Empty;

			base.OnTextChanged (e);
		}

		protected override void OnValidating (CancelEventArgs e)
		{
			decimal value;
			if (String.IsNullOrEmpty (this.Text) || this.Text.Trim() == String.Empty || !Decimal.TryParse (this.Text, out value))
			{
				e.Cancel = true;
				this.BackColor = Color.Red;
			}

			base.OnValidating (e);
		}
	}
}