using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Gablarski.Server.Telnet
{
	public class TelnetConnectionProvider
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
			return new TelnetConnection (this.listener.AcceptTcpClient());
		}
	}
}
