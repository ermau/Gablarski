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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Gablarski.Messages;
using Gablarski.Network;

namespace Gablarski.Client
{
	public class ServerQuery
	{
		internal ServerQuery (string host, int port, object tag, Action<ServerQuery> callback)
		{
			if (host == null)
				throw new ArgumentNullException ("host");
			if (callback == null)
				throw new ArgumentNullException ("callback");
			if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
				throw new ArgumentOutOfRangeException ("port");

			this.host = host;
			this.port = port;
			this.tag = tag;
			this.callback = callback;
		}

		public object Tag
		{
			get { return this.tag; }
		}

		public ConnectionRejectedReason RejectedReason
		{
			get;
			private set;
		}

		public ServerInfo ServerInfo
		{
			get;
			private set;
		}

		public IEnumerable<IChannelInfo> Channels
		{
			get;
			private set;
		}

		public IEnumerable<IUserInfo> Users
		{
			get;
			private set;
		}

		internal void Run()
		{
			this.client.Connected += ClientOnConnected;
			this.client.ConnectionRejected += ClientOnConnectionRejected;
			this.client.Connect (this.host, this.port);
		}

		private readonly GablarskiClient client = new GablarskiClient (new NetworkClientConnection());
		private readonly string host;
		private readonly int port;
		private readonly object tag;
		private readonly Action<ServerQuery> callback;

		private void ClientOnConnected (object sender, EventArgs e)
		{
			this.client.Connected -= ClientOnConnected;
			this.client.ConnectionRejected -= ClientOnConnectionRejected;

			Channels = this.client.Channels;
			Users = this.client.Users;
			ServerInfo = this.client.ServerInfo;

			this.callback (this);
			this.client.Disconnect();
		}

		private void ClientOnConnectionRejected(object sender, RejectedConnectionEventArgs e)
		{
			RejectedReason = e.Reason;

			this.client.Connected -= ClientOnConnected;
			this.client.ConnectionRejected -= ClientOnConnectionRejected;

			this.callback (this);
			this.client.Disconnect();
		}
	}
}