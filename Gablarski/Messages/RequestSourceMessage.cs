using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class RequestSourceMessage
		: ClientMessage
	{
		public RequestSourceMessage ()
			: base (ClientMessages.RequestSource)
		{
		}

		public RequestSourceMessage (int token, MediaType type, byte channels)
			: this ()
		{
			this.Token = token;
			this.MediaType = type;
			this.Channels = channels;
		}

		public int Token
		{
			get;
			set;
		}

		public MediaType MediaType
		{
			get;
			set;
		}

		public int Channels
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.Token);
			writer.WriteByte ((byte)this.MediaType);
			writer.WriteByte ((byte)this.Channels);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Token = reader.ReadInt32 ();
			this.MediaType = (MediaType)reader.ReadByte ();
			this.Channels = reader.ReadByte ();
		}
	}
}