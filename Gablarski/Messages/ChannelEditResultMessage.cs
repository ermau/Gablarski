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

		public ChannelEditResultMessage (ChannelInfo channel, ChannelEditResult result)
			: this ()
		{
			this.ChannelId = channel.ChannelId;
			this.Result = result;
		}

		public int ChannelId
		{
			get;
			set;
		}

		public ChannelEditResult Result
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.ChannelId);
			writer.WriteByte ((byte)this.Result);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.ChannelId = reader.ReadInt32();
			this.Result = (ChannelEditResult)reader.ReadByte();
		}
	}
}