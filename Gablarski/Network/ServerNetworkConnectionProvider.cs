using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using Gablarski.Server;

namespace Gablarski.Network
{
	public class ServerNetworkConnectionProvider
		: ConnectionProviderBase
	{
		private TcpListener listener;

		protected override void Start ()
		{
			this.listener = new TcpListener (IPAddress.Any, 6112);
			this.listener.Start();
		}

		protected override void Stop ()
		{
			this.listener.Stop();
			this.listener = null;
		}

		protected override IConnection CheckForConnection ()
		{
			try
			{
				if (this.listener == null)
					return null;

				return new ServerNetworkConnection (this.listener.AcceptTcpClient ());
			}
			catch (SocketException sex)
			{
				switch (sex.SocketErrorCode)
				{
					case SocketError.Interrupted:
						return null;

					default:
						throw;
				}
			}
		}
	}
}