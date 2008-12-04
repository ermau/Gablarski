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

		public User Login (string username, string password)
		{
			rwl.EnterUpgradeableReadLock ();
			if (!users.ContainsKey (username))
			{
				NickAuthUser user = new NickAuthUser ((uint)users.Count, username);

				rwl.EnterWriteLock ();
				users.Add (username, user);
				rwl.ExitWriteLock ();
				rwl.ExitUpgradeableReadLock ();
				
				return user;
			}
			else
			{
				rwl.ExitUpgradeableReadLock ();
				return null;
			}
		}

		#endregion

		private readonly static ReaderWriterLockSlim rwl = new ReaderWriterLockSlim ();
		private readonly static Dictionary<string, User> users = new Dictionary<string, User> ();
	}

	public class NickAuthUser
		: User
	{
		internal NickAuthUser (uint userID, string nickname)
			: base (nickname)
		{
			this.id = userID;
		}

		public override uint ID
		{
			get { return this.id; }
		}

		private readonly uint id;
	}
}