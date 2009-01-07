using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public interface IAuthProvider
	{
		/// <summary>
		/// Gets whether or not unregistered users are supported
		/// </summary>
		bool GuestsAllowed { get; }

		/// <summary>
		/// Checks to see if the user exists.
		/// </summary>
		/// <param name="username">The username to check</param>
		/// <returns><c>true</c> if the user exists, <c>false</c> if not</returns>
		bool CheckUserExists (string username);

		/// <summary>
		/// Attempts to log the user in without a registered login.
		/// </summary>
		/// <param name="nickname">Temporary, unregistered nickname.</param>
		/// <returns>The User instance if login succeeded, <c>null</c> if failed.</returns>
		LoginResult Login (string nickname);

		/// <summary>
		/// Logs the user in and returns their user instance.
		/// </summary>
		/// <returns>The User instance if login succeeded, <c>null</c> if failed.</returns>
		LoginResult Login (string username, string password);
	}
}