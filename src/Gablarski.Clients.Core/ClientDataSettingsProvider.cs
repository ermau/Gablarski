//
// ClientDataSettingsProvider.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2013-2014, Xamarin Inc.
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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Clients.Persistence;

namespace Gablarski.Clients
{
	public sealed class ClientDataSettingsProvider
		: ISettingsProvider
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public async Task LoadAsync()
		{
			var settingEntries = await ClientData.GetSettingsAsync().ConfigureAwait (false);
			this.settings = settingEntries.ToDictionary (s => s.Name);
		}

		public Task SaveAsync()
		{
			return Task.Run (() => {
				if (this.settings == null)
					return;

				lock (this.settingLock) {
					foreach (SettingEntry entry in this.settings.Values)
						ClientData.SaveOrUpdate (entry);
				}
			});
		}

		public void Unload()
		{
			this.settings = null;
		}

		public string GetValue (string key)
		{
			return GetValue (key, null);
		}

		public string GetValue (string key, string defaultValue)
		{
			EnsureLoaded();

			string value;
			lock (this.settingLock) {
				if (!settings.ContainsKey (key))
					settings[key] = new SettingEntry (0) { Name = key, Value = defaultValue };

				value = this.settings[key].Value;
			}

			return value;
		}

		public void SetValue (string key, string value)
		{
			lock (this.settings) {
				SettingEntry entry;
				if (this.settings.TryGetValue (key, out entry)) {
					if (entry.Value != value) {
						entry.Value = value;
						OnPropertyChanged (key);
					}
				} else {
					this.settings[key] = new SettingEntry (0) { Name = key, Value = value };
					OnPropertyChanged (key);
				}
			}
		}

		public IReadOnlyDictionary<string, string> ToDictionary()
		{
			lock (this.settingLock)
				return new ReadOnlyDictionary<string, string> (this.settings.ToDictionary (kvp => kvp.Key, kvp => kvp.Value.Value));
		}

		private readonly object settingLock = new object();

		private Dictionary<string, SettingEntry> settings;

		private void EnsureLoaded()
		{
			if (this.settings == null)
				throw new InvalidOperationException ("You must call LoadAsync before accessing settings");
		}

		private void OnPropertyChanged (string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null)
				handler (this, new PropertyChangedEventArgs (propertyName));
		}
	}
}
