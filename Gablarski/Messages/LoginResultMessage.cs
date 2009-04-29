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

		public LoginResultMessage (LoginResult result)
			: this()
		{
			this.Result = result;
		}

		public LoginResult Result
		{
			get;
			set;
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Result = new LoginResult(reader);
		}

		public override void WritePayload (IValueWriter writer)
		{
			this.Result.Serialize (writer);
		}
	}
}
