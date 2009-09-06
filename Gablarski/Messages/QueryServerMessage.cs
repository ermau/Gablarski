using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class QueryServerMessage
		: ClientMessage
	{
		public QueryServerMessage()
			: base (ClientMessageType.QueryServer)
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