using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gablarski
{
	public class User
		: TokenedClient
	{
		internal User (int authHash, IConnection recipient)
			: base (authHash, recipient)
		{
		}

		internal User (TokenedClient client)
			: base (client)
		{
		}

		public virtual string Nickname
		{
			get;
			set;
		}

		public virtual string Username
		{
			get;
			set;
		}
	}
}