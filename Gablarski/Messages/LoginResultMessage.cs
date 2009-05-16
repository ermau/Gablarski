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

		public LoginResultMessage (LoginResult result, PlayerInfo playerInfo)
			: this()
		{
			this.Result = result;

			if (this.Result.Succeeded && playerInfo == null)
				throw new ArgumentNullException ("playerInfo");

			this.PlayerInfo = playerInfo;
		}

		public LoginResult Result
		{
			get;
			set;
		}

		public PlayerInfo PlayerInfo
		{
			get;
			set;
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Result = new LoginResult(reader);

			if (this.Result.Succeeded)
				this.PlayerInfo = new PlayerInfo (reader);
		}

		public override void WritePayload (IValueWriter writer)
		{
			this.Result.Serialize (writer);

			if (this.Result.Succeeded)
				this.PlayerInfo.Serialize (writer);
		}
	}
}
