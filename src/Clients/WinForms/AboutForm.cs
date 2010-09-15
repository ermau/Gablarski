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
	public partial class AboutForm : Form
	{
		public AboutForm()
		{
			Icon = Resources.HelpImage.ToIcon();
			InitializeComponent();
		}
	}
}
