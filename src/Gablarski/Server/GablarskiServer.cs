// Copyright (c) 2010, Eric Maupin
// All rights reserved.
//
// Redistribution and use in source and binary forms, with
// or without modification, are permitted provided that
// the following conditions are met:
//
// - Redistributions of source code must retain the above 
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// - Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// - Neither the name of Gablarski nor the names of its
//   contributors may be used to endorse or promote products
//   or services derived from this software without specific
//   prior written permission.
//
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
using System.Threading;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Messages;
using System.Diagnostics;
using Cadenza;
using log4net;

namespace Gablarski.Server
{
	public partial class GablarskiServer
		: IServerContext
	{
		// ReSharper disable ConvertToConstant.Global
		public static readonly int ProtocolVersion = 6;
		// ReSharper restore ConvertToConstant.Global

		/// <summary>
		/// Initializes a new <c>GablarskiServer</c> instance.
		/// </summary>
		/// <param name="settings">The settings for the server, providing name, description, etc.</param>
		/// <param name="authProvider">The user authentication provider for the server to use.</param>
		/// <param name="permissionProvider">The user permissions provider for the server to use.</param>
		/// <param name="channelProvider">The channel provider for the server to use.</param>
		public GablarskiServer (ServerSettings settings, IAuthenticationProvider authProvider, IPermissionsProvider permissionProvider, IChannelProvider channelProvider)
			: this (settings)
		{
			this.authProvider = authProvider;
			
			this.permissionProvider = permissionProvider;
			this.permissionProvider.PermissionsChanged += OnPermissionsChanged;
		
			this.channelProvider = channelProvider;

			SetupHandlers();
		}

		/// <summary>
		/// Initializes a new <c>GablarskiServer</c> instance.
		/// </summary>
		/// <param name="settings">The settings for the server, providing name, description, etc.</param>
		/// <param name="provider">The backend provider for the server to use.</param>
		public GablarskiServer (ServerSettings settings, IBackendProvider provider)
			: this (settings)
		{
			this.backendProvider = provider;
			this.backendProvider.PermissionsChanged += OnPermissionsChanged;

			SetupHandlers();
		}

		public object SyncRoot
		{
			get { return syncRoot; }
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

		public ServerSettings Settings
		{
			get { return this.settings; }
		}

		public IServerUserHandler Users
		{
			get;
			private set;
		}

		public IConnectionHandler Connections
		{
			get { return Users; }
		}

		public IServerUserManager UserManager
		{
			get;
			private set;
		}

		public IServerSourceHandler Sources
		{
			get;
			private set;
		}

		public IServerChannelHandler Channels
		{
			get;
			private set;
		}

		public IEnumerable<IRedirector> Redirectors
		{
			get
			{
				lock (redirectors)
				{
					return redirectors.ToList();
				}
			}
		}

		#region Public Methods
		/// <summary>
		/// Adds and starts an <c>IConnectionProvider</c>.
		/// </summary>
		/// <param name="provider">The <c>IConnectionProvider</c> to add and start listening.</param>
		/// <exception cref="ArgumentNullException"><paramref name="provider"/> is <c>null</c>.</exception>
		public void AddConnectionProvider (IConnectionProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			this.Log.InfoFormat ("{0} added.", provider.GetType().Name);

			// MUST provide a gaurantee of persona
			lock (this.availableConnections)
			{
				provider.ConnectionMade += OnConnectionMade;
				provider.ConnectionlessMessageReceived += OnMessageReceived;
				provider.StartListening (this);
		
				this.availableConnections.Add (provider);
			}
		}

		/// <summary>
		/// Stops and removes an <c>IConnectionProvider</c>.
		/// </summary>
		/// <param name="provider">The connection provider to remove.</param>
		/// <exception cref="ArgumentNullException"><paramref name="provider"/> is <c>null</c>.</exception>
		public void RemoveConnectionProvider (IConnectionProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");

			RemoveConnectionProvider (provider, true);
		}

		/// <summary>
		/// Adds <paramref name="redirector"/> to the list of redirectors.
		/// </summary>
		/// <param name="redirector">The redirector to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="redirector"/> is <c>null</c>.</exception>
		public void AddRedirector (IRedirector redirector)
		{
			if (redirector == null)
				throw new ArgumentNullException ("redirector");

			lock (redirectors)
			{
				redirectors.Add (redirector);
			}
		}

		/// <summary>
		/// Removes <paramref name="redirector"/> from the list of redirectors.
		/// </summary>
		/// <param name="redirector">The redirector to remove.</param>
		/// <returns><c>true</c> if <paramref name="redirector"/> was found</returns>
		/// <exception cref="ArgumentNullException"><paramref name="redirector"/> is <c>null</c>.</exception>
		public bool RemoveRedirector (IRedirector redirector)
		{
			if (redirector == null)
				throw new ArgumentNullException ("redirector");

			lock (redirectors)
			{
				return redirectors.Remove (redirector);
			}
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
					RemoveConnectionProvider (provider);
				}
			}

			this.incomingWait.Set ();
			this.messageRunnerThread.Join();

			Users.Disconnect (c => true);
		}
		#endregion

