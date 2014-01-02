// Copyright (c) 2011-2014, Eric Maupin
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
using Gablarski.Messages;
using Tempest;
using Timer = System.Threading.Timer;

namespace Gablarski.Server
{
	public class GablarskiServer
		: TempestServer, IGablarskiServerContext
	{
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

		private readonly ServerSettings settings;

		private readonly List<IConnection> connections = new List<IConnection>();
		private readonly List<IRedirector> redirectors = new List<IRedirector>();

		private readonly IGablarskiServerContext context;
		private readonly IChannelProvider channelProvider;
		private readonly IPermissionsProvider permissionProvider;
		private readonly IUserProvider authProvider;

		private readonly object syncRoot = new object();

		private IServerUserHandler users;
		private ServerSourceHandler sources;
		private ServerChannelHandler channels;
		private ServerUserManager userManager;
		private Timer pingTimer;

		IServerUserHandler IGablarskiServerContext.Users
		{
			get { return this.users; }
		}

		IServerSourceHandler IGablarskiServerContext.Sources
		{
			get { return this.sources; }
		}

		IServerChannelHandler IGablarskiServerContext.Channels
		{
			get { return this.channels; }
		}

		IChannelProvider IGablarskiServerContext.ChannelsProvider
		{
			get { return this.channelProvider; }
		}

		IPermissionsProvider IGablarskiServerContext.PermissionsProvider
		{
			get { return this.permissionProvider; }
		}

		IUserProvider IGablarskiServerContext.UserProvider
		{
			get { return this.authProvider; }
		}

		IEnumerable<IConnection> IGablarskiServerContext.Connections
		{
			get
			{
				lock (this.syncRoot)
					return this.userManager.GetConnections();
			}
		}

		protected GablarskiServer (ServerSettings serverSettings)
			: base (MessageTypes.All)
		{
			this.settings = serverSettings;
			this.context = this;
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
			
			RegisterConnectionlessMessageHandler (GablarskiProtocol.Instance, (ushort)GablarskiMessageType.QueryServer, ClientQueryServer);
		}

		private void OnPermissionsChanged (object sender, PermissionsChangedEventArgs e)
		{
			IUserInfo user = this.users[e.UserId];
			if (user != null)
				this.userManager.GetConnection (user).SendAsync (new PermissionsMessage (e.UserId, this.context.PermissionsProvider.GetPermissions (e.UserId)));
		}

		protected override void OnConnectionDisconnectedGlobal (object sender, DisconnectedEventArgs e)
		{
			IUserInfo user = this.userManager.GetUser (e.Connection);
			if (user != null)
			{
				this.sources.Remove (user);
				this.users.DisconnectAsync (user, DisconnectionReason.Unknown); // TODO reason convert
			}

			base.OnConnectionDisconnectedGlobal (sender, e);
		}

		protected override void OnConnectionMadeGlobal (object sender, ConnectionMadeEventArgs e)
		{
			lock (this.syncRoot)
				this.connections.Add (e.Connection);

			// TODO
			//foreach (BanInfo ban in this.authProvider.GetBans().Where (b => b.IPMask != null))
			//{
			//    string[] parts = ban.IPMask.Split ('.');
			//    string[] addressParts = e.Connection.IPAddress.ToString().Split ('.');
			//    for (int i = 0; i < parts.Length; ++i)
			//    {
			//        if (i + 1 == parts.Length || parts[i] == "*")
			//        {
			//            e.Rejected = true;
			//            return;
			//        }

			//        if (addressParts[i] != parts[i])
			//            break;
			//    }
			//}

			base.OnConnectionMadeGlobal (sender, e);
		}

		protected ServerInfo GetServerInfo()
		{
			return new ServerInfo (this.settings, ((IGablarskiServerContext)this).UserProvider);
		}

		private void ClientQueryServer (ConnectionlessMessageEventArgs e)
		{
			var msg = (QueryServerMessage)e.Message;
			
			if (!msg.ServerInfoOnly && !context.GetPermission (PermissionName.RequestChannelList)) {
				var denied = new PermissionDeniedMessage (GablarskiMessageType.QueryServer);
				e.Messenger.SendConnectionlessMessageAsync (denied, e.From);

				return;
			}

			var result = new QueryServerResultMessage();

			if (!msg.ServerInfoOnly) {
				result.Channels = this.context.ChannelsProvider.GetChannels();
				result.Users = this.users.ToList();
			}

			result.ServerInfo = GetServerInfo();
			e.Messenger.SendConnectionlessMessageAsync (result, e.From);
		}
	}
}