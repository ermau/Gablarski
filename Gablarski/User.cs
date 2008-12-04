using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class User
	{
		public User (string nickname)
		{
			this.Nickname = nickname;
		}

		public string Nickname
		{
			get;
			private set;
		}

		public string Username
		{
			get;
			private set;
		}
	}
}