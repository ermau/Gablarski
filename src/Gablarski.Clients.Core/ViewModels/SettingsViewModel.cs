//
// SettingsViewModel.cs
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
using System.Windows.Input;

namespace Gablarski.Clients.ViewModels
{
	public class SettingsViewModel
		: ViewModelBase
	{
		public SettingsViewModel (IntPtr windowHandle)
		{
			Playback = new AudioPlaybackSettingsViewModel();
			Capture = new AudioCaptureSettingsViewModel();
			Notifications = new NotificationSettingsViewModel();
			Input = new InputSettingsViewModel (windowHandle);
			Music = new MusicSettingsViewModel();

			SaveCommand = new RelayCommand (SaveSettings);
			CloseCommand = new RelayCommand (Close);
		}

		public event EventHandler Closing;

		public NotificationSettingsViewModel Notifications
		{
			get;
			private set;
		}

		public AudioPlaybackSettingsViewModel Playback
		{
			get;
			private set;
		}

		public AudioCaptureSettingsViewModel Capture
		{
			get;
			private set;
		}

		public InputSettingsViewModel Input
		{
			get;
			private set;
		}

		public MusicSettingsViewModel Music
		{
			get;
			private set;
		}

		public ICommand SaveCommand
		{
			get;
			private set;
		}

		public ICommand CloseCommand
		{
			get;
			private set;
		}

		private async void SaveSettings()
		{
			Playback.UpdateSettings();
			Capture.UpdateSettings();
			Input.UpdateSettings();
			Music.UpdateSettings();
			Notifications.UpdateSettings();
			await Settings.SaveAsync();

			Close();
		}

		private void Close()
		{
			Capture.Dispose();
			Input.Dispose();

			var closing = Closing;
			if (closing != null)
				closing (this, EventArgs.Empty);
		}
	}
}
