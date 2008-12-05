﻿using System;
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
		public GablarskiServer (IAuthProvider authProvider)
		{
			this.auth = authProvider;
		}

		public event EventHandler<ConnectionEventArgs> ClientConnected;
		public event EventHandler<ConnectionEventArgs> UserLogin;

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

		private IAuthProvider auth;

		private int port = 6112;
		private int maxConnections = 128;
		private NetServer Server;
		private Thread ServerThread;

		private readonly ReaderWriterLockSlim userRWL = new ReaderWriterLockSlim();
		private readonly Dictionary<int, DateTime> pendingLogins = new Dictionary<int, DateTime>();
		private readonly Dictionary<int, UserConnection> users = new Dictionary<int, UserConnection>();

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
					ConnectionEventArgs e = new ConnectionEventArgs(sender, buffer);

					switch (type)
					{
						case NetMessageType.StatusChanged:
							if (sender.Status == NetConnectionStatus.Connected)
								sender.Tag = new UserConnection(GenerateHash(), Server);

							this.OnClientConnected (e);

							break;

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

		protected virtual void OnClientConnected(ConnectionEventArgs e)
		{
			userRWL.EnterWriteLock();
			this.pendingLogins.Add (e.UserConnection.AuthHash, DateTime.Now);
			userRWL.ExitWriteLock();

			ServerMessage msg = new ServerMessage (ServerMessages.Connected, e.UserConnection);
			msg.Send (this.Server, e.Connection, NetChannel.ReliableInOrder1);

			var connected = this.ClientConnected;
			if (connected != null)
				connected (this, e);
		}

		protected virtual void OnClientLogin (ConnectionEventArgs e)
		{
			userRWL.EnterUpgradeableReadLock();
			
			if (!this.pendingLogins.ContainsKey (e.UserConnection.AuthHash))
				this.DisconnectUser (e.UserConnection, "Auth Hash Failure", e.Connection);

			userRWL.ExitUpgradeableReadLock();

			string nickname = e.Buffer.ReadString();
			string username = e.Buffer.ReadString();
			string password = e.Buffer.ReadString();

			LoginResult result = null;

			if (String.IsNullOrEmpty (username))
			{
				if (this.auth.CheckUserExists (nickname))
				{
					this.DisconnectUser (e.UserConnection, "Registered user owns this login already.", e.Connection);
					return;
				}

				userRWL.EnterReadLock();

				if (this.users.Values.Where(uc => uc.User.Nickname == nickname).Any())
				{
					this.DisconnectUser(e.UserConnection, "User already logged in.", e.Connection);
					return;
				}

				userRWL.ExitReadLock();

				result = this.auth.Login (nickname);
			}
			else
			{
				if (!this.auth.CheckUserExists(nickname))
				{
					this.DisconnectUser(e.UserConnection, "User does not exist.", e.Connection);
					return;
				}

				userRWL.EnterReadLock();

				if (this.users.Values.Where(uc => uc.User.Username == username).Any())
				{
					this.DisconnectUser(e.UserConnection, "User already logged in.", e.Connection);
					return;
				}

				userRWL.ExitReadLock();

				result = this.auth.Login (username, password);
			}
			
			if (!result.Succeeded)
			{
				this.DisconnectUser (e.UserConnection, result.FailureReason, e.Connection);
				return;
			}

			e.UserConnection.User = result.User;

			userRWL.EnterWriteLock();
			this.pendingLogins.Remove (e.UserConnection.AuthHash);
			this.users.Add (e.UserConnection.AuthHash, e.UserConnection);
			userRWL.ExitWriteLock();

			ServerMessage msg = new ServerMessage (ServerMessages.LoggedIn, e.UserConnection);

			var login = this.UserLogin;
			if (login != null)
				login (this, e);
		}

		protected void DisconnectUser (UserConnection userc, string reason, NetConnection netc)
		{
			netc.Disconnect (reason, 0);

			userRWL.EnterWriteLock();
			this.pendingLogins.Remove (userc.AuthHash);
			this.users.Remove (userc.AuthHash);
			userRWL.ExitWriteLock();
		}

		static int GenerateHash()
		{
			return DateTime.Now.Millisecond + DateTime.Now.Second + 42;
		}
	}
}