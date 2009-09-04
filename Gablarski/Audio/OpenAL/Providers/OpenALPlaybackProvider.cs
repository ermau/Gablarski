using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Audio.OpenAL.Providers
{
	public class OpenALPlaybackProvider
		: IPlaybackProvider
	{
		//public OpenALPlaybackProvider()
		//{
		//	this.pool.SourceFinished += PoolSourceFinished;
		//}

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

				if (!this.device.IsOpen)
					this.device.Open();

				this.context = this.device.CreateAndActivateContext();
			}
		}

		public int GetBuffersFree (AudioSource source)
		{
			var s = pool.RequestSource (source);
			
			if (s.ProcessedBuffers > 0)
				return s.ProcessedBuffers;
			else if (s.State == SourceState.Initial || s.State == SourceState.Stopped)
				return 5;
			else
				return 0;
		}

		public void QueuePlayback (AudioSource audioSource, byte[] data)
		{
			Stack<SourceBuffer> bufferStack;

			if (!this.buffers.TryGetValue (audioSource, out bufferStack))
				this.buffers[audioSource] = bufferStack = new Stack<SourceBuffer> (SourceBuffer.Generate (100));

			Source source = this.pool.RequestSource (audioSource);

			SourceBuffer[] dbuffers = source.Dequeue();
			for (int i = 0; i < dbuffers.Length; ++i)
				bufferStack.Push (dbuffers[i]);

			if (data.Length == 0)
				return;

			SourceBuffer buffer = bufferStack.Pop();

			buffer.Buffer (data, (audioSource.Channels == 1) ? AudioFormat.Mono16Bit : AudioFormat.Stereo16Bit, (uint)audioSource.Frequency);
			source.QueueAndPlay (buffer);
		}

		public IEnumerable<IAudioDevice> GetDevices ()
		{
			return Audio.OpenAL.OpenAL.PlaybackDevices.Cast<IAudioDevice>();
		}

		public IAudioDevice DefaultDevice
		{
			get { return Audio.OpenAL.OpenAL.DefaultPlaybackDevice; }
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
		//private readonly object bufferLock = new object ();
		private readonly Dictionary<AudioSource, Stack<SourceBuffer>> buffers = new Dictionary<AudioSource, Stack<SourceBuffer>> ();

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