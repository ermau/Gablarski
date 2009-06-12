using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class UserLoggedIn
		: ServerMessage
	{
		public UserLoggedIn()
			: base (ServerMessageType.PlayerLoggedIn)
		{
		}

		public UserLoggedIn (UserInfo playerInfo)
			: base (ServerMessageType.PlayerLoggedIn)
		{
			this.UserInfo = playerInfo;
		}

		public UserInfo UserInfo
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			this.UserInfo.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.UserInfo = new UserInfo (reader);
		}
	}
}