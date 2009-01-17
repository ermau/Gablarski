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
		: GablarskiBase
	{
		public GablarskiServer (IAuthProvider authProvider)
		{
			this.auth = authProvider;
		}

		public event EventHandler<ConnectionEventArgs> ClientConnected;
		public event EventHandler<ReasonEventArgs> ClientDisconnected;

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
				
		public IEnumerable<IUser> Users
		{
			get { return this.state.Users; }
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

		private ServerState state = new ServerState ();

		private Dictionary<IUser, List<IMediaSource>> sources = new Dictionary<IUser, List<IMediaSource>> ();

		private bool RequestSource (MediaSourceType type, IUser requester, out IMediaSource source)
		{
			source = null;

			if (!sources.ContainsKey (requester))
				sources.Add (requester, new List<IMediaSource>());

			if (!sources[requester].Any (s => s.Type == type))
			{
				source = CreateSource (sources.Values.MaxOrDefault (m => m.MaxOrDefault (s => s.ID, 0), 0) + 1, type);

				if (source != null)
				{
					sources[requester].Add (source);
					return true;
				}
			}

			this.OnSourceCreated (new SourceEventArgs (requester, source));

			return false;
		}

		private IMediaSource CreateSource (int sourceID, MediaSourceType type)
		{
			IMediaSource source;

			switch (type)
			{
				case MediaSourceType.Voice:
				default:
					source = new VoiceSource (sourceID);
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
					if (sender == null)
						continue;

					if (!this.state.Contains (sender))
						this.state.AddConnection (sender, GenerateHash ());

					ConnectionEventArgs e = new ConnectionEventArgs (sender, buffer);

					switch (type)
					{
						case NetMessageType.StatusChanged:
						{
							switch (sender.Status)
							{
								case NetConnectionStatus.Connected:
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

							if (!this.state.Contains (hash))
								this.ClientDisconnect (new ReasonEventArgs (e, "Auth failure"));

							switch (messageType)
							{
								case ClientMessages.Login:
									LoginResult result = this.AttemptUserLogin (e);
									if (result.Succeeded)
									{
										this.state.AddUser (sender, result.User);
										this.OnUserLogin (new UserEventArgs (result.User));
									}
									else
										this.DisconnectUser (sender, result.FailureReason.ToString ());

									break;

								case ClientMessages.Disconnect:
									this.ClientDisconnect (new ReasonEventArgs (e, "Client requested"));
									break;

								case ClientMessages.AudioData:
									//Trace.WriteLine ("Received voice data from: " + e.UserConnection.User.Nickname);
									IMediaSource source = buffer.ReadSource ();

									int voiceLen = buffer.ReadVariableInt32();
									if (voiceLen <= 0)
										continue;

									ServerMessage msg = new ServerMessage (ServerMessages.AudioData, this.state.Connections);//.Where (uc => uc != e.UserConnection));
									var msgbuffer = msg.GetBuffer ();
									msgbuffer.Write (source);
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

		private LoginResult AttemptUserLogin (ConnectionEventArgs e)
		{
			string nickname = e.Buffer.ReadString();
			string username = e.Buffer.ReadString();
			string password = e.Buffer.ReadString();

			Trace.WriteLine("Login attempt: " + nickname);

			if (String.IsNullOrEmpty (username))
			{
				if (this.auth.CheckUserExists (nickname))
					return new LoginResult (LoginFailureReason.NicknameOwned);

				if (this.state.Users.Any (u => u.Nickname == nickname))
					return new LoginResult (LoginFailureReason.NicknameUsed);

				Trace.WriteLine (nickname + " logged in.");
				return this.auth.Login (nickname);
			}
			else
			{
				if (!this.auth.CheckUserExists (username))
					return new LoginResult (LoginFailureReason.UserDoesntExist);

				if (this.state.Users.Any (u => u.Username == username))
					return new LoginResult (LoginFailureReason.UserLoggedIn);

				Trace.WriteLine (username + " logged in.");
				return this.auth.Login (username, password);
			}
		}

		private void ClientDisconnect (ReasonEventArgs e)
		{
			this.state.Remove (e.Connection);

			e.Connection.Disconnect (String.Empty, 0.0f);

			var user = this.state[e.Connection];

			if (user != null)
				Trace.WriteLine (user.Nickname + " disconnected: " + e.Reason);
			else
				Trace.WriteLine ("Unknown disconnected: " + e.Reason);

			if (user != null)
			{
				ServerMessage msg = new ServerMessage (ServerMessages.UserDisconnected, this.state.Connections.Where (c => c.Status == NetConnectionStatus.Connected));
				msg.GetBuffer ().Write (user);
				msg.Send (this.Server, NetChannel.ReliableUnordered);
			}
		}

		protected override void OnSourceCreated (SourceEventArgs e)
		{
			var msg = new ServerMessage (ServerMessages.SourceCreated, this.state.Connections);

			var buffer = msg.GetBuffer();
			buffer.Write(e.User);
			buffer.Write(e.Source);

			msg.Send (this.Server, NetChannel.ReliableInOrder1);

			base.OnSourceCreated(e);
		}

		protected virtual void OnClientDisconnected (ReasonEventArgs e)
		{
			this.ClientDisconnect (e);

			var disconnected = this.ClientDisconnected;
			if (disconnected != null)
				disconnected(this, e);
		}

		protected virtual void OnClientConnected (ConnectionEventArgs e)
		{
			ServerMessage msg = new ServerMessage (ServerMessages.Connected, e.Connection);
			msg.Send (this.Server, NetChannel.ReliableInOrder1);

			var connected = this.ClientConnected;
			if (connected != null)
				connected (this, e);
		}

		protected override void OnUserLogin (UserEventArgs e)
		{
			var connection = this.state[e.User];

			IMediaSource source;
			if (!this.RequestSource (MediaSourceType.Voice, e.User, out source))
			{
				this.DisconnectUser (connection, "Voice source request failed.");
				return;
			}

			ServerMessage msg = new ServerMessage (ServerMessages.LoggedIn, connection);
			var mbuffer = msg.GetBuffer();
			mbuffer.Write(e.User);
			mbuffer.Write(source);
			msg.Send (this.Server, NetChannel.ReliableInOrder1);

			msg = new ServerMessage (ServerMessages.UserConnected, this.state.Connections.Where (c => c.Status == NetConnectionStatus.Connected));
			msg.GetBuffer ().Write (e.User);
			msg.Send (this.Server, NetChannel.ReliableInOrder1);

			msg = new ServerMessage (ServerMessages.UserList, connection);
			mbuffer = msg.GetBuffer();
			mbuffer.WriteVariableInt32 (this.state.Users.Count());
			foreach (IUser u in this.state.Users)
				mbuffer.Write (u);

			msg.Send (this.Server, NetChannel.ReliableInOrder1);

			base.OnUserLogin (e);
		}

		protected void DisconnectUser (NetConnection netc, string reason)
		{
			netc.Disconnect (reason, 0);
			this.state.Remove (netc);
		}

		static int GenerateHash()
		{
			return DateTime.Now.Millisecond + DateTime.Now.Second + 42;
		}
	}
}