		private readonly ServerSettings settings;

		private readonly List<IRedirector> redirectors = new List<IRedirector>();
		private readonly List<IConnectionProvider> availableConnections = new List<IConnectionProvider> ();

		private readonly IBackendProvider backendProvider;

		private readonly IServerContext context;
		private readonly IChannelProvider channelProvider;
		private readonly IPermissionsProvider permissionProvider;
		private readonly IAuthenticationProvider authProvider;
		private volatile bool running = true;

		private readonly object syncRoot = new object();

		protected readonly ILog Log;

		private Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>> handlers;

		private readonly Thread messageRunnerThread;
		private readonly Queue<MessageReceivedEventArgs> mqueue = new Queue<MessageReceivedEventArgs> (1000);
		private readonly AutoResetEvent incomingWait = new AutoResetEvent (false);

		IBackendProvider IServerContext.BackendProvider
		{
			get { return backendProvider; }
		}

		IChannelProvider IServerContext.ChannelsProvider
		{
			get { return (backendProvider ?? channelProvider); }
		}

		IPermissionsProvider IServerContext.PermissionsProvider
		{
			get { return (backendProvider ?? permissionProvider); }
		}

		IAuthenticationProvider IServerContext.AuthenticationProvider
		{
			get { return (backendProvider ?? authProvider); }
		}

		int IServerContext.ProtocolVersion
		{
			get { return GablarskiServer.ProtocolVersion; }
		}

		protected GablarskiServer (ServerSettings serverSettings)
		{
			Log = LogManager.GetLogger (serverSettings.Name.Remove (" "));

			this.settings = serverSettings;
			this.context = this;

			this.messageRunnerThread = new Thread (this.MessageRunner);
			this.messageRunnerThread.Name = "Gablarski Server Message Runner";
		}

		private void SetupHandlers()
		{
			this.UserManager = new ServerUserManager();

			var userHandler = new ServerUserHandler (this, this.UserManager);
			this.Users = userHandler;

			var sourceHandler = new ServerSourceHandler (this, new ServerSourceManager (this));
			this.Sources = sourceHandler;
			
			var channelHandler = new ServerChannelHandler (this);
			this.Channels = channelHandler;

			this.handlers = new Dictionary<ClientMessageType, Action<MessageReceivedEventArgs>>
			{
				{ ClientMessageType.Connect, userHandler.ConnectMessage },
				{ ClientMessageType.Disconnect, ClientDisconnected },
				{ ClientMessageType.Login, userHandler.LoginMessage },
				{ ClientMessageType.Join, userHandler.JoinMessage },
				{ ClientMessageType.SetComment, userHandler.SetCommentMessage },
				{ ClientMessageType.SetStatus, userHandler.SetStatusMessage },

				{ ClientMessageType.RequestSource, sourceHandler.RequestSourceMessage },
				{ ClientMessageType.AudioData, sourceHandler.SendAudioDataMessage },
				{ ClientMessageType.ClientAudioSourceStateChange, sourceHandler.ClientAudioSourceStateChangeMessage },
				{ ClientMessageType.RequestMuteUser, userHandler.RequestMuteUserMessage },
				{ ClientMessageType.RequestMuteSource, sourceHandler.RequestMuteSourceMessage },

				{ ClientMessageType.QueryServer, ClientQueryServer },
				{ ClientMessageType.RequestChannelList, channelHandler.RequestChanneListMessage },
				{ ClientMessageType.RequestUserList, userHandler.RequestUserListMessage },
				{ ClientMessageType.RequestSourceList, sourceHandler.RequestSourceListMessage },

				{ ClientMessageType.ChannelChange, userHandler.ChannelChangeMessage },
				{ ClientMessageType.ChannelEdit, channelHandler.ChannelEditMessage },
			};
		}

