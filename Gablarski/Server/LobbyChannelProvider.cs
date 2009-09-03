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
			this.lobby = new ChannelInfo (1)
			{
				Name = "Lobby",
				Description = String.Empty,
				ReadOnly = true
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

		public ChannelInfo DefaultChannel
		{
			get { return this.lobby; }
		}

		public IEnumerable<ChannelInfo> GetChannels ()
		{
			yield return this.lobby;

			foreach (var c in channels.Values)
				yield return c;
		}

		public ChannelEditResult SaveChannel (ChannelInfo channel)
		{
			lock (this.channels)
			{
				if (GetChannels().Any (c => c.Name.ToLower().Trim() == channel.Name.ToLower().Trim()))
					return ChannelEditResult.FailedChannelExists;

				if (channel.ChannelId.Equals (0))
				{
					int id = Interlocked.Increment (ref this.lastId);
					channels.Add (id, new ChannelInfo (id, channel));
				}
				else if (channels.ContainsKey (channel.ChannelId))
					channels[channel.ChannelId] = channel;
				else
					return ChannelEditResult.FailedChannelDoesntExist;
			}

			return ChannelEditResult.Success;
		}

		public ChannelEditResult DeleteChannel (ChannelInfo channel)
		{
			lock (this.channels)
			{
				if (this.channels.Remove (channel.ChannelId))
					return ChannelEditResult.Success;
				else
                    return ChannelEditResult.FailedChannelDoesntExist;
			}
		}

		private int lastId = 1;
		private readonly ChannelInfo lobby;
		private readonly Dictionary<object, ChannelInfo> channels = new Dictionary<object, ChannelInfo> ();
	}
}