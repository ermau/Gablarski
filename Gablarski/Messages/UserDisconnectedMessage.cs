using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class UserDisconnectedMessage
		: ServerMessage
	{
		public UserDisconnectedMessage()
			: base (ServerMessageType.PlayerDisconnected)
		{
			
		}

		public UserDisconnectedMessage (object playerId)
			: this()
		{
			this.PlayerId = playerId;
		}

		public object PlayerId
		{
			get;
			private set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			idTypes.WriteUser (writer, this.PlayerId);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.PlayerId = idTypes.ReadUser (reader);
		}
	}
}