		private void MessageRunner ()
		{
			while (this.running)
			{
				if (mqueue.Count == 0)
					incomingWait.WaitOne ();

				while (mqueue.Count > 0)
				{
					MessageReceivedEventArgs e;
					lock (mqueue)
					{
						e = mqueue.Dequeue();
					}

					var msg = (e.Message as ClientMessage);
					if (msg == null)
					{
						Disconnect (e.Connection);
						return;
					}

					Action<MessageReceivedEventArgs> handler;
					if (this.handlers.TryGetValue (msg.MessageType, out handler))
						handler (e);
				}
			}
		}

		protected virtual void OnMessageReceived (object sender, MessageReceivedEventArgs e)
		{
			lock (mqueue)
			{
				mqueue.Enqueue (e);
			}

			incomingWait.Set ();
		}

		private void Disconnect (IConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");

			connection.Disconnect ();
			connection.MessageReceived -= this.OnMessageReceived;
			connection.Disconnected -= this.OnClientDisconnected;
			Users.Disconnect (connection);
		}

		private void RemoveConnectionProvider (IConnectionProvider provider, bool listRemove)
		{
			Log.InfoFormat ("{0} removed.", provider.GetType ().Name);

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

		private void OnPermissionsChanged (object sender, PermissionsChangedEventArgs e)
		{
			UserInfo user = Users[e.UserId];
			if (user != null)
				UserManager.GetConnection (user).Send (new PermissionsMessage (e.UserId, this.context.PermissionsProvider.GetPermissions (e.UserId)));
		}

		protected void ClientDisconnected (MessageReceivedEventArgs e)
		{
			e.Connection.Disconnect();

			OnClientDisconnected (this, new ConnectionEventArgs (e.Connection));
		}

		private void OnClientDisconnected (object sender, ConnectionEventArgs e)
		{
			Log.Debug ("Client disconnected");

			e.Connection.MessageReceived -= this.OnMessageReceived;
			e.Connection.Disconnected -= this.OnClientDisconnected;

			UserInfo user = UserManager.GetUser (e.Connection);
			if (user != null)
			{
				Sources.Remove (user);
				Users.Disconnect (e.Connection);
			}
		}
		
		private void OnConnectionMade (object sender, ConnectionEventArgs e)
		{
			Log.Debug ("Connection Made");

			e.Connection.MessageReceived += this.OnMessageReceived;
			e.Connection.Disconnected += this.OnClientDisconnected;
		}

		protected ServerInfo GetServerInfo()
		{
			return new ServerInfo (this.settings);
		}

		private void ClientQueryServer (MessageReceivedEventArgs e)
		{
			var msg = (QueryServerMessage)e.Message;
			var connectionless = (e as ConnectionlessMessageReceivedEventArgs);
			
			if (!msg.ServerInfoOnly && (connectionless != null || !context.GetPermission (PermissionName.RequestChannelList, e.Connection)))
			{
				e.Connection.Send (new PermissionDeniedMessage (ClientMessageType.QueryServer));
				return;
			}

			var result = new QueryServerResultMessage();

			if (!msg.ServerInfoOnly)
			{
				result.Channels = this.context.ChannelsProvider.GetChannels();
				result.Users = this.Users;
			}

			result.ServerInfo = GetServerInfo();

			if (connectionless == null)
				e.Connection.Send (result);
			else
				connectionless.Provider.SendConnectionlessMessage (result, connectionless.EndPoint);
		}
	}
}