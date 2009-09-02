using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class MutedMessage
		: ServerMessage
	{
		public MutedMessage()
			: base (ServerMessageType.Muted)
		{
		}

		public bool Unmuted
		{
			get; set;
		}

		public object Target
		{
			get; set;
		}

		public MuteType Type
		{
			get; set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteByte ((byte)this.Type);
			writer.WriteBool (this.Unmuted);
			
			if ((this.Type & MuteType.User) == MuteType.User)
				writer.WriteString ((string)this.Target);
			else
				writer.WriteInt32 ((int)this.Target);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Type = (MuteType)reader.ReadByte();
			this.Unmuted = reader.ReadBool();

			if ((this.Type & MuteType.User) == MuteType.User)
				this.Target = reader.ReadString();
			else
				this.Target = reader.ReadInt32();
		}
	}
}
