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

		public UserDisconnectedMessage (int userId)
			: this()
		{
			this.UserId = userId;
		}

		public int UserId
		{
			get;
			private set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 (this.UserId);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.UserId = reader.ReadInt32();
		}
	}
}