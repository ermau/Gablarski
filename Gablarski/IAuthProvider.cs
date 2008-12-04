using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public interface IAuthProvider
	{
		bool CheckUserExists (string username);
		bool CheckUserLoggedIn (string username);
		bool Login (string username, string password);
	}
}