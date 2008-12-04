using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace Gablarski.Client
{
	public class ClientConnection
	{
		public void Connect (string host, int port)
		{
			this.client.Connect (host, port);
		}

		public void Disconnect ()
		{
			this.client.Disconnect (String.Empty);
		}

		private readonly NetClient client = new NetClient (new NetConfiguration ("Gablarski"));
	}
}