using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Clients.Input;
using Gablarski.Clients.Windows.Properties;

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

		private void SettingsForm_Load (object sender, EventArgs e)
		{
			this.inInputProvider.DataSource = Modules.Input.ToList();
			this.inDisplaySources.Checked = Settings.DisplaySources;
		}

		private void btnOk_Click (object sender, EventArgs e)
		{
			DisableInput();
			Settings.InputProvider = this.inInputProvider.SelectedItem.ToString();

			if (this.inputSettings != null)
				Settings.InputSettings = this.inputSettings;

			Settings.DisplaySources = this.inDisplaySources.Checked;
			Settings.SaveSettings();

			this.Close();
		}

		private void btnCancel_Click (object sender, EventArgs e)
		{
			DisableInput();
		}

		private string inputSettings;
		private IInputProvider currentInputProvider;

		void OnInputStateChanged (object sender, InputStateChangedEventArgs e)
		{
			if (e.State == InputState.Off)
				return;

			string nice;
			this.inputSettings = this.currentInputProvider.EndRecord (out nice);

			BeginInvoke ((Action<string>)(s =>
			{
				this.dispInput.Text = s;
				this.linkSet.Enabled = true;
			}), nice);
		}

		private void inInputProvider_SelectedIndexChanged (object sender, EventArgs e)
		{
			DisableInput();

			if (this.inInputProvider.SelectedItem == null)
				return;

			currentInputProvider = (IInputProvider)Activator.CreateInstance ((Type)this.inInputProvider.SelectedItem);
			currentInputProvider.InputStateChanged += OnInputStateChanged;
			currentInputProvider.Attach (this.Handle, null);
		}

		private void DisableInput()
		{
			if (currentInputProvider == null)
				return;

			currentInputProvider.Detach();
			currentInputProvider.InputStateChanged -= OnInputStateChanged;
			currentInputProvider.Dispose();
			currentInputProvider = null;
		}

		private void linkSet_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (this.currentInputProvider == null)
				return;

			this.linkSet.Enabled = false;
			this.currentInputProvider.BeginRecord();
		}

		private void linkClear_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.dispInput.Text = String.Empty;
			this.inputSettings = null;
		}
	}
}
