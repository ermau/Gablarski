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
			: base (ClientMessageType.RequestSource)
		{
		}

		public RequestSourceMessage (int token, Type mediaSourceType, byte channels)
			: this ()
		{
			this.Token = token;
			this.MediaSourceType = mediaSourceType;
			this.Channels = channels;
		}

		public int Token
		{
			get;
			set;
		}

		public Type MediaSourceType
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
			writer.WriteString (this.MediaSourceType.AssemblyQualifiedName);
			writer.WriteByte ((byte)this.Channels);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Token = reader.ReadInt32 ();
			this.MediaSourceType = Type.GetType (reader.ReadString ());
			this.Channels = reader.ReadByte ();
		}
	}
}