using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Audio.Speex;
using Gablarski.Client;

namespace Gablarski.Audio
{
	public enum AudioEngineThreadingMode
	{
		ThreadPerCore = 0,
		//ThreadPerProvider
		SingleThread,
	}

	public enum AudioEngineCaptureMode
	{
		Signal = 0,
		Activated
	}

	public class AudioEngineCaptureOptions
	{
		public AudioEngineCaptureMode Mode
		{
			get; set;
		}

		public int VoiceActivityStartProbability
		{
			get; set;
		}
	}

	public class AudioEngine
	{
		public void Capture (ICaptureProvider capturor, AudioSource source, AudioEngineCaptureOptions options)
		{
			if (capturor == null)
				throw new ArgumentNullException ("capturor");
			if (source == null)
				throw new ArgumentNullException ("source");
			if (options == null)
				throw new ArgumentNullException ("options");

			lock (entities)
			{
				entities.Add (source, new AudioEntity (capturor, source, options));
			}
		}

		public void Stop (ICaptureProvider provider)
		{
			lock (entities)
			{
				foreach (var entity in entities.Values.Where (e => e.Capture == provider).ToList())
					entities.Remove (entity.Source);
			}
		}

		public void Stop (AudioSource source)
		{
			lock (entities)
			{
				entities.Remove (source);
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

		private Dictionary<AudioSource, AudioEntity> entities = new Dictionary<AudioSource, AudioEntity>();

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