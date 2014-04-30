//
// AudioEngine.cs
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
using System.Linq;
using Gablarski.Client;
using Gablarski.Messages;

namespace Gablarski.Audio
{
	public class AudioEngine
		: IAudioEngine
	{
		public event EventHandler<AudioSourceEventArgs> SourceFinished;

		/// <summary>
		/// Gets or sets the client context.
		/// </summary>
		public IGablarskiClientContext Context
		{
			get { return this.context; }
			set
			{
				if (IsRunning)
					throw new InvalidOperationException ("Can not change context while the engine is running.");

				this.context = value;
			}
		}

		/// <summary>
		/// Gets or sets the audio receiver.
		/// </summary>
		public IAudioReceiver AudioReceiver
		{
			get { return this.audioReceiver; }
			set
			{
				if (IsRunning)
					throw new InvalidOperationException ("Can not change audio receivers while the engine is running.");

				this.audioReceiver = value;
			}
		}

		public bool IsRunning
		{
			get { return this.running; }
		}

		public IAudioSender AudioSender
		{
			get { return this.audioSender; }
			set
			{
				if (IsRunning)
					throw new InvalidOperationException ("Can not change AudioSender while the engine is running.");

				this.audioSender = value;
			}
		}

		public void Attach (IAudioPlaybackProvider audioPlayback, IEnumerable<AudioSource> sources, AudioEnginePlaybackOptions options)
		{
			if (audioPlayback == null)
				throw new ArgumentNullException ("audioPlayback");
			if (sources == null)
				throw new ArgumentNullException ("sources");
			if (options == null)
				throw new ArgumentNullException ("options");

			lock (playbacks)
			{
				if (!this.playbackProviders.Contains (audioPlayback))
				{
					audioPlayback.SourceFinished += OnSourceFinished;
					this.playbackProviders.Add (audioPlayback);
				}

				foreach (var s in sources.Where (s => !playbacks.ContainsKey (s) && s.OwnerId != Context.CurrentUser.UserId))
				{
					audioPlayback.SetGain (s, options.Gain);
					playbacks[s] = new AudioPlaybackEntity (audioPlayback, s, options);

					if (mutedPlayers.Contains (audioPlayback))
						playbacks[s].Muted = true;
				}
			}
		}

		public void Attach (IAudioPlaybackProvider audioPlayback, AudioSource source, AudioEnginePlaybackOptions options)
		{
			if (audioPlayback == null)
				throw new ArgumentNullException ("audioPlayback");
			if (source == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");

			lock (playbacks)
			{
				if (!this.playbackProviders.Contains (audioPlayback))
				{
					audioPlayback.SourceFinished += OnSourceFinished;
					this.playbackProviders.Add (audioPlayback);
				}

				audioPlayback.SetGain (source, options.Gain);
				playbacks[source] = new AudioPlaybackEntity (audioPlayback, source, options);

				if (mutedPlayers.Contains (audioPlayback))
					playbacks[source].Muted = true;
			}
		}

		public void Attach (IAudioCaptureProvider audioCapture, AudioSource source, AudioEngineCaptureOptions options)
		{
			if (audioCapture == null)
				throw new ArgumentNullException ("audioCapture");
			if (source == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");

			if (audioCapture.Device == null)
				audioCapture.Device = audioCapture.DefaultDevice;

			lock (captures)
			{
				audioCapture.SamplesAvailable += OnSamplesAvailable;
				audioCapture.BeginCapture (source.CodecSettings, source.CodecSettings.FrameSize);
				captures[source] = new AudioCaptureEntity (audioCapture, source, options);
				captureToSourceLookup[audioCapture] = source;

				if (this.captureMuted || mutedCaptures.Contains (audioCapture))
					captures[source].Muted = true;
			}
		}

		public void Update (AudioSource source, AudioEngineCaptureOptions options)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");

			lock (captures)
			{
				AudioCaptureEntity c;
				if (!captures.TryGetValue (source, out c))
					throw new ArgumentException ("source is not attached", "source");

				if (c.Talking && c.Options.Mode != options.Mode)
					AudioSender.EndSending (source);
				
				var newc = new AudioCaptureEntity (c.AudioCapture, source, options);
				newc.TargetType = c.TargetType;
				newc.CurrentTargets = c.CurrentTargets;

				captures[source] = newc;
			}
		}

		public void Update (AudioSource source, AudioEnginePlaybackOptions options)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");

			lock (playbacks)
			{
				AudioPlaybackEntity p;
				if (!playbacks.TryGetValue (source, out p))
					throw new ArgumentException ("source is not attached", "source");

				p.AudioPlayback.SetGain (source, options.Gain);
				playbacks[source] = new AudioPlaybackEntity (p.AudioPlayback, source, options);
			}
		}

		public void Update (AudioSource source, IEnumerable<ChannelInfo> channels)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (channels == null)
				throw new ArgumentNullException ("channels");

			lock (captures)
			{
				AudioCaptureEntity ce;
				if (!captures.TryGetValue (source, out ce))
					throw new ArgumentException ("source is not attached", "source");

				ce.TargetType = TargetType.Channel;
				ce.CurrentTargets = channels.Select (c => c.ChannelId).ToArray();
			}
		}

