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
		: GablarskiBase
	{
		public event EventHandler<ConnectionEventArgs> Connected;
		public event EventHandler<ReasonEventArgs> Disconnected;
		public event EventHandler<ConnectionEventArgs> LoggedIn;

		public event EventHandler<UserListEventArgs> UserList;

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
			ClientMessage msg = new ClientMessage (ClientMessages.AudioData, this.connection);
			var buffer = msg.GetBuffer ();

			byte[] encoded = this.voiceSource.Codec.Encode (data);

			buffer.WriteVariableInt32(encoded.Length);
			buffer.Write(encoded);
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

		private bool connecting;

		private IUser user;
		private List<IMediaSource> sources = new List<IMediaSource>();
		private IMediaSource voiceSource;

		private Thread runnerThread;
		private NetClient client;

		private NetConnection connection;

		private Dictionary<int, IUser> users = new Dictionary<int, IUser> ();

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

			this.user = e.Buffer.ReadUser();

			this.sources.Add (e.Buffer.ReadSource ());

			var loggedin = this.LoggedIn;
			if (loggedin != null)
				loggedin (this, e);
		}

		protected override void OnUserLogin (UserEventArgs e)
		{
			if (!ValidateAndAddUser (e.User))
				return;

			base.OnUserLogin (e);
		}

		protected virtual void OnUserList (UserListEventArgs e)
		{
			foreach (IUser user in e.Users)
			{
				if (this.users.ContainsKey (user.ID))
					continue;

				this.users.Add (user.ID, user);
			}

			var ulist = this.UserList;
			if (ulist != null)
				ulist (this, e);
		}

		protected override void OnUserLogout (UserEventArgs e)
		{
			if (!this.users.ContainsKey(e.User.ID))
				return;

			this.users.Remove (e.User.ID);
		}
		
		private bool ValidateAndAddUser (IUser user)
		{
			if (this.users.ContainsKey (user.ID))
				return false;

			this.users.Add (user.ID, user);
			return true;
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
									this.connection = client.ServerConnection;
									this.connection.Tag = buffer.ReadVariableInt32();
									this.OnConnected (new ConnectionEventArgs (this.connection, buffer));
									break;

								case ServerMessages.UserList:
									IUser[] userList = new IUser[buffer.ReadVariableInt32()];
									for (int i = 0; i < userList.Length; ++i)
										userList[i] = buffer.ReadUser();

									this.OnUserList (new UserListEventArgs (userList));

									break;

								case ServerMessages.LoggedIn:
									this.OnLoggedIn (new ConnectionEventArgs (this.connection, buffer));
									break;

								case ServerMessages.UserConnected:
									this.OnUserLogin (new UserEventArgs (buffer.ReadUser()));
									break;

								case ServerMessages.UserDisconnected:
									this.OnUserLogout(new UserEventArgs(buffer.ReadUser()));
									break;

								case ServerMessages.SourceCreated:
									this.OnSourceCreated (new SourceEventArgs(buffer.ReadUser(), buffer.ReadSource()));
									break;

								case ServerMessages.AudioData:
									IMediaSource source = buffer.ReadSource();

									int audioLen = buffer.ReadVariableInt32();
									if (audioLen <= 0)
										continue;

									this.OnAudioReceived(new AudioEventArgs (source, buffer.ReadBytes (audioLen)));
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