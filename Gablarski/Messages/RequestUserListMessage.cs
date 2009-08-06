using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class RequestUserListMessage
		: ClientMessage
	{
		public RequestUserListMessage ()
			: base (ClientMessageType.RequestUserList)
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