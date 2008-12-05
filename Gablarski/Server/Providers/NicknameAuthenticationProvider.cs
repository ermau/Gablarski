using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Gablarski.Server.Providers
{
	/// <summary>
	/// An authentication provider for no-registration
	/// </summary>
	public class NicknameAuthenticationProvider
		: IAuthProvider
	{
		#region IAuthProvider Members

		public bool CheckUserExists (string username)
		{
			return false;
		}

		public LoginResult Login (string nickname)
		{
			return Login (nickname, null);
		}

		public LoginResult Login (string username, string password)
		{
			Interlocked.Increment (ref this.lastID);

			return new LoginResult (true, new NickAuthUser ((uint)this.lastID, username));
		}

		#endregion

		private int lastID;
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