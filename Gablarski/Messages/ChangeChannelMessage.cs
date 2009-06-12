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

		public ChangeChannelMessage (long targetUserId, long targetChannelId)
			: this()
		{
			this.MoveInfo = new ChannelChangeInfo (targetUserId, targetChannelId);
		}

		public ChangeChannelMessage (long targetUserId, long targetChannelId, long requestingPlayerId)
			: this ()
		{
			this.MoveInfo = new ChannelChangeInfo (targetUserId, targetChannelId, requestingPlayerId);
		}

		public ChannelChangeInfo MoveInfo
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			this.MoveInfo.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.MoveInfo = new ChannelChangeInfo (reader);
		}
	}
}