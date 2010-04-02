// Copyright (c) 2010, Eric Maupin
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

					if (this.playbackMuted || mutedPlayers.Contains (playback))
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

				if (this.playbackMuted || mutedPlayers.Contains (playback))
					playbacks[source].Muted = true;
			}
		}

		public void Attach (ICaptureProvider capture, AudioSource source, AudioEngineCaptureOptions options)
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
				capture.BeginCapture (source.Frequency, source.Format);
				captures[source] = new AudioCaptureEntity (capture, source, options);

				if (this.captureMuted || mutedCaptures.Contains (capture))
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
				
				var newc = new AudioCaptureEntity (c.Capture, source, options);
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

				playbacks[source] = new AudioPlaybackEntity (p.Playback, source, options);
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

		public void Update (AudioSource source, IEnumerable<UserInfo> users)
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

		public void Mute (ICaptureProvider capture)
		{
			MuteCore (capture, true);
		}

		public void Unmute (ICaptureProvider capture)
		{
			MuteCore (capture, false);
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
		private bool captureMuted;
		private bool playbackMuted;

		private void OnReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			if (this.playbackMuted)
				return;

			AudioPlaybackEntity entity;
			lock (playbacks)
			{
				if (!playbacks.TryGetValue (e.Source, out entity) || entity.Muted)
					return;
			}

			for (int i = 0; i < e.AudioData.Length; ++i)
			{
				byte[] decoded = e.Source.Decode (e.AudioData[i]);

				lock (playbacks)
				{
					if (!entity.Muted)
						entity.Playback.QueuePlayback (e.Source, decoded);
				}
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
						AudioCaptureEntity entity = c.Value;
						AudioSource source = c.Key;

						bool muted = (entity.Muted || this.captureMuted);

						if ((!entity.Talking && muted) || (!entity.Capture.IsCapturing && entity.Options.Mode == AudioEngineCaptureMode.Explicit))
							continue;
						
						if (entity.Talking && muted)
						{
							entity.Talking = false;
							AudioSender.EndSending (source);
							continue;
						}

						if (entity.Capture.AvailableSampleCount > source.FrameSize)
						{
							bool talking = entity.Talking;
							if (entity.CurrentTargets == null || entity.CurrentTargets.Length == 0)
							{
								entity.TargetType = TargetType.Channel;
								entity.CurrentTargets = new[] { Context.GetCurrentChannel().ChannelId };
							}

							AudioEngineCaptureMode mode = entity.Options.Mode;
							
							int framesAvailable = entity.Capture.AvailableSampleCount / source.FrameSize;
							int talkingFrames = (mode == AudioEngineCaptureMode.Explicit) ? framesAvailable : 0;
							int talkingIndex = -1;

							byte[][] frames = new byte[framesAvailable][];
							for (int i = 0; i < framesAvailable; ++i)
							{
								byte[] samples = entity.Capture.ReadSamples (source.FrameSize);
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
								AudioSender.SendAudioData (source, entity.TargetType, entity.CurrentTargets, frames);
							else if (entity.Talking && entity.Options.Mode == AudioEngineCaptureMode.Activated)
								AudioSender.EndSending (source);

							entity.Talking = talking;
						}

						if (!skipSwitch)
							skipSwitch = (entity.Capture.AvailableSampleCount > source.FrameSize);
					}
				}

				if (!skipSwitch)
					Thread.Sleep (1);
			}
		}
	}
}