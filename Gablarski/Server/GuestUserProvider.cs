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
			return false;
		}

		public LoginResult Login (string username, string password)
		{
			return new LoginResult (true);
		}

		#endregion

		private object userLock = new object ();
	}
}