using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	public interface IPlaybackProvider
		: IDisposable
	{
		void QueuePlayback (byte[] data);
	}
}