using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Client;
using Gablarski.Messages;
using System.Diagnostics;
using Gablarski.Media.Sources;

namespace Gablarski.Server
{
	public partial class GablarskiServer
	{
		public static readonly Version MinimumApiVersion = new Version (0,3,0,0);

		/// <summary>
		/// Initializes a new <c>GablarskiServer</c> instance.
		/// </summary>
		/// <param name="serverInfo">The info for the server, providing name, description, etc.</param>
		/// <param name="userProvider">The user authentication provider for the server to use.</param>
		/// <param name="permissionProvider">The user permissions provider for the server to use.</param>
		/// <param name="channelProvider">The channel provider for the server to use.</param>
		public GablarskiServer (ServerInfo serverInfo, IUserProvider userProvider, IPermissionsProvider permissionProvider, IChannelProvider channelProvider)
			: this()
		{
			this.serverInfo = serverInfo;
			this.userProvider = userProvider;
			this.permissionProvider = permissionProvider;
			
			this.channelProvider = channelProvider;
			this.channelProvider.ChannelsUpdatedExternally += OnChannelsUpdatedExternally;
			this.UpdateChannels (false);
		}

		/// <summary>
		/// Initializes a new <c>GablarskiServer</c> instance.
		/// </summary>
		/// <param name="serverInfo">The info for the server, providing name, description, etc.</param>
		/// <param name="provider">The backend provider for the server to use.</param>
		public GablarskiServer (ServerInfo serverInfo, IBackendProvider provider)
			: this()
		{
			this.serverInfo = serverInfo;

			this.backendProvider = provider;
			this.backendProvider.ChannelsUpdatedExternally += OnChannelsUpdatedExternally;
			this.UpdateChannels (false);
		}

		#region Public Methods
		/// <summary>
		/// Gets or sets the list of <c>IConnectionProvider</c>'s for the server to use.
		/// </summary>
		public IEnumerable<IConnectionProvider> ConnectionProviders
		{
			get
			{
				lock (this.availableConnections)
				{
					return this.availableConnections.ToArray ();
				}
			}

			set
			{
				lock (this.availableConnections)
				{
					foreach (var provider in this.availableConnections)
						RemoveConnectionProvider (provider, false);

					this.availableConnections.Clear ();

					foreach (var provider in value)
						this.AddConnectionProvider (provider);
				}
			}
		}

		/// <summary>
		/// Adds and starts an <c>IConnectionProvider</c>.
		/// </summary>
		/// <param name="provider">The <c>IConnectionProvider</c> to add and start listening.</param>
		public void AddConnectionProvider (IConnectionProvider provider)
		{
			Trace.WriteLine ("[Server] " + provider.GetType().Name + " added.");

			// MUST provide a gaurantee of persona
			provider.ConnectionMade += OnConnectionMade;
			provider.StartListening ();

			lock (this.availableConnections)
			{
				this.availableConnections.Add (provider);
			}
		}

		/// <summary>
		/// Stops and removes an <c>IConnectionProvider</c>.
		/// </summary>
		/// <param name="connection"></param>
		public void RemoveConnectionProvider (IConnectionProvider connection)
		{
			RemoveConnectionProvider (connection, true);
		}

		/// <summary>
		/// Disconnections an <c>IConnection</c>.
		/// </summary>
		/// <param name="connection">The connection to disconnect.</param>
		public void Disconnect (PlayerInfo player)
		{
			if (player == null)
				throw new ArgumentNullException ("player");

			var playerConnection = this.connections[player];
			if (playerConnection != null)
				this.Disconnect (playerConnection);
		}

		public void Shutdown ()
		{
			lock (this.availableConnections)
			{
				for (int i = 0; i < this.availableConnections.Count;)
				{
					var provider = this.availableConnections[i];

					provider.StopListening ();
					provider.ConnectionMade -= OnConnectionMade;
					this.RemoveConnectionProvider (provider);
				}
			}
		}
		#endregion

		private readonly ServerInfo serverInfo = new ServerInfo();

		private List<IConnectionProvider> availableConnections = new List<IConnectionProvider> ();

		private readonly IBackendProvider backendProvider;

		private readonly IChannelProvider channelProvider;
		private readonly IPermissionsProvider permissionProvider;
		private readonly IUserProvider userProvider;

		protected IBackendProvider BackendProvider
		{
			get { return backendProvider; }
		}

		protected IChannelProvider ChannelProvider
		{
			get { return (backendProvider ?? channelProvider); }
		}

		protected IPermissionsProvider PermissionProvider
		{
			get { return (backendProvider ?? permissionProvider); }
		}

		protected IUserProvider UserProvider
		{
			get { return (backendProvider ?? userProvider); }
		}

		private object sourceLock = new object ();
		private readonly Dictionary<IConnection, List<IMediaSource>> sources = new Dictionary<IConnection, List<IMediaSource>> ();

		private readonly ConnectionCollection connections = new ConnectionCollection();

		private readonly object channelLock = new object ();
		private Channel defaultChannel;
		private Dictionary<long, Channel> channels;

		private void Disconnect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			connection.Disconnect ();
			connection.MessageReceived -= this.OnMessageReceived;
		}

		private void RemoveConnectionProvider (IConnectionProvider provider, bool listRemove)
		{
			Trace.WriteLine ("[Server] " + provider.GetType ().Name + " removed.");

			provider.StopListening ();
			provider.ConnectionMade -= this.OnConnectionMade;

			if (listRemove)
			{
				lock (this.availableConnections)
				{
					this.availableConnections.Remove (provider);
				}
			}
		}

		private void UpdateChannels (bool sendUpdate)
		{
			this.channels = this.ChannelProvider.GetChannels ().ToDictionary (c => c.ChannelId);
			this.defaultChannel = this.ChannelProvider.DefaultChannel;

			if (sendUpdate)
				this.connections.Send (new ChannelListMessage (this.channels.Values));
		}

		private void OnChannelsUpdatedExternally (object sender, EventArgs e)
		{
			lock (channelLock)
			{
				this.UpdateChannels (true);
			}
		}

		private IEnumerable<MediaSourceInfo> GetSourceInfoList ()
		{
			IEnumerable<MediaSourceInfo> agrSources = Enumerable.Empty<MediaSourceInfo> ();
			lock (this.sourceLock)
			{
				foreach (var kvp in this.sources)
				{
					IConnection connection = kvp.Key;
					agrSources = agrSources.Concat (
							kvp.Value.Select (s => new MediaSourceInfo (s) { PlayerId = this.connections[connection].PlayerId }));
				}

				agrSources = agrSources.ToList ();
			}

			return agrSources;
		}

		protected bool GetPermission (PermissionName name, long channelId, long playerId)
		{
			if (this.BackendProvider != null)
				return this.BackendProvider.GetPermissions (channelId, playerId).GetPermission (name);
			else
				return this.PermissionProvider.GetPermissions (playerId).GetPermission (name);
		}

		protected bool GetPermission (PermissionName name, long channelId, IConnection connection)
		{
			return GetPermission (name, channelId, this.connections[connection].PlayerId);
		}

		protected bool GetPermission (PermissionName name, IConnection connection)
		{
			var player = this.connections[connection];

			return GetPermission (name, player.CurrentChannelId, player.PlayerId);
		}

		protected bool GetPermission (PermissionName name, PlayerInfo player)
		{
			return GetPermission (name, player.CurrentChannelId, player.PlayerId);
		}

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			var msg = (e.Message as ClientMessage);
			if (msg == null)
			{
				Disconnect (e.Connection);
				return;
			}

			Trace.WriteLine ("[Server] Message Received: " + msg.MessageType);

			#if !DEBUG
				if (this.Handlers.ContainsKey (msg.MessageType))
			#endif
					this.Handlers[msg.MessageType] (e);
		}

		protected void ClientDisconnected (MessageReceivedEventArgs e)
		{
			OnClientDisconnected (this, new ConnectionEventArgs (e.Connection));
		}

		private void OnClientDisconnected (object sender, ConnectionEventArgs e)
		{
			Trace.WriteLine ("[Server] Client disconnected");

			e.Connection.Disconnect();

			long playerId;
			if (this.connections.Remove (e.Connection, out playerId))
				this.connections.Send (new PlayerDisconnectedMessage (playerId));

			lock (sourceLock)
			{
				this.sources.Remove (e.Connection);
			}
		}
		
		private void OnConnectionMade (object sender, ConnectionEventArgs e)
		{
			Trace.WriteLine ("[Server] Connection Made");

			this.connections.Add (e.Connection);

			e.Connection.MessageReceived += this.OnMessageReceived;
			e.Connection.Disconnected += this.OnClientDisconnected;
		}
	}
}