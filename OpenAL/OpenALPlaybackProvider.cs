﻿using System;
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
			//int e = Alc.alcIsExtensionPresent (IntPtr.Zero, "ALC_ENUMERATION_EXT");
			//string f = Alc.alcGetString (IntPtr.Zero, Alc.ALC_ALL_DEVICES_SPECIFIER);

			this.device = Alc.alcOpenDevice (null);//Alc.alcGetString (IntPtr.Zero, Alc.ALC_DEVICE_SPECIFIER));
			this.context = Alc.alcCreateContext (this.device, IntPtr.Zero);
			
			Al.alDistanceModel (Al.AL_NONE);
			Alc.alcMakeContextCurrent (this.context);

			this.sourcePool = new OpenALSourcePool (this.device);
			
			//this.PlayerThread = new Thread (this.Player)
			//{
			//    IsBackground = true,
			//    Name = "OpenAL Player"
			//};
			//this.PlayerThread.Start ();
		}

		#region IPlaybackProvider Members

		public void QueuePlayback (byte[] data, IMediaSource source)
		{
			bool stereo = true;// (source.Channels == AudioSourceChannels.Stereo);
			int alSource = this.sourcePool.RequestSource (source.ID, stereo);
			if (alSource == -1)
				return;
			
			int newBuffer;
			Al.alGenBuffers (1, out newBuffer);
			Al.alBufferData (newBuffer, ((stereo) ? Al.AL_FORMAT_STEREO16 : Al.AL_FORMAT_MONO16), data, data.Length, 44100);
			Al.alSourceQueueBuffers (alSource, 1, ref newBuffer);

			int state;
			Al.alGetSourcei (alSource, Al.AL_SOURCE_STATE, out state);
			if (state != Al.AL_PLAYING)
				Al.alSourcePlay (alSource);
		}

		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
			this.Dispose (true);
		}

		protected void Dispose (bool disposing)
		{
			if (this.device != IntPtr.Zero)
				Alc.alcCloseDevice (this.device);

			this.playing = false;

			if (this.PlayerThread != null)
				this.PlayerThread.Join ();
		}

		#endregion

		private object lck = new object ();

		private readonly OpenALSourcePool sourcePool;

		private bool playing = true;

		private readonly IntPtr device;
		private readonly IntPtr context;

		private readonly Thread PlayerThread;
		private void Player ()
		{
			while (this.playing)
			{
				//byte[] samples;

				//lock (lck)
				//{
				//    if (q.Count == 0)
				//    {
				//        Thread.Sleep (1);
				//        continue;
				//    }

				//    samples = q.Dequeue ();
				//}

				//IntPtr data;
				//fixed (byte* pbuffer = samples)
				//    data = new IntPtr (pbuffer);

				//Al.alBufferData (this.buffer, Al.AL_FORMAT_MONO16, data, samples.Length, 44100);
				//Al.alSourcei (this.source, Al.AL_BUFFER, this.buffer);

				//Al.alSourcePlay (this.source);

				//int state = Al.AL_PLAYING;
				//while (state == Al.AL_PLAYING)
				//{
				//    Thread.Sleep (1);
				//    Al.alGetSourcei (this.source, Al.AL_SOURCE_STATE, out state);
				//}

				//Al.alDeleteBuffers (1, ref buffer);
				//Al.alGenBuffers (1, out buffer);
			}
		}
	}
}