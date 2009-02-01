using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client.Providers.OpenAL
{
	public class OpenALPlaybackProvider
		: IPlaybackProvider
	{
		#region IPlaybackProvider Members

		public void QueuePlayback (byte[] data, IMediaSource source)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IDisposable Members

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}