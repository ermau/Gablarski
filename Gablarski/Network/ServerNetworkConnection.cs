using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Gablarski.Messages;
using System.Threading;

namespace Gablarski.Network
{
	public class ServerNetworkConnection
		: NetworkConnectionBase
	{
		public const int DefaultPort = 6112;

		public ServerNetworkConnection (TcpClient client)
			: base (client)
		{
			this.rstream = tcp.GetStream ();
			this.rwriter = new StreamValueWriter (this.rstream);
			this.rreader = new StreamValueReader (this.rstream);

			this.StartListener ();
		}

		protected override string Name
		{
			get { return "Server"; }
		}
	}
}