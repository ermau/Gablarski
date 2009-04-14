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

		public int Token
		{
			get;
			set;
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
			bool guest = String.IsNullOrEmpty (Username);
			
			if (String.IsNullOrEmpty (this.Password) && !guest)
				throw new InvalidOperationException ("Can not login without a password.");

			writer.WriteInt32 (this.Token);
			writer.WriteBool (guest);
			writer.WriteString (Nickname);

			if (!guest)
			{
				writer.WriteString (Username);
				writer.WriteString (Password);
			}
		}

		public override void ReadPayload (IValueReader reader)
		{
			this.Token = reader.ReadInt32 ();
			bool guest = reader.ReadBool ();

			this.Nickname = reader.ReadString ();
			
			if (!guest)
			{
				this.Username = reader.ReadString ();
				this.Password = reader.ReadString ();
			}
		}
	}
}