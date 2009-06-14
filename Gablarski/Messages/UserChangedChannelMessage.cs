using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class UserChangedChannelMessage
		: ServerMessage
	{
		public UserChangedChannelMessage()
			: base (ServerMessageType.UserChangedChannel)
		{
		}

		public ChannelChangeInfo ChangeInfo
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			this.ChangeInfo.Serialize (writer, idTypes);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.ChangeInfo = new ChannelChangeInfo (reader, idTypes);
		}
	}
}