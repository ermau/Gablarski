using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	[Flags]
	public enum MuteType
		: byte
	{
		User = 1,
		AudioSource = 2,
	}

	public class RequestMuteMessage
		: ClientMessage
	{
		public RequestMuteMessage()
			: base (ClientMessageType.RequestMute)
		{
		}

		public bool Unmute
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
			writer.WriteBool (this.Unmute);
			
			if (this.Type == MuteType.User)
				writer.WriteString ((string)this.Target);
			else
				writer.WriteInt32 ((int)this.Target);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Type = (MuteType)reader.ReadByte();
			this.Unmute = reader.ReadBool();

			if (this.Type == MuteType.User)
				this.Target = reader.ReadString();
			else
				this.Target = reader.ReadInt32();
		}
	}
}