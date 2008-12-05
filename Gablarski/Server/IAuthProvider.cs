using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public interface IAuthProvider
	{
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