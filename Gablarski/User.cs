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