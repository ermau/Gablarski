using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gablarski.Clients.Windows
{
	public class WebLinkLabel
		: LinkLabel
	{
		public string URL
		{
			get
			{
				if (this.uri == null)
					return String.Empty;

				return this.uri.ToString();
			}

			set { this.uri = new Uri (value); }
		}

		private Uri uri;

		protected override void OnLinkClicked (LinkLabelLinkClickedEventArgs e)
		{
			Process.Start (URL);
			base.OnLinkClicked(e);
		}
	}
}