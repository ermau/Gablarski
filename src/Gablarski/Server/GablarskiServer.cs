// Copyright (c) 2011, Eric Maupin
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cadenza.Collections;
using Gablarski.Messages;
using Cadenza;
using log4net;

namespace Gablarski.Server
{
	public class GablarskiServer
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
		public GablarskiServer (ServerSettings settings, IUserProvider authProvider, IPermissionsProvider permissionProvider, IChannelProvider channelProvider)
			: this (settings)
		{
			this.authProvider = authProvider;
			
			this.permissionProvider = permissionProvider;
			this.permissionProvider.PermissionsChanged += OnPermissionsChanged;
		
			this.channelProvider = channelProvider;

			SetupHandlers();
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

			// MUST provide a guarantee of persona
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
			this.pingTimer = new Timer (s => this.users.Send (new ServerPingMessage()), null, 20000, 20000);
		}

		/// <summary>
		/// Removes all connection providers, disconnects all users and shuts the server down.
		/// </summary>
		public void Shutdown ()
		{
			this.running = false;
			
			lock (this.availableConnections)
			{
				while (this.availableConnections.Count > 0)
					RemoveConnectionProvider (this.availableConnections[0]);
			}

			this.incomingWait.Set ();
			this.messageRunnerThread.Join();

			this.users.Disconnect (c => true);
		}

		public void RegisterMessageHandler (ClientMessageType messageType, Action<MessageReceivedEventArgs> handler)
		{
			if (handler == null)
				throw new ArgumentNullException ("handler");

			this.handlers.Add (messageType, handler);
		}

		#endregion

		private readonly ServerSettings settings;

		private readonly List<IRedirector> redirectors = new List<IRedirector>();
		private readonly List<IConnectionProvider> availableConnections = new List<IConnectionProvider> ();

		private readonly Decryption decryption = new Decryption();

		private readonly IServerContext context;
		private readonly IChannelProvider channelProvider;
		private readonly IPermissionsProvider permissionProvider;
		private readonly IUserProvider authProvider;
		private volatile bool running = true;

		private readonly object syncRoot = new object();

		protected readonly ILog Log;

		private readonly MutableLookup<ClientMessageType, Action<MessageReceivedEventArgs>> handlers = new MutableLookup<ClientMessageType, Action<MessageReceivedEventArgs>>();
		private Dictionary<ClientMessageType, Action<IEnumerable<MessageReceivedEventArgs>>> setHandlers;

		private readonly Thread messageRunnerThread;
		private readonly Queue<MessageReceivedEventArgs> mqueue = new Queue<MessageReceivedEventArgs> (1000);
		private readonly AutoResetEvent incomingWait = new AutoResetEvent (false);
		private IServerUserHandler users;
		private ServerSourceHandler sources;
		private ServerChannelHandler channels;
		private ServerUserManager userManager;
		private Timer pingTimer;

		IServerUserHandler IServerContext.Users
		{
			get { return this.users; }
		}

		IConnectionHandler IServerContext.Connections
		{
			get { return this.users; }
		}

		IServerSourceHandler IServerContext.Sources
		{
			get { return this.sources; }
		}

		IServerChannelHandler IServerContext.Channels
		{
			get { return this.channels; }
		}

		IChannelProvider IServerContext.ChannelsProvider
		{
			get { return this.channelProvider; }
		}

		IPermissionsProvider IServerContext.PermissionsProvider
		{
			get { return this.permissionProvider; }
		}

		IUserProvider IServerContext.UserProvider
		{
			get { return this.authProvider; }
		}

