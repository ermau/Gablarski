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
using System.Text;
using Gablarski.Messages;
using Gablarski.Server;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerUserManagerTests
	{
		private IServerUserManager manager;
		private MockServerConnection server;
		private MockClientConnection client;
		private UserInfo user;

		[SetUp]
		public void Setup()
		{
			user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			manager = new ServerUserManager();
			server = new MockServerConnection();
			client = new MockClientConnection (server);
		}

		[TearDown]
		public void TearDown()
		{
			user = null;
			manager = null;
			server = null;
			client = null;
		}

		[Test]
		public void LoginNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Login (null, user));
			Assert.Throws<ArgumentNullException> (() => manager.Login (client, null));
		}

		[Test]
		public void Login()
		{
			manager.Connect (client);

			Assert.IsFalse (manager.GetIsLoggedIn (client));

			manager.Login (client, user);

			Assert.IsTrue (manager.GetIsLoggedIn (client));
		}

		[Test]
		public void LoginIgnoreBeforeConnect()
		{
			manager.Login (client, user);
			Assert.IsFalse (manager.GetIsLoggedIn (client));
			Assert.IsFalse (manager.GetIsLoggedIn (user));
		}

		[Test]
		public void GetIsLoggedInNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetIsLoggedIn ((IConnection)null));
			Assert.Throws<ArgumentNullException> (() => manager.GetIsLoggedIn ((UserInfo)null));
		}

		[Test]
		public void GetIsLoggedInConnection()
		{
			manager.Connect (client);

			Assert.IsFalse (manager.GetIsLoggedIn (client));

			manager.Login (client, user);

			Assert.IsTrue (manager.GetIsLoggedIn (client));
		}

		[Test]
		public void GetIsLoggedInConnectionNotFound()
		{
			Assert.IsFalse (manager.GetIsLoggedIn (client));
		}

		[Test]
		public void GetIsLoggedInUser()
		{
			manager.Connect (client);

			Assert.IsFalse (manager.GetIsLoggedIn (user));

			manager.Login (client, user);

			Assert.IsTrue (manager.GetIsLoggedIn (user));
		}

		[Test]
		public void GetIsLoggedInUserNotFound()
		{
			Assert.IsFalse (manager.GetIsLoggedIn (user));
		}

		[Test]
		public void JoinNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Join (client, null));
			Assert.Throws<ArgumentNullException> (() => manager.Join (null, user));
		}

		[Test]
		public void JoinIgnoreNotConnected()
		{
			manager.Join (client, user);
			Assert.IsFalse (manager.GetIsJoined (client));
			Assert.IsFalse (manager.GetIsJoined (user));
		}

		[Test]
		public void Join()
		{
			manager.Connect (client);
			manager.Join (client, user);

			Assert.IsTrue (manager.GetIsJoined (client));
			Assert.IsTrue (manager.GetIsJoined (user));

			UserInfo joinedUser = manager.GetUser (client);
			Assert.AreEqual (user.UserId, joinedUser.UserId);
			Assert.AreEqual (user.Username, joinedUser.Username);
			Assert.AreEqual (user.Nickname, joinedUser.Nickname);
			Assert.AreEqual (user.Phonetic, joinedUser.Phonetic);
			Assert.AreEqual (user.Status, joinedUser.Status);
			Assert.AreEqual (user.IsMuted, joinedUser.IsMuted);
		}

		[Test]
		public void JoinIgnoreDupe()
		{
			manager.Connect (client);
			manager.Join (client, user);
			manager.Join (client, user);

			Assert.IsTrue (manager.GetIsJoined (client));
			Assert.IsTrue (manager.GetIsJoined (user));
		}

		[Test]
		public void GetIsJoinedNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetIsJoined ((IConnection)null));
			Assert.Throws<ArgumentNullException> (() => manager.GetIsJoined ((UserInfo)null));
		}

		[Test]
		public void GetIsJoinedConnection()
		{
			manager.Connect (client);

			Assert.IsFalse (manager.GetIsJoined (client));

			manager.Join (client, user);

			Assert.IsTrue (manager.GetIsJoined (client));
		}

		[Test]
		public void GetIsJoinedConnectionNotFound()
		{
			Assert.IsFalse (manager.GetIsJoined (client));
		}

		[Test]
		public void GetIsJoinedUser()
		{
			manager.Connect (client);

			Assert.IsFalse (manager.GetIsJoined (user));

			manager.Join (client, user);

			Assert.IsTrue (manager.GetIsJoined (user));
		}

		[Test]
		public void GetIsJoinedUserNotFound()
		{
			Assert.IsFalse (manager.GetIsJoined (user));
		}

		[Test]
		public void MoveNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Move (null, new ChannelInfo()));
			Assert.Throws<ArgumentNullException> (() => manager.Move (user, null));
		}

		[Test]
		public void MoveSameChannel()
		{
			var c = new ChannelInfo(2);

			manager.Connect (client);
			manager.Join (client, user);

			manager.Move (user, c);

			user = manager.GetUser (client);

			Assert.AreEqual (c.ChannelId, user.CurrentChannelId);
		}

		[Test]
		public void MoveNotJoined()
		{
			var c = new ChannelInfo (3);
			
			manager.Connect (client);
			manager.Move (user, c);

			user = manager.GetUser (client);

			Assert.IsNull (user);
		}

		[Test]
		public void MoveChannel()
		{
			var c = new ChannelInfo(3);

			manager.Connect (client);
			manager.Join (client, user);

			manager.Move (user, c);

			user = manager.GetUser (client);

			Assert.AreEqual (c.ChannelId, user.CurrentChannelId);
		}

		[Test]
		public void ToggleMuteNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.ToggleMute (null));
		}

		[Test]
		public void ToggleMuteUnmuted()
		{
			user.IsMuted = false;
			Assert.IsFalse (user.IsMuted);

			manager.Connect (client);
			manager.Join (client, user);
			user = manager.GetUser (client);
			Assert.IsNotNull (user);
			Assert.IsFalse (user.IsMuted);

			Assert.IsTrue (manager.ToggleMute (user));

			user = manager.GetUser (client);
			Assert.IsNotNull (user);
			Assert.IsTrue (user.IsMuted);
		}

		[Test]
		public void ToggleMuteMuted()
		{
			user.IsMuted = true;
			Assert.IsTrue (user.IsMuted);

			manager.Connect (client);
			manager.Join (client, user);
			user = manager.GetUser (client);
			Assert.IsNotNull (user);
			Assert.IsTrue (user.IsMuted);

			Assert.IsFalse (manager.ToggleMute (user));

			user = manager.GetUser (client);
			Assert.IsNotNull (user);
			Assert.IsFalse (user.IsMuted);
		}

		[Test]
		public void ConnectNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Connect (null));
		}

		[Test]
		public void Connect()
		{
			Assert.IsFalse (manager.GetIsConnected (client));

			manager.Connect (client);

			Assert.IsTrue (manager.GetIsConnected (client));
		}

		[Test]
		public void GetIsConnectedNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetIsConnected (null));
		}

		[Test]
		public void GetIsConnected()
		{
			Assert.IsFalse (manager.GetIsConnected (client));

			manager.Connect (client);

			Assert.IsTrue (manager.GetIsConnected (client));
			Assert.IsFalse (manager.GetIsConnected (new MockClientConnection (new MockServerConnection())));

			manager.Disconnect (client);

			Assert.IsFalse (manager.GetIsConnected (client));
		}

		[Test]
		public void DisconnectNull()
		{
			Assert.Throws<ArgumentNullException> (() => new ServerUserManager().Disconnect ((IConnection)null));
			Assert.Throws<ArgumentNullException> (() => new ServerUserManager().Disconnect ((Func<IConnection, bool>)null));
		}

		[Test]
		public void Disconnect()
		{
			manager.Connect (client);
			Assert.IsTrue (manager.GetIsConnected (client));

			manager.Disconnect (client);
			Assert.IsFalse (manager.GetIsConnected (client));
			Assert.IsFalse (manager.GetIsLoggedIn (client));
		}

		[Test]
		public void DisconnectJoinedUser()
		{
			manager.Connect (client);
			manager.Join (client, user);

			manager.Disconnect (client);

			Assert.IsNull (manager.GetConnection (user));
			Assert.IsNull (manager.GetUser (client));
			Assert.IsFalse (manager.GetIsJoined (client));
			Assert.IsFalse (manager.GetIsJoined (user));
			Assert.IsFalse (manager.GetIsConnected (client));
		}

		[Test]
		public void DisconnectLoggedInUser()
		{
			manager.Connect (client);
			manager.Login (client, user);

			manager.Disconnect (client);

			Assert.IsNull (manager.GetConnection (user));
			Assert.IsNull (manager.GetUser (client));
			Assert.IsFalse (manager.GetIsJoined (user));
			Assert.IsFalse (manager.GetIsLoggedIn (user));
			Assert.IsFalse (manager.GetIsConnected (client));
		}

		[Test]
		public void DisconnectPredicate()
		{
			manager.Connect (client);
			manager.Join (client, user);

			manager.Disconnect (c => c == client);

			Assert.IsNull (manager.GetConnection (user));
			Assert.IsNull (manager.GetUser (client));
			Assert.IsFalse (manager.GetIsJoined (user));
			Assert.IsFalse (manager.GetIsConnected (client));
		}

		[Test]
		public void GetUserNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetUser (null));
		}

		[Test]
		public void GetUser()
		{
			manager.Connect (client);
			manager.Join (client, user);

			Assert.AreEqual (user, manager.GetUser (client));
		}

		[Test]
		public void GetUserNotFound()
		{
			Assert.IsNull (manager.GetUser (new MockClientConnection (server)));
		}

		[Test]
		public void GetConnectionNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetConnection (null));
		}

		[Test]
		public void GetConnection()
		{
			manager.Connect (client);
			manager.Join (client, user);

			Assert.AreEqual (client, manager.GetConnection (user));
		}

		[Test]
		public void GetConnectionNotFound()
		{
			Assert.IsNull (manager.GetConnection (new UserInfo ("Username", 1, 2, true)));
		}

		[Test]
		public void GetIsNicknameInUseNull ()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetIsNicknameInUse (null));
		}

		[Test]
		public void GetIsNicknameInUse()
		{
			manager.Connect (client);

			Assert.IsFalse (manager.GetIsNicknameInUse (user.Nickname));

			manager.Join (client, user);

			Assert.IsTrue (manager.GetIsNicknameInUse (user.Nickname));
			Assert.IsTrue (manager.GetIsNicknameInUse (user.Nickname + " "));
			Assert.IsTrue (manager.GetIsNicknameInUse (user.Nickname.ToUpper()));
			Assert.IsFalse (manager.GetIsNicknameInUse ("asdf"));
		}

		[Test]
		public void GetIsNicknameInUseNoNickname()
		{
			user = new UserInfo ("Username", 1, 2, true);

			manager.Connect (client);
			manager.Login (client, user);

			Assert.IsFalse (manager.GetIsNicknameInUse (user.Username));
		}
	}
}