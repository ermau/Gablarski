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

		public override void WritePayload (IValueWriter writer)
		{
			this.ChangeInfo.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.ChangeInfo = new ChannelChangeInfo (reader);
		}
	}
}