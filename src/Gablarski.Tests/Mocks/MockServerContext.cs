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


using System.Collections.Generic;
using System.Threading;
using Gablarski.Server;
using Tempest;

namespace Gablarski.Tests.Mocks
{
	public class MockServerContext
		: ServerBase, IGablarskiServerContext
	{
		public MockServerContext()
			: base (MessageTypes.All)
		{
		}

		public MockServerContext (IConnectionProvider provider)
			: base (provider, MessageTypes.All)
		{
		}

		public ReaderWriterLockSlim Synchronization
		{
			get { return this.syncRoot; }
		}

		public IUserProvider UserProvider
		{
			get;
			set;
		}

		public IPermissionsProvider PermissionsProvider
		{
			get;
			set;
		}

		public IChannelProvider ChannelsProvider
		{
			get;
			set;
		}

		public IEnumerable<IConnection> Connections
		{
			get { return this.connections.Keys; }
		}

		public IServerUserHandler Users
		{
			get;
			set;
		}

		public IServerUserManager UserManager
		{
			get;
			set;
		}

		public IServerSourceHandler Sources
		{
			get;
			set;
		}

		public IServerChannelHandler Channels
		{
			get;
			set;
		}

		public IEnumerable<IRedirector> Redirectors
		{
			get;
			set;
		}

		public ServerSettings Settings
		{
			get;
			set;
		}

		private readonly ReaderWriterLockSlim syncRoot = new ReaderWriterLockSlim();
	}
}