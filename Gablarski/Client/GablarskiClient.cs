using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Server;
using Gablarski.Network;

namespace Gablarski.Client
{
	public class GablarskiClient
	{
		public GablarskiClient (IClientConnection connection)
		{
			this.connection = connection;
		}

		public void Connect (string host, int port)
		{
			connection.Connect (host, port);
		}

		public void Login ()
		{

		}

		private readonly IClientConnection connection;
	}
}