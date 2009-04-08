using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public class GuestUserProvider
		: IUserProvider
	{
		#region IUserProvider Members

		public bool UserExists (string username)
		{
			lock (userLock)
			{
				return this.users.Contains (username);
			}
		}

		public LoginResult Login (string username, string password)
		{
			if (this.UserExists (username))
				return new LoginResult (false, "User already logged in.");

			lock (userLock)
			{
				this.users.Add (username);
			}

			return new LoginResult (true);
		}

		#endregion

		private object userLock = new object ();
		private HashSet<string> users = new HashSet<string> ();
	}
}