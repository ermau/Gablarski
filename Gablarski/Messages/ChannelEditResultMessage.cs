using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;

namespace Gablarski.Messages
{
	public class ChannelEditResultMessage
		: ServerMessage
	{
		public ChannelEditResultMessage ()
			: base (ServerMessageType.ChannelEditResult)
		{
		}

		public ChannelEditResultMessage (Channel channel, ChannelEditResult result)
			: this ()
		{
			this.ChannelId = channel.ChannelId;
			this.Result = result;
		}

		public object ChannelId
		{
			get;
			set;
		}

		public ChannelEditResult Result
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			idTypes.WriteChannel (writer, this.ChannelId);
			writer.WriteByte ((byte)this.Result);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.ChannelId = idTypes.ReadChannel (reader);
			this.Result = (ChannelEditResult)reader.ReadByte();
		}
	}
}