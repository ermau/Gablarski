using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace Gablarski.Server
{
	public class GablarskiServer
	{
		public GablarskiServer (int port)
		{
			this.udp = new UdpClient (port);
			this.tcp = new TcpListener (port);
		}

		public void Start ()
		{
			this.listening = true;
			this.tcp.Start ();

			(this.connectionListenerThread = new Thread (this.ConnectionListener)
			{
				Name = "Connection Lister",
				IsBackground = true
			}).Start ();
		}

		public void Stop ()
		{
			this.listening = false;

			if (this.connectionListenerThread != null)
				this.connectionListenerThread.Join ();
		}

		private UdpClient udp;
		private TcpListener tcp;

		private Thread connectionListenerThread;
		private volatile bool listening;

		private object clientLock = new object ();
		private List<AuthedClient> clients = new List<AuthedClient> ();

		private void ConnectionListener ()
		{
			while (this.listening)
			{
				var client = ProcessNewClient (tcp.AcceptTcpClient ());
				if (client != null)
				{
					lock (clientLock)
					{
						clients.Add (client);
					}
				}

				if (!tcp.Pending ())
					Thread.Sleep (100);
			}
		}

		private void Listener ()
		{
			while (this.listening)
			{
				foreach (AuthedClient client in this.clients)
				{
					if (!client.tcp.DataAvailable)
						continue;

					if (!SanityCheck (client.tcp))
						client.tcp.Close ();

					byte[] buffer = new byte[2];
					client.tcp.Read (buffer, 0, 2);

					
				}

				Thread.Sleep (1);
			}
		}

		private bool SanityCheck (NetworkStream stream)
		{
			return (stream.ReadByte () == 42);
		}

		private AuthedClient ProcessNewClient (TcpClient client)
		{
			NetworkStream stream = client.GetStream ();
			int sanity = stream.ReadByte ();
			if (sanity != 42 && sanity != 43)
				return null;

			return new AuthedClient (GenerateHash(), sanity == 43);
		}

		private int GenerateHash ()
		{
			return DateTime.Now.Millisecond + DateTime.Now.Second + 42;
		}
	}
}