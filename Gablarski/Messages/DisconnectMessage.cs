using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class DisconnectMessage
		: ClientMessage
	{
		public DisconnectMessage (IEndPoint endpoint)
			: base (ClientMessages.Disconnect, endpoint)
		{
		}

		public string Reason
		{
			get;
			set;
		}

		protected override void WritePayload (IValueWriter writer)
		{
			writer.WriteString (this.Reason);
		}

		protected override void ReadPayload (IValueReader reader)
		{
			this.Reason = reader.ReadString ();
		}
	}
}