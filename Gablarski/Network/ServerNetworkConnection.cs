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
		public ServerNetworkConnection (TcpClient client)
			: base (client)
		{
			this.rstream = tcp.GetStream ();
			this.rwriter = new StreamValueWriter (this.rstream);
			this.rreader = new StreamValueReader (this.rstream);

			this.StartListener ();
		}
	}
}