using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Gablarski.Media.Sources;

namespace Gablarski.OpenAL.Providers
{
	public class PlaybackProvider
		: IPlaybackProvider
	{
		#region IPlaybackProvider Members

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

		public void QueuePlayback (IMediaSource mediaSource, byte[] data, int frequency)
		{
			if (!this.device.IsOpen)
				this.device.Open();

			if (this.context == null)
				this.context = this.device.CreateAndActivateContext();

			Source source = this.pool.RequestSource (mediaSource);
			if (source.ProcessedBuffers > 0)
			{
				SourceBuffer[] freeBuffers = source.Dequeue ();
				for (int i = 0; i < freeBuffers.Length; ++i)
				{
					lock (bufferLock)
					{
						if (!this.buffers.ContainsKey (mediaSource))
							this.buffers[mediaSource] = new Stack<SourceBuffer> ();

						this.buffers[mediaSource].Push (freeBuffers[i]);
					}
				}
			}

			if (data.Length == 0)
				return;

			SourceBuffer buffer = null;
			lock (bufferLock)
			{
				if (!this.buffers.ContainsKey (mediaSource))
				{
					this.buffers[mediaSource] = new Stack<SourceBuffer> ();
					this.PushBuffers (mediaSource, 10);
				}

				if (this.buffers[mediaSource].Count == 0)
					this.PushBuffers (mediaSource, 10);

				buffer = this.buffers[mediaSource].Pop ();
			}

			//var buffer = SourceBuffer.Generate ();
			buffer.Buffer (data, AudioFormat.Mono16Bit, (uint)frequency);
			source.QueueAndPlay (buffer);
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
		private readonly SourcePool<IMediaSource> pool = new SourcePool<IMediaSource>();
		private object bufferLock = new object ();
		private readonly Dictionary<IMediaSource, Stack<SourceBuffer>> buffers = new Dictionary<IMediaSource, Stack<SourceBuffer>> ();

		private void PushBuffers (IMediaSource mediaSource, int number)
		{
			SourceBuffer[] sbuffers = SourceBuffer.Generate (number);
			for (int i = 0; i < sbuffers.Length; ++i)
				this.buffers[mediaSource].Push (sbuffers[i]);
		}
	}
}