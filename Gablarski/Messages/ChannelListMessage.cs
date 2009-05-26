using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ChannelListMessage
		: ServerMessage
	{
		public ChannelListMessage ()
			: base (ServerMessageType.ChannelListReceived)
		{

		}

		public ChannelListMessage (IEnumerable<Channel> channels)
			: this()
		{
			this.Channels = channels;
		}

		public IEnumerable<Channel> Channels
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.Channels.Count ());
			foreach (var c in this.Channels)
				c.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			Channel[] channels = new Channel[reader.ReadInt32 ()];
			for (int i = 0; i < channels.Length; ++i)
				channels[i] = new Channel (reader);

			this.Channels = channels;
		}
	}
}