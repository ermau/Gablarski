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

		public bool Delete
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			this.Channel.Serialize (writer, idTypes);
			writer.WriteBool (this.Delete);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Channel = new Channel (reader, idTypes);
			this.Delete = reader.ReadBool ();
		}
	}
}