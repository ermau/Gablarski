// Copyright (c) 2009, Eric Maupin
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
using System.Threading;
using Gablarski.Client;
using Gablarski.Messages;

namespace Gablarski.Audio
{
	public class AudioEngine
		: IAudioEngine
	{
		/// <summary>
		/// Gets or sets the client context.
		/// </summary>
		public IClientContext Context
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

		public void Attach (IPlaybackProvider playback, IEnumerable<AudioSource> sources, AudioEnginePlaybackOptions options)
		{
			if (playback == null)
				throw new ArgumentNullException ("playback");
			if (sources == null)
				throw new ArgumentNullException ("sources");
			if (options == null)
				throw new ArgumentNullException ("options");

			lock (playbacks)
			{
				foreach (var s in sources.Where (s => !playbacks.ContainsKey (s) && s.OwnerId != Context.CurrentUser.UserId))
				{
					playbacks[s] = new AudioPlaybackEntity (playback, s, options);

					if (mutedPlayers.Contains (playback))
						playbacks[s].Muted = true;
				}
			}
		}

		public void Attach (IPlaybackProvider playback, AudioSource source, AudioEnginePlaybackOptions options)
		{
			if (playback == null)
				throw new ArgumentNullException ("playback");
			if (source == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");

			lock (playbacks)
			{
				playbacks[source] = new AudioPlaybackEntity (playback, source, options);

				if (mutedPlayers.Contains (playback))
					playbacks[source].Muted = true;
			}
		}

		public void Attach (ICaptureProvider capture, AudioFormat format, AudioSource source, AudioEngineCaptureOptions options)
		{
			if (capture == null)
				throw new ArgumentNullException ("capture");
			if (source == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");

			if (capture.Device == null)
				capture.Device = capture.DefaultDevice;

			lock (captures)
			{
				if (options.Mode == AudioEngineCaptureMode.Activated)
					capture.BeginCapture (format);

				captures[source] = new AudioCaptureEntity (capture, format, source, options);
			}
		}

		public bool Detach (IPlaybackProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			bool removed = false;

			lock (playbacks)
			{
				foreach (var entity in playbacks.Values.Where (e => e.Playback == provider).ToList())
				{
					playbacks.Remove (entity.Source);
					removed = true;
				}

				mutedPlayers.Remove (provider);
			}

			return removed;
		}

		public bool Detach (ICaptureProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			bool removed = false;

			lock (captures)
			{
				foreach (var entity in captures.Values.Where (e => e.Capture == provider).ToList())
				{
					captures.Remove (entity.Source);
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
				removed = captures.Remove (source);
			}

			if (!removed)
			{
				lock (playbacks)
				{
					AudioPlaybackEntity p;
					if (playbacks.TryGetValue (source, out p))
					{
						p.Playback.FreeSource (source);
						removed = playbacks.Remove (source);
					}
				}
			}

			return removed;
		}

		public void BeginCapture (AudioSource source, IEnumerable<ChannelInfo> channels)
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
				e.Capture.BeginCapture (AudioFormat.Mono16Bit);
			}
		}

		public void BeginCapture (AudioSource source, IEnumerable<UserInfo> users)
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
				e.Capture.BeginCapture (AudioFormat.Mono16Bit);
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
				e.Capture.EndCapture ();
			}
		}

		public void Mute (ICaptureProvider capture)
		{
			MuteCore (capture, true);
		}

		public void Unmute (ICaptureProvider capture)
		{
			MuteCore (capture, false);
		}

		public void Mute (IPlaybackProvider playback)
		{
			MuteCore (playback, true);
		}

		public void Unmute (IPlaybackProvider playback)
		{
			MuteCore (playback, false);
		}

		public void Start()
		{
			if (IsRunning)
				return;

			if (this.AudioReceiver == null)
				throw new InvalidOperationException ("AudioReceiver is not set.");
			if (this.AudioSender == null)
				throw new InvalidOperationException ("AudioSender is not set.");
			if (this.Context == null)
				throw new InvalidOperationException ("Context is not set.");

			this.running = true;

			this.AudioReceiver.ReceivedAudio += OnReceivedAudio;

			this.engineThread = new Thread (Engine);
			this.engineThread.Name = "Audio Engine";
			this.engineThread.Start();
		}

		public void Stop ()
		{
			if (!IsRunning)
				return;

			this.running = false;

			if (this.AudioReceiver != null)
				this.AudioReceiver.ReceivedAudio -= OnReceivedAudio;

			if (this.engineThread != null)
				this.engineThread.Join();

			lock (captures)
			{
				captures.Clear();
				mutedCaptures.Clear();
			}

			lock (playbacks)
			{
				playbacks.Clear();
				playbackProviders.Clear();
				mutedPlayers.Clear();
			}
		}

		private volatile bool running;
		
		private readonly Dictionary<AudioSource, AudioCaptureEntity> captures = new Dictionary<AudioSource, AudioCaptureEntity>();
		private readonly Dictionary<AudioSource, AudioPlaybackEntity> playbacks = new Dictionary<AudioSource, AudioPlaybackEntity>();
		private readonly List<IPlaybackProvider> playbackProviders = new List<IPlaybackProvider>();

		private readonly HashSet<IPlaybackProvider> mutedPlayers = new HashSet<IPlaybackProvider>();
		private readonly HashSet<ICaptureProvider> mutedCaptures = new HashSet<ICaptureProvider>();

		private Thread engineThread;

		private IClientContext context;
		private IAudioSender audioSender;
		private IAudioReceiver audioReceiver;

		private void OnReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			byte[] decoded = e.Source.Decode (e.AudioData);

			lock (playbacks)
			{
				AudioPlaybackEntity entity;
				if (playbacks.TryGetValue (e.Source, out entity) && !entity.Muted)
					entity.Playback.QueuePlayback (e.Source, decoded);
			}
		}

		private void MuteCore (ICaptureProvider capture, bool mute)
		{
			if (capture == null)
				throw new ArgumentNullException ("capture");

			lock (captures)
			{
				if (mute)
					mutedCaptures.Add (capture);
				else
					mutedCaptures.Add (capture);

				var ps = captures.Values.Where (e => e.Capture == capture);

				foreach (var e in ps)
					e.Muted = mute;
			}
		}

		private void MuteCore (IPlaybackProvider playback, bool mute)
		{
			if (playback == null)
				throw new ArgumentNullException ("playback");

			lock (playbacks)
			{
				if (mute)
					mutedPlayers.Add (playback);
				else
					mutedPlayers.Remove (playback);

				var ps = playbacks.Values.Where (e => e.Playback == playback);

				foreach (var e in ps)
					e.Muted = mute;
			}
		}

		private void Engine ()
		{
			byte[] previousFrame = null;

			while (this.running)
			{
				lock (playbackProviders)
				{
					foreach (var p in playbackProviders)
						p.Tick();
				}

				bool skipSwitch = false;

				lock (captures)
				{
					foreach (var c in captures)
					{
						bool muted = c.Value.Muted;

						if ((!c.Value.Talking && muted) || (!c.Value.Capture.IsCapturing && c.Value.Options.Mode == AudioEngineCaptureMode.Explicit))
							continue;

						if (c.Value.Talking && muted)
						{
							c.Value.Talking = false;
							AudioSender.EndSending (c.Key);
							continue;
						}

						if (c.Value.Capture.AvailableSampleCount > c.Key.FrameSize)
						{
							bool talking = c.Value.Talking;
							if (c.Value.CurrentTargets == null || c.Value.CurrentTargets.Length == 0)
								break;

							byte[] samples = c.Value.Capture.ReadSamples (c.Key.FrameSize);
							if (c.Value.Options.Mode == AudioEngineCaptureMode.Activated)
							{
								talking = c.Value.VoiceActivation.IsTalking (samples);

								if (talking && !c.Value.Talking)
								{
									AudioSender.BeginSending (c.Key);
									
									if (previousFrame != null)
										AudioSender.SendAudioData (c.Key, c.Value.TargetType, c.Value.CurrentTargets, previousFrame);
								}

								previousFrame = samples;
							}

							if (talking)
								AudioSender.SendAudioData (c.Key, c.Value.TargetType, c.Value.CurrentTargets, samples);
							else if (c.Value.Talking && c.Value.Options.Mode == AudioEngineCaptureMode.Activated)
								AudioSender.EndSending (c.Key);

							c.Value.Talking = talking;
						}

						if (!skipSwitch)
							skipSwitch = (c.Value.Capture.AvailableSampleCount > c.Key.FrameSize);
					}
				}

				if (!skipSwitch)
					Thread.Sleep (1);
			}
		}
	}
}