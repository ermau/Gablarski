namespace Gablarski
{
	public interface IChannelInfo
	{
		/// <summary>
		/// Gets the ID of this channel.
		/// </summary>
		int ChannelId { get; }

		/// <summary>
		/// Gets or sets the channel ID this is a sub-channel of. default if a main channel.
		/// </summary>
		int ParentChannelId { get; }

		/// <summary>
		/// Gets or sets the name of the channel.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets or sets the description of the channel.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Gets or sets the player limit. 0 for no limit.
		/// </summary>
		int UserLimit { get; }

		/// <summary>
		/// Gets whether this individual channel can be modified or not.
		/// </summary>
		bool ReadOnly { get; }
	}
}