		public void Update (AudioSource source, IEnumerable<IUserInfo> users)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (users == null)
				throw new ArgumentNullException ("users");

			lock (captures)
			{
				AudioCaptureEntity ce;
				if (!captures.TryGetValue (source, out ce))
					throw new ArgumentException ("source is not attached", "source");

				ce.TargetType = TargetType.User;
				ce.CurrentTargets = users.Select (c => c.UserId).ToArray();
			}
		}

		public bool Detach (IAudioPlaybackProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			bool removed = false;

			lock (playbacks)
			{
				this.playbackProviders.Remove (provider);
				provider.SourceFinished -= OnSourceFinished;

				foreach (var entity in playbacks.Values.Where (e => e.AudioPlayback == provider).ToList())
				{
					entity.AudioPlayback.FreeSource (entity.Source);
					playbacks.Remove (entity.Source);
					removed = true;
				}

				mutedPlayers.Remove (provider);
			}

			return removed;
		}

		public bool Detach (IAudioCaptureProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			bool removed = false;

			lock (captures)
			{
				foreach (var entity in captures.Values.Where (e => e.AudioCapture == provider).ToList())
				{
					captures.Remove (entity.Source);
					captureToSourceLookup.Remove (provider);
					removed = true;
				}

				mutedCaptures.Remove (provider);
			}
			
			return removed;
		}

		public bool Detach (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			bool removed;
			lock (captures)
			{
				AudioCaptureEntity entity;
				if (captures.TryGetValue (source, out entity))
				{
					removed = true;
					captures.Remove (source);
					captureToSourceLookup.Remove (entity.AudioCapture);
				}
				else
					removed = false;
			}

			if (!removed)
			{
				lock (playbacks)
				{
					AudioPlaybackEntity p;
					if (playbacks.TryGetValue (source, out p))
					{
						p.AudioPlayback.FreeSource (source);
						removed = playbacks.Remove (source);
					}
				}
			}

			return removed;
		}

		public void BeginCapture (AudioSource source, IEnumerable<IChannelInfo> channels)
		{
			#if DEBUG
			if (source == null)
				throw new ArgumentNullException ("source");
			if (channels == null)
				throw new ArgumentNullException ("channels");
			#endif

			lock (captures)
			{
				AudioCaptureEntity e;
				if (!captures.TryGetValue (source, out e))
					return;

				e.TargetType = TargetType.Channel;
				e.CurrentTargets = channels.Select (c => c.ChannelId).ToArray();
				e.Talking = true;
				AudioSender.BeginSending (source);
			}
		}

		public void BeginCapture (AudioSource source, IEnumerable<IUserInfo> users)
		{
			#if DEBUG
			if (source == null)
				throw new ArgumentNullException ("source");
			if (users == null)
				throw new ArgumentNullException ("users");
			if (AudioSender == null)
				throw new InvalidOperationException ("AudioSender not set.");
			#endif

			lock (captures)
			{
				AudioCaptureEntity e;
				if (!captures.TryGetValue (source, out e))
					return;

				e.TargetType = TargetType.User;
				e.CurrentTargets = users.Select (c => c.UserId).ToArray();
				e.Talking = true;
				AudioSender.BeginSending (source);
				//e.Capture.BeginCapture (AudioFormat.Mono16Bit);
			}
		}

		public void EndCapture (AudioSource source)
		{
			#if DEBUG
			if (source == null)
				throw new ArgumentNullException ("source");
			#endif

			lock (captures)
			{
				AudioCaptureEntity e;
				if (!captures.TryGetValue (source, out e))
					return;

				e.Talking = false;
				AudioSender.EndSending (source);

				//if (e.Capture.IsCapturing)
				//	e.Capture.EndCapture ();
			}
		}

		public void MuteCapture()
		{
			lock (captures)
			{
				this.captureMuted = true;
			}
		}

		public void UnmuteCapture()
		{
			lock (captures)
			{
				this.captureMuted = false;
			}
		}

		public void Mute (IAudioCaptureProvider audioCapture)
		{
			MuteCore (audioCapture, true);
		}

		public void Unmute (IAudioCaptureProvider audioCapture)
		{
			MuteCore (audioCapture, false);
		}

		public void MutePlayback()
		{
			lock (this.playbacks)
			{
				this.playbackMuted = true;
			}
		}

		public void UnmutePlayback()
		{
			lock (this.playbacks)
			{
				this.playbackMuted = false;
			}
		}

		public void Mute (IAudioPlaybackProvider audioPlayback)
		{
			MuteCore (audioPlayback, true);
		}

		public void Unmute (IAudioPlaybackProvider audioPlayback)
		{
			MuteCore (audioPlayback, false);
		}

		public void Start()
		{
			if (IsRunning)
				return;

			if (AudioReceiver == null)
				throw new InvalidOperationException ("AudioReceiver is not set.");
			if (AudioSender == null)
				throw new InvalidOperationException ("AudioSender is not set.");
			if (Context == null)
				throw new InvalidOperationException ("Context is not set.");

			this.running = true;

			AudioReceiver.ReceivedAudio += OnReceivedAudio;
		}

		public void Clear()
		{
			lock (captures)
			{
				captureToSourceLookup.Clear();
				captures.Clear();
				mutedCaptures.Clear();

				this.captureMuted = false;
			}

			lock (playbacks)
			{
				playbacks.Clear();
				playbackProviders.Clear();
				mutedPlayers.Clear();

				this.playbackMuted = false;
			}
		}

		public void Stop ()
		{
			this.running = false;

			if (AudioReceiver != null)
				AudioReceiver.ReceivedAudio -= OnReceivedAudio;

			Clear();
		}

		private volatile bool running;

		private readonly Dictionary<IAudioCaptureProvider, AudioSource> captureToSourceLookup = new Dictionary<IAudioCaptureProvider, AudioSource>();
		private readonly Dictionary<AudioSource, AudioCaptureEntity> captures = new Dictionary<AudioSource, AudioCaptureEntity>();
		private readonly Dictionary<AudioSource, AudioPlaybackEntity> playbacks = new Dictionary<AudioSource, AudioPlaybackEntity>();
		private readonly HashSet<IAudioPlaybackProvider> playbackProviders = new HashSet<IAudioPlaybackProvider>();

		private readonly HashSet<IAudioPlaybackProvider> mutedPlayers = new HashSet<IAudioPlaybackProvider>();
		private readonly HashSet<IAudioCaptureProvider> mutedCaptures = new HashSet<IAudioCaptureProvider>();

		private IGablarskiClientContext context;
		private IAudioSender audioSender;
		private IAudioReceiver audioReceiver;
		private bool captureMuted;
		private bool playbackMuted;

		private void OnSourceFinished (object sender, AudioSourceEventArgs e)
		{
			var stopped = SourceFinished;
			if (stopped != null)
				stopped (sender, new AudioSourceEventArgs (e.Source));
		}

		private void OnSamplesAvailable (object sender, SamplesAvailableEventArgs e)
		{
			if (!this.running)
				return;

			lock (captures)
			{
				AudioSource source;
				AudioCaptureEntity entity;
				if (!captureToSourceLookup.TryGetValue (e.Provider, out source) || !captures.TryGetValue (source, out entity))
					return;

				bool muted = (entity.Muted || this.captureMuted);

				if ((!entity.Talking && muted) || (!entity.AudioCapture.IsCapturing && entity.Options.Mode == AudioEngineCaptureMode.Explicit))
					return;
						
				if (entity.Talking && muted)
				{
					entity.Talking = false;
					AudioSender.EndSending (source);
					return;
				}

				if (e.Available < source.CodecSettings.FrameSize)
					return;

				bool talking = entity.Talking;
				if (entity.CurrentTargets == null || entity.CurrentTargets.Length == 0)
				{
					entity.TargetType = TargetType.Channel;
					var currentChannel = Context.GetCurrentChannel();
					if (currentChannel == null)
						return;

					entity.CurrentTargets = new[] { currentChannel.ChannelId };
				}

				AudioEngineCaptureMode mode = entity.Options.Mode;
					
				int framesAvailable = e.Available / source.CodecSettings.FrameSize;
				int talkingFrames = (mode == AudioEngineCaptureMode.Explicit) ? framesAvailable : 0;
				int talkingIndex = -1;

				byte[][] frames = new byte[framesAvailable][];
				for (int i = 0; i < frames.Length; ++i)
				{
					byte[] samples = entity.AudioCapture.ReadSamples (source.CodecSettings.FrameSize);
					if (mode == AudioEngineCaptureMode.Activated)
					{
						talking = entity.VoiceActivation.IsTalking (samples);
						if (talking)
						{
							talkingFrames++;
							if (talkingIndex == -1)
								talkingIndex = i;
						}

						if (talking && !entity.Talking)
							AudioSender.BeginSending (source);
					}

					frames[i] = samples;
				}

				if (talkingFrames > 0 && talkingFrames != frames.Length)
				{
					byte[][] actualFrames = new byte[talkingFrames][];
					Array.Copy (frames, talkingIndex, actualFrames, 0, talkingFrames);
					frames = actualFrames;
				}

				if (talking)
					AudioSender.SendAudioDataAsync (source, entity.TargetType, entity.CurrentTargets, frames);
				else if (entity.Talking && entity.Options.Mode == AudioEngineCaptureMode.Activated)
					AudioSender.EndSending (source);

				entity.Talking = talking;
			}
		}

		private void OnReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			if (this.playbackMuted)
				return;

			AudioPlaybackEntity entity;
			lock (playbacks) {
				if (!playbacks.TryGetValue (e.Source, out entity) || entity.Muted)
					return;
			}

			for (int i = 0; i < e.AudioData.Length; ++i) {
				lock (playbacks) {
					if (!entity.Muted)
						entity.AudioPlayback.QueuePlayback (e.Source, e.AudioData[i]);
				}
			}
		}

		private void MuteCore (IAudioCaptureProvider audioCapture, bool mute)
		{
			if (audioCapture == null)
				throw new ArgumentNullException ("audioCapture");

			lock (captures)
			{
				if (mute)
					mutedCaptures.Add (audioCapture);
				else
					mutedCaptures.Add (audioCapture);

				var ps = captures.Values.Where (e => e.AudioCapture == audioCapture);

				foreach (var e in ps)
					e.Muted = mute;
			}
		}

		private void MuteCore (IAudioPlaybackProvider audioPlayback, bool mute)
		{
			if (audioPlayback == null)
				throw new ArgumentNullException ("audioPlayback");

			lock (playbacks)
			{
				if (mute)
					mutedPlayers.Add (audioPlayback);
				else
					mutedPlayers.Remove (audioPlayback);

				var ps = playbacks.Values.Where (e => e.AudioPlayback == audioPlayback);

				foreach (var e in ps)
					e.Muted = mute;
			}
		}
	}
}