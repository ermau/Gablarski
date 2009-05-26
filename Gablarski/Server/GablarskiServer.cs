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

		public GablarskiServer (ServerInfo serverInfo, IUserProvider userProvider, IPermissionsProvider permissionProvider, IChannelProvider channelProvider)
			: this()
		{
			this.serverInfo = serverInfo;
			this.userProvider = userProvider;
			this.permissionProvider = permissionProvider;
			
			this.channelProvider = channelProvider;
			this.channelProvider.ChannelsUpdatedExternally += OnChannelsUpdatedExternally;
			this.UpdateChannels ();
		}

		#region Public Methods
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
				this.availableConnections = value.ToList();
			}
		}

		public void AddConnectionProvider (IConnectionProvider connection)
		{
			Trace.WriteLine ("[Server] " + connection.GetType().Name + " added.");

			// MUST provide a gaurantee of persona
			connection.ConnectionMade += OnConnectionMade;
			connection.StartListening ();

			lock (this.availableConnections)
			{
				this.availableConnections.Add (connection);
			}
		}

		public void RemoveConnectionProvider (IConnectionProvider connection)
		{
			Trace.WriteLine ("[Server] " + connection.GetType ().Name + " removed.");

			connection.StopListening ();
			connection.ConnectionMade -= this.OnConnectionMade;

			lock (this.availableConnections)
			{
				this.availableConnections.Remove (connection);
			}
		}

		public void Disconnect (IConnection connection, string reason)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			connection.Disconnect ();
			connection.MessageReceived -= this.OnMessageReceived;
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

		private readonly IPermissionsProvider permissionProvider;
		private readonly IUserProvider userProvider;
		private readonly IChannelProvider channelProvider;

		private object sourceLock = new object ();
		private readonly Dictionary<IConnection, List<IMediaSource>> sources = new Dictionary<IConnection, List<IMediaSource>> ();

		private readonly ConnectionCollection connections = new ConnectionCollection();

		private readonly object channelLock = new object ();
		private Channel defaultChannel;
		private Dictionary<long, Channel> channels;

		private void UpdateChannels ()
		{
			this.channels = this.channelProvider.GetChannels ().ToDictionary (c => c.ChannelId);
			this.defaultChannel = this.channelProvider.DefaultChannel;
		}

		private void OnChannelsUpdatedExternally (object sender, EventArgs e)
		{
			lock (channelLock)
			{
				this.UpdateChannels ();
				this.connections.Send (new ChannelListMessage (this.channels.Values));
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
							kvp.Value.Select (s => new MediaSourceInfo (s) { PlayerId = this.connections.GetPlayerId (connection) }));
				}

				agrSources = agrSources.ToList ();
			}

			return agrSources;
		}

		protected bool GetPermission (PermissionName name, long playerId)
		{
			return this.permissionProvider.GetPermissions (playerId).GetPermission (name);
		}

		protected bool GetPermission (PermissionName name, IConnection connection)
		{
			return GetPermission (name, this.connections.GetPlayerId (connection));
		}

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			var msg = (e.Message as ClientMessage);
			if (msg == null)
			{
				Disconnect (e.Connection, "Invalid message.");
				return;
			}

			Trace.WriteLine ("[Server] Message Received: " + msg.MessageType);

			#if !DEBUG
				if (this.Handlers.ContainsKey (msg.MessageType))
			#endif
					this.Handlers[msg.MessageType] (e);
		}

		protected void AudioDataReceived (MessageReceivedEventArgs e)
		{
			var msg = (SendAudioDataMessage) e.Message;

			this.connections.Send (new AudioDataReceivedMessage (msg.SourceId, msg.Data), (IConnection c) => c != e.Connection);
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