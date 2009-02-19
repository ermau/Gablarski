using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IChannelProvider
	{
		/// <summary>
		/// Gets whether or not clients can create/update/delete channels.
		/// </summary>
		bool UpdateSupported { get; }

		/// <summary>
		/// Gets a listing channels from the underlying source.
		/// </summary>
		/// <returns>The listing of channels.</returns>
		IEnumerable<Channel> GetChannels ();

		/// <summary>
		/// Creates or updates the <paramref name="channel"/>.
		/// </summary>
		/// <param name="channel">The channel to create or update.</param>
		void SaveChannel (Channel channel);

		/// <summary>
		/// Deletes the <paramref name="channel"/>.
		/// </summary>
		/// <param name="channel">The channel to delete.</param>
		void DeleteChannel (Channel channel);
	}
}