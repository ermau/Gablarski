using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public class UserEventArgs
		: EventArgs
	{
		public UserEventArgs (User user)
		{
			this.User = user;
		}

		public User User
		{
			get;
			private set;
		}
	}
}