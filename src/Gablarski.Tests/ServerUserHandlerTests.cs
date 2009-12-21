// Copyright (c) 2009, Eric Maupin
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
using System.Text;
using Gablarski.Messages;
using Gablarski.Server;
using Gablarski.Tests.Mocks;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerUserHandlerTests
	{
		private ServerUserHandler handler;
		private MockServerConnection server;

		[SetUp]
		public void Setup()
		{
			handler = new ServerUserHandler (new MockServerContext(), new ServerUserManager());
			server = new MockServerConnection();
		}

		[TearDown]
		public void Teardown()
		{
			handler = null;
			server = null;
		}

		[Test]
		public void Connect()
		{
			handler.ConnectMessage (new MessageReceivedEventArgs (server.Client, 
				new ConnectMessage { ProtocolVersion = GablarskiServer.ProtocolVersion }));

			Assert.IsTrue (handler.Manager.GetIsConnected (server.Client));
		}

		[Test]
		public void ConnectInvalidVersion()
		{
			handler.ConnectMessage (new MessageReceivedEventArgs (server, 
				new ConnectMessage { ProtocolVersion = 1 }));

			Assert.IsFalse (handler.Manager.GetIsConnected (server.Client));

			var rejected = server.Client.DequeueAndAssertMessage<ConnectionRejectedMessage>();
			Assert.AreEqual (ConnectionRejectedReason.IncompatibleVersion, rejected.Reason);
		}
	}
}