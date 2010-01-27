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
		private MockServerContext context;

		[SetUp]
		public void Setup()
		{
			context = new MockServerContext
			{
				Settings = new ServerSettings(),
				AuthenticationProvider = new GuestAuthProvider(),
				PermissionsProvider = new GuestPermissionProvider(),
				ChannelsProvider = new LobbyChannelProvider()
			};

			context.Sources = new ServerSourceHandler (context, new ServerSourceManager (context));

			handler = new ServerUserHandler (context, new ServerUserManager());
			server = new MockServerConnection();
		}

		[TearDown]
		public void Teardown()
		{
			handler = null;
			server = null;
			context = null;
		}

		[Test]
		public void DisconnectNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.Disconnect ((IConnection)null));
			Assert.Throws<ArgumentNullException> (() => handler.Disconnect ((Func<IConnection, bool>)null));
		}

		[Test]
		public void MoveNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.Move (null, new ChannelInfo()));
			Assert.Throws<ArgumentNullException> (() => handler.Move (new UserInfo(), null));
		}

		//[Test]
		//public void Move()
		//{
		//    Connect();
		//    JoinAsGuest();

		//    UserInfo user = handler.First();
			
		//    server.Client.DequeueAndAssertMessage<ChannelChangeResultMessage>();
		//}

		[Test]
		public void SendNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.Send (new ConnectMessage(), (Func<IConnection, bool>)null));
			Assert.Throws<ArgumentNullException> (() => handler.Send (null, c => false));
			Assert.Throws<ArgumentNullException> (() => handler.Send (new ConnectMessage(), (Func<IConnection, UserInfo, bool>)null));
			Assert.Throws<ArgumentNullException> (() => handler.Send (null, (c, u) => false));
		}

		[Test]
		public void SendConnection()
		{
			Connect();

			var msg = new ConnectMessage { ProtocolVersion = 42 };
			handler.Send (msg, cc => cc == server);

			server.DequeueAndAssertMessage<ConnectMessage>();
		}

		[Test]
		public void SendJoinedUser()
		{
			Connect();
			JoinAsGuest();

			var user = handler.Manager.First();

			var msg = new ConnectMessage { ProtocolVersion = 42 };
			handler.Send (msg, (cc, u) => cc == server && u == user);
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

		[Test]
		public void JoinAsGuest()
		{
			JoinAsGuest (true, true, null, "Nickname", null);
		}
		
		[Test]
		public void JoinAsGuestWithServerPassword()
		{
			JoinAsGuest (true, true, "pass", "Nickname", "pass");
		}
		
		[Test]
		public void JoinAsGuestWhenNotAllowed()
		{
			JoinAsGuest (false, false, null, "Nickname", null);
		}
		
		[Test]
		public void JoinAsGuestWhenNotAllowedWithPassword()
		{
			JoinAsGuest (false, false, "pass", "Nickname", "pass");
		}
		
		[Test]
		public void JoinAsGuestWhenNotAllowedWithWrongPassword()
		{
			JoinAsGuest (false, false, "pass", "Nickname", "wrong");
		}
		
		[Test]
		public void JoinAsGuestWithNoPassword()
		{
			JoinAsGuest (false, true, "pass", "Nickname", null);
		}
		
		[Test]
		public void JoinAsGuestWithWrongPassword()
		{
			JoinAsGuest (false, true, "pass", "Nickname", "wrong");
		}
		
//		[TestCase (true, true, null, "Nickname", null)]
//		[TestCase (true, true, "pass", "Nickname", "pass")]
//		[TestCase (false, false, null, "Nickname", null)]
//		[TestCase (false, false, "pass", "Nickname", "pass")]
//		[TestCase (false, false, "pass", "Nickname", "pwrong")]
//		[TestCase (false, true, "pass", "Nickname", null)]
//		[TestCase (false, true, "pass", "Nickname", "wrong")]
		public void JoinAsGuest (bool shouldWork, bool allowGuests, string serverPassword, string nickname, string userServerpassword)
		{
			context.Settings.AllowGuestLogins = allowGuests;
			context.Settings.ServerPassword = serverPassword;

			handler.Manager.Connect (server);
			
			handler.JoinMessage (new MessageReceivedEventArgs (server,
				new JoinMessage (nickname, userServerpassword)));
			
			if (shouldWork)
				Assert.IsTrue (handler.Manager.GetIsJoined (server));
			else
				Assert.IsFalse (handler.Manager.GetIsJoined (server));
		}
	}
}