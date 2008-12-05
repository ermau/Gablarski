using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public class LoginResult
	{
		public LoginResult (bool success, User user)
		{
			this.Succeeded = success;
			this.User = user;
		}

		public LoginResult (bool success, User user, string failureReason)
		{
			this.Succeeded = success;
			this.User = user;
			this.FailureReason = failureReason;
		}

		public bool Succeeded
		{
			get; private set;
		}

		public User User
		{
			get; private set;
		}

		public string FailureReason
		{
			get; private set;
		}
	}
}