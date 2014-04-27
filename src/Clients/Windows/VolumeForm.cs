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
		private readonly Action<float> onChanged;
		private readonly Action<float> onCommit;

		public VolumeForm (float initialGain, Action<float> onChanged, Action<float> onCommit)
		{
			if (onChanged == null)
				throw new ArgumentNullException ("onChanged");
			if (onCommit == null)
				throw new ArgumentNullException ("onCommit");

			InitializeComponent();
			this.onChanged = onChanged;
			this.onCommit = onCommit;

			this.volume.Value = (int)(initialGain * 100);
		}

		private void volume_Scroll (object sender, EventArgs e)
		{
			onChanged (this.volume.Value / (float)100);
		}

		private void normal_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.volume.Value = 100;
			onChanged (1.0f);
		}

		private void VolumeForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			onCommit (this.volume.Value / (float)100);
		}
	}
}
