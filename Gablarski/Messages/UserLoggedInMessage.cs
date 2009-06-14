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

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			this.UserInfo.Serialize (writer, idTypes);
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.UserInfo = new UserInfo (reader, idTypes);
		}
	}
}