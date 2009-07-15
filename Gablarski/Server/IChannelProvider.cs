using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	/// <summary>
	/// Contract for providers of channels
	/// </summary>
	public interface IChannelProvider
	{
		/// <summary>
		/// Fired when the channel list or default channel is updated from outside of Gablarski.
		/// </summary>
		event EventHandler ChannelsUpdatedExternally;

		/// <summary>
		/// Gets the type used to uniquely identify a channel.
		/// </summary>
		Type IdentifyingType { get; }

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
		ChannelEditResult SaveChannel (Channel channel);

		/// <summary>
		/// Deletes the <paramref name="channel"/>.
		/// </summary>
		/// <param name="channel">The channel to delete.</param>
		void DeleteChannel (Channel channel);
	}

	public static class ChannelProviderExtensions
	{
		/// <summary>
		/// Gets the default channel or the first channel if no default set.
		/// </summary>
		/// <param name="self">The <c>IChannelProvider</c> to retrieve the channels from.</param>
		/// <returns>The default channel or the first channel if no default set.</returns>
		public static Channel GetDefaultOrFirst (this IChannelProvider self)
		{
			return (self.DefaultChannel ?? self.GetChannels ().FirstOrDefault ());
		}
	}

	public enum ChannelEditResult
		: byte
	{
		/// <summary>
		/// Failed for an unknown reason.
		/// </summary>
		FailedUnknown = 0,

		/// <summary>
		/// Great Success!
		/// </summary>
		Success = 1,

		/// <summary>
		/// Failed because the player does not have sufficient permissions.
		/// </summary>
		FailedPermissions = 2,

		/// <summary>
		/// Failed because no channels are updateable.
		/// </summary>
		FailedChannelsReadOnly = 3,

		/// <summary>
		/// Failed because the channel is marked as readonly.
		/// </summary>
		FailedChannelReadOnly = 4,

		/// <summary>
		/// Failed because channel doesn't exist on the server.
		/// </summary>
		FailedChannelDoesntExist = 5,

		/// <summary>
		/// Failed because you can not delete the last remaining channel.
		/// </summary>
		FailedLastChannel = 6,

		/// <summary>
		/// Failed because a channel with this name already exists.
		/// </summary>
		FailedChannelExists = 7,
	}
}