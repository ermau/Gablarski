using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Gablarski.Network
{
	public class ClientNetworkConnection
		: NetworkConnectionBase, IClientConnection
	{
		#region IClientConnection Members

		public void Connect (string host, int port)
		{
			tcp.Connect (host, port);
			this.rstream = tcp.GetStream ();
			this.rwriter = new StreamValueWriter (this.rstream);
			this.rreader = new StreamValueReader (this.rstream);

			udp.Connect (host, port);

			this.StartListener ();
		}

		#endregion
	}
}