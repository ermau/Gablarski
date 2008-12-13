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

		public event EventHandler<UserListEventArgs> UserList;
		public event EventHandler<UserEventArgs> UserLogin;
		public event EventHandler<UserEventArgs> UserLogout;
		public event EventHandler<VoiceEventArgs> VoiceReceived;

		public bool IsRunning
		{
			get; private set;
		}

		public bool IsConnected
		{
			get; private set;
		}

		public bool IsLoggedin
		{
			get; private set;
		}

		public IEnumerable<IUser> Users
		{
			get { return this.users.Values; }
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

		public void SendVoiceData (byte[] data)
		{
			ClientMessage msg = new ClientMessage (ClientMessages.VoiceData, this.connection);
			var buffer = msg.GetBuffer ();
			buffer.WriteVariableInt32 (data.Length);
			buffer.Write (data);
			msg.Send (this.client, NetChannel.Unreliable);
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
			buffer.Write (username);
			buffer.Write (password);

			msg.Send (this.client, NetChannel.ReliableInOrder1);
		}

		private readonly ReaderWriterLockSlim userRWL = new ReaderWriterLockSlim ();
		private readonly Dictionary<uint, IUser> users = new Dictionary<uint, IUser> ();

		private bool connecting;
		private UserConnection connection;
		private Thread runnerThread;
		private NetClient client;

		protected virtual void OnDisconnected (ReasonEventArgs e)
		{
			this.IsLoggedin = false;
			this.IsConnected = false;
			this.IsRunning = false;
			this.runnerThread.Join();

			this.client.Dispose();


			var dced = this.Disconnected;
			if (dced != null)
				dced (this, e);
		}

		protected virtual void OnConnected (ConnectionEventArgs e)
		{
			this.IsConnected = true;
			this.connecting = false;


			var connected = this.Connected;
			if (connected != null)
				connected (this, e);
		}

		protected virtual void OnLoggedIn (ConnectionEventArgs e)
		{
			this.IsLoggedin = true;


			var loggedin = this.LoggedIn;
			if (loggedin != null)
				loggedin (this, e);
		}

		protected virtual void OnUserLogin (UserEventArgs e)
		{
			if (!ValidateAndAddUser (e.User))
				return;


			var ulogin = this.UserLogin;
			if (ulogin != null)
				ulogin (this, e);
		}

		protected virtual void OnUserList (UserListEventArgs e)
		{
			userRWL.EnterUpgradeableReadLock();
			foreach (IUser user in e.Users)
			{
				if (this.users.ContainsKey (user.ID))
					continue;

				if (!userRWL.IsWriteLockHeld)
					userRWL.EnterWriteLock();

				this.users.Add (user.ID, user);
			}

			if (userRWL.IsWriteLockHeld)
				userRWL.ExitWriteLock();

			userRWL.ExitUpgradeableReadLock();


			var ulist = this.UserList;
			if (ulist != null)
				ulist (this, e);
		}

		protected virtual void OnUserLogout (UserEventArgs e)
		{
			userRWL.EnterUpgradeableReadLock ();
			if (!this.users.ContainsKey (e.User.ID))
			{
				userRWL.ExitUpgradeableReadLock ();
				return;
			}

			userRWL.EnterWriteLock ();
			this.users.Remove (e.User.ID);
			userRWL.ExitWriteLock ();
			userRWL.ExitUpgradeableReadLock ();


			var ulogout = this.UserLogout;
			if (ulogout != null)
				ulogout (this, e);
		}

		protected virtual void OnVoiceReceived (VoiceEventArgs e)
		{
			var voice = this.VoiceReceived;
			if (voice != null)
				voice (this, e);
		}

		private bool ValidateAndAddUser (IUser user)
		{
			userRWL.EnterUpgradeableReadLock ();
			if (this.users.ContainsKey (user.ID))
			{
				userRWL.ExitUpgradeableReadLock ();
				return false;
			}

			userRWL.EnterWriteLock ();
			this.users.Add (user.ID, user);
			userRWL.ExitWriteLock ();
			userRWL.ExitUpgradeableReadLock ();
			
			return true;
		}

		private IUser GetUser (uint id)
		{
			IUser user = null;

			userRWL.EnterReadLock ();
			if (this.users.ContainsKey (id))
				user = this.users[id];

			userRWL.ExitReadLock ();
			return user;
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

								case ServerMessages.UserList:
									IUser[] userList = new IUser[buffer.ReadVariableInt32()];
									for (int i = 0; i < userList.Length; ++i)
										userList[i] = (new DecodedUser().Decode (buffer));

									this.OnUserList (new UserListEventArgs (userList));

									break;

								case ServerMessages.LoggedIn:
									this.OnLoggedIn (new ConnectionEventArgs (this.connection, buffer));
									break;

								case ServerMessages.UserConnected:
									this.OnUserLogin (new UserEventArgs (new DecodedUser ().Decode (buffer)));
									break;

								case ServerMessages.UserDisconnected:
									this.OnUserLogout(new UserEventArgs(new DecodedUser().Decode(buffer)));
									break;

								case ServerMessages.VoiceData:
									IUser user = this.GetUser (buffer.ReadVariableUInt32());
									if (user == null)
										continue;

									int voiceLen = buffer.ReadVariableInt32();
									if (voiceLen <= 0)
										continue;

									this.OnVoiceReceived (new VoiceEventArgs (user, buffer.ReadBytes (voiceLen)));
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