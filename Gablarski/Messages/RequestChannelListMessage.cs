using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class RequestChannelListMessage
		: ClientMessage
	{
		public RequestChannelListMessage ()
			: base (ClientMessageType.RequestChannelList)
		{
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
		}
	}
}