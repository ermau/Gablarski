using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class ChannelChangeMessage
		: ClientMessage
	{
		public ChannelChangeMessage ()
			: base (ClientMessageType.ChangeChannel)
		{
		}

		public ChannelChangeMessage (int targetUserId, int targetChannelId)
			: this()
		{
			this.MoveInfo = new ChannelChangeInfo (targetUserId, targetChannelId);
		}

		public ChannelChangeMessage (int targetUserId, int targetChannelId, int requestingPlayerId)
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