﻿// Copyright (c) 2013, Eric Maupin
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
using Gablarski.Clients.Media;

namespace Gablarski.Clients.ViewModels
{
	public class MusicSettingsViewModel
		: ViewModelBase
	{
		public MusicSettingsViewModel()
		{
			MediaPlayers = Modules.MediaPlayers.Select (
					p => new MediaPlayerViewModel (p, Settings.EnabledMediaPlayerIntegrations.Contains (p.GetType().GetSimpleName())))
					.ToArray();

			EnableVolumeControl = Settings.EnableMediaVolumeControl;
			IgnoreYourSources = Settings.MediaVolumeControlIgnoresYou;
			TalkingVolume = Settings.TalkingMusicVolume;
			NormalVolume = Settings.NormalMusicVolume;
			UseMusicCurrentVolume = Settings.UseMusicCurrentVolume;
		}

		public bool EnableVolumeControl
		{
			get;
			set;
		}

		public bool IgnoreYourSources
		{
			get;
			set;
		}

		public int TalkingVolume
		{
			get;
			set;
		}

		public int NormalVolume
		{
			get;
			set;
		}

		public bool UseMusicCurrentVolume
		{
			get;
			set;
		}

		public class MediaPlayerViewModel
			: ViewModelBase
		{
			public MediaPlayerViewModel (IMediaPlayer player, bool enabled)
			{
				Player = player;
				IsEnabled = enabled;
			}

			public IMediaPlayer Player
			{
				get;
				private set;
			}

			private bool isEnabled;
			public bool IsEnabled
			{
				get { return this.isEnabled; }
				set
				{
					if (isEnabled == value)
						return;

					this.isEnabled = value; 
					OnPropertyChanged();
				}
			}
		}

		public IEnumerable<MediaPlayerViewModel> MediaPlayers
		{
			get;
			private set;
		}

		public void UpdateSettings()
		{
			Settings.EnableMediaVolumeControl = EnableVolumeControl;
			Settings.MediaVolumeControlIgnoresYou = IgnoreYourSources;
			Settings.TalkingMusicVolume = TalkingVolume;
			Settings.UseMusicCurrentVolume = UseMusicCurrentVolume;
			Settings.EnabledMediaPlayerIntegrations = MediaPlayers.Where (vm => vm.IsEnabled).Select (vm => vm.Player.GetType().GetSimpleName()).ToArray();
		}
	}
}
