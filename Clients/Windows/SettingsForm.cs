using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Clients.Windows.Properties;
using Kennedy.ManagedHooks;

namespace Gablarski.Clients.Windows
{
	public partial class SettingsForm
		: Form
	{
		public SettingsForm ()
		{
			this.Icon = Resources.SettingsImage.ToIcon();
			InitializeComponent ();
		}

		private Keys key;

		private void btnOk_Click (object sender, EventArgs e)
		{
			Settings.PushToTalk = new PushToTalk (key);
			Settings.SaveSettings();
			this.Close();
		}

		private void inPTT_Enter (object sender, EventArgs e)
		{
			Program.KHook.KeyboardEvent += KHookKeyboardEvent;
		}

		private void inPTT_Leave (object sender, EventArgs e)
		{
			Program.KHook.KeyboardEvent -= KHookKeyboardEvent;
		}

		private void KHookKeyboardEvent (KeyboardEvents kEvent, Keys key)
		{
			if (kEvent != KeyboardEvents.KeyDown)
				return;

			this.key = key;
			this.inPTT.Text = key.ToString();
		}
	}
}
