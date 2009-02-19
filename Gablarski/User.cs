using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class User
		: AuthedClient
	{
		internal User (int authHash, IConnection recipient)
			: base (authHash, recipient)
		{
		}

		internal User (AuthedClient client)
			: base (client)
		{
		}

		public string Nickname
		{
			get;
			internal set;
		}

		public string Username
		{
			get;
			internal set;
		}
	}
}