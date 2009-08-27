using System;
using System.Collections.Generic;
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

			lock (playbacks)
			{
				foreach (var s in sources)
				{
					if (playbacks.ContainsKey (s) && !(s is ClientAudioSource))
						continue;

					playbacks.Add (s, new AudioPlaybackEntity (playback, s, options));
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
				playbacks.Add (source, new AudioPlaybackEntity (playback, source, options));
			}
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
			
			lock (playbacks)
			{
				foreach (var entity in playbacks.Values.Where (e => e.Playback == provider).ToList())
				{
					playbacks.Remove (entity.Source);
					removed = true;
				}
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
			}
			
			return removed;
		}

		public bool Detach (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			bool removed = false;
			lock (captures)
			{
				removed = captures.Remove (source);
			}

			if (!removed)
			{
				lock (playbacks)
				{
					removed = playbacks.Remove (source);
				}
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

			lock (playbacks)
			{
				playbacks.Clear();
			}
		}

		private volatile bool running;
		private Thread playbackRunnerThread;

		private readonly Dictionary<AudioSource, AudioCaptureEntity> captures = new Dictionary<AudioSource, AudioCaptureEntity>();
		private readonly Dictionary<AudioSource, AudioPlaybackEntity> playbacks = new Dictionary<AudioSource, AudioPlaybackEntity>();

		private IAudioReceiver audioReceiver;

		private void OnAudioSourceStopped (object sender, AudioSourceEventArgs e)
		{
			lock (playbacks)
			{
				playbacks[e.Source].Playing = false;
			}
		}

		private void OnAudioSourceStarted (object sender, AudioSourceEventArgs e)
		{
			lock (playbacks)
			{
				playbacks[e.Source].Playing = true;
			}
		}

		private void OnReceivedAudio (object sender, ReceivedAudioEventArgs e)
		{
			var packet = new SpeexJitterBufferPacket (e.AudioData, (uint)e.Sequence, e.Source);

			lock (playbacks)
			{
				var p = playbacks[e.Source];
				if (p.Playing)
					p.Buffer.Push (packet);
			}
		}

		private void PlaybackRunner() 
		{
			while (this.running)
			{
				lock (playbacks)
				{
					foreach (var e in playbacks.Values)
					{
						if (!this.running)
							return;

						var s = e.Source;

						DateTime last = e.Last;

						DateTime n = DateTime.Now;
						if (e.Playing && n.Subtract (last) >= e.FrameTimeSpan)
						{
							var packet = e.Buffer.Pull (s.FrameSize);

							e.Playback.QueuePlayback (s, (packet.Encoded) ? s.Decode (packet.Data) : packet.Data);
							e.Last = n;
						}
					}
				}

				Thread.Sleep (0);
			}
		}
	}
}