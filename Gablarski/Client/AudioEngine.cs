using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Speex;

namespace Gablarski.Client
{
	public class AudioEngine
	{
		public void Start (IPlaybackProvider playback)
		{
			if (this.runnerThread != null)
				return;

			this.jitter = SpeexJitterBuffer.Create(256);
			(this.runnerThread = new Thread (Runner) { Name = "AudioEngine" }).Start();
		}

		public void StopRunner()
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
		private SpeexJitterBuffer jitter;

		private void Runner()
		{
			while (this.running)
			{
			}
		}
	}
}