﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Client;

namespace Gablarski.OpenAL.Providers
{
	public class PlaybackProvider
		: IPlaybackProvider
	{
		public PlaybackProvider()
		{
			this.pool.SourceFinished += PoolSourceFinished;
		}

		#region IPlaybackProvider Members
		public event EventHandler<SourceFinishedEventArgs> SourceFinished;

		public IDevice Device
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

			Source source = this.pool.RequestSource (audioSource);
			if (source.ProcessedBuffers > 0)
			{
				SourceBuffer[] freeBuffers = source.Dequeue ();
				for (int i = 0; i < freeBuffers.Length; ++i)
				{
					lock (bufferLock)
					{
						if (!this.buffers.ContainsKey (audioSource))
							this.buffers[audioSource] = new Stack<SourceBuffer> ();

						this.buffers[audioSource].Push (freeBuffers[i]);
					}
				}
			}

			if (data.Length == 0)
				return;

			SourceBuffer buffer;
			lock (bufferLock)
			{
				if (!this.buffers.ContainsKey (audioSource))
				{
					this.buffers[audioSource] = new Stack<SourceBuffer> ();
					this.PushBuffers (audioSource, 10);
				}

				if (this.buffers[audioSource].Count == 0)
					this.PushBuffers (audioSource, 10);

				buffer = this.buffers[audioSource].Pop ();
			}

			//var buffer = SourceBuffer.Generate ();
			buffer.Buffer (data, (audioSource.Channels == 1) ? AudioFormat.Mono16Bit : AudioFormat.Stereo16Bit, (uint)audioSource.Frequency);
			source.QueueAndPlay (buffer);
			pool.PlayingSource (source);
		}

		public IEnumerable<IDevice> GetDevices ()
		{
			return OpenAL.PlaybackDevices.Cast<IDevice>();
		}

		public IDevice DefaultDevice
		{
			get { return OpenAL.DefaultPlaybackDevice; }
		}

		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
			this.device.Dispose();
		}

		#endregion

		private Context context;
		private PlaybackDevice device;
		private readonly SourcePool<AudioSource> pool = new SourcePool<AudioSource>();
		private readonly object bufferLock = new object ();
		private readonly Dictionary<AudioSource, Stack<SourceBuffer>> buffers = new Dictionary<AudioSource, Stack<SourceBuffer>> ();

		private void PushBuffers (AudioSource audioSource, int number)
		{
			SourceBuffer[] sbuffers = SourceBuffer.Generate (number);
			for (int i = 0; i < sbuffers.Length; ++i)
				this.buffers[audioSource].Push (sbuffers[i]);
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