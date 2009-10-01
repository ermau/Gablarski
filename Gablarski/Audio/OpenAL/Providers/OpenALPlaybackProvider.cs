﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio.OpenAL.Providers
{
	public class OpenALPlaybackProvider
		: IPlaybackProvider
	{
		public OpenALPlaybackProvider()
		{
			this.pool.SourceFinished += PoolSourceFinished;
		}

		#region IPlaybackProvider Members
		public event EventHandler<SourceFinishedEventArgs> SourceFinished;

		public IAudioDevice Device
		{
			get { return this.device; }
			set
			{
				this.device = (value as PlaybackDevice);
				if (this.device == null)
					throw new ArgumentException ("Can only accept OpenAL PlaybackDevice devices");
			}
		}

		public void QueuePlayback (AudioSource audioSource, byte[] data)
		{
			if (!this.device.IsOpen)
				this.device.Open();

			if (this.context == null)
				this.context = this.device.CreateAndActivateContext();

			Stack<SourceBuffer> bufferStack;
			if (!this.buffers.TryGetValue (audioSource, out bufferStack))
				this.buffers[audioSource] = bufferStack = new Stack<SourceBuffer>();

			Source source = this.pool.RequestSource (audioSource);

			const int bufferLen = 4;

			if (data.Length == 0)
				return;

			RequireBuffers (bufferStack, source, (source.IsPlaying) ? 1 : bufferLen + 1);

			if (!source.IsPlaying)
			{
				for (int i = 0; i < bufferLen; ++i)
				{
					SourceBuffer wait = bufferStack.Pop();
					wait.Buffer (new byte[audioSource.FrameSize*2*audioSource.Channels],
					             (audioSource.Channels == 1) ? AudioFormat.Mono16Bit : AudioFormat.Stereo16Bit,
					             (uint)audioSource.Frequency);
					source.QueueAndPlay (wait);
					pool.PlayingSource (source);
				}
			}

			SourceBuffer buffer = bufferStack.Pop ();

			buffer.Buffer (data, (audioSource.Channels == 1) ? AudioFormat.Mono16Bit : AudioFormat.Stereo16Bit, (uint)audioSource.Frequency);
			source.QueueAndPlay (buffer);
			pool.PlayingSource (source);
		}

		public IEnumerable<IAudioDevice> GetDevices ()
		{
			return Audio.OpenAL.OpenAL.PlaybackDevices.Cast<IAudioDevice>();
		}

		public void FreeSource (AudioSource source)
		{
			buffers.Remove (source);
			pool.FreeSource (source);
		}

		public void Tick()
		{
			pool.Tick();
		}

		public IAudioDevice DefaultDevice
		{
			get { return Audio.OpenAL.OpenAL.DefaultPlaybackDevice; }
		}

		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
			if (this.device != null)
				this.device.Dispose();
		}

		#endregion

		private Context context;
		private PlaybackDevice device;
		private readonly SourcePool<AudioSource> pool = new SourcePool<AudioSource>();
		private readonly object bufferLock = new object ();
		private readonly Dictionary<AudioSource, Stack<SourceBuffer>> buffers = new Dictionary<AudioSource, Stack<SourceBuffer>> ();

		private void RequireBuffers (Stack<SourceBuffer> bufferStack, Source source, int num)
		{
			if (source.ProcessedBuffers > 0)
			{
				SourceBuffer[] freeBuffers = source.Dequeue ();
				for (int i = 0; i < freeBuffers.Length; ++i)
				{
					lock (bufferStack)
					{
						bufferStack.Push (freeBuffers[i]);
					}
				}
			}

			if (bufferStack.Count < num)
				PushBuffers (bufferStack, num);
		}

		private static void PushBuffers (Stack<SourceBuffer> bufferStack, int number)
		{
			SourceBuffer[] sbuffers = SourceBuffer.Generate (number);
			for (int i = 0; i < sbuffers.Length; ++i)
				bufferStack.Push (sbuffers[i]);
		}

		private void OnSourceFinished (SourceFinishedEventArgs e)
		{
			var finished = this.SourceFinished;
			if (finished != null)
				finished (this, e);
		}
		
		private void PoolSourceFinished (object sender, SourceFinishedEventArgs<AudioSource> e)
		{
			OnSourceFinished (new SourceFinishedEventArgs (e.Owner));
		}
	}
}