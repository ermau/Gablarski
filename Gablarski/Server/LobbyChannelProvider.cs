using System;
using System.Linq;
using System.Collections.Generic;

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

		public void SaveChannel (Channel channel)
		{
			lock (this.channels)
			{
				if (channel.ChannelId == 0)
				{
					long id = ++this.lastId;
					channels.Add (id, new Channel (id, channel));
				}
				//else if (!Channel.ReferenceEquals (channel, channel[channel.ChannelId]))
				//{

				//}
			}
		}

		public void DeleteChannel (Channel channel)
		{
			lock (this.channels)
			{
				this.channels.Remove (channel.ChannelId);
			}
		}

		private long lastId = 1;
		private readonly Channel lobby;
		private readonly Dictionary<long, Channel> channels = new Dictionary<long, Channel> ();
	}
}