using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tao.OpenAl;

namespace Gablarski.Client.Providers.OpenAL
{
	public unsafe class OpenALPlaybackProvider
		: IPlaybackProvider
	{
		public OpenALPlaybackProvider ()
		{
			this.device = Alc.alcOpenDevice (null);
			this.context = Alc.alcCreateContext (this.device, IntPtr.Zero);
			Alc.alcMakeContextCurrent (this.context);

			Al.alGenSources (1, out this.source);
			Al.alGenBuffers (1, out this.buffer);

			Al.alDistanceModel (Al.AL_NONE);

			this.PlayerThread = new Thread (this.Player)
			{
				IsBackground = true,
				Name = "OpenAL Player"
			};
			this.PlayerThread.Start ();
		}

		#region IPlaybackProvider Members

		public void QueuePlayback (byte[] data)
		{
			lock (lck)
			{
				q.Enqueue (data);
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
			this.playing = false;

			if (this.PlayerThread != null)
				this.PlayerThread.Join ();
		}

		#endregion

		private object lck = new object ();
		private Queue<byte[]> q = new Queue<byte[]> ();

		private bool playing = true;

		private readonly IntPtr device;
		private readonly IntPtr context;
		private int buffer;
		private readonly int source;

		private readonly Thread PlayerThread;
		private void Player ()
		{
			while (this.playing)
			{
				byte[] samples;

				lock (lck)
				{
					if (q.Count == 0)
					{
						Thread.Sleep (1);
						continue;
					}

					samples = q.Dequeue ();
				}

				IntPtr data;
				fixed (byte* pbuffer = samples)
					data = new IntPtr ((void*)pbuffer);

				Al.alBufferData (this.buffer, Al.AL_FORMAT_MONO16, data, samples.Length, 44100);
				Al.alSourcei (this.source, Al.AL_BUFFER, this.buffer);

				Al.alSourcePlay (this.source);

				int state = Al.AL_PLAYING;
				while (state == Al.AL_PLAYING)
				{
					Thread.Sleep (1);
					Al.alGetSourcei (this.source, Al.AL_SOURCE_STATE, out state);
				}
			}
		}
	}
}