using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class LoginResultMessage
		: ServerMessage
	{
		public LoginResultMessage ()
			: base (ServerMessageType.LoginResult)
		{
		}

		public LoginResultMessage (LoginResult result, UserInfo playerInfo)
			: this()
		{
			this.Result = result;

			if (this.Result.Succeeded && playerInfo == null)
				throw new ArgumentNullException ("playerInfo");

			this.UserInfo = playerInfo;
		}

		public LoginResult Result
		{
			get;
			set;
		}

		public UserInfo UserInfo
		{
			get;
			set;
		}

		public override void ReadPayload (IValueReader reader, IdentifyingTypes idTypes)
		{
			this.Result = new LoginResult(reader, idTypes);

			if (this.Result.Succeeded)
				this.UserInfo = new UserInfo (reader, idTypes);
		}

		public override void WritePayload (IValueWriter writer, IdentifyingTypes idTypes)
		{
			this.Result.Serialize (writer, idTypes);

			if (this.Result.Succeeded)
				this.UserInfo.Serialize (writer, idTypes);
		}
	}
}
