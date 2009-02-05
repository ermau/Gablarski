using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IChannelProvider
	{
		/// <summary>
		/// Gets whether or not clients can create channels
		/// </summary>
		bool CreateSupported { get; }

		/// <summary>
		/// Gets a listing channels from the underlying source.
		/// </summary>
		/// <returns>The listing of channels</returns>
		IEnumerable<IChannel> GetChannels ();
	}
}