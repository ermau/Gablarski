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
		public event EventHandler ClientConnected;

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
					switch (type)
					{
						case NetMessageType.StatusChanged:
							{
								if (sender.Status == NetConnectionStatus.Connected)
									sender.Tag = new UserConnection(GenerateHash(), Server);

								ConnectionHandler (this, sender, buffer);

								break;
							}

						case NetMessageType.Data:
							byte sanity = buffer.ReadByte();
							if (sanity != Message<ServerMessages>.FirstByte)
								continue;

							ClientMessages messageType = (ClientMessages)buffer.ReadVariableUInt32();

							if (Handlers.ContainsKey (messageType))
								Handlers[messageType](this, sender, buffer);

							break;
					}
				}
			}
		}

		protected virtual void OnClientConnected (EventArgs e)
		{
			var connected = this.ClientConnected;
			if (connected != null)
				connected (this, e);
		}

		private static Dictionary<ClientMessages, Action<GablarskiServer, NetConnection, NetBuffer>> Handlers;
		static GablarskiServer()
		{
			Handlers = new Dictionary<ClientMessages, Action<GablarskiServer, NetConnection, NetBuffer>>
			{
				{ ClientMessages.Login, LoginHandler }
			};

		}

		static int GenerateHash()
		{
			return DateTime.Now.Millisecond + DateTime.Now.Second + 42;
		}

		static void ConnectionHandler (GablarskiServer server, NetConnection connection, NetBuffer buffer)
		{
			ServerMessage msg = new ServerMessage((UserConnection)connection.Tag);
			msg.MessageType = ServerMessages.Connected;
			msg.Send (server.Server, connection, NetChannel.ReliableInOrder1);
		}

		static void LoginHandler (GablarskiServer server, NetConnection connection, NetBuffer buffer)
		{
			
		}
	}
}