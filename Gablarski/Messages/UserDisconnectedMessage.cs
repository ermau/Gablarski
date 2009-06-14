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
			: base (ServerMessageType.UserDisconnected)
		{
			
		}

		public UserDisconnectedMessage (object userId)
			: this()
		{
			this.UserId = userId;
		}

		public object UserId
		{
			get;
			private set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			idTypes.WriteUser (writer, this.UserId);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.UserId = idTypes.ReadUser (reader);
		}
	}
}