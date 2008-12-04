using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public abstract class User
	{
		protected User (string nickname)
		{
			this.Nickname = nickname;
		}

		public abstract uint ID
		{
			get;
		}

		public string Nickname
		{
			get;
			private set;
		}

		public string Username
		{
			get { return (this.username ?? this.Nickname); }
		}

		private string username;
	}
}