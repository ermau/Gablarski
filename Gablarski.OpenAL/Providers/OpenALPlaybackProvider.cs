using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.OpenAL;

namespace Gablarski.Client.Providers
{
	public class OpenALPlaybackProvider
		: IPlaybackProvider
	{
		public OpenALPlaybackProvider ()
		{
			this.playback = OpenAL.OpenAL.DefaultPlaybackDevice.Open ();
			this.context = this.playback.CreateAndActivateContext ();
		}

		#region IPlaybackProvider Members

		public void QueuePlayback (byte[] data, IMediaSource mediaSource)
		{
			Source source = pool.RequestSource (mediaSource);
			
			SourceBuffer buffer = SourceBuffer.Generate ();
			buffer.Buffer (data, AudioFormat.Mono16Bit, 44100);

			source.QueueAndPlay (buffer);
		}

		#endregion

		private readonly PlaybackDevice playback;
		private readonly Context context;

		private SourcePool<IMediaSource> pool = new SourcePool<IMediaSource> ();

		#region IDisposable Members

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}