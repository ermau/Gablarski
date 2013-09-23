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
			set
			{
				if (SetSetting ("FirstRun", value))
					OnSettingsChanged ("FirstRun");
			}
		}

		public const string VersionName = "Version";
		public static string Version
		{
			get { return GetSetting (VersionName, null); }
			set
			{
				if (SetSetting (VersionName, value))
					OnSettingsChanged (VersionName);
			}
		}

		public const string NicknameName = "Nickname";
		public static string Nickname
		{
			get { return GetSetting (NicknameName, null); }
			set
			{
				if (SetSetting (NicknameName, value))
					OnSettingsChanged (NicknameName);
			}
		}

		public const string AvatarName = "AvatarUrl";
		public static string Avatar
		{
			get { return GetSetting (AvatarName, null); }
			set
			{
				if (SetSetting (AvatarName, value))
					OnSettingsChanged (AvatarName);
			}
		}

		public const string GlobalVolumeName = "GlobalVolume";
		public static float GlobalVolume
		{
			get { return GetSetting (GlobalVolumeName, 1.0f); }
			set
			{
				if (SetSetting (GlobalVolumeName, value))
					OnSettingsChanged (GlobalVolumeName);
			}
		}

		public const string UseMusicCurrentVolumeName = "UseMusicCurrentVolume";
		public static bool UseMusicCurrentVolume
		{
			get { return GetSetting (UseMusicCurrentVolumeName, true); }
			set
			{
				if (SetSetting (UseMusicCurrentVolumeName, value))
					OnSettingsChanged (UseMusicCurrentVolumeName);
			}
		}

		public const string UsePushToTalkSettingName = "UsePushToTalk";
		public static bool UsePushToTalk
		{
			get { return GetSetting (UsePushToTalkSettingName, true); }
			set
			{
				if (SetSetting (UsePushToTalkSettingName, value))
					OnSettingsChanged (UsePushToTalkSettingName);
			}
		}

		public const string VoiceActivationLevelSettingName = "VoiceActivationLevel";
		public static int VoiceActivationLevel
		{
			get { return GetSetting (VoiceActivationLevelSettingName, 4000); }
			set
			{
				if (SetSetting (VoiceActivationLevelSettingName, value))
					OnSettingsChanged (VoiceActivationLevelSettingName);
			}
		}

		public const string VoiceActivationContinueThresholdSettingName = "VoiceActivationContinueThreshold";
		public static int VoiceActivationContinueThreshold
		{
			get { return GetSetting (VoiceActivationContinueThresholdSettingName, 600); }
			set
			{
				if (SetSetting (VoiceActivationContinueThresholdSettingName, value))
					OnSettingsChanged (VoiceActivationContinueThresholdSettingName);
			}
		}

		public static string VoiceProvider
		{
			get { return GetSetting ("VoiceProvider", "Gablarski.OpenAL.Providers.OpenALCaptureProvider, Gablarski.OpenAL"); }
			set
			{
				if (SetSetting ("VoiceProvider", value))
					OnSettingsChanged ("VoiceProvider");
			}
		}

		public static string VoiceDevice
		{
			get { return GetSetting ("VoiceDevice", "Default"); }
			set
			{
				if (SetSetting ("VoiceDevice", value))
					OnSettingsChanged ("VoiceDevice");
			}
		}

		public const string PlaybackProviderSettingName = "PlaybackProvider";
		public static string PlaybackProvider
		{
			get { return GetSetting (PlaybackProviderSettingName, "Gablarski.OpenAL.Providers.OpenALPlaybackProvider, Gablarski.OpenAL"); }
			set
			{
				if (SetSetting (PlaybackProviderSettingName, value))
					OnSettingsChanged (PlaybackProviderSettingName);
			}
		}

		public const string PlaybackDeviceSettingName = "PlaybackDevice";
		public static string PlaybackDevice
		{
			get { return GetSetting (PlaybackDeviceSettingName, "Default"); }
			set
			{
				if (SetSetting (PlaybackDeviceSettingName, value))
					OnSettingsChanged (PlaybackDeviceSettingName);
			}
		}

		public const string TextToSpeechSettingName = "TextToSpeech";
		public static string TextToSpeech
		{
			get { return GetSetting (TextToSpeechSettingName, "Gablarski.SpeechNotifier.EventSpeech, Gablarski.SpeechNotifier"); }
			set
			{
				if (SetSetting (TextToSpeechSettingName, value))
					OnSettingsChanged (TextToSpeechSettingName);
			}
		}

		public static string InputProvider
		{
			get { return GetSetting ("InputProvider", "Gablarski.Input.DirectInput.DirectInputProvider, Gablarski.Input.DirectInput"); }
			set
			{
				if (SetSetting ("InputProvider", value))
					OnSettingsChanged ("InputProvider");
			}
		}

		public static ICollection<CommandBinding> CommandBindings
		{
			get { return commandBindings; }
		}

		public static bool DisplaySources
		{
			get { return GetSetting ("DisplaySources", false); }
			set
			{
				if (SetSetting ("DisplaySources", value))
					OnSettingsChanged ("DisplaySources");
			}
		}

		public static bool ShowConnectOnStart
		{
			get { return GetSetting ("ShowConnectOnStart", true); }
			set
			{
				if (SetSetting ("ShowConnectOnStart", value))
					OnSettingsChanged ("ShowConnectOnStart");
			}
		}

		public const string EnableNotificationsSettingName = "EnableNotifications";
		public static bool EnableNotifications
		{
			get { return GetSetting (EnableNotificationsSettingName, true); }
			set
			{
				if (SetSetting (EnableNotificationsSettingName, value))
					OnSettingsChanged (EnableNotificationsSettingName);
			}
		}

		public const string EnabledNotificationsSettingName = "EnabledNotifications";
		public static ILookup<string, NotificationType> EnabledNotifications
		{
			get
			{
				string data = GetSetting (EnabledNotificationsSettingName, 
					"Gablarski.Growl.GrowlNotifier, Gablarski.Growl:*;Gablarski.SpeechNotifier.EventSpeech, Gablarski.SpeechNotifier:*;");

				var notifications = new	MutableLookup<string, NotificationType>();

				string[] notifiers = data.Split (';');
				for (int i = 0; i < notifiers.Length; ++i)
				{
					if (notifiers[i].IsNullOrWhitespace())
						continue;

					string[] parts = notifiers[i].Split (':');
					if (parts[1] == "*")
					{
						foreach (NotificationType t in Enum.GetValues (typeof (NotificationType)))
							notifications.Add (parts[0], t);
					}
					else
					{
						string[] types = parts[1].Split (',');
						for (int t = 0; t < types.Length; ++t)
							notifications.Add (parts[0], (NotificationType)Enum.Parse (typeof(NotificationType), types[t]));
					}
				}

				return notifications;
			}

			set
			{
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

				if (SetSetting (EnabledNotificationsSettingName, data.ToString()))
					OnSettingsChanged (EnabledNotificationsSettingName);
			}
		}

		public const string EnabledMediaPlayerIntegrationsSettingName = "EnabledMediaPlayerIntegrations";
		public static IEnumerable<string> EnabledMediaPlayerIntegrations
		{
			get
			{
				return GetSetting (EnabledMediaPlayerIntegrationsSettingName,
					"Gablarski.iTunes.iTunesIntegration, Gablarski.iTunes;Gablarski.Winamp.WinampIntegration, Gablarski.Winamp"
					).Split (';').Where (s => !s.IsNullOrWhitespace());
			}

			set
			{
				if (SetSetting (EnabledMediaPlayerIntegrationsSettingName, value.Implode (";")))
					OnSettingsChanged (EnabledMediaPlayerIntegrationsSettingName);
			}
		}

		public const string EnableMediaVolumeControlSettingName = "EnableMediaVolumeControl";
		public static bool EnableMediaVolumeControl
		{
			get { return GetSetting (EnableMediaVolumeControlSettingName, true); }
			set
			{
				if (SetSetting (EnableMediaVolumeControlSettingName, value))
					OnSettingsChanged (EnableMediaVolumeControlSettingName);
			}
		}

		public const string MediaVolumeControlIgnoresYouSettingName = "MediaVolumeControlIgnoresYou";
		public static bool MediaVolumeControlIgnoresYou
		{
			get { return GetSetting (MediaVolumeControlIgnoresYouSettingName, true); }
			set
			{
				if (SetSetting (MediaVolumeControlIgnoresYouSettingName, value))
					OnSettingsChanged (MediaVolumeControlIgnoresYouSettingName);
			}
		}

		public const string TalkingMusicVolumeSettingName = "TalkingMusicVolume";
		public static int TalkingMusicVolume
		{
			get { return GetSetting (TalkingMusicVolumeSettingName, 30); }
			set
			{
				if (SetSetting (TalkingMusicVolumeSettingName, value))
					OnSettingsChanged (TalkingMusicVolumeSettingName);
			}
		}

		public const string NormalMusicVolumeSettingName = "NormalMusicVolume";
		public static int NormalMusicVolume
		{
			get { return GetSetting (NormalMusicVolumeSettingName, 100); }
			set
			{
				if (SetSetting (NormalMusicVolumeSettingName, value))
					OnSettingsChanged (NormalMusicVolumeSettingName);
			}
		}

		public const string EnableGablarskiURLsSettingName = "EnableGablarskiURLs";
		public static bool EnableGablarskiURLs
		{
			get { return GetSetting (EnableGablarskiURLsSettingName, true); }
			set
			{
				if (SetSetting (EnableGablarskiURLsSettingName, value))
					OnSettingsChanged (EnableGablarskiURLsSettingName);
			}
		}

		public static void Save()
		{
			LoadSettings();

			lock (SettingLock)
			{
				foreach (var entry in settings.Values)
					ClientData.SaveOrUpdate (entry);

				settings = null;
				if (commandBindings != null)
				{
					commandBindings.CollectionChanged -= OnCommandBindingsChanged;

					ClientData.DeleteAllBindings();
					foreach (var binding in CommandBindings)
						ClientData.Create (new CommandBindingEntry (binding));

					commandBindings = null;
				}
			}
		}

		public static Task SaveAsync()
		{
			return Task.Run ((Action)Save);
		}

		public static void Clear()
		{
			lock (SettingLock)
				settings = null;
		}

		private readonly static object SettingLock = new object();

		private static ObservableCollection<CommandBinding> commandBindings;
		private static Dictionary<string, SettingEntry> settings;

		private static void LoadSettings ()
		{
			
			lock (SettingLock)
			{
				if (settings != null)
					return;

				var settingsEntries = ClientData.GetSettings();
				settings = settingsEntries.ToDictionary (s => s.Name);

				if (ClientData.CheckForUpdates())
					settings = ClientData.GetSettings().ToDictionary (s => s.Name);

				commandBindings = new ObservableCollection<CommandBinding>();
				commandBindings.CollectionChanged += OnCommandBindingsChanged;
				foreach (var cbe in ClientData.GetCommandBindings())
				{
					IInputProvider provider = Enumerable.FirstOrDefault<IInputProvider> (Modules.Input, ip => Extensions.GetSimpleName(ip.GetType()) == cbe.ProviderType);
					if (provider == null)
						continue;

					commandBindings.Add (new CommandBinding (provider, cbe.Command, cbe.Input));
				}
			}
		}

		private static void OnCommandBindingsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			OnSettingsChanged ("CommandBindings");
		}

		private static string GetSetting (string settingName, string defaultValue)
		{
			LoadSettings();

			lock (SettingLock)
			{
				if (!settings.ContainsKey (settingName))
					settings[settingName] = new SettingEntry (0) { Name = settingName, Value = defaultValue };

				return settings[settingName].Value;
			}
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

		private static bool SetSetting (string settingName, string value)
		{
			LoadSettings();

			lock (SettingLock)
			{
				SettingEntry entry;
				if (settings.TryGetValue (settingName, out entry))
				{
					if (entry.Value != value)
					{
						entry.Value = value;
						return true;
					}
				}
				else
				{
					settings[settingName] = new SettingEntry (0) { Name = settingName, Value = value };
					return true;
				}
			}

			return false;
		}

		private static bool SetSetting (string settingName, bool value)
		{
			return SetSetting (settingName, (value) ? "1" : "0");
		}

		private static bool SetSetting<T> (string settingName, T value)
		{
			return SetSetting (settingName, value.ToString());
		}

		private static void OnSettingsChanged (string propertyName)
		{
			var changed = SettingChanged;
			if (changed != null)
				changed (null, new PropertyChangedEventArgs (propertyName));
		}
	}
}