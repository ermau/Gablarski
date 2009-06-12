using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class DisconnectMessage
		: ClientMessage
	{
		public DisconnectMessage ()
			: base (ClientMessageType.Disconnect)
		{
		}

		public string Reason
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteString (this.Reason);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Reason = reader.ReadString ();
		}
	}
}