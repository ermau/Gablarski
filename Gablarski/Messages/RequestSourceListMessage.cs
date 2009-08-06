using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class RequestSourceListMessage
		: ClientMessage
	{
		public RequestSourceListMessage()
			: base (ClientMessageType.RequestSourceList)
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