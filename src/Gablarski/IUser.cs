using System;
using System.Collections.Generic;
using System.Linq;

namespace Gablarski
{
	public interface IUser
	{
		int UserId { get; }
		string Username { get; }
	}
}