using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IUserProvider
	{
		/// <summary>
		/// Gets whether a user exists or not.
		/// </summary>
		/// <param name="username">The username to check.</param>
		/// <returns><c>true</c> if the username exists, <c>false</c> otherwise</returns>
		bool UserExists (string username);

		/// <summary>
		/// Attempts to login a user using the supplied username and password.
		/// </summary>
		/// <param name="username">The username to login with.</param>
		/// <param name="password">The password to login to the username with.</param>
		/// <returns></returns>
		LoginResult Login (string username, string password);
	}

	/// <summary>
	/// Provides results for an attempted login.
	/// </summary>
	public class LoginResult
	{
		public LoginResult (bool success)
		{
			this.Succeeded = success;
		}

		public LoginResult (bool success, string failureReason)
			: this (success)
		{
			this.FailureReason = failureReason;
		}

		/// <summary>
		/// Gets whether the login succeeded or not.
		/// </summary>
		public bool Succeeded
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the reason for a login failure, <c>null</c> otherwise.
		/// </summary>
		public string FailureReason
		{
			get;
			private set;
		}
	}
}