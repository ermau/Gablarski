using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gablarski.Media.Sources;
using Gablarski.Messages;

namespace Gablarski.Client
{
	public partial class GablarskiClient
		: IClientContext
	{
		public static readonly Version ApiVersion = Assembly.GetAssembly (typeof (GablarskiClient)).GetName ().Version;

		public GablarskiClient (IClientConnection connection)
		{
			this.Connection = connection;
			this.Connection.Connected += this.OnConnected;
			this.Connection.Disconnected += this.OnDisconnected;
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
		/// Gets the channel manager for this client.
		/// </summary>
		public ClientChannelManager Channels
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the user manager for this client.
		/// </summary>
		public ClientUserManager Users
		{
			get; 
			set;
		}

		/// <summary>
		/// Gets the source manager for this client.
		/// </summary>
		public ClientSourceManager Sources
		{
			get;
			set;
		}

		public CurrentUser CurrentUser
		{
			get;
			set;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Connects to a server server at <paramref name="host"/>:<paramref name="port"/>.
		/// </summary>
		/// <param name="host">The hostname of the server to connect to.</param>
		/// <param name="port">The port of the server to connect to.</param>
		/// <exception cref="System.ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException"><paramref name="port" /> is outside the acceptable port range.</exception>
		public void Connect (string host, int port)
		{
			if (host.IsEmpty ())
				throw new ArgumentException ("host must not be null or empty", "host");

			this.Setup ();

			this.running = true;
			this.messageRunnerThread.Start ();	

			Connection.MessageReceived += OnMessageReceived;
			Connection.Connect (host, port);
			Connection.Send (new ConnectMessage (ApiVersion));
		}

		/// <summary>
		/// Disconnects from the current server.
		/// </summary>
		public void Disconnect()
		{
			this.Connection.Disconnect();
			this.running = false;
			this.messageRunnerThread.Join ();
		}
		#endregion

		#region Event Invokers
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

		IEnumerable<MediaSourceBase> IClientContext.Sources
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