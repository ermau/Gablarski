using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class UserLoggedInMessage
		: ServerMessage
	{
		public UserLoggedInMessage()
			: base (ServerMessageType.UserLoggedIn)
		{
		}

		public UserLoggedInMessage (UserInfo userInfo)
			: base (ServerMessageType.UserLoggedIn)
		{
			this.UserInfo = userInfo;
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