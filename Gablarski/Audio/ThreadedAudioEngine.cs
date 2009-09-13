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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Audio.Speex;
using Gablarski.Client;

namespace Gablarski.Audio
{
	public class ThreadedAudioEngine
		: IAudioEngine
	{
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

		public bool DetailedTracing
		{
			get { return true; }
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
					Trace.WriteLineIf (DetailedTracing, "[Audio] " + s.Name + " attached for playback");

					if (playbacks.ContainsKey (s) && !(s is OwnedAudioSource))
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

			Trace.WriteLineIf (DetailedTracing, "[Audio] " + source.Name + " attached for playback");

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

			Trace.WriteLineIf (DetailedTracing, "[Audio] " + source.Name + " attached for capture");

			captureLock.EnterWriteLock();
			{
				captures.Add (source, new AudioCaptureEntity (capture, format, source, options));
			}
			captureLock.ExitWriteLock();
		}

		public bool Detatch (IPlaybackProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");

			bool removed = false;

			playbackLock.EnterUpgradeableReadLock();
			{
				foreach (var entity in playbacks.Values.Where (e => e.Playback == provider).ToList())
				{
					playbackLock.EnterWriteLock();
					playbacks.Remove (entity.Source);
					playbackLock.ExitWriteLock();
					removed = true;
				}
			}
			playbackLock.ExitUpgradeableReadLock();

			return removed;
		}

		public bool Detach (ICaptureProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			bool removed = false;

			captureLock.EnterUpgradeableReadLock();
			{
				foreach (var entity in captures.Values.Where (e => e.Capture == provider).ToList())
				{
					captureLock.EnterWriteLock();
					captures.Remove (entity.Source);
					captureLock.ExitWriteLock();
					removed = true;
				}
			}
			captureLock.ExitUpgradeableReadLock();
			
			return removed;
		}

		public bool Detach (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			bool removed;
			captureLock.EnterWriteLock();
			{
				removed = captures.Remove (source);
			}
			captureLock.ExitWriteLock();

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

		public void BeginCapture (AudioSource source, ChannelInfo channel)
		{
			#if DEBUG
			if (source == null)
				throw new ArgumentNullException ("source");
			if (channel == null)
				throw new ArgumentNullException ("channel");
			#endif

			captureLock.EnterReadLock();
			{
				captures[source].BeginCapture (channel);
			}
			captureLock.ExitReadLock();
		}

		public void EndCapture (AudioSource source)
		{
			#if DEBUG
			if (source == null)
				throw new ArgumentNullException ("source");				
			#endif

			captureLock.EnterReadLock();
			{
				captures[source].EndCapture();
			}
			captureLock.ExitReadLock();
		}

		public void Start()
		{
			this.running = true;

			this.AudioReceiver.ReceivedAudio += OnReceivedAudio;
			this.AudioReceiver.AudioSourceStarted += OnAudioSourceStarted;
			this.AudioReceiver.AudioSourceStopped += OnAudioSourceStopped;
			this.AudioReceiver.AudioSourcesRemoved += OnAudioSourceRemoved;

			this.playbackRunnerThread = new Thread (this.PlaybackRunner) { Name = "ThreadedAudioEngine Playback Runner" };
			this.playbackRunnerThread.Start();
		}

		public void Stop ()
		{
			this.running = false;

			this.AudioReceiver.ReceivedAudio -= OnReceivedAudio;
			this.AudioReceiver.AudioSourceStarted -= OnAudioSourceStarted;
			this.AudioReceiver.AudioSourceStopped -= OnAudioSourceStopped;
			this.AudioReceiver.AudioSourcesRemoved -= OnAudioSourceRemoved;
			
			if (this.playbackRunnerThread != null)
			{
				this.playbackRunnerThread.Join();
				this.playbackRunnerThread = null;
			}

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
		private Thread playbackRunnerThread;

		private readonly Dictionary<AudioSource, AudioCaptureEntity> captures = new Dictionary<AudioSource, AudioCaptureEntity>();
		private readonly Dictionary<AudioSource, AudioPlaybackEntity> playbacks = new Dictionary<AudioSource, AudioPlaybackEntity>();
		private readonly ReaderWriterLockSlim playbackLock = new ReaderWriterLockSlim();
		private readonly ReaderWriterLockSlim captureLock = new ReaderWriterLockSlim();

		private IAudioReceiver audioReceiver;

		private void OnAudioSourceRemoved (object sender, ReceivedListEventArgs<ClientAudioSource> e)
		{
			foreach (var s in e.Data)
			{
				playbackLock.EnterUpgradeableReadLock();
				if (playbacks.ContainsKey (s))
				{
					playbackLock.EnterWriteLock();
					playbacks[s].Playback.FreeSource (s);
					playbacks.Remove (s);
					playbackLock.ExitWriteLock();
				}
				playbackLock.ExitUpgradeableReadLock();

				captureLock.EnterUpgradeableReadLock();
				if (captures.ContainsKey (s))
				{
					captureLock.EnterWriteLock();
					captures.Remove (s);
					captureLock.ExitWriteLock();
				}
				captureLock.ExitUpgradeableReadLock();
			}
		}

		private void OnAudioSourceStopped (object sender, AudioSourceEventArgs e)
		{
			playbackLock.EnterReadLock();
			{
				AudioPlaybackEntity playbackEntity;
				if (playbacks.TryGetValue (e.Source, out playbackEntity))
				{
					playbackEntity.Playing = false;
					playbackEntity.Buffer.Reset();
				}
			}
			playbackLock.ExitReadLock();
		}

		private void OnAudioSourceStarted (object sender, AudioSourceEventArgs e)
		{
			playbackLock.EnterReadLock();
			{
				AudioPlaybackEntity playbackEntity;
				if (playbacks.TryGetValue (e.Source, out playbackEntity))
				{
					for (int i = 0; i < 20; ++i)
						playbackEntity.Playback.QueuePlayback (e.Source, new byte[e.Source.FrameSize * 2]);

					playbackEntity.Playing = true;
				}
				else
					Trace.WriteLine ("[Audio] Attempting to playback an unknown source");
			}
			playbackLock.ExitReadLock();
		}

		private void OnReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			var packet = new SpeexJitterBufferPacket (e.AudioData, (uint)e.Sequence, e.Source);

			playbackLock.EnterReadLock();
			{
				AudioPlaybackEntity p;
				if (playbacks.TryGetValue (e.Source, out p) && p.Playing)
				{
					p.Buffer.Push (packet);
					Trace.WriteLineIf (DetailedTracing, "[Audio] Received audio packet for '" + e.Source.Name + "'");
				}
				else
					Trace.WriteLine ("[Audio] Received audio packet for unknown or not playing source");
			}
			playbackLock.ExitReadLock();
		}

		private void PlaybackRunner()
		{
			while (this.running)
			{
				playbackLock.EnterReadLock();
				{
					foreach (var e in playbacks.Values)
					{
						if (!this.running)
							return;
						if (!e.Playing)
							continue;

						var s = e.Source;

						int free = e.Playback.GetBuffersFree (s);
						while (free-- > 0)
						{
							var packet = e.Buffer.Pull (s.FrameSize);

							Trace.WriteLineIf (DetailedTracing && !packet.Encoded, "[Audio] Packet missing");

							e.Playback.QueuePlayback (s, (packet.Encoded) ? s.Decode (packet.Data) : packet.Data);
						}
					}
				}
				playbackLock.ExitReadLock();

				Thread.Sleep (1);
			}
		}
	}
}