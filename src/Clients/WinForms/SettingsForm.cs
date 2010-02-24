using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Audio;
using Gablarski.Clients.Input;
using Gablarski.Clients.Media;
using Gablarski.Clients.Windows.Properties;
using Cadenza;

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
			this.inDisplaySources.Checked = Settings.DisplaySources;
			this.inConnectOnStart.Checked = Settings.ShowConnectOnStart;
			this.gablarskiURLs.Checked = Settings.EnableGablarskiURLs;

			this.voiceActivation.Checked = !Settings.UsePushToTalk;
			this.inInputProvider.DisplayMember = "DisplayName";
			this.inInputProvider.DataSource = Modules.Input.ToList();
			this.inInputProvider.SelectedText = Settings.InputProvider;

			this.playbackSelector.ProviderSource = Modules.Playback.Cast<IAudioDeviceProvider>();
			this.playbackSelector.SetProvider (Settings.PlaybackProvider);
			this.playbackSelector.SetDevice (Settings.PlaybackDevice);

			this.voiceSelector.ProviderSource = Modules.Capture.Cast<IAudioDeviceProvider>();
			this.voiceSelector.SetProvider (Settings.VoiceProvider);
			this.voiceSelector.SetDevice (Settings.VoiceDevice);
			
			this.inputSettings = Settings.InputSettings;

			if (currentInputProvider != null)
				this.dispInput.Text = currentInputProvider.GetNiceInputName (this.inputSettings);

			this.threshold.Value = Settings.VoiceActivationContinueThreshold / 100;
			this.dispThreshold.Text = String.Format ("{0:N1}s", (double)this.threshold.Value / 10);
			this.vadSensitivity.Value = Settings.VoiceActivationLevel;

			this.volumeControl.Checked = Settings.EnableMediaVolumeControl;
			this.musicIgnoreYou.Checked = Settings.MediaVolumeControlIgnoresYou;
			this.talkingVolume.Value = Settings.TalkingMusicVolume;
			this.normalVolume.Value = Settings.NormalMusicVolume;

			foreach (IMediaPlayer player in Modules.MediaPlayers)
			{
				this.musicPlayers.Items.Add (player.GetType().Name.Remove ("Integration", "Provider"), Settings.EnabledMediaPlayerIntegrations.Any (s => s.Contains (player.GetType().FullName)));
			}

			this.enableNotifications.Checked = Settings.EnableNotifications;
			foreach (INotifier n in Modules.Notifiers)
			{
				try
				{
					bool enabled = false;
					foreach (var s in Settings.EnabledNotifiers)
					{
						if (!n.GetType().AssemblyQualifiedName.Contains (s))
							continue;

						enabled = true;
					}

					this.notifiers.Items.Add (new ListViewItem (n.Name)
					{
						Tag = n,
						Checked = enabled
					});
				}
				catch
				{
				}
			}
		}

		private void btnOk_Click (object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Settings.ShowConnectOnStart = this.inConnectOnStart.Checked;
			Settings.DisplaySources = this.inDisplaySources.Checked;
			Settings.EnableGablarskiURLs = this.gablarskiURLs.Checked;

			DisableInput();
			Settings.InputProvider = (this.inInputProvider.SelectedItem != null) ? this.inInputProvider.SelectedItem.ToString () : String.Empty;
			Settings.InputSettings = this.inputSettings;
			Settings.UsePushToTalk = !this.voiceActivation.Checked;

			if (this.playbackSelector.Provider != null)
			{
				Settings.PlaybackProvider = this.playbackSelector.Provider.GetType().AssemblyQualifiedName;
				Settings.PlaybackDevice = this.playbackSelector.Device.Name;
			}
			else
			{
				Settings.PlaybackProvider = null;
				Settings.PlaybackDevice = null;
			}

			if (this.voiceSelector.Provider != null)
			{
				Settings.VoiceProvider = this.voiceSelector.Provider.GetType().AssemblyQualifiedName;
				Settings.VoiceDevice = this.voiceSelector.Device.Name;
			}
			else
			{
				Settings.VoiceProvider = null;
				Settings.VoiceDevice = null;
			}

			Settings.VoiceActivationContinueThreshold = this.threshold.Value * 100;
			Settings.VoiceActivationLevel = this.vadSensitivity.Value;

			Settings.EnableMediaVolumeControl = this.volumeControl.Checked;
			Settings.TalkingMusicVolume = this.talkingVolume.Value;
			Settings.NormalMusicVolume = this.normalVolume.Value;
			Settings.MediaVolumeControlIgnoresYou = this.musicIgnoreYou.Checked;

			List<string> enabledPlayers = new List<string> ();
			foreach (Type playerSupport in Modules.MediaPlayers.Select (m => m.GetType()))
			{
				foreach (string enabled in this.musicPlayers.CheckedItems.Cast<string> ())
				{
					if (!playerSupport.Name.Contains (enabled))
						continue;
					
					enabledPlayers.Add (playerSupport.FullName + ", " + playerSupport.Assembly.GetName().Name);
				}
			}
			Settings.EnabledMediaPlayerIntegrations = enabledPlayers;

			Settings.EnableNotifications = this.enableNotifications.Checked;
			List<string> enabledNotifiers = new List<string>();
			foreach (ListViewItem li in this.notifiers.CheckedItems.Cast<ListViewItem>().Where (li => li.Checked))
			{
				var t = li.Tag.GetType();
				enabledNotifiers.Add (t.FullName + ", " + t.Assembly.GetName().Name);
			}
			Settings.EnabledNotifiers = enabledNotifiers;

			Settings.SaveSettings();

			Close();
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
				this.linkSet.Text = "Set";
			}), nice);
		}

		private void inInputProvider_SelectedValueChanged(object sender, EventArgs e)
		{
			DisableInput();

			if (this.inInputProvider.SelectedItem == null)
				return;

			currentInputProvider = (IInputProvider)this.inInputProvider.SelectedItem;
			currentInputProvider.InputStateChanged += OnInputStateChanged;
			currentInputProvider.Attach (this.Handle, null);
		}

		private void DisableInput()
		{
			if (currentInputProvider == null)
				return;

			currentInputProvider.Detach();
			currentInputProvider.InputStateChanged -= OnInputStateChanged;
			//currentInputProvider.Dispose();
			//currentInputProvider = null;
		}

		private void linkSet_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (this.currentInputProvider == null)
				return;

			if (this.linkSet.Text == "Set")
			{
				this.dispInput.Text = "Press a key";
				this.linkSet.Text = "Cancel";
				this.currentInputProvider.BeginRecord();
			}
			else
			{
				this.dispInput.Text = currentInputProvider.GetNiceInputName (this.inputSettings);
				this.linkSet.Text = "Set";
				this.currentInputProvider.EndRecord();
			}
		}

		private void linkClear_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.dispInput.Text = String.Empty;
			this.inputSettings = null;
		}

		private void threshold_Scroll (object sender, EventArgs e)
		{
			this.dispThreshold.Text = String.Format ("{0:N1}s", (double)this.threshold.Value / 10);
		}

		private void SettingsForm_FormClosing (object sender, FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.OK)
				DisableInput();
		}
	}
}