using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class UserConnection
	{
		public UserConnection (User user)
		{
			this.User = user;
		}

		public User User
		{
			get;
			private set;
		}

		public int AuthHash
		{
			get;
			private set;
		}
	}
}