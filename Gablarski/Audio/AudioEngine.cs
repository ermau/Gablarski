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
		public void Attach (ICaptureProvider provider, AudioSource source, AudioEngineCaptureOptions options)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			if (source == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");

			if (provider.Device == null)
				provider.Device = provider.DefaultDevice;

			lock (entities)
			{
				entities.Add (source, new AudioEntity (provider, source, options));
			}
		}

		public bool Detach (ICaptureProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			bool removed = false;

			lock (entities)
			{
				foreach (var entity in entities.Values.Where (e => e.Capture == provider).ToList())
				{
					entities.Remove (entity.Source);
					removed = true;
				}
			}

			return removed;
		}

		public bool Detach (AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
				
			lock (entities)
			{
				return entities.Remove (source);
			}
		}

		public void BeginCapture (AudioSource source)
		{
			lock (entities)
			{
				entities[source].Capture.BeginCapture();
			}
		}

		public void EndCapture (AudioSource source)
		{
			lock (entities)
			{
				entities[source].Capture.EndCapture();
			}
		}

		public void Stop ()
		{
			this.running = false;
			
			if (this.runnerThread != null)
			{
				this.runnerThread.Join();
				this.runnerThread = null;
			}
		}

		private volatile bool running;
		private Thread runnerThread;

		private readonly Dictionary<AudioSource, AudioEntity> entities = new Dictionary<AudioSource, AudioEntity>();

		private class AudioEntity
		{
			public AudioEntity (ICaptureProvider capture, AudioSource source, AudioEngineCaptureOptions options)
			{
				this.capture = capture;
				this.source = source;
				this.options = options;

				jitter = SpeexJitterBuffer.Create (this.source.FrameSize);
			}

			public ICaptureProvider Capture
			{
				get { return this.capture; }
			}

			public AudioSource Source
			{
				get { return this.source; }
			}

			public AudioEngineCaptureOptions Options
			{
				get { return this.options; }
			}

			private readonly SpeexJitterBuffer jitter;
			private readonly SpeexPreprocessor preprocessor;
			private readonly ICaptureProvider capture;
			private readonly AudioSource source;
			private readonly AudioEngineCaptureOptions options;

			~AudioEntity()
			{
				capture.Dispose();
				jitter.Dispose();
			}
		}

		private void Runner()
		{
			while (this.running)
			{

			}
		}
	}
}