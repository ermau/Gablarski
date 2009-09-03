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

		public void Attach (ICaptureProvider capture, AudioFormat format, ClientAudioSource source, AudioEngineCaptureOptions options)
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

			bool removed;
			lock (captures)
			{
				removed = captures.Remove (source);
			}

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

			lock (captures)
			{
				captures[source].BeginCapture (channel);
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
				captures[source].EndCapture();
			}
		}

		public void Start()
		{
			this.running = true;

			this.AudioReceiver.ReceivedAudio += OnReceivedAudio;
			this.AudioReceiver.AudioSourceStarted += OnAudioSourceStarted;
			this.AudioReceiver.AudioSourceStopped += OnAudioSourceStopped;

			this.playbackRunnerThread = new Thread (this.PlaybackRunner) { Name = "ThreadedAudioEngine Playback Runner" };
			this.playbackRunnerThread.Start();
		}

		public void Stop ()
		{
			this.running = false;

			this.AudioReceiver.ReceivedAudio -= OnReceivedAudio;
			this.AudioReceiver.AudioSourceStarted -= OnAudioSourceStarted;
			this.AudioReceiver.AudioSourceStopped -= OnAudioSourceStopped;
			
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

		private IAudioReceiver audioReceiver;

		private void OnAudioSourceStopped (object sender, AudioSourceEventArgs e)
		{
			playbackLock.EnterReadLock();
			{
				AudioPlaybackEntity playbackEntity;
				if (playbacks.TryGetValue (e.Source, out playbackEntity))
					playbackEntity.Playing = false;
			}
			playbackLock.ExitReadLock();
		}

		private void OnAudioSourceStarted (object sender, AudioSourceEventArgs e)
		{
			playbackLock.EnterReadLock();
			{
				AudioPlaybackEntity playbackEntity;
				if (playbacks.TryGetValue (e.Source, out playbackEntity))
					playbackEntity.Playing = true;
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
					p.Buffer.Push (packet);
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

						var j = e.Buffer;
						var s = e.Source;

						DateTime n = DateTime.Now;
						if (n.Subtract (e.Last) >= e.FrameTimeSpan)
						{
							var packet = j.Pull (s.FrameSize);

							e.Playback.QueuePlayback (s, (packet.Encoded) ? s.Decode (packet.Data) : packet.Data);
							e.Last = n;
						}

						while (e.Buffer.AvailableCount > 0)
						{
							var packet = j.Pull (s.FrameSize);
							e.Playback.QueuePlayback (s, packet.Data);
						}

						j.Tick();
					}
				}
				playbackLock.ExitReadLock();

				Thread.Sleep (1);
			}
		}
	}
}