using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class JoinResultMessage
		: ServerMessage
	{
		public JoinResultMessage()
			: base (ServerMessageType.JoinResult)
		{
		}

		public JoinResultMessage (LoginResultState state, UserInfo user)
			: this()
		{
			this.Result = state;
			this.UserInfo = user;
		}

		public LoginResultState Result
		{
			get; set;
		}
		
		public UserInfo UserInfo
		{
			get;
			set;
		}

		#region Overrides of MessageBase

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteInt32 ((int)this.Result);
			
			if (this.UserInfo != null)
				this.UserInfo.Serialize (writer);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Result = (LoginResultState)reader.ReadInt32();

			if (this.Result == LoginResultState.Success)
				this.UserInfo = new UserInfo (reader);
		}

		#endregion
	}
}