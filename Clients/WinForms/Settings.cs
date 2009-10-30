using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Clients.Windows.Entities;
using Mono.Rocks;

namespace Gablarski.Clients.Windows
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
			get { return GetSetting (VoiceActivationLevelSettingName, 2200); }
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
			get { return GetSetting ("VoiceProvider", "Gablarski.Audio.OpenAL.Providers.OpenALCaptureProvider, Gablarski"); }
			set
			{
				if (SetSetting ("VoiceProvider", value))
					OnSettingsChanged ("VoiceProvider");
			}
		}

		public static string VoiceDevice
		{
			get { return GetSetting ("VoiceDevice", String.Empty); }
			set
			{
				if (SetSetting ("VoiceDevice", value))
					OnSettingsChanged ("VoiceDevice");
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

		public static string InputSettings
		{
			get { return GetSetting ("InputSettings", "k45"); }
			set
			{
				if (SetSetting ("InputSettings", value))
					OnSettingsChanged ("InputSettings");
			}
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

		public const string EnableNotificationsSettingName = "EnabledNotifications";
		public static bool EnableNotifications
		{
			get { return GetSetting (EnableNotificationsSettingName, true); }
			set
			{
				if (SetSetting (EnableNotificationsSettingName, value))
					OnSettingsChanged (EnableNotificationsSettingName);
			}
		}

		public const string EnabledNotifiersSettingName = "EnabledNotifiers";
		public static IEnumerable<string> EnabledNotifiers
		{
			get
			{
				return GetSetting (EnabledNotifiersSettingName,
					"Gablarski.Growl.GrowlNotifier, Gablarski.Growl;Gablarski.SpeechNotifier.EventSpeech, Gablarski.SpeechNotifier"
					).Split (';').Where (s => !s.IsEmpty());
			}

			set
			{
				if (SetSetting (EnabledNotifiersSettingName, value.Implode (";")))
					OnSettingsChanged (EnabledNotifiersSettingName);
			}
		}

		public const string EnabledMediaPlayerIntegrationsSettingName = "EnabledMediaPlayerIntegrations";
		public static IEnumerable<string> EnabledMediaPlayerIntegrations
		{
			get
			{
				return GetSetting (EnabledMediaPlayerIntegrationsSettingName,
					"Gablarski.iTunes.iTunesIntegration, Gablarski.iTunes;Gablarski.Winamp.WinampIntegration, Gablarski.Winamp"
					).Split (';').Where (s => !s.IsEmpty());
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

		public static void SaveSettings()
		{
			LoadSettings();

			lock (SettingLock)
			{
				using (var trans = Persistance.CurrentSession.BeginTransaction())
				{
					foreach (var entry in settings.Values)
						Persistance.CurrentSession.SaveOrUpdate (entry);

					trans.Commit();
				}
			}
		}

		private readonly static object SettingLock = new object();

		private static Dictionary<string, SettingEntry> settings;
		private static void LoadSettings ()
		{
			lock (SettingLock)
			{
				if (settings != null)
					return;

				settings = Persistance.CurrentSession.CreateQuery ("from SettingEntry").Enumerable().Cast<SettingEntry>().ToDictionary (e => e.Name);
			}
		}

		private static string GetSetting (string settingName, string defaultValue)
		{
			LoadSettings();

			lock (SettingLock)
			{
				if (!settings.ContainsKey (settingName))
					settings[settingName] = new SettingEntry { Name = settingName, Value = defaultValue };

				return settings[settingName].Value;
			}
		}

		private static bool GetSetting (string settingName, bool defaultValue)
		{
			return (GetSetting (settingName, (defaultValue) ? "1" : "0") == "1");
		}

		private static int GetSetting (string settingName, int defautlValue)
		{
			return Int32.Parse (GetSetting (settingName, defautlValue.ToString()));
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
					settings[settingName] = new SettingEntry { Name = settingName, Value = value };
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