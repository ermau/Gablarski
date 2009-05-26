using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public interface IChannelProvider
	{
		/// <summary>
		/// Fired when the channel list or default channel is updated from outside of Gablarski.
		/// </summary>
		event EventHandler ChannelsUpdatedExternally;

		/// <summary>
		/// Gets whether or not clients can create/update/delete channels.
		/// </summary>
		bool UpdateSupported { get; }

		/// <summary>
		/// Gets the default channel. <c>null</c> if none set or not supported.
		/// </summary>
		/// <remarks>
		/// If no default channel is set or supported, the first channel returned from GetChannels will be used.
		/// </remarks>
		Channel DefaultChannel { get; }

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

	public static class ChannelProviderExtensions
	{
		public static Channel GetDefaultOrFirst (this IChannelProvider self)
		{
			return (self.DefaultChannel ?? self.GetChannels ().FirstOrDefault ());
		}
	}
}