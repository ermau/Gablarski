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
		public GablarskiServer (IAuthProvider authProvider)
		{
			this.auth = authProvider;
		}

		public event EventHandler<ConnectionEventArgs> ClientConnected;
		public event EventHandler<ReasonEventArgs> ClientDisconnected;
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
				
		public IEnumerable<UserConnection> Users
		{
			get
			{
				userRWL.EnterReadLock ();
				IEnumerable<UserConnection> u = this.users.Values.ToList ();
				userRWL.ExitReadLock ();

				return u;
			}
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
			
			Server.Shutdown("Server shutting down.");
			this.ServerThread.Join();
			
			Server.Dispose();
		}

		private readonly IAuthProvider auth;

		private int port = 6112;
		private int maxConnections = 128;
		private NetServer Server;
		private Thread ServerThread;

		private readonly ReaderWriterLockSlim userRWL = new ReaderWriterLockSlim();
		private readonly Dictionary<int, DateTime> pendingLogins = new Dictionary<int, DateTime>();
		private readonly Dictionary<int, UserConnection> users = new Dictionary<int, UserConnection> ();
		private readonly Dictionary<IUser, List<IMediaSource>> sources = new Dictionary<IUser, List<IMediaSource>>();

		private bool RequestSource (MediaSourceType type, IUser requester, out IMediaSource source)
		{
			source = null;

			userRWL.EnterUpgradeableReadLock();
			
			if (!sources.ContainsKey (requester))
			{
				userRWL.EnterWriteLock();
				sources.Add (requester, new List<IMediaSource>());
				userRWL.ExitWriteLock();
			}

			if (!sources[requester].Any (s => s.Type == type))
			{
				userRWL.EnterWriteLock();
				source = CreateSource (sources.Values.MaxOrDefault (m => m.MaxOrDefault (s => s.ID, 0), 0) + 1, type, requester);

				if (source != null)
				{
					sources[requester].Add (source);
					userRWL.ExitWriteLock ();
					return true;
				}
				else
					userRWL.ExitWriteLock();
			}
			
			userRWL.ExitUpgradeableReadLock();

			return false;
		}

		private IMediaSource CreateSource (int sourceID, MediaSourceType type, IUser owner)
		{
			IMediaSource source;

			switch (type)
			{
				case MediaSourceType.Voice:
				default:
					source = new VoiceSource (sourceID, owner);
					break;
			}

			return source;
		}

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
					UserConnection connection = users.Values.FirstOrDefault (uc => uc.Connection == sender) ?? new UserConnection (Server, sender);
					ConnectionEventArgs e = new ConnectionEventArgs (connection, buffer);

					switch (type)
					{
						case NetMessageType.StatusChanged:
						{
							switch (sender.Status)
							{
								case NetConnectionStatus.Connected:
									e.UserConnection.AuthHash = GenerateHash();
									this.OnClientConnected (e);
									break;

								case NetConnectionStatus.Disconnected:
									this.OnClientDisconnected (new ReasonEventArgs (e, buffer.ReadString()));
									break;
							}

							break;
						}

						case NetMessageType.Data:
							byte sanity = buffer.ReadByte();
							if (sanity != Message<ServerMessages>.FirstByte)
								continue;

							ClientMessages messageType = (ClientMessages)buffer.ReadVariableUInt32();

							int hash = e.Buffer.ReadVariableInt32 ();
							e.UserConnection.AuthHash = hash;

							userRWL.EnterReadLock ();
							if (!this.users.ContainsKey (hash) && !this.pendingLogins.ContainsKey (hash))
							{
								userRWL.ExitReadLock ();
								this.ClientDisconnect (new ReasonEventArgs (e, "Auth failure"), false);
							}
							userRWL.ExitReadLock ();

							switch (messageType)
							{
								case ClientMessages.Login:
									this.OnClientLogin (e);
									break;

								case ClientMessages.Disconnect:
									this.ClientDisconnect (new ReasonEventArgs (e, "Client requested"), true);
									break;

								case ClientMessages.AudioData:
									Trace.WriteLine ("Received voice data from: " + e.UserConnection.User.Nickname);

									int voiceLen = buffer.ReadVariableInt32();
									if (voiceLen <= 0)
										continue;

									ServerMessage msg = new ServerMessage(ServerMessages.AudioData, this.users.Values);//.Where (uc => uc != e.UserConnection));
									var msgbuffer = msg.GetBuffer ();
									msgbuffer.WriteVariableUInt32 (e.UserConnection.User.ID);
									msgbuffer.WriteVariableInt32 (voiceLen);
									msgbuffer.Write (buffer.ReadBytes (voiceLen));
									msg.Send (this.Server, NetChannel.Unreliable);

									break;
							}

							break;
					}
				}

				Thread.Sleep (1);
			}
		}

		private void ClientDisconnect (ReasonEventArgs e, bool commanded)
		{
			userRWL.EnterUpgradeableReadLock ();
			if (this.pendingLogins.ContainsKey (e.UserConnection.AuthHash) || this.users.ContainsKey (e.UserConnection.AuthHash))
			{
				userRWL.EnterWriteLock ();
				this.pendingLogins.Remove (e.UserConnection.AuthHash);
				this.users.Remove (e.UserConnection.AuthHash);
				userRWL.ExitWriteLock ();
			}
			userRWL.ExitUpgradeableReadLock ();

			//if (!commanded)
				e.Connection.Disconnect (String.Empty, 0.0f);

			if (e.UserConnection.User != null)
				Trace.WriteLine (e.UserConnection.User.Nickname + " disconnected: " + e.Reason);
			else
				Trace.WriteLine ("Unknown disconnected: " + e.Reason);

			if (e.UserConnection.User != null)
			{
				ServerMessage msg = new ServerMessage (ServerMessages.UserDisconnected, this.users.Values.Where (uc => uc.Connection.Status == NetConnectionStatus.Connected));
				msg.GetBuffer ().Write (e.UserConnection.User);
				msg.Send (this.Server, NetChannel.ReliableUnordered);
			}

			var disconnected = this.ClientDisconnected;
			if (disconnected != null)
				disconnected (this, e);
		}

		protected virtual void OnClientDisconnected (ReasonEventArgs e)
		{
			this.ClientDisconnect (e, false);
		}

		protected virtual void OnClientConnected (ConnectionEventArgs e)
		{
			userRWL.EnterUpgradeableReadLock ();
			if (this.pendingLogins.ContainsKey (e.UserConnection.AuthHash))
			{
				userRWL.ExitUpgradeableReadLock ();
				return;
			}

			userRWL.EnterWriteLock();
			this.pendingLogins.Add (e.UserConnection.AuthHash, DateTime.Now);
			userRWL.ExitWriteLock();
			userRWL.ExitUpgradeableReadLock ();

			ServerMessage msg = new ServerMessage (ServerMessages.Connected, e.UserConnection);
			msg.Send (this.Server, NetChannel.ReliableInOrder1);

			var connected = this.ClientConnected;
			if (connected != null)
				connected (this, e);
		}

		protected virtual void OnClientLogin (ConnectionEventArgs e)
		{
			string nickname = e.Buffer.ReadString();
			string username = e.Buffer.ReadString();
			string password = e.Buffer.ReadString();

			Trace.WriteLine ("Login attempt: " + nickname);

			LoginResult result;

			if (String.IsNullOrEmpty (username))
			{
				if (this.auth.CheckUserExists (nickname))
				{
					this.DisconnectUser (e.UserConnection, "Registered user owns this login already.", e.Connection);
					return;
				}

				userRWL.EnterUpgradeableReadLock();

				if (this.users.Values.Any (uc => uc.User.Nickname == nickname))
				{
					this.DisconnectUser (e.UserConnection, "User already logged in.", e.Connection);
					userRWL.ExitUpgradeableReadLock ();
					return;
				}

				userRWL.ExitUpgradeableReadLock();

				result = this.auth.Login (nickname);
			}
			else
			{
				if (!this.auth.CheckUserExists(nickname))
				{
					this.DisconnectUser (e.UserConnection, "User does not exist.", e.Connection);
					return;
				}

				userRWL.EnterUpgradeableReadLock();

				if (this.users.Values.Any (uc => uc.User.Username == username))
				{
					this.DisconnectUser (e.UserConnection, "User already logged in.", e.Connection);
					userRWL.ExitUpgradeableReadLock ();
					return;
				}

				userRWL.ExitUpgradeableReadLock ();

				result = this.auth.Login (username, password);
			}
			
			if (!result.Succeeded)
			{
				this.DisconnectUser (e.UserConnection, result.FailureReason, e.Connection);
				return;
			}

			Trace.WriteLine (result.User.Nickname + " logged in.");

			e.UserConnection.User = result.User;

			IMediaSource source;
			if (!this.RequestSource (MediaSourceType.Voice, result.User, out source))
			{
				this.DisconnectUser (e.UserConnection, "Voice source request failed.", e.Connection);
				return;
			}

			userRWL.EnterWriteLock();
			this.pendingLogins.Remove (e.UserConnection.AuthHash);
			this.users.Add (e.UserConnection.AuthHash, e.UserConnection);
			userRWL.ExitWriteLock();

			ServerMessage msg = new ServerMessage (ServerMessages.LoggedIn, e.UserConnection);
			var mbuffer = msg.GetBuffer();
			mbuffer.Write(e.UserConnection.User);
			mbuffer.Write(source);
			msg.Send (this.Server, NetChannel.ReliableInOrder1);

			userRWL.EnterReadLock();
			msg = new ServerMessage (ServerMessages.UserConnected, this.users.Values.Where (uc => uc.Connection.Status == NetConnectionStatus.Connected));
			msg.GetBuffer ().Write (e.UserConnection.User);
			msg.Send (this.Server, NetChannel.ReliableInOrder1);

			msg = new ServerMessage (ServerMessages.UserList, e.UserConnection);
			mbuffer = msg.GetBuffer();
			mbuffer.WriteVariableInt32 (this.users.Count);
			foreach (IUser u in this.users.Values.Select (uc => uc.User))
				mbuffer.Write (u);

			msg.Send (this.Server, NetChannel.ReliableInOrder1);

			userRWL.ExitReadLock();

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
