using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gablarski.Clients.Windows
{
	public partial class VolumeForm : Form
	{
		private Action<float> onChanged;

		public VolumeForm (Action<float> onChanged)
		{
			if (onChanged == null)
				throw new ArgumentNullException ("onChanged");

			InitializeComponent();
			this.onChanged = onChanged;
		}

		private void volume_Scroll (object sender, EventArgs e)
		{
			onChanged (this.volume.Value / (float)100);
		}
	}
}
