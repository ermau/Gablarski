using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Server;
using Lidgren.Network;

namespace Gablarski.Client
{
	public class GablarskiClient
	{
		public event EventHandler<ConnectionEventArgs> Connected;
		public event EventHandler<ReasonEventArgs> Disconnected;
		public event EventHandler<ConnectionEventArgs> LoggedIn;

		public event EventHandler<UserEventArgs> UserLogin;
		public event EventHandler<UserEventArgs> UserLogout;

		public bool IsRunning
		{
			get; private set;
		}

		public void Connect (string host, int port)
		{
			if (this.IsRunning)
				throw new InvalidOperationException ("Client already running.");

			this.IsRunning = true;
			this.connecting = true;

			this.client = new NetClient(new NetConfiguration("Gablarski"));

			this.runnerThread = new Thread (this.ClientRunner)
			                    {
			                    	IsBackground = true,
			                    	Name = "Client: " + host + ":" + port
			                    };
			this.runnerThread.Start ();
			
			this.client.Connect (host, port);
		}

		public void Disconnect ()
		{
			if (!this.IsRunning)
				return;

			ClientMessage msg = new ClientMessage (ClientMessages.Disconnect, this.connection);
			msg.Send (this.client, NetChannel.ReliableInOrder1);

			this.IsRunning = false;

			this.client.Shutdown ("Closed.");
		}

		public void Login (string nickname)
		{
			Login (nickname, String.Empty, String.Empty);
		}

		public void Login (string nickname, string username, string password)
		{
			if (!this.IsRunning || (this.connection == null && !this.connecting))
				throw new InvalidOperationException("Must be connected before logging in.");
			else if (this.connecting || this.connection != null)
			{
				while (this.connecting)
					Thread.Sleep (1);
			}

			ClientMessage msg = new ClientMessage (ClientMessages.Login, this.connection);
			NetBuffer buffer = msg.GetBuffer ();
			buffer.Write (nickname);
			buffer.Write (username ?? String.Empty);
			buffer.Write (password ?? String.Empty);

			msg.Send (this.client, NetChannel.ReliableInOrder1);
		}

		private bool connecting;
		private UserConnection connection;
		private Thread runnerThread;
		private NetClient client;

		protected virtual void OnDisconnected (ReasonEventArgs e)
		{
			this.IsRunning = false;
			this.runnerThread.Join();

			this.client.Dispose();

			var dced = this.Disconnected;
			if (dced != null)
				dced (this, e);
		}

		protected virtual void OnConnected (ConnectionEventArgs e)
		{
			this.connecting = false;

			var connected = this.Connected;
			if (connected != null)
				connected (this, e);
		}

		protected virtual void OnLoggedIn (ConnectionEventArgs e)
		{
			var loggedin = this.LoggedIn;
			if (loggedin != null)
				loggedin (this, e);
		}

		protected virtual void OnUserLogin (UserEventArgs e)
		{
			var ulogin = this.UserLogin;
			if (ulogin != null)
				ulogin (this, e);
		}

		protected virtual void OnUserLogout (UserEventArgs e)
		{
			var ulogout = this.UserLogout;
			if (ulogout != null)
				ulogout (this, e);
		}

		private void ClientRunner ()
		{
			NetBuffer buffer = this.client.CreateBuffer();

			while (this.IsRunning)
			{
				NetMessageType type;
				
				while (this.client.ReadMessage (buffer, out type))
				{
					switch (type)
					{
						case NetMessageType.StatusChanged:
						{
							if (this.client.Status == NetConnectionStatus.Disconnected)
								this.OnDisconnected (new ReasonEventArgs (this.connection, buffer, buffer.ReadString()));

							break;
						}

						case NetMessageType.Data:
						{
							byte sanity = buffer.ReadByte();
							if (sanity != Message<ServerMessages>.FirstByte)
								continue;

							ServerMessages message = (ServerMessages)buffer.ReadVariableUInt32();
							switch (message)
							{
								case ServerMessages.Connected:
									this.connection = new UserConnection (client, client.ServerConnection);
									this.connection.AuthHash = buffer.ReadVariableInt32();
									this.OnConnected (new ConnectionEventArgs (this.connection, buffer));
									
									break;

								case ServerMessages.LoggedIn:
									this.OnLoggedIn (new ConnectionEventArgs (this.connection, buffer));
									break;

								case ServerMessages.UserConnected:
									this.OnUserLogin (new UserEventArgs (new User ().Decode (buffer)));
									break;

								case ServerMessages.UserDisconnected:
									this.OnUserLogout (new UserEventArgs (new User ().Decode (buffer)));
									break;
							}

							break;
						}
					}
				}

				Thread.Sleep (1);
			}
		}
	}
}