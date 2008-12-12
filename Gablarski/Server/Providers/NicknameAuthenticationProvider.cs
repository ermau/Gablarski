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
		: IUser
	{
		internal NickAuthUser (uint userID, string nickname)
		{
			this.ID = userID;
			this.Nickname = nickname;

			this.State = (userID == 1) ? UserState.Registered : UserState.Unregistered;
		}

		#region IUser Members

		public uint ID
		{
			get; set;
		}

		public string Nickname
		{
			get; set;
		}

		public string Username
		{
			get; set;
		}

		public UserState State
		{
			get; set;
		}

		public ChannelInfo Channel
		{
			get; set;
		}

		#endregion
	}
}