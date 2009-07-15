using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Gablarski.Server
{
	public class LobbyChannelProvider
		: IChannelProvider
	{
		public LobbyChannelProvider ()
		{
			this.lobby = new Channel (1)
			{
				Name = "Lobby",
				Description = String.Empty
			};
		}

		public event EventHandler ChannelsUpdatedExternally;

		public Type IdentifyingType
		{
			get { return typeof (Int32); }
		}

		public bool UpdateSupported
		{
			get { return true; }
		}

		public Channel DefaultChannel
		{
			get { return this.lobby; }
		}

		public IEnumerable<Channel> GetChannels ()
		{
			yield return this.lobby;

			foreach (var c in channels.Values)
				yield return c;
		}

		public ChannelEditResult SaveChannel (Channel channel)
		{
			lock (this.channels)
			{
				if (GetChannels().Any (c => c.Name.ToLower().Trim() == channel.Name.ToLower().Trim()))
					return ChannelEditResult.FailedChannelExists;

				if (channel.ChannelId.Equals (0))
				{
					int id = Interlocked.Increment (ref this.lastId);
					channels.Add (id, new Channel (id, channel));
				}
				else if (channels.ContainsKey (channel.ChannelId))
					channels[channel.ChannelId] = channel;
			}

			return ChannelEditResult.Success;
		}

		public void DeleteChannel (Channel channel)
		{
			lock (this.channels)
			{
				this.channels.Remove (channel.ChannelId);
			}
		}

		private int lastId = 1;
		private readonly Channel lobby;
		private readonly Dictionary<object, Channel> channels = new Dictionary<object, Channel> ();
	}
}