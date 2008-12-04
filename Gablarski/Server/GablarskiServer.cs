using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Client;
using Lidgren.Network;

namespace Gablarski.Server
{
	public class GablarskiServer
	{
		public event EventHandler<ServerEventArgs> ClientConnected;

		public bool IsRunning
		{
			get;
			private set;
		}

		public int Port
		{ 
			get { return this.port; }
			set { this.port = value; }
		}

		public int MaxConnections
		{
			get { return this.maxConnections; }
			set { this.maxConnections = value; }
		}

		public void Start ()
		{
			if (this.IsRunning)
				throw new InvalidOperationException ("Server already running.");

			this.IsRunning = true;

			this.ServerThread = new Thread (this.ServerRunner);
			this.ServerThread.Name = "Gablarski Server THread";
			this.ServerThread.Start ();
		}

		public void Shutdown ()
		{
			this.IsRunning = false;
			Server.Connections.ForEach (nc => nc.Disconnect ("Server shutting down.", 0.5f));
			this.ServerThread.Join();

			Server.Dispose();
		}

		private int port = 6112;
		private int maxConnections = 128;
		private NetServer Server;
		private Thread ServerThread;

		private void ServerRunner ()
		{
			NetConfiguration config = new NetConfiguration("Gablarski");
			config.MaxConnections = this.MaxConnections;
			config.Port = this.Port;

			Trace.WriteLine ("Starting listener...");
			
			Server = new NetServer(config);
			Server.Start();

			NetBuffer buffer = Server.CreateBuffer();

			while (this.IsRunning)
			{
				NetConnection sender;
				NetMessageType type;
				while (Server.ReadMessage (buffer, out type, out sender))
				{
					ServerEventArgs e = new ServerEventArgs (sender, buffer);

					switch (type)
					{
						case NetMessageType.StatusChanged:
							{
								if (sender.Status == NetConnectionStatus.Connected)
									sender.Tag = new UserConnection(GenerateHash(), Server);

								this.OnClientConnected (e);

								break;
							}

						case NetMessageType.Data:
							byte sanity = buffer.ReadByte();
							if (sanity != Message<ServerMessages>.FirstByte)
								continue;

							ClientMessages messageType = (ClientMessages)buffer.ReadVariableUInt32();

							switch (messageType)
							{
								case ClientMessages.Login:
									this.OnClientLogin (e);
									break;
							}

							break;
					}
				}
			}
		}

		protected virtual void OnClientConnected (ServerEventArgs e)
		{
			ServerMessage msg = new ServerMessage((UserConnection)e.Connection.Tag);
			msg.MessageType = ServerMessages.Connected;
			msg.Send(this.Server, e.Connection, NetChannel.ReliableInOrder1);

			var connected = this.ClientConnected;
			if (connected != null)
				connected (this, e);
		}

		protected virtual void OnClientLogin (ServerEventArgs e)
		{
			
		}

		static int GenerateHash()
		{
			return DateTime.Now.Millisecond + DateTime.Now.Second + 42;
		}
	}

	public class ServerEventArgs
		: EventArgs
	{
		public ServerEventArgs (NetConnection connection, NetBuffer buffer)
		{
			this.Connection = connection;
			this.Buffer = buffer;
		}

		public NetConnection Connection
		{
			get; private set;
		}

		public NetBuffer Buffer
		{
			get; private set;
		}
	}
}