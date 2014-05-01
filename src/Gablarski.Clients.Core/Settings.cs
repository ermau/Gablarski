//
// Settings.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2009-2011, Eric Maupin
// Copyright (c) 2011-2014, Xamarin Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cadenza.Collections;
using Gablarski.Clients.Input;
using Gablarski.Clients.Persistence;
using Cadenza;

namespace Gablarski.Clients
{
	public static class Settings
	{
		public static event PropertyChangedEventHandler SettingChanged;

		public static bool FirstRun
		{
			get { return GetSetting ("FirstRun", true); }
			set { SetSetting ("FirstRun", value); }
		}

		public const string VersionName = "Version";
		public static string Version
		{
			get { return GetSetting (VersionName, null); }
			set { SetSetting (VersionName, value); }
		}

		public const string NicknameName = "Nickname";
		public static string Nickname
		{
			get { return GetSetting (NicknameName, null); }
			set { SetSetting (NicknameName, value); }
		}

		public const string AvatarName = "AvatarUrl";
		public static string Avatar
		{
			get { return GetSetting (AvatarName, null); }
			set { SetSetting (AvatarName, value); }
		}

		public const string GlobalVolumeName = "GlobalVolume";
		public static float GlobalVolume
		{
			get { return GetSetting (GlobalVolumeName, 1.0f); }
			set { SetSetting (GlobalVolumeName, value); }
		}

		public const string UseMusicCurrentVolumeName = "UseMusicCurrentVolume";
		public static bool UseMusicCurrentVolume
		{
			get { return GetSetting (UseMusicCurrentVolumeName, true); }
			set { SetSetting (UseMusicCurrentVolumeName, value); }
		}

		public const string UsePushToTalkSettingName = "UsePushToTalk";
		public static bool UsePushToTalk
		{
			get { return GetSetting (UsePushToTalkSettingName, true); }
			set { SetSetting (UsePushToTalkSettingName, value); }
		}

		public const string VoiceActivationLevelSettingName = "VoiceActivationLevel";
		public static int VoiceActivationLevel
		{
			get { return GetSetting (VoiceActivationLevelSettingName, 4000); }
			set { SetSetting (VoiceActivationLevelSettingName, value); }
		}

		public const string VoiceActivationContinueThresholdSettingName = "VoiceActivationContinueThreshold";
		public static int VoiceActivationContinueThreshold
		{
			get { return GetSetting (VoiceActivationContinueThresholdSettingName, 600); }
			set { SetSetting (VoiceActivationContinueThresholdSettingName, value); }
		}

		public static string VoiceProvider
		{
			get { return GetSetting ("VoiceProvider", "Gablarski.OpenAL.Providers.OpenALCaptureProvider, Gablarski.OpenAL"); }
			set { SetSetting ("VoiceProvider", value); }
		}

		public static string VoiceDevice
		{
			get { return GetSetting ("VoiceDevice", "Default"); }
			set { SetSetting ("VoiceDevice", value); }
		}

		public const string PlaybackProviderSettingName = "PlaybackProvider";
		public static string PlaybackProvider
		{
			get { return GetSetting (PlaybackProviderSettingName, "Gablarski.OpenAL.Providers.OpenALPlaybackProvider, Gablarski.OpenAL"); }
			set { SetSetting (PlaybackProviderSettingName, value); }
		}

		public const string PlaybackDeviceSettingName = "PlaybackDevice";
		public static string PlaybackDevice
		{
			get { return GetSetting (PlaybackDeviceSettingName, "Default"); }
			set { SetSetting (PlaybackDeviceSettingName, value); }
		}

		public const string TextToSpeechSettingName = "TextToSpeech";
		public static string TextToSpeech
		{
			get { return GetSetting (TextToSpeechSettingName, "Gablarski.SpeechNotifier.EventSpeech, Gablarski.SpeechNotifier"); }
			set { SetSetting (TextToSpeechSettingName, value); }
		}

		public const string InputProviderSettingName = "InputProvider";
		public static string InputProvider
		{
			get { return GetSetting ("InputProvider", "Gablarski.Input.DirectInput.DirectInputProvider, Gablarski.Input.DirectInput"); }
			set { SetSetting ("InputProvider", value); }
		}

		public static ICollection<CommandBinding> CommandBindings
		{
			get { return commandBindings; }
		}

		public static bool DisplaySources
		{
			get { return GetSetting ("DisplaySources", false); }
			set { SetSetting ("DisplaySources", value); }
		}

		public static bool ShowConnectOnStart
		{
			get { return GetSetting ("ShowConnectOnStart", true); }
			set { SetSetting ("ShowConnectOnStart", value); }
		}

		public const string EnableNotificationsSettingName = "EnableNotifications";
		public static bool EnableNotifications
		{
			get { return GetSetting (EnableNotificationsSettingName, true); }
			set { SetSetting (EnableNotificationsSettingName, value); }
		}

		public const string EnabledNotificationsSettingName = "EnabledNotifications";

		private static string lastNotificationValue;
		private static MutableLookup<string, NotificationType> notifications;

		public static ILookup<string, NotificationType> EnabledNotifications
		{
			get
			{
				string data = GetSetting (EnabledNotificationsSettingName, 
					"Gablarski.Growl.GrowlNotifier, Gablarski.Growl:*;Gablarski.SpeechNotifier.EventSpeech, Gablarski.SpeechNotifier:*;");

				if (data != lastNotificationValue) {
					notifications = new MutableLookup<string, NotificationType>();

					string[] notifiers = data.Split (';');
					for (int i = 0; i < notifiers.Length; ++i) {
						if (notifiers[i].IsNullOrWhitespace())
							continue;

						string[] parts = notifiers[i].Split (':');
						if (parts[1] == "*") {
							foreach (NotificationType t in Enum.GetValues (typeof (NotificationType)))
								notifications.Add (parts[0], t);
						} else {
							string[] types = parts[1].Split (',');
							for (int t = 0; t < types.Length; ++t)
								notifications.Add (parts[0], (NotificationType) Enum.Parse (typeof (NotificationType), types[t]));
						}
					}
				}

				return notifications;
			}

			set
			{
				if (value == null)
					throw new ArgumentNullException ("value");

				var types = Enum.GetValues (typeof (NotificationType));

				StringBuilder data = new StringBuilder();
				foreach (var group in value)
				{
					data.Append (group.Key);
					data.Append (":");

					if (group.Count() == types.Length)
						data.Append ("*");
					else
					{
						bool first = true;
						foreach (NotificationType t in group)
						{
							if (first)
								first = false;
							else
								data.Append (",");

							data.Append (t);
						}
					}

					data.Append (";");
				}

				lastNotificationValue = data.ToString();
				notifications = new MutableLookup<string, NotificationType> (value);

				SetSetting (EnabledNotificationsSettingName, data.ToString());
			}
		}

		private const string DefaultMediaPlayers = "Gablarski.iTunes.iTunesIntegration, Gablarski.iTunes;Gablarski.Winamp.WinampIntegration, Gablarski.Winamp";
		public const string EnabledMediaPlayerIntegrationsSettingName = "EnabledMediaPlayerIntegrations";
		public static IEnumerable<string> EnabledMediaPlayerIntegrations
		{
			get
			{
				return GetSetting (EnabledMediaPlayerIntegrationsSettingName, DefaultMediaPlayers)
					.Split (';').Where (s => !s.IsNullOrWhitespace());
			}

			set { SetSetting (EnabledMediaPlayerIntegrationsSettingName, value.Implode (";")); }
		}

		public const string EnableMediaVolumeControlSettingName = "EnableMediaVolumeControl";
		public static bool EnableMediaVolumeControl
		{
			get { return GetSetting (EnableMediaVolumeControlSettingName, true); }
			set { SetSetting (EnableMediaVolumeControlSettingName, value); }
		}

		public const string MediaVolumeControlIgnoresYouSettingName = "MediaVolumeControlIgnoresYou";
		public static bool MediaVolumeControlIgnoresYou
		{
			get { return GetSetting (MediaVolumeControlIgnoresYouSettingName, true); }
			set { SetSetting (MediaVolumeControlIgnoresYouSettingName, value); }
		}

		public const string TalkingMusicVolumeSettingName = "TalkingMusicVolume";
		public static int TalkingMusicVolume
		{
			get { return GetSetting (TalkingMusicVolumeSettingName, 30); }
			set { SetSetting (TalkingMusicVolumeSettingName, value); }
		}

		public const string NormalMusicVolumeSettingName = "NormalMusicVolume";
		public static int NormalMusicVolume
		{
			get { return GetSetting (NormalMusicVolumeSettingName, 100); }
			set { SetSetting (NormalMusicVolumeSettingName, value); }
		}

		public const string EnableGablarskiUrlsSettingName = "EnableGablarskiURLs";
		public static bool EnableGablarskiUrls
		{
			get { return GetSetting (EnableGablarskiUrlsSettingName, true); }
			set { SetSetting (EnableGablarskiUrlsSettingName, value); }
		}

		public static IReadOnlyDictionary<string, string> CurrentSettings
		{
			get { return settings.ToDictionary(); }
		}

		public static async Task LoadAsync()
		{
			if (settings == null)
				throw new InvalidOperationException ("Attempting to load settings without set provider");

			await settings.LoadAsync().ConfigureAwait (false);

			commandBindings = new ObservableCollection<CommandBinding>();
			commandBindings.CollectionChanged += OnCommandBindingsChanged;

			foreach (var cbe in ClientData.GetCommandBindings()) {
				IInputProvider provider = await Modules.GetImplementerAsync<IInputProvider> (cbe.ProviderType).ConfigureAwait (false);
				if (provider == null)
					continue;

				commandBindings.Add (new CommandBinding (provider, cbe.Command, cbe.Input));
			}
		}

		public static Task LoadAsync (ISettingsProvider settingsProvider)
		{
			if (settingsProvider == null)
				throw new ArgumentNullException ("settingsProvider");

			if (settings != null) {
				settings.PropertyChanged -= OnSettingsPropertyChanged;
			}

			settings = settingsProvider;
			settings.PropertyChanged += OnSettingsPropertyChanged;
			
			return LoadAsync();
		}

		private static void OnSettingsPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			OnSettingsChanged (e.PropertyName);
		}

		public static async Task SaveAsync()
		{
			if (settings == null)
				return;

			lock (settings) {
				if (commandBindings != null) {
					commandBindings.CollectionChanged -= OnCommandBindingsChanged;

					ClientData.DeleteAllBindings();
					foreach (var binding in CommandBindings)
						ClientData.Create (new CommandBindingEntry (binding));

					commandBindings = null;
				}
			}

			await settings.SaveAsync().ConfigureAwait (false);
			await LoadAsync().ConfigureAwait (false);
		}

		private static ObservableCollection<CommandBinding> commandBindings;
		private static ISettingsProvider settings;

		private static void OnCommandBindingsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			OnSettingsChanged ("CommandBindings");
		}

		private static string GetSetting (string settingName, string defaultValue)
		{
			return settings.GetValue (settingName, defaultValue);
		}

		private static bool GetSetting (string settingName, bool defaultValue)
		{
			return (GetSetting (settingName, (defaultValue) ? "1" : "0") == "1");
		}

		private static int GetSetting (string settingName, int defaultValue)
		{
			return Int32.Parse (GetSetting (settingName, defaultValue.ToString()));
		}

		private static float GetSetting (string settingName, float defaultValue)
		{
			return Single.Parse (GetSetting (settingName, defaultValue.ToString()));
		}

		private static void SetSetting (string settingName, string value)
		{
			settings.SetValue (settingName, value);
		}

		private static void SetSetting (string settingName, bool value)
		{
			SetSetting (settingName, (value) ? "1" : "0");
		}

		private static void SetSetting<T> (string settingName, T value)
		{
			SetSetting (settingName, value.ToString());
		}

		private static void OnSettingsChanged (string propertyName)
		{
			var changed = SettingChanged;
			if (changed != null)
				changed (null, new PropertyChangedEventArgs (propertyName));
		}
	}
}