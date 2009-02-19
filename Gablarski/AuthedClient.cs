using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Gablarski
{
	public class AuthedClient
	{
		internal AuthedClient (int authHash, IEndPoint endpoint)
		{
			this.AuthHash = authHash;
			this.EndPoint = endpoint;
		}

		internal AuthedClient (AuthedClient client)
		{
			this.AuthHash = client.AuthHash;
			this.EndPoint = client.EndPoint;
		}

		public int AuthHash
		{
			get;
			private set;
		}

		public IEndPoint EndPoint
		{
			get;
			private set;
		}
	}
}