//
// AudioHandler.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2014, Xamarin Inc.
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Gablarski.Audio;
using Gablarski.Client;

namespace Gablarski.Clients
{
	public sealed class AudioHandler
	{
		public AudioHandler (IGablarskiClientContext context)
		{
			if (context == null)
				throw new ArgumentNullException ("context");

			this.context = context;
			this.context.Sources.CollectionChanged += OnSourcesChanged;

			Settings.SettingChanged += OnSettingsChanged;
		}

		public Task SetupAudioAsync()
		{
			return Task.WhenAll (SetupPlayback(), SetupCapture());
		}

		public void DisableAudio()
		{
			this.context.Audio.Clear();

			var p = this.playback;
			if (p != null)
				p.Close();

			var c = this.capture;
			if (c != null && c.IsCapturing)
				c.Close();
		}

		public async Task<AudioSource> RequestVoiceSourceAsync (AudioCodecArgs format)
		{
			var options = GetVoiceCaptureOptions();
			this.voiceSource = await RequestAudioSourceAsync ("voice", format, options);

			this.capture.Open (this.voiceSource.CodecSettings);
			this.context.Audio.Attach (this.capture, this.voiceSource, options);

			return this.voiceSource;
		}

		async Task<AudioSource> RequestAudioSourceAsync (string name, AudioCodecArgs format, AudioEngineCaptureOptions options)
		{
			AudioSource source = await this.context.Sources.RequestSourceAsync (name, format, format.FrameSize, format.Bitrate).ConfigureAwait (false);
			if (source == null)
				return source;

			IAudioCaptureProvider captureProvider = this.capture;
			if (captureProvider == null)
				throw new OperationCanceledException ("There was no capture provider to attach the audio source to");

			return source;
		}

		private readonly IGablarskiClientContext context;
		private IAudioCaptureProvider capture;
		private IAudioPlaybackProvider playback;
		private AudioSource voiceSource;

		private AudioEngineCaptureOptions GetVoiceCaptureOptions()
		{
			return new AudioEngineCaptureOptions {
				StartVolume = Settings.VoiceActivationLevel,
				ContinuationVolume = Settings.VoiceActivationLevel / 2,
				ContinueThreshold = TimeSpan.FromMilliseconds (Settings.VoiceActivationContinueThreshold),
				Mode = (!Settings.UsePushToTalk) ? AudioEngineCaptureMode.Activated : AudioEngineCaptureMode.Explicit
			};
		}

		private void UpdateVoiceSourceSettings()
		{
			AudioSource source = this.voiceSource;
			if (source == null)
				return;

			this.context.Audio.Update (source, GetVoiceCaptureOptions());
		}

		private async void OnSettingsChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) {
				case Settings.PlaybackProviderSettingName:
					throw new NotImplementedException();
					break;

				case Settings.GlobalVolumeName:
					var p = this.playback;
					if (p != null)
						p.Gain = Settings.GlobalVolume;

					break;

				case Settings.VoiceActivationLevelSettingName:
				case Settings.VoiceActivationContinueThresholdSettingName:
				case Settings.UsePushToTalkSettingName:
					UpdateVoiceSourceSettings();
					break;
			}
		}

		private async Task SetupPlayback()
		{
			IAudioPlaybackProvider playbackProvider = await Modules.GetImplementerAsync<IAudioPlaybackProvider> (Settings.PlaybackProvider).ConfigureAwait (false);

			playbackProvider.Open (Settings.VoiceDevice);
			playbackProvider.Gain = Settings.GlobalVolume;
			this.playback = playbackProvider;

			OnSourcesChanged (this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		private void AttachSource (AudioSource source)
		{
			var p = this.playback;
			if (p == null)
				return;

			this.context.Audio.Attach (p, source, new AudioEnginePlaybackOptions());
		}

		private async Task SetupCapture()
		{
			IAudioCaptureProvider captureProvider = await Modules.GetImplementerAsync<IAudioCaptureProvider> (Settings.VoiceProvider).ConfigureAwait (false);
			captureProvider.Device = captureProvider.GetDevices().FirstOrDefault (d => d.Name == Settings.VoiceDevice) ?? captureProvider.DefaultDevice;
			this.capture = captureProvider;
		}

		private void Reset()
		{
			this.context.Audio.Clear();

			foreach (AudioSource source in this.context.Sources)
				AttachSource (source);
		}

		private void OnSourcesChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
				case NotifyCollectionChangedAction.Reset:
					Reset();
					break;
			}
		}
	}
}