		PublicRSAParameters IServerContext.EncryptionParameters
		{
			get { return this.decryption.PublicParameters; }
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
			this.userManager = new ServerUserManager();

			var userHandler = new ServerUserHandler (this, this.userManager);
			this.users = userHandler;

			var sourceHandler = new ServerSourceHandler (this, new ServerSourceManager (this));
			this.sources = sourceHandler;
			
			var channelHandler = new ServerChannelHandler (this);
			this.channels = channelHandler;

			RegisterMessageHandler (ClientMessageType.Disconnect, ClientDisconnected);
			RegisterMessageHandler (ClientMessageType.QueryServer, ClientQueryServer);
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
					MessageReceivedEventArgs next = null;
					lock (mqueue)
					{
						e = mqueue.Dequeue();

						if (mqueue.Count > 0)
							next = mqueue.Peek();
					}

					var msg = (e.Message as ClientMessage);
					if (msg == null)
					{
						Disconnect (e.Connection);
						return;
					}

					if (next != null && next.Message.MessageTypeCode == e.Message.MessageTypeCode)
					{
						Action<IEnumerable<MessageReceivedEventArgs>> setHandler;
						if (this.setHandlers.TryGetValue (msg.MessageType, out setHandler))
						{
							var messages = new List<MessageReceivedEventArgs> (mqueue.Count);
							lock (mqueue)
							{
								while (next != null && next.Message.MessageTypeCode == msg.MessageTypeCode)
								{
									e = mqueue.Dequeue();

									messages.Add (e);

									next = (mqueue.Count > 0) ? mqueue.Peek() : null;
								}

								setHandler (messages);
							}
						}
					}
					else
					{
						IEnumerable<Action<MessageReceivedEventArgs>> ehandlers;
						if (this.handlers.TryGetValues (msg.MessageType, out ehandlers))
						{
							foreach (var handler in ehandlers)
								handler (e);
						}
					}
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
			this.users.Disconnect (connection);
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
			IUserInfo user = this.users[e.UserId];
			if (user != null)
				this.userManager.GetConnection (user).Send (new PermissionsMessage (e.UserId, this.context.PermissionsProvider.GetPermissions (e.UserId)));
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

			IUserInfo user = this.userManager.GetUser (e.Connection);
			if (user != null)
			{
				this.sources.Remove (user);
				this.users.Disconnect (e.Connection);
			}
		}
		
		private void OnConnectionMade (object sender, ConnectionMadeEventArgs e)
		{
			Log.Debug ("Connection Made");

			if (e.Cancel)
				return;

			foreach (BanInfo ban in this.authProvider.GetBans().Where (b => b.IPMask != null))
			{
				string[] parts = ban.IPMask.Split ('.');
				string[] addressParts = e.Connection.IPAddress.ToString().Split ('.');
				for (int i = 0; i < parts.Length; ++i)
				{
					if (i + 1 == parts.Length || parts[i] == "*")
					{
						e.Cancel = true;
						return;
					}

					if (addressParts[i] != parts[i])
						break;
				}
			}

			e.Connection.Decryption = this.decryption;
			e.Connection.MessageReceived += this.OnMessageReceived;
			e.Connection.Disconnected += this.OnClientDisconnected;
		}

		protected ServerInfo GetServerInfo()
		{
			return new ServerInfo (this.settings, ((IServerContext)this).UserProvider, this.decryption.PublicParameters);
		}

		private void ClientQueryServer (MessageReceivedEventArgs e)
		{
			var msg = (QueryServerMessage)e.Message;
			var connectionless = (e as ConnectionlessMessageReceivedEventArgs);
			
			if (!msg.ServerInfoOnly && !context.GetPermission (PermissionName.RequestChannelList, e.Connection))
			{
				var denied = new PermissionDeniedMessage (ClientMessageType.QueryServer);

				if (connectionless != null)
					connectionless.Provider.SendConnectionlessMessage (denied, connectionless.EndPoint);
				else
					e.Connection.Send (denied);

				return;
			}

			var result = new QueryServerResultMessage();

			if (!msg.ServerInfoOnly)
			{
				result.Channels = this.context.ChannelsProvider.GetChannels();
				result.Users = this.users.ToList();
			}

			result.ServerInfo = GetServerInfo();

			if (connectionless == null)
				e.Connection.Send (result);
			else
				connectionless.Provider.SendConnectionlessMessage (result, connectionless.EndPoint);
		}
	}
}