// Copyright (c) 2009, Eric Maupin
// All rights reserved.

// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:

// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.

// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.

// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission.

// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS
// AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Messages;
using System.Diagnostics;

namespace Gablarski.Server
{
	public partial class GablarskiServer
	{
		public static readonly Version MinimumApiVersion = new Version (0,11,1,0);

		/// <summary>
		/// Initializes a new <c>GablarskiServer</c> instance.
		/// </summary>
		/// <param name="settings">The settings for the server, providing name, description, etc.</param>
		/// <param name="authProvider">The user authentication provider for the server to use.</param>
		/// <param name="permissionProvider">The user permissions provider for the server to use.</param>
		/// <param name="channelProvider">The channel provider for the server to use.</param>
		public GablarskiServer (ServerSettings settings, IAuthenticationProvider authProvider, IPermissionsProvider permissionProvider, IChannelProvider channelProvider)
			: this()
		{
			this.settings = settings;
			this.authProvider = authProvider;
			
			this.permissionProvider = permissionProvider;
			this.permissionProvider.PermissionsChanged += OnPermissionsChanged;
		
			this.channelProvider = channelProvider;
			this.channelProvider.ChannelsUpdated += OnChannelsUpdatedExternally;
			this.UpdateChannels (false);
		}

		/// <summary>
		/// Initializes a new <c>GablarskiServer</c> instance.
		/// </summary>
		/// <param name="settings">The settings for the server, providing name, description, etc.</param>
		/// <param name="provider">The backend provider for the server to use.</param>
		public GablarskiServer (ServerSettings settings, IBackendProvider provider)
			: this()
		{
			this.settings = settings;

			this.backendProvider = provider;
			this.backendProvider.ChannelsUpdated += OnChannelsUpdatedExternally;
			this.backendProvider.PermissionsChanged += OnPermissionsChanged;
			this.UpdateChannels (false);
		}

		/// <summary>
		/// Gets or sets whether to trace verbosely (trace audio data mostly).
		/// </summary>
		public bool VerboseTracing
		{
			get; set;
		}

		public bool IsRunning
		{
			get { return this.running; }
		}

		public int UserCount
		{
			get { return this.connections.UserCount; }
		}

		public ServerSettings Settings
		{
			get { return this.settings; }
		}

		#region Public Methods
		/// <summary>
		/// Adds and starts an <c>IConnectionProvider</c>.
		/// </summary>
		/// <param name="provider">The <c>IConnectionProvider</c> to add and start listening.</param>
		public void AddConnectionProvider (IConnectionProvider provider)
		{
			Trace.WriteLine ("[Server] " + provider.GetType().Name + " added.");

			// MUST provide a gaurantee of persona
			lock (this.availableConnections)
			{
				provider.ConnectionMade += OnConnectionMade;
				provider.ConnectionlessMessageReceived += OnMessageReceived;
				provider.StartListening ();
		
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
		/// Starts the server.
		/// </summary>
		public void Start ()
		{
			this.running = true;
			this.messageRunnerThread.Start ();
		}

		/// <summary>
		/// Removes all connection providers, disconnects all users and shuts the server down.
		/// </summary>
		public void Shutdown ()
		{
			this.running = false;
			
			lock (this.availableConnections)
			{
				for (int i = 0; i < this.availableConnections.Count;)
				{
					var provider = this.availableConnections[i];

					provider.ConnectionMade -= OnConnectionMade;
					this.RemoveConnectionProvider (provider);
				}
			}

			this.incomingWait.Set ();
			this.messageRunnerThread.Join();

			while (this.connections.ConnectionCount > 0)
				this.Disconnect (this.connections[0].Key);
		}

		public IEnumerable<UserInfo> GetUsers()
		{
			return this.connections.Users.Select (u => new UserInfo (u));
		}

		public IEnumerable<ChannelInfo> GetChannels()
		{
			IEnumerable<ChannelInfo> cs;
			lock (channelLock)
			{
				cs = this.channels.Values.ToList();
			}

			return cs.Select (c => new ChannelInfo (c));
		}

		/// <summary>
		/// Disconnections an <c>IConnection</c>.
		/// </summary>
		/// <param name="user">The user to disconnect.</param>
		public void Disconnect (ServerUserInfo user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			var userConnection = this.connections[user];
			if (userConnection != null)
				this.Disconnect (userConnection);
		}
		#endregion

		private readonly ServerSettings settings;

		private readonly List<IConnectionProvider> availableConnections = new List<IConnectionProvider> ();

		private readonly IBackendProvider backendProvider;

		private readonly IChannelProvider channelProvider;
		private readonly IPermissionsProvider permissionProvider;
		private readonly IAuthenticationProvider authProvider;
		private volatile bool running = true;

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

		protected IAuthenticationProvider AuthenticationProvider
		{
			get { return (backendProvider ?? authProvider); }
		}

		private readonly object sourceLock = new object ();
		private ILookup<IConnection, AudioSource> sourceLookup;
		private readonly List<AudioSource> sources = new List<AudioSource>();

		private readonly ConnectionCollection connections = new ConnectionCollection();
		private Dictionary<int, object> nativeusers = new Dictionary<int, object>();

		private readonly object channelLock = new object ();
		private ChannelInfo defaultChannel;
		private Dictionary<int, ChannelInfo> channels;
		private Dictionary<int, object> nativeChannels = new Dictionary<int, object>();

		private void Disconnect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			connection.Disconnect ();
			connection.MessageReceived -= this.OnMessageReceived;
			connection.Disconnected -= this.OnClientDisconnected;
			this.connections.Remove (connection);
		}

		private void RemoveConnectionProvider (IConnectionProvider provider, bool listRemove)
		{
			Trace.WriteLine ("[Server] " + provider.GetType ().Name + " removed.");

			provider.StopListening ();
			provider.ConnectionMade -= OnConnectionMade;
			provider.ConnectionlessMessageReceived -= OnMessageReceived;

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
			lock (channelLock)
			{
				this.channels = this.ChannelProvider.GetChannels().ToDictionary (c => c.ChannelId);
				this.defaultChannel = this.ChannelProvider.DefaultChannel;
			}

			if (sendUpdate)
			{
				int defaultChannelId;
				List<int> movedUsers = new List<int>();
				lock (channelLock)
				{
					defaultChannelId = ChannelProvider.DefaultChannel.ChannelId;
					foreach (var u in this.connections.Users)
					{
						if (!this.channels.ContainsKey (u.CurrentChannelId))
						{
							movedUsers.Add (u.UserId);
							this.connections.UpdateIfExists (new ServerUserInfo (u) { CurrentChannelId = defaultChannelId  });
						}
					}
				}

				foreach (var userId in movedUsers)
				{
					this.connections.Send (new UserChangedChannelMessage 
						{ ChangeInfo = new ChannelChangeInfo (userId, defaultChannelId) });
				}

				this.connections.Send (new ChannelListMessage (this.channels.Values));
			}
		}

		private void OnPermissionsChanged (object sender, PermissionsChangedEventArgs e)
		{
			this.connections.GetConnection (e.UserId).Send (new PermissionsMessage (e.UserId, PermissionProvider.GetPermissions (e.UserId)));
		}

		private void OnChannelsUpdatedExternally (object sender, EventArgs e)
		{
			UpdateChannels (true);
		}

		private IEnumerable<AudioSource> GetSourceInfoList ()
		{
			AudioSource[] currentSources;
			lock (this.sourceLock)
			{
				currentSources = new AudioSource[this.sources.Count];
				this.sources.CopyTo (currentSources, 0);
			}

			return currentSources;
		}

		protected bool GetPermission (PermissionName name, int channelId, int userId)
		{
			if (this.BackendProvider != null)
				return this.BackendProvider.GetPermissions (channelId, userId).CheckPermission (name);
			else
				return this.PermissionProvider.GetPermissions (userId).CheckPermission (name);
		}

		protected bool GetPermission (PermissionName name, int channelId, IConnection connection)
		{
			return GetPermission (name, channelId, this.connections[connection].UserId);
		}

		protected bool GetPermission (PermissionName name, IConnection connection)
		{
			var user = this.connections[connection];

			return GetPermission (name, (user != null) ? user.CurrentChannelId : 0, (user != null) ? user.UserId : 0);
		}

		protected bool GetPermission (PermissionName name, UserInfo user)
		{
			return GetPermission (name, user.CurrentChannelId, user.UserId);
		}

		protected void ClientDisconnected (MessageReceivedEventArgs e)
		{
			OnClientDisconnected (this, new ConnectionEventArgs (e.Connection));
		}

		private void OnClientDisconnected (object sender, ConnectionEventArgs e)
		{
			Trace.WriteLine ("[Server] Client disconnected");

			e.Connection.MessageReceived -= this.OnMessageReceived;
			e.Connection.Disconnected -= this.OnClientDisconnected;
			e.Connection.Disconnect();

			int userId;
			if (this.connections.Remove (e.Connection, out userId))
				this.connections.Send (new UserDisconnectedMessage (userId));

			lock (sourceLock)
			{
				if (this.sourceLookup == null || !this.sourceLookup.Contains (e.Connection))
					return;

				this.connections.Send (new SourcesRemovedMessage (this.sourceLookup[e.Connection]));
				foreach (var source in this.sourceLookup[e.Connection])
					this.sources.Remove (source);

				this.sourceLookup = this.sources.ToLookup (k => this.connections.GetConnection (k.OwnerId));
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