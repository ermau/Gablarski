using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ChannelEditMessage
		: ClientMessage
	{
		public ChannelEditMessage ()
			: base (ClientMessageType.EditChannel)
		{
		}

		public ChannelEditMessage (Channel channel)
			: this()
		{
			this.Channel = channel;
		}

		public Channel Channel
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			this.Channel.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Channel = new Channel (reader);
		}
	}
}