using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class QueryServerResultMessage
		: ServerMessage
	{
		public QueryServerResultMessage()
			: base (ServerMessageType.QueryServerResult)
		{
		}

		public IEnumerable<UserInfo> Users
		{
			get; set;
		}

		public IEnumerable<ChannelInfo> Channels
		{
			get; set;
		}

		public ServerInfo ServerInfo
		{
			get; set;
		}

		#region Overrides of MessageBase

		public override void WritePayload(IValueWriter writer)
		{
			writer.WriteInt32 (this.Users.Count());
			foreach (var u in this.Users)
				u.Serialize (writer);

			writer.WriteInt32 (this.Channels.Count());
			foreach (var c in this.Channels)
				c.Serialize (writer);

			this.ServerInfo.Serialize (writer);
		}

		public override void ReadPayload(IValueReader reader)
		{
			UserInfo[] users = new UserInfo[reader.ReadInt32()];
			for (int i = 0; i < users.Length; ++i)
				users[i] = new UserInfo(reader);
			
			this.Users = users;

			ChannelInfo[] channels = new ChannelInfo[reader.ReadInt32()];
			for (int i = 0; i < channels.Length; ++i)
				channels[i] = new ChannelInfo(reader);

			this.Channels = channels;

			this.ServerInfo = new ServerInfo (reader);
		}

		#endregion
	}
}