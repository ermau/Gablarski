using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ClientPingMessage
		: ClientMessage
	{
		public ClientPingMessage()
			: base (ClientMessageType.Ping)
		{
		}

		public override void WritePayload (IValueWriter writer)
		{
		}

		public override void ReadPayload (IValueReader reader)
		{
		}
	}

	public class ServerPingMessage
		: ServerMessage
	{
		public ServerPingMessage()
			: base(ServerMessageType.Ping)
		{
		}

		public override void WritePayload(IValueWriter writer)
		{
		}

		public override void ReadPayload(IValueReader reader)
		{
		}
	}
}