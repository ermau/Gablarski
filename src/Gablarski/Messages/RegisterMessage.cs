using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cadenza;

namespace Gablarski.Messages
{
	public class RegisterMessage
		: ClientMessage
	{
		public RegisterMessage()
			: base (ClientMessageType.Register)
		{
		}

		public RegisterMessage (string username, string password)
			: base (ClientMessageType.Register)
		{
			if (username == null)
				throw new ArgumentNullException ("username");
			if (username.IsNullOrWhitespace())
				throw new ArgumentException ("username");
			if (password == null)
				throw new ArgumentNullException ("password");
			if (password.IsNullOrWhitespace())
				throw new ArgumentException ("password");

			Username = username;
			Password = password;
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

		public override void WritePayload(IValueWriter writer)
		{
			writer.WriteString (Username);
			writer.WriteString (Password);
		}

		public override void ReadPayload(IValueReader reader)
		{
			Username = reader.ReadString();
			Password = reader.ReadString();
		}
	}
}