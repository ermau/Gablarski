using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Gablarski.Media.Sources;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public partial class GablarskiClient
		: IClientContext
	{
		public static readonly Version ApiVersion = Assembly.GetAssembly (typeof (GablarskiClient)).GetName ().Version;

		public GablarskiClient (IClientConnection connection)
			: this (connection, true)
		{
		}
		
		public GablarskiClient (IClientConnection connection, ClientUserManager userMananger, ClientChannelManager channelManager, ClientSourceManager sourceManager, CurrentUser currentUser)
			: this (connection, false)
		{
			this.Setup (userMananger, channelManager, sourceManager, currentUser);
		}

		private GablarskiClient (IClientConnection connection, bool setupDefaults)
		{
			this.Connection = connection;

			if (setupDefaults)
				this.Setup (new ClientUserManager (this), new ClientChannelManager (this), new ClientSourceManager (this), new CurrentUser (this));
		}

		#region Events
		/// <summary>
		/// The client has connected to the server.
		/// </summary>
		public event EventHandler Connected;
		
		/// <summary>
		/// The connection to the server has been rejected.
		/// </summary>
		public event EventHandler<RejectedConnectionEventArgs> ConnectionRejected;

		/// <summary>
		/// The connection to the server has been lost (or forcibly closed.)
		/// </summary>
		public event EventHandler Disconnected;

		#endregion

		#region Public Properties
		/// <summary>
		/// Gets whether the client is currently connected.
		/// </summary>
		public bool IsConnected
		{
			get { return this.Connection.IsConnected; }
		}

		/// <summary>
		/// Gets the channel manager for this client.
		/// </summary>
		public ClientChannelManager Channels
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the user manager for this client.
		/// </summary>
		public ClientUserManager Users
		{
			get; 
			private set;
		}

		/// <summary>
		/// Gets the source manager for this client.
		/// </summary>
		public ClientSourceManager Sources
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the current user.
		/// </summary>
		public CurrentUser CurrentUser
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the <see cref="ServerInfo"/> for the currently connected server. <c>null</c> if not connected.
		/// </summary>
		public ServerInfo ServerInfo
		{
			get { return this.serverInfo; }
		}

		/// <summary>
		/// Gets the <see cref="IdentifyingTypes"/> for this client's connection. <c>null</c> if not connected.
		/// </summary>
		public IdentifyingTypes IdentifyingTypes
		{
			get { return this.Connection.IdentifyingTypes; }
		}

		/// <summary>
		/// Gets or sets whether to trace verbosely (trace audio data mostly).
		/// </summary>
		public bool VerboseTracing
		{
			get; set;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Connects to a server server at <paramref name="host"/>:<paramref name="port"/>.
		/// </summary>
		/// <param name="host">The hostname of the server to connect to.</param>
		/// <param name="port">The port of the server to connect to.</param>
		/// <exception cref="ArgumentException"><paramref name="host"/> is invalid.</exception>
		/// <exception cref="System.ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="port" /> is outside the acceptable port range.</exception>
		/// <exception cref="System.Net.Sockets.SocketException">Hostname could not be resolved.</exception>
		public void Connect (string host, int port)
		{
			if (host.IsEmpty ())
				throw new ArgumentException ("host must not be null or empty", "host");

			try
			{
				IPEndPoint endPoint = new IPEndPoint (Dns.GetHostAddresses (host).Where (ip => ip.AddressFamily != AddressFamily.InterNetworkV6).First(), port);

				this.running = true;

				this.messageRunnerThread = new Thread (this.MessageRunner) { Name = "Gablarski Client Message Runner" };
				this.messageRunnerThread.SetApartmentState (ApartmentState.STA);
				this.messageRunnerThread.Start();

				Connection.Disconnected += this.OnDisconnected;
				Connection.MessageReceived += OnMessageReceived;
				Connection.Connect (endPoint);
				Connection.Send (new ConnectMessage (ApiVersion));
			}
			catch (SocketException)
			{
				OnConnectionRejected (new RejectedConnectionEventArgs (ConnectionRejectedReason.CouldNotConnect));
			}
		}

		/// <summary>
		/// Disconnects from the current server.
		/// </summary>
		public void Disconnect()
		{
			if (!this.running)
				return;

			this.running = false;
			lock (this.mqueue)
			{
				this.mqueue.Clear();
			}

			Connection.Disconnected -= this.OnDisconnected;
			Connection.MessageReceived -= this.OnMessageReceived;
			Connection.Disconnect();

			OnDisconnected (this, EventArgs.Empty);

			if (this.messageRunnerThread != null)
				this.messageRunnerThread.Join ();

			this.Users.Clear();
			this.Channels.Clear();
			this.Sources.Clear();
		}
		#endregion

		#region Event Invokers
		// ReSharper disable UnusedParameter.Global
		protected virtual void OnConnected (object sender, EventArgs e)
		{
			var connected = this.Connected;
			if (connected != null)
				connected (this, e);
		}

		protected virtual void OnDisconnected (object sender, EventArgs e)
		{
			var disconnected = this.Disconnected;
			if (disconnected != null)
				disconnected (this, e);
		}
		// ReSharper restore UnusedParameter.Global

		protected virtual void OnConnectionRejected (RejectedConnectionEventArgs e)
		{
			var rejected = this.ConnectionRejected;
			if (rejected != null)
				rejected (this, e);
		}
		#endregion

		#region IClientContext Members

		IClientConnection IClientContext.Connection
		{
			get { return this.Connection; }
		}

		IEnumerable<Channel> IClientContext.Channels
		{
			get { return this.Channels; }
		}

		IEnumerable<AudioSource> IClientContext.Sources
		{
			get { return this.Sources; }
		}

		IEnumerable<ClientUser> IClientContext.Users
		{
			get { return this.Users; }
		}
		#endregion
	}

	#region Event Args
	public class RejectedConnectionEventArgs
		: EventArgs
	{
		public RejectedConnectionEventArgs (ConnectionRejectedReason reason)
		{
			this.Reason = reason;
		}

		/// <summary>
		/// Gets the reason for rejecting the connection.
		/// </summary>
		public ConnectionRejectedReason Reason
		{
			get;
			private set;
		}
	}

	public class ReceivedListEventArgs<T>
		: EventArgs
	{
		public ReceivedListEventArgs (IEnumerable<T> data)
		{
			this.Data = data;
		}

		public IEnumerable<T> Data
		{
			get;
			private set;
		}
	}
	#endregion
}