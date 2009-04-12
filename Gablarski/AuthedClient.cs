using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Gablarski.Messages;

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

		public void Send (MessageBase message)
		{
			this.Connection.Send (message);
		}
	}
}