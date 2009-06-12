using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ChangeChannelMessage
		: ClientMessage
	{
		public ChangeChannelMessage ()
			: base (ClientMessageType.ChangeChannel)
		{
		}

		public ChangeChannelMessage (object targetUserId, object targetChannelId)
			: this()
		{
			this.MoveInfo = new ChannelChangeInfo (targetUserId, targetChannelId);
		}

		public ChangeChannelMessage (object targetUserId, object targetChannelId, object requestingPlayerId)
			: this ()
		{
			this.MoveInfo = new ChannelChangeInfo (targetUserId, targetChannelId, requestingPlayerId);
		}

		public ChannelChangeInfo MoveInfo
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			this.MoveInfo.Serialize (writer, idTypes);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.MoveInfo = new ChannelChangeInfo (reader, idTypes);
		}
	}
}