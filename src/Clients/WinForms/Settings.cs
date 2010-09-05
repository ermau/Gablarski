using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cadenza.Collections;
using Gablarski.Clients.Input;
using Gablarski.Clients.Windows.Entities;
using Cadenza;

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
			get { return GetSetting ("VoiceDevice", String.Empty); }
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
			get { return GetSetting (PlaybackDeviceSettingName, String.Empty); }
			set
			{
				if (SetSetting (PlaybackDeviceSettingName, value))
					OnSettingsChanged (PlaybackDeviceSettingName);
			}
		}

		public const string TextToSpeechSettingName = "TextToSpeech";
		public static string TextToSpeech
		{
			get { return GetSetting (TextToSpeechSettingName, "Gablarski.SpeechNotifier.EventSpeech"); }
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
					).Split (';').Where (s => !s.IsNullOrWhitespace());
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

		public static void SaveSettings()
		{
			LoadSettings();

			lock (SettingLock)
			{
				foreach (var entry in settings.Values)
					Persistance.SaveOrUpdate (entry);

				settings = null;
				commandBindings.CollectionChanged -= OnCommandBindingsChanged;
				Persistance.DeleteAllBindings();
				foreach (var binding in CommandBindings)
					Persistance.Create (new CommandBindingEntry (binding));

				commandBindings = null;
			}
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

				settings = Persistance.GetSettings().ToDictionary (s => s.Name);
				commandBindings = new ObservableCollection<CommandBinding>();
				commandBindings.CollectionChanged += OnCommandBindingsChanged;
				foreach (var cbe in Persistance.GetCommandBindings())
				{
					IInputProvider provider = Modules.Input.FirstOrDefault (ip => ip.GetType().Name == cbe.ProviderType);
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