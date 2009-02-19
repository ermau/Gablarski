﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Messages
{
	public class LoginMessage
		: ClientMessage
	{
		public LoginMessage (IEndPoint endpoint)
			: base (ClientMessages.Login, endpoint)
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

		protected override void WritePayload (IValueWriter writer)
		{
			writer.WriteString (Nickname);
			writer.WriteString (Username);
			writer.WriteString (Password);
		}

		protected override void ReadPayload (IValueReader reader)
		{
			this.Nickname = reader.ReadString ();
			this.Username = reader.ReadString ();
			this.Password = reader.ReadString ();
		}
	}
}