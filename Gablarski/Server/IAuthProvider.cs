using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski.Server
{
	public interface IAuthProvider
	{
		bool CheckUserExists (string username);
		bool CheckUserLoggedIn (string username);

		/// <summary>
		/// Logs the user in and returns their user instance.
		/// </summary>
		/// <returns>The User instance if login succeeded, <c>null</c> if failed.</returns>
		User Login (string username, string password);
	}
}