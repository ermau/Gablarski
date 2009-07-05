using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gablarski.Clients.Windows.Entities;

namespace Gablarski.Clients.Windows
{
	public static class Settings
	{
		public static event PropertyChangedEventHandler SettingChanged;

		public static bool UsePushToTalk
		{
			get { return true; }
		}

		public static PushToTalk PushToTalk
		{
			get { return PushToTalk.Parse (GetSetting ("PushToTalk", "k" + 160)); }
			set
			{
				SetSetting ("PushToTalk", value);
				OnSettingsChanged ("PushToTalk");
			}
		}

		public static void SaveSettings()
		{
			LoadSettings();

			lock (SettingLock)
			{
				foreach (var entry in settings.Values)
					Persistance.CurrentSession.SaveOrUpdate (entry);
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

		private static void SetSetting (string settingName, string value)
		{
			LoadSettings();

			lock (SettingLock)
			{
				if (settings.ContainsKey (settingName))
					settings[settingName].Value = value;
				else
					settings[settingName] = new SettingEntry { Name = settingName, Value = value };
			}
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