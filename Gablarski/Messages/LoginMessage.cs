using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class LoginMessage
		: ClientMessage
	{
		public LoginMessage ()
			: base (ClientMessageType.Login)
		{
		}

		public string Nickname
		{
			get;
			set;
		}

		public string Username
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}

		public override void WritePayload (IValueWriter writer)
		{
			writer.WriteString (Nickname);
			writer.WriteString (Username);
			writer.WriteString (Password);
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Nickname = reader.ReadString ();
			this.Username = reader.ReadString ();
			
			if (!String.IsNullOrEmpty (this.Username))
				this.Password = reader.ReadString ();
		}
	}
}