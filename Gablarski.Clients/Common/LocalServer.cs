using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Network;
using Gablarski.Server;

namespace Gablarski.Clients.Common
{
	public static class LocalServer
	{
		public static bool IsRunning
		{
			get { return (server != null && server.IsRunning); }
		}

		public static ServerSettings Settings
		{
			get { return settings; }
		}

		public static LobbyChannelProvider Channels
		{
			get { return channels; }
		}

		public static GuestPermissionProvider Permissions
		{
			get { return permissions; }
		}

		public static GuestAuthProvider Authorization
		{
			get { return authorization; }
		}

		public static void Start()
		{
			channels = new LobbyChannelProvider();
			authorization = new GuestAuthProvider();
			permissions = new GuestPermissionProvider();
			settings = new ServerSettings();
			server = new GablarskiServer (settings, authorization, permissions, channels);
			server.AddConnectionProvider (new NetworkServerConnectionProvider());

			server.Start();
		}

		public static void Shutdown()
		{
			if (server == null)
				return;

			server.Shutdown();
			server = null;
			channels = null;
			authorization = null;
			permissions = null;
			settings = null;
		}

		private static LobbyChannelProvider channels;
		private static GuestAuthProvider authorization;
		private static GuestPermissionProvider permissions;
		private static ServerSettings settings;
		private static GablarskiServer server;
	}
}