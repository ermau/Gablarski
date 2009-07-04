using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Messages;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;

namespace Gablarski.Network
{
	public class ClientNetworkConnection
		: NetworkConnectionBase, IClientConnection
	{
		#region IClientConnection Members

		public event EventHandler Connected;

		public void Connect (string host, int port)
		{
			if (host == null)
				throw new ArgumentNullException ("host");

			if (tcp == null)
				tcp = new TcpClient();
			if (udp == null)
				udp = new UdpClient();

			tcp.Connect (host, port);
			udp.Connect (host, port);

			this.SetupReadersWriters ();
		}

		public void Connect (IPEndPoint endpoint)
		{
			tcp.Connect (endpoint);
			udp.Connect (endpoint);

			this.SetupReadersWriters ();
		}

		#endregion

		protected override string Name
		{
			get { return "Client"; }
		}

		private void OnConnected (EventArgs e)
		{
			EventHandler connected = this.Connected;
			if (connected != null)
				connected (this, e);
		}

		private void SetupReadersWriters ()
		{
			this.rstream = tcp.GetStream ();
			this.rwriter = new StreamValueWriter (this.rstream);
			this.rreader = new StreamValueReader (this.rstream);

			this.OnConnected (EventArgs.Empty);
			this.StartListener ();
		}
	}
}