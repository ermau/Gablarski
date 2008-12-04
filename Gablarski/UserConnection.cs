using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class UserConnection
	{
		public UserConnection (int hash)
		{
			this.AuthHash = hash;
		}

		public User User
		{
			get;
			set;
		}

		public int AuthHash
		{
			get;
			private set;
		}
	}
}