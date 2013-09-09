using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Cadenza.Collections;
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

			this.bindingViewModel = new BindingListViewModel (this.Handle, new AnonymousCommand (OnRecord, CanRecord));
			this.bindingViewModel.PropertyChanged += OnBindingPropertyChanged;
			this.bindingList.DataContext = this.bindingViewModel;
			if (this.bindingViewModel.InputProvider == null)
				this.addBinding.Enabled = false;

			IInputProvider sprovider = null;
			foreach (IInputProvider provider in Modules.Input)
			{
				if (provider.GetType().GetSimpleName() == Settings.InputProvider)
					sprovider = provider;

				this.inInputProvider.Items.Add (provider);
			}

			this.inInputProvider.SelectedItem = sprovider;

			this.playbackSelector.ProviderSource = Modules.Playback.Cast<IAudioDeviceProvider>();
			this.playbackSelector.SetProvider (Settings.PlaybackProvider);
			this.playbackSelector.SetDevice (Settings.PlaybackDevice);

			this.voiceSelector.ProviderSource = Modules.Capture.Cast<IAudioDeviceProvider>();
			this.voiceSelector.SetProvider (Settings.VoiceProvider);
			this.voiceSelector.SetDevice (Settings.VoiceDevice);

			this.playbackVolume.Value = (int)(Settings.GlobalVolume * 100);

			this.voiceActivation.Checked = !Settings.UsePushToTalk;
			this.threshold.Value = Settings.VoiceActivationContinueThreshold / 100;
			this.dispThreshold.Text = String.Format ("{0:N1}s", (double)this.threshold.Value / 10);
			this.vadSensitivity.Value = Settings.VoiceActivationLevel;

			this.inUseCurrentVolume.Checked = Settings.UseMusicCurrentVolume;
			this.volumeControl.Checked = Settings.EnableMediaVolumeControl;
			this.musicIgnoreYou.Checked = Settings.MediaVolumeControlIgnoresYou;
			this.talkingVolume.Value = Settings.TalkingMusicVolume;
			this.normalVolume.Value = Settings.NormalMusicVolume;

			foreach (IMediaPlayer player in Modules.MediaPlayers)
			{
				this.musicPlayers.Items.Add (player.Name, Settings.EnabledMediaPlayerIntegrations.Any (s => s == player.GetType().GetSimpleName()));
			}

			this.ignoreNotificationChanges = true;
			this.notifications = new MutableLookup<string, NotificationType> (Settings.EnabledNotifications);
			this.enableNotifications.Checked = Settings.EnableNotifications;
			foreach (var n in Modules.Notifiers.Cast<INamedComponent>().Concat (Modules.TextToSpeech.Cast<INamedComponent>()))
				this.notifiers.Items.Add (n, Settings.EnabledNotifications.Any (ig => ig.Key == n.GetType().GetSimpleName()));

			this.ignoreNotificationChanges = false;
		}

		private void OnBindingPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "InputProvider":
					this.addBinding.Enabled = (this.bindingViewModel.InputProvider != null);
					break;
			}
		}

		private void btnOk_Click (object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Settings.ShowConnectOnStart = this.inConnectOnStart.Checked;
			Settings.DisplaySources = this.inDisplaySources.Checked;
			Settings.EnableGablarskiURLs = this.gablarskiURLs.Checked;

			DisableInput();

			var inputProvider = this.inInputProvider.SelectedItem as IInputProvider;
			if (inputProvider != null)
			{
				Settings.InputProvider = inputProvider.GetType().GetSimpleName();
				Settings.CommandBindings.Clear();

				foreach (var b in this.bindingViewModel.Bindings)
					Settings.CommandBindings.Add (new CommandBinding (inputProvider, b.Command, b.Input));
			}
			else
			{
				Settings.InputProvider = String.Empty;
			}

			Settings.UsePushToTalk = !this.voiceActivation.Checked;

			if (this.playbackSelector.Provider != null)
			{
				Settings.PlaybackProvider = this.playbackSelector.Provider.GetType().GetSimpleName();
				Settings.PlaybackDevice = this.playbackSelector.Device.Name;
			}
			else
			{
				Settings.PlaybackProvider = null;
				Settings.PlaybackDevice = null;
			}

			if (this.voiceSelector.Provider != null)
			{
				Settings.VoiceProvider = this.voiceSelector.Provider.GetType().GetSimpleName();
				Settings.VoiceDevice = this.voiceSelector.Device.Name;
			}
			else
			{
				Settings.VoiceProvider = null;
				Settings.VoiceDevice = null;
			}

			Settings.GlobalVolume = this.playbackVolume.Value / (float)100;

			Settings.VoiceActivationContinueThreshold = this.threshold.Value * 100;
			Settings.VoiceActivationLevel = this.vadSensitivity.Value;

			Settings.EnableMediaVolumeControl = this.volumeControl.Checked;
			Settings.UseMusicCurrentVolume = this.inUseCurrentVolume.Checked;
			Settings.TalkingMusicVolume = this.talkingVolume.Value;
			Settings.NormalMusicVolume = this.normalVolume.Value;
			Settings.MediaVolumeControlIgnoresYou = this.musicIgnoreYou.Checked;

			List<string> enabledPlayers = new List<string> ();
			foreach (IMediaPlayer player in Modules.MediaPlayers)
			{
				foreach (string enabled in this.musicPlayers.CheckedItems.Cast<string> ())
				{
					if (player.Name != enabled)
						continue;
					
					enabledPlayers.Add (player.GetType().GetSimpleName());
				}
			}
			Settings.EnabledMediaPlayerIntegrations = enabledPlayers;

			Settings.EnableNotifications = this.enableNotifications.Checked;
			Settings.EnabledNotifications = this.notifications;

			Settings.Save();

			Close();
		}

		private MutableLookup<string, NotificationType> notifications;
		private BindingListViewModel bindingViewModel;
		private CommandBindingSettingEntry recordingEntry;
		private readonly object inputSync = new object();
		private bool ignoreNotificationChanges;

		private void inInputProvider_SelectedValueChanged(object sender, EventArgs e)
		{
			if (this.inInputProvider.SelectedItem == null)
			{
				this.addBinding.Enabled = false;
				return;
			}

			this.addBinding.Enabled = true;
			this.bindingViewModel.InputProvider = (IInputProvider)this.inInputProvider.SelectedItem;
		}

		private void DisableInput()
		{
			if (this.bindingViewModel != null)
				this.bindingViewModel.InputProvider = null;
		}

		private void threshold_Scroll (object sender, EventArgs e)
		{
			this.dispThreshold.Text = String.Format ("{0:N1}s", (double)this.threshold.Value / 10);
		}

		private void SettingsForm_FormClosing (object sender, FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
				return;

			DisableInput();
		}

		private void inUseCurrentVolume_CheckedChanged(object sender, EventArgs e)
		{
			this.normalVolume.Enabled = !this.inUseCurrentVolume.Checked;
		}

		private void OnRecord (object obj)
		{
			var entry = (obj as CommandBindingSettingEntry);
			if (entry == null)
				return;

			this.recordingEntry = entry;
			this.recordingEntry.Recording = true;
			((AnonymousCommand)this.bindingViewModel.RecordCommand).ChangeCanExecute();

			this.addBinding.Enabled = false;
			this.AcceptButton = null;
			this.CancelButton = null;
			this.bindingViewModel.InputProvider.NewRecording += OnNewRecording;
			this.bindingViewModel.InputProvider.BeginRecord();
		}

		private bool CanRecord (object arg)
		{
			var binding = (arg as CommandBindingSettingEntry);
			if (binding == null)
				return false;

			return !binding.Recording;
		}

		private void addBinding_LinkClicked (object sender, LinkLabelLinkClickedEventArgs e)
		{
			this.bindingViewModel.Bindings.Add (new CommandBindingSettingEntry (this.bindingViewModel.InputProvider));
		}

		private void OnNewRecording (object sender, RecordingEventArgs e)
		{
			if (this.recordingEntry == null)
				return;

			CommandBindingSettingEntry entry;
			lock (this.inputSync)
			{
				if (this.recordingEntry == null)
					return;

				e.Provider.NewRecording -= OnNewRecording;
				e.Provider.EndRecord();

				entry = this.recordingEntry;
				this.recordingEntry = null;

				entry.Input = e.RecordedInput;
				entry.ProviderType = e.Provider.GetType().Name;
				entry.Recording = false;
			}

			BeginInvoke ((Action<CommandBindingSettingEntry>)(be =>
			{
				((AnonymousCommand)this.bindingViewModel.RecordCommand).ChangeCanExecute();
				this.addBinding.Enabled = true;
				this.AcceptButton = this.btnOk;
				this.CancelButton = this.btnCancel;
			}), entry);
		}

		private void enabledNotifications_ItemCheck (object sender, ItemCheckEventArgs e)
		{
			string typeName = this.notifiers.SelectedItem.GetType().GetSimpleName();
			var type = (NotificationType)this.enabledNotifications.Items[e.Index];
			
			if (e.NewValue == CheckState.Unchecked)
				this.notifications.Remove (typeName, type);
			else if (!this.notifications[typeName].Contains (type))
				this.notifications.Add (typeName, type);
		}

		private void notifiers_ItemCheck (object sender, ItemCheckEventArgs e)
		{
			if (this.ignoreNotificationChanges)
				return;

			string typeName = this.notifiers.Items[e.Index].GetType().GetSimpleName();

			if (e.NewValue != CheckState.Checked)
				this.notifications.Remove (typeName);
			else if (!this.notifications.Contains (typeName))
				this.notifications.Add (typeName, (NotificationType[])Enum.GetValues (typeof (NotificationType)));

			notifiers_SelectedIndexChanged (this.notifiers, EventArgs.Empty);
		}

		private void notifiers_SelectedIndexChanged (object sender, EventArgs e)
		{
			this.enabledNotifications.Items.Clear();

			if (this.notifiers.SelectedItem == null)
				this.enabledNotifications.Enabled = false;
			else
			{
				this.enabledNotifications.Enabled = true;

				var enabled = new HashSet<NotificationType> (this.notifications[this.notifiers.SelectedItem.GetType().GetSimpleName()]);
				foreach (NotificationType type in Enum.GetValues (typeof(NotificationType)))
					this.enabledNotifications.Items.Add (type, enabled.Contains (type));
			}
		}
	}
}