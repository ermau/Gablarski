using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class User
		: AuthedClient
	{
		internal User (int authHash, bool isLittleEndian)
			: base (authHash, isLittleEndian)
		{
		}

		internal User (AuthedClient client)
			: base (client.AuthHash, client.IsLittleEndian)
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