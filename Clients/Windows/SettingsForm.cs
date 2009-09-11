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

		private Keys pttKey;

		private void SettingsForm_Load (object sender, EventArgs e)
		{
			this.inDisplaySources.Checked = Settings.DisplaySources;
			this.inPTT.SetText (Settings.PushToTalk.KeyboardKeys.ToString());
		}

		private void btnOk_Click (object sender, EventArgs e)
		{
			Settings.DisplaySources = this.inDisplaySources.Checked;
			Settings.PushToTalk = new PushToTalk (pttKey);
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

			this.pttKey = key;
			this.inPTT.SetText (key.ToString());
		}
	}
}
