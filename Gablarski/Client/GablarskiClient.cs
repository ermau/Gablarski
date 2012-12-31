// Copyright (c) 2012, Eric Maupin
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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tempest;
using Tempest.Providers.Network;
using Tempest.Social;

namespace Gablarski.Client
{
	public sealed class GablarskiClient
	{
		public GablarskiClient (IAsymmetricKey key)
			: this (
				new NetworkClientConnection (GablarskiProtocol.Instance, CryptoFactory, key),
				new NetworkClientConnection (SocialProtocol.Instance, CryptoFactory, key))
		{
		}

		public GablarskiClient (IClientConnection connection, IClientConnection socialConnection)
		{
			if (connection == null)
				throw new ArgumentNullException ("connection");
			if (socialConnection == null)
				throw new ArgumentNullException ("socialConnection");

			var persona = new Person (GetIdentity (socialConnection.LocalKey))
			{
				Nickname = "Gablarski User",
				Status = Status.Online
			};

			this.social = new SocialClient (socialConnection, persona);
			this.client = new LocalClient (connection, MessageTypes.All);

			this.social.StartingConnection += OnStartingConnection;
		}

		private void OnStartingConnection (object sender, ConnectEventArgs e)
		{
			if (e.YoureHosting)
			{
				
			}
			else
			{
				this.client.ConnectAsync (e.Target);
			}
		}

		/// <summary>
		/// Raised when a social connection is requested.
		/// </summary>
		public event EventHandler<RequestConnectEventArgs> ConnectionRequest
		{
			add { this.social.ConnectionRequest += value; }
			remove { this.social.ConnectionRequest -= value; }
		}

		/// <summary>
		/// Gets your persona that includes your identifier as well as profile information.
		/// </summary>
		public Person Persona
		{
			get { return this.social.Persona; }
		}

		public WatchList BuddyList
		{
			get { return this.social.WatchList; }
		}

		/// <summary>
		/// Asynchronously connects to a Tempest.Social server
		/// </summary>
		/// <param name="socialServer">The location of the social server.</param>
		/// <returns>The connection result.</returns>
		public Task<ClientConnectionResult> ConnectSocialAsync (Target socialServer)
		{
			return this.social.ConnectAsync (socialServer);
		}

		private readonly SocialClient social;
		private readonly LocalClient client;

		private static IPublicKeyCrypto CryptoFactory()
		{
			return new RSACrypto();
		}

		private static string GetIdentity (IAsymmetricKey key)
		{
			StringBuilder builder = new StringBuilder (key.PublicSignature.Length * 2);
			foreach (byte b in key.PublicSignature)
				builder.Append (b.ToString ("X2"));

			return builder.ToString();
		}
	}
}