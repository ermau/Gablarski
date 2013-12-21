// Copyright (c) 2011-2013, Eric Maupin
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

using System.Threading.Tasks;
using Gablarski.Server;
using Tempest;
using Tempest.Providers.Network;

namespace Gablarski
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

		public static GuestUserProvider Authorization
		{
			get { return authorization; }
		}

		public static Task StartAsync ()
		{
			return Task.Run (() => {
				var crypto = new RSACrypto();
				return crypto.ExportKey (true);
			}).ContinueWith (t => StartAsync (t.Result), TaskScheduler.Default).Unwrap();
		}

		public static Task StartAsync (RSAAsymmetricKey key)
		{
			return Task.Factory.StartNew (Start, key, TaskCreationOptions.HideScheduler);
		}

		public static void Shutdown()
		{
			if (server == null)
				return;

			server.Stop();
			server = null;
			channels = null;
			authorization = null;
			permissions = null;
			settings = null;
		}

		private static readonly object Sync = new object();

		private static LobbyChannelProvider channels;
		private static GuestUserProvider authorization;
		private static GuestPermissionProvider permissions;
		private static ServerSettings settings;
		private static GablarskiServer server;

		private static void Start (object s)
		{
			var key = (RSAAsymmetricKey)s;

			lock (Sync) {
				if (IsRunning)
					return;

				channels = new LobbyChannelProvider();
				authorization = new GuestUserProvider { FirstUserIsAdmin = true };
				permissions = new GuestPermissionProvider();
				settings = new ServerSettings();

				server = new GablarskiServer (settings, authorization, permissions, channels);
				server.AddConnectionProvider (new UdpConnectionProvider (GablarskiProtocol.Port, GablarskiProtocol.Instance, key), ExecutionMode.GlobalOrder);

				server.Start();
			}
		}
	}
}