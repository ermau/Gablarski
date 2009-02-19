using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Gablarski
{
	public class AuthedClient
	{
		internal AuthedClient (int authHash, IConnection connection)
		{
			this.AuthHash = authHash;
			this.Connection = connection;
		}

		internal AuthedClient (AuthedClient client)
		{
			this.AuthHash = client.AuthHash;
			this.Connection = client.Connection;
		}

		public int AuthHash
		{
			get;
			private set;
		}

		public IConnection Connection
		{
			get;
			private set;
		}
	}
}