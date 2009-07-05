using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gablarski.Clients.Windows
{
	public class PushToTalkTextBox
		: TextBox
	{
		public void SetText (string stext)
		{
			this.ignoreChange = true;
			this.Text = stext;
			this.text = stext;
			this.ignoreChange = false;
		}

		private string text;
		private bool ignoreChange = false;

		protected override void OnTextChanged (EventArgs e)
		{
			if (ignoreChange)
				return;

			this.ignoreChange = true;
			this.Text = text;
			this.ignoreChange = false;
		}
	}
}