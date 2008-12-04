using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gablarski.Server.Providers
{
	public class NicknameAuthenticationProvider
		: IAuthProvider
	{
		#region IAuthProvider Members

		public bool CheckUserExists (string username)
		{
			return true;
		}

		public bool CheckUserLoggedIn (string username)
		{
			rwl.EnterReadLock ();
			bool loggedIn = users.ContainsKey (username);
			rwl.ExitReadLock ();

			return loggedIn;
		}

		public bool Login (string username, string password)
		{
			rwl.EnterUpgradeableReadLock ();
			if (!users.ContainsKey (username))
			{
				rwl.EnterWriteLock ();
				users.Add (username, new User (username));
				rwl.ExitWriteLock ();
				rwl.ExitUpgradeableReadLock ();
				return true;
			}
			else
			{
				rwl.ExitUpgradeableReadLock ();
				return false;
			}
		}

		#endregion

		private readonly static ReaderWriterLockSlim rwl = new ReaderWriterLockSlim ();
		private readonly static Dictionary<string, User> users = new Dictionary<string, User> ();
	}
}