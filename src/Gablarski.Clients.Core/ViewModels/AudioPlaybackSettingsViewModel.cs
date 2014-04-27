//
// AudioPlaybackSettingsViewModel.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Audio;

namespace Gablarski.Clients.ViewModels
{
	public class AudioPlaybackSettingsViewModel
		: BusyViewModel
	{
		public AudioPlaybackSettingsViewModel()
		{
			IsBusy = true;

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

			var getSelectedProvider = Modules.GetImplementerOrDefaultAsync<IAudioPlaybackProvider> (Settings.PlaybackProvider);
			Modules.GetImplementersAsync<IAudioPlaybackProvider>().ContinueWith (t => {
				PlaybackProviders = t.Result;
				getSelectedProvider.ContinueWith (ts => {
					CurrentPlaybackProvider = ts.Result;

					if (Settings.PlaybackDevice != null) {
						var setDevice = PlaybackDevices.FirstOrDefault (d => d.Device.Name == Settings.PlaybackDevice);
						if (setDevice != null)
							CurrentPlaybackDevice = setDevice;
					}

					IsBusy = false;
				});
			}, scheduler);

			Volume = Settings.GlobalVolume * 100;
		}

		private double volume;
		public double Volume
		{
			get { return this.volume; }
			set
			{
				if (this.volume == value)
					return;

				this.volume = value;
				OnPropertyChanged();
			}
		}

		public IEnumerable<IAudioPlaybackProvider> PlaybackProviders
		{
			get { return this.playbackProviders; }
			private set
			{
				if (this.playbackProviders == value)
					return;

				this.playbackProviders = value;
				OnPropertyChanged();
			}
		}

		private IAudioPlaybackProvider currentPlaybackProvider;
		public IAudioPlaybackProvider CurrentPlaybackProvider
		{
			get { return this.currentPlaybackProvider; }
			set
			{
				if (this.currentPlaybackProvider == value)
					return;

				this.currentPlaybackProvider = value; 
				OnPropertyChanged();

				if (value != null) {
					PlaybackDevices = new[] { new DefaultDevice() }
						.Concat (value.GetDevices())
						.Select (d => new DeviceViewModel (value, d))
						.ToArray();

					CurrentPlaybackDevice = PlaybackDevices.First();
				} else {
					PlaybackDevices = Enumerable.Empty<DeviceViewModel>();
					CurrentPlaybackDevice = null;
				}
			}
		}

		private IEnumerable<DeviceViewModel> playbackDevices;
		public IEnumerable<DeviceViewModel> PlaybackDevices
		{
			get { return this.playbackDevices; }
			set
			{
				if (this.playbackDevices == value)
					return;

				this.playbackDevices = value;
				OnPropertyChanged();
			}
		}

		private DeviceViewModel currentPlaybackDevice;
		private IEnumerable<IAudioPlaybackProvider> playbackProviders;

		public DeviceViewModel CurrentPlaybackDevice
		{
			get { return this.currentPlaybackDevice; }
			set
			{
				if (this.currentPlaybackDevice == value)
					return;

				if (value == null && CurrentPlaybackProvider != null)
					value = PlaybackDevices.FirstOrDefault();

				this.currentPlaybackDevice = value;
				OnPropertyChanged();
			}
		}

		public void UpdateSettings()
		{
			Settings.PlaybackProvider = (CurrentPlaybackProvider != null)
				? CurrentPlaybackProvider.GetType().GetSimpleName()
				: null;

			Settings.PlaybackDevice = (CurrentPlaybackDevice != null)
				? CurrentPlaybackDevice.Device.Name
				: null;

			Settings.GlobalVolume = (float)Volume / 100f;
		}
	}
}
