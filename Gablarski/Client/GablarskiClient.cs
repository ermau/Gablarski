using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Gablarski.Audio;
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
			: this(connection, userMananger, channelManager, sourceManager, currentUser, new ThreadedAudioEngine())
		{
		}

		public GablarskiClient (IClientConnection connection, ClientUserManager userMananger, ClientChannelManager channelManager, ClientSourceManager sourceManager, CurrentUser currentUser, IAudioEngine audioEngine)
			: this (connection, false)
		{
			this.Setup (userMananger, channelManager, sourceManager, currentUser, audioEngine);
		}

		private GablarskiClient (IClientConnection connection, bool setupDefaults)
		{
			this.Connection = connection;

			if (setupDefaults)
				this.Setup (new ClientUserManager (this), new ClientChannelManager (this), new ClientSourceManager (this), new CurrentUser (this), new ThreadedAudioEngine());
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

		public IAudioEngine Audio
		{
			get; private set;
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

		private bool reconnectAutomatically = true;
		/// <summary>
		/// Gets or sets whether to reconnect automatically on disconnection. <c>true</c> by default.
		/// </summary>
		/// <seealso cref="ReconnectAttemptFrequency"/>
		public bool ReconnectAutomatically
		{
			get { return reconnectAutomatically; }
			set { reconnectAutomatically = value; }
		}

		private int reconnectAttemptFrequency = 5000;
		/// <summary>
		/// Gets or sets the frequency (ms) at which to attempt reconnection. (5s default).
		/// </summary>
		public int ReconnectAttemptFrequency
		{
			get { return reconnectAttemptFrequency; }
			set { reconnectAttemptFrequency = value; }
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
				IPEndPoint endPoint = new IPEndPoint (Dns.GetHostAddresses (host).Where (ip => ip.AddressFamily == AddressFamily.InterNetwork).First(), port);

				this.running = true;

				this.messageRunnerThread = new Thread (this.MessageRunner) { Name = "Gablarski Client Message Runner" };
				this.messageRunnerThread.SetApartmentState (ApartmentState.STA);
				this.messageRunnerThread.Start();

				Connection.Disconnected += this.OnDisconnectedInternal;
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
			DisconnectCore (DisconnectHandling.None, this.Connection);
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

		IEnumerable<ChannelInfo> IClientContext.Channels
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