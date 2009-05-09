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

		public void QueuePlayback (IMediaSource mediaSource, byte[] data)
		{
			if (this.context == null)
			{
				this.device.Open();
				this.context = this.device.CreateAndActivateContext();
			}

			Source source = this.pool.RequestSource (mediaSource);

			SourceBuffer buffer = SourceBuffer.Generate();
			buffer.Buffer (data, AudioFormat.Mono16Bit, 44100);

			source.QueueAndPlay (buffer);
		}

		public IEnumerable<IDevice> GetDevices ()
		{
			return OpenAL.PlaybackDevices.Cast<IDevice>();
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
	}
}