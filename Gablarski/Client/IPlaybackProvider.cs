using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Client
{
	/// <summary>
	/// 
	/// </summary>
	public interface IPlaybackProvider
		: IDisposable
	{
		/// <summary>
		/// Queues playback of a media source
		/// </summary>
		/// <param name="data">Media data to queue</param>
		/// <param name="source">The media source the data is from</param>
		void QueuePlayback (byte[] data, IMediaSource source);
	}
}