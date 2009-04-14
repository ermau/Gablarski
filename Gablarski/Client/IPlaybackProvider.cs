using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Media.Sources;

namespace Gablarski.Client
{
	public interface IPlaybackProvider
		: IDisposable
	{
		object Device { get; set; }

		void QueuePlayback (IMediaSource source, byte[] data);

		IEnumerable<object> GetDevices ();
	}
}