using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public ServerNetworkConnectionProvider ()
			: this (6112)
		{
		}

		public ServerNetworkConnectionProvider (int port)
		{
			this.Port = port;
		}

		public int Port
		{
			get;
			private set;
		}

		private TcpListener listener;

		protected override void Start ()
		{
			Trace.WriteLine ("[Network] Server listening on port " + this.Port);
			this.listener = new TcpListener (IPAddress.Any, this.Port);
			this.listener.Start();
		}

		protected override void Stop ()
		{
			Trace.WriteLine ("[Network] Server stopped listening to port " + this.Port);
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