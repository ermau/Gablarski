// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

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
using System.Text;
using System.Threading;
using Gablarski.Audio.Speex;
using Gablarski.Client;

namespace Gablarski.Audio
{
	public class AudioEngine
		: IAudioEngine
	{
		public event EventHandler<CaptureSourceStateChangedEventArgs> CaptureSourceStateChanged;

		/// <summary>
		/// Gets or sets the audio receiver
		/// </summary>
		public IAudioReceiver AudioReceiver
		{
			get { return this.audioReceiver; }
			set
			{
				if (this.running)
					throw new InvalidOperationException ("Can not change audio receivers while running.");

				this.audioReceiver = value;
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

			playbackLock.EnterWriteLock();
			{
				foreach (var s in sources)
				{
					if (playbacks.ContainsKey (s) && !(s is ClientAudioSource))
						continue;

					playbacks.Add (s, new AudioPlaybackEntity (playback, s, options));
				}
			}
			playbackLock.ExitWriteLock();
		}

		public void Attach (IPlaybackProvider playback, AudioSource source, AudioEnginePlaybackOptions options)
		{
			if (playback == null)
				throw new ArgumentNullException ("playback");
			if (source == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");

			playbackLock.EnterWriteLock();
			{
				playbacks.Add (source, new AudioPlaybackEntity (playback, source, options));
			}
			playbackLock.ExitWriteLock();
		}

		public void Attach (ICaptureProvider capture, AudioFormat format, OwnedAudioSource source, AudioEngineCaptureOptions options)
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

				captures.Add (source, new AudioCaptureEntity (capture, format, source, options));
			}
		}

		public bool Detatch (IPlaybackProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			bool removed = false;

			playbackLock.EnterWriteLock();
			{
				foreach (var entity in playbacks.Values.Where (e => e.Playback == provider).ToList())
				{
					playbacks.Remove (entity.Source);
					removed = true;
				}
			}
			playbackLock.ExitWriteLock();

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
			}
			
			return removed;
		}

		public bool Detach (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			bool removed = false;
			var osource = source as OwnedAudioSource;
			if (osource != null)
				removed = Detatch (osource);

			if (!removed)
			{
				playbackLock.EnterWriteLock();
				{
					removed = playbacks.Remove (source);
				}
				playbackLock.ExitWriteLock();
			}

			return removed;
		}

		public bool Detatch (OwnedAudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			bool removed;
			lock (captures)
			{
				removed = captures.Remove (source);
			}

			return removed;
		}

		public void BeginCapture (OwnedAudioSource source, ChannelInfo channel)
		{
			#if DEBUG
			if (source == null)
				throw new ArgumentNullException ("source");
			if (channel == null)
				throw new ArgumentNullException ("channel");
			#endif

			lock (captures)
			{
				captures[source].BeginCapture (channel);
			}
		}

		public void EndCapture (OwnedAudioSource source)
		{
			#if DEBUG
			if (source == null)
				throw new ArgumentNullException ("source");				
			#endif

			lock (captures)
			{
				captures[source].EndCapture();
			}
		}

		public void Start()
		{
			if (this.AudioReceiver == null)
				throw new InvalidOperationException ("AudioReceive not set.");
			if (this.running)
				throw new InvalidOperationException ("Engine is already running.");

			this.running = true;

			this.AudioReceiver.ReceivedAudio += OnReceivedAudio;
			this.AudioReceiver.AudioSourceStarted += OnAudioSourceStarted;
			this.AudioReceiver.AudioSourceStopped += OnAudioSourceStopped;

			this.engineThread = new Thread (Engine);
			this.engineThread.Name = "Audio Engine";
			this.engineThread.Start();
		}

		public void Stop ()
		{
			this.running = false;

			this.AudioReceiver.ReceivedAudio -= OnReceivedAudio;
			this.AudioReceiver.AudioSourceStarted -= OnAudioSourceStarted;
			this.AudioReceiver.AudioSourceStopped -= OnAudioSourceStopped;

			if (this.engineThread != null)
				this.engineThread.Join();

			lock (captures)
			{
				captures.Clear();
			}

			playbackLock.EnterWriteLock();
			{
				playbacks.Clear();
			}
			playbackLock.ExitWriteLock();
		}

		private volatile bool running;
		
		private readonly Dictionary<OwnedAudioSource, AudioCaptureEntity> captures = new Dictionary<OwnedAudioSource, AudioCaptureEntity>();
		private readonly Dictionary<AudioSource, AudioPlaybackEntity> playbacks = new Dictionary<AudioSource, AudioPlaybackEntity>();
		private readonly List<IPlaybackProvider> playbackProviders = new List<IPlaybackProvider>();
		private readonly ReaderWriterLockSlim playbackLock = new ReaderWriterLockSlim();

		private Thread engineThread;

		private IAudioReceiver audioReceiver;

		private void OnAudioSourceStopped (object sender, AudioSourceEventArgs e)
		{
			playbackLock.EnterReadLock();
			{
				AudioPlaybackEntity entity;
				if (playbacks.TryGetValue (e.Source, out entity))
				{
					entity.Playing = false;
					entity.Playback.FreeSource (e.Source);
				}
			}
			playbackLock.ExitReadLock();
		}

		private void OnAudioSourceStarted (object sender, AudioSourceEventArgs e)
		{
			playbackLock.EnterReadLock();
			{
				AudioPlaybackEntity entity;
				if (playbacks.TryGetValue (e.Source, out entity))
					entity.Playing = true;
			}
			playbackLock.ExitReadLock();
		}

		private void OnReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			byte[] decoded = e.Source.Decode (e.AudioData);

			playbackLock.EnterReadLock();
			{
				AudioPlaybackEntity entity;
				if (playbacks.TryGetValue (e.Source, out entity))
					entity.Playback.QueuePlayback (e.Source, decoded);
			}
			playbackLock.ExitReadLock();
		}

		protected void OnCaptureSourceStateChanged (CaptureSourceStateChangedEventArgs e)
		{
			var changed = this.CaptureSourceStateChanged;
			if (changed != null)
				changed (this, e);
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

				lock (captures)
				{
					foreach (var c in captures)
					{
						if (!c.Value.Capture.IsCapturing && c.Value.Options.Mode == AudioEngineCaptureMode.Explicit)
							continue;

						if (c.Value.Capture.AvailableSampleCount < c.Key.FrameSize)
							continue;

						bool talking = true;

						byte[] samples = c.Value.Capture.ReadSamples (c.Key.FrameSize);
						if (c.Value.Options.Mode == AudioEngineCaptureMode.Activated)
							talking = c.Value.VoiceActivation.IsTalking (samples);

						if (talking)
						{
							if (!c.Value.Talking)
							{
								c.Value.BeginCapture (this.AudioReceiver.CurrentChannel);
								OnCaptureSourceStateChanged (new CaptureSourceStateChangedEventArgs (c.Key, true));
							}

							c.Key.SendAudioData (samples);
						}
						else if (c.Value.Talking)
						{
							OnCaptureSourceStateChanged (new CaptureSourceStateChangedEventArgs (c.Key, false));
							c.Value.EndCapture();
						}
					}
				}

				Thread.Sleep (1);
			}
		}
	}
}