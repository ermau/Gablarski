using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class UserListMessage
		: ServerMessage
	{
		public UserListMessage()
			: base (ServerMessageType.PlayerListReceived)
		{
		}

		public UserListMessage (IEnumerable<UserInfo> users)
			: this()
		{
			this.Users = users;
		}

		public IEnumerable<UserInfo> Users
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			writer.WriteInt32 (this.Users.Count());
			foreach (UserInfo info in this.Users)
				info.Serialize (writer, idTypes);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			UserInfo[] users = new UserInfo[reader.ReadInt32()];
			for (int i = 0; i < users.Length; ++i)
				users[i] = new UserInfo (reader, idTypes);

			this.Users = users;
		}
	}
}