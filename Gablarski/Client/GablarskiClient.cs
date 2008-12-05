﻿using System;
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
		public GablarskiClient ()
		{
		}

		public event EventHandler<ConnectionEventArgs> Connected;
		public event EventHandler<ReasonEventArgs> Disconnected;
		public event EventHandler<ConnectionEventArgs> LoggedIn;

		public bool IsRunning
		{
			get; private set;
		}

		public void Connect (string host, int port)
		{
			if (this.IsRunning)
				throw new InvalidOperationException ("Client already running.");

			this.IsRunning = true;
			this.client = new NetClient(new NetConfiguration("Gablarski"));

			this.runnerThread = new Thread (this.ClientRunner)
			                    {
			                    	IsBackground = true,
			                    	Name = "Client: " + host + ":" + port
			                    };
			this.runnerThread.Start ();
			
			this.client.Connect (host, port);
		}

		public void Login (string nickname)
		{
			Login (nickname, String.Empty, String.Empty);
		}

		public void Login (string nickname, string username, string password)
		{
			if (!this.IsRunning)
				throw new InvalidOperationException("Must be connected before logging in.");

			ClientMessage msg = new ClientMessage (ClientMessages.Login, this.connection);
			NetBuffer buffer = msg.GetBuffer ();
			buffer.Write (nickname);
			buffer.Write (username ?? String.Empty);
			buffer.Write (password ?? String.Empty);

			msg.Send (this.client, NetChannel.ReliableInOrder1);
		}

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
								this.OnDisconnected (new ReasonEventArgs (client.ServerConnection, buffer, buffer.ReadString()));

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
									this.connection = new UserConnection (buffer.ReadVariableInt32(), client);
									this.OnConnected (new ConnectionEventArgs (this.client.ServerConnection, buffer));
									
									break;

								case ServerMessages.LoggedIn:
									this.OnLoggedIn (new ConnectionEventArgs (this.client.ServerConnection, buffer));
									break;
							}

							break;
						}
					}
				}
			}
		}
	}
}