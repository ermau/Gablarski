using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public class LoginResult
	{
		public LoginResult (bool success, IUser user)
		{
			this.Succeeded = success;
			this.User = user;
		}

		public LoginResult (LoginFailureReason reason)
		{
			this.Succeeded = false;
			this.FailureReason = reason;
		}

		public bool Succeeded
		{
			get; private set;
		}

		public IUser User
		{
			get; private set;
		}

		public LoginFailureReason FailureReason
		{
			get; private set;
		}
	}

	public enum LoginFailureReason
	{
		NicknameOwned,
		NicknameUsed,
		UserDoesntExist,
		UserLoggedIn
	}
}