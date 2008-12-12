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

		public LoginResult (bool success, IUser user, string failureReason)
		{
			this.Succeeded = success;
			this.User = user;
			this.FailureReason = failureReason;
		}

		public bool Succeeded
		{
			get; private set;
		}

		public IUser User
		{
			get; private set;
		}

		public string FailureReason
		{
			get; private set;
		}
	}
}