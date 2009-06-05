using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class RequestPlayerListMessage
		: ClientMessage
	{
		public RequestPlayerListMessage ()
			: base (ClientMessageType.RequestPlayerList)
		{
		}

		public override void WritePayload (IValueWriter writer)
		{
		}

		public override void ReadPayload (IValueReader reader)
		{
		}
	}
}