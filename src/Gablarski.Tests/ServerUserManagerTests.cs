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
using Tempest;
using Tempest.Tests;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerUserManagerTests
	{
		private MockConnectionProvider provider;
		private ServerUserManager manager;
		private MockClientConnection client;
		private MockServerConnection server;
		private IUserInfo user;

		[SetUp]
		public void Setup()
		{
			user = new UserInfo ("Nickname", "Phonetic", "Username", 1, 2, true);
			manager = new ServerUserManager();

			provider = new MockConnectionProvider (GablarskiProtocol.Instance);
			provider.Start (MessageTypes.All);

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			server = cs.Item2;
			client = cs.Item1;
		}

		[TearDown]
		public void TearDown()
		{
			user = null;
			manager = null;
			server = null;
			server = null;
		}

		[Test]
		public void LoginNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Login (null, user));
			Assert.Throws<ArgumentNullException> (() => manager.Login (server, null));
		}

		[Test]
		public void Login()
		{
			manager.Connect (server);

			Assert.IsFalse (manager.GetIsLoggedIn (server));

			manager.Login (server, user);

			Assert.IsTrue (manager.GetIsLoggedIn (server));
		}

		[Test]
		public void LoginIgnoreBeforeConnect()
		{
			manager.Login (server, user);
			Assert.IsFalse (manager.GetIsLoggedIn (server));
			Assert.IsFalse (manager.GetIsLoggedIn (user));
		}

		[Test]
		public void LoginAlreadyLoggedIn()
		{
			manager.Connect (server);
			manager.Login (server, user);
			Assert.IsTrue (manager.GetIsLoggedIn (server));

			var c = provider.GetServerConnection();
			manager.Connect (c);
			manager.Login (c, user);

			Assert.IsFalse (server.IsConnected);
			Assert.IsFalse (manager.GetIsLoggedIn (server));
			Assert.IsTrue (manager.GetIsLoggedIn (user));
			Assert.IsTrue (manager.GetIsLoggedIn (c));
		}

		[Test]
		public void GetIsLoggedInNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetIsLoggedIn ((IServerConnection)null));
			Assert.Throws<ArgumentNullException> (() => manager.GetIsLoggedIn ((UserInfo)null));
		}

		[Test]
		public void GetIsLoggedInConnection()
		{
			manager.Connect (server);

			Assert.IsFalse (manager.GetIsLoggedIn (server));

			manager.Login (server, user);

			Assert.IsTrue (manager.GetIsLoggedIn (server));
		}

		[Test]
		public void GetIsLoggedInConnectionNotFound()
		{
			Assert.IsFalse (manager.GetIsLoggedIn (server));
		}

		[Test]
		public void GetIsLoggedInUser()
		{
			manager.Connect (server);

			Assert.IsFalse (manager.GetIsLoggedIn (server));

			manager.Login (server, user);

			Assert.IsTrue (manager.GetIsLoggedIn (server));
		}

		[Test]
		public void GetIsLoggedInUserNotFound()
		{
			Assert.IsFalse (manager.GetIsLoggedIn (server));
		}

		[Test]
		public void JoinNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Join (server, null));
			Assert.Throws<ArgumentNullException> (() => manager.Join (null, user));
		}

		[Test]
		public void JoinIgnoreNotConnected()
		{
			manager.Join (server, user);
			Assert.IsFalse (manager.GetIsJoined (server));
			Assert.IsFalse (manager.GetIsJoined (user));
		}

		[Test]
		public void Join()
		{
			manager.Connect (server);
			manager.Join (server, user);

			Assert.IsTrue (manager.GetIsJoined (server));
			Assert.IsTrue (manager.GetIsJoined (user));

			IUserInfo joinedUser = manager.GetUser (server);
			Assert.AreEqual (user.UserId, joinedUser.UserId);
			Assert.AreEqual (user.Username, joinedUser.Username);
			Assert.AreEqual (user.Nickname, joinedUser.Nickname);
			Assert.AreEqual (user.Phonetic, joinedUser.Phonetic);
			Assert.AreEqual (user.Status, joinedUser.Status);
			Assert.AreEqual (user.IsMuted, joinedUser.IsMuted);
		}

		[Test]
		public void JoinLoggedIn()
		{
			manager.Connect (server);
			manager.Login (server, new UserInfo (user.Username, user.UserId, user.CurrentChannelId, false));
			manager.Join (server, user);

			Assert.IsTrue (manager.GetIsJoined (server));
			Assert.IsTrue (manager.GetIsJoined (user));

			IUserInfo joinedUser = manager.GetUser (server);
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
			manager.Connect (server);
			manager.Join (server, user);
			manager.Join (server, user);

			Assert.IsTrue (manager.GetIsJoined (server));
			Assert.IsTrue (manager.GetIsJoined (user));
		}

		[Test]
		public void GetIsJoinedNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetIsJoined ((IServerConnection)null));
			Assert.Throws<ArgumentNullException> (() => manager.GetIsJoined ((UserInfo)null));
		}

		[Test]
		public void GetIsJoinedConnection()
		{
			manager.Connect (server);

			Assert.IsFalse (manager.GetIsJoined (server));

			manager.Join (server, user);

			Assert.IsTrue (manager.GetIsJoined (server));
		}

		[Test]
		public void GetIsJoinedConnectionNotFound()
		{
			Assert.IsFalse (manager.GetIsJoined (server));
		}

		[Test]
		public void GetIsJoinedUser()
		{
			manager.Connect (server);

			Assert.IsFalse (manager.GetIsJoined (user));

			manager.Join (server, user);

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
			Assert.Throws<ArgumentNullException> (() => manager.Move (null, new ChannelInfo (1)));
			Assert.Throws<ArgumentNullException> (() => manager.Move (user, null));
		}

		[Test]
		public void MoveNotConnected()
		{
			Assert.DoesNotThrow (() =>
				manager.Move (user, new ChannelInfo (1)));

			Assert.IsNull (manager.GetUser (server));
		}

		[Test]
		public void MoveNotJoined()
		{
			manager.Connect (server);
			Assert.DoesNotThrow (() =>
				manager.Move (user, new ChannelInfo (1)));

			Assert.IsNull (manager.GetUser (server));
		}

		[Test]
		public void MoveSameChannel()
		{
			var c = new ChannelInfo (2);

			manager.Connect (server);
			manager.Join (server, user);

			manager.Move (user, c);

			user = manager.GetUser (server);

			Assert.AreEqual (c.ChannelId, user.CurrentChannelId);
		}

		[Test]
		public void MoveChannel()
		{
			var c = new ChannelInfo(3);

			manager.Connect (server);
			manager.Join (server, user);

			manager.Move (user, c);

			user = manager.GetUser (server);

			Assert.AreEqual (c.ChannelId, user.CurrentChannelId);
		}

		[Test]
		public void ToggleMuteNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.ToggleMute (null));
		}

		[Test]
		public void ToggleMuteNotConnected()
		{
			Assert.DoesNotThrow (() => 
				manager.ToggleMute (user));

			Assert.IsNull (manager.GetConnection (user));
		}

		[Test]
		public void ToggleMuteUnmuted()
		{
			user = new UserInfo (user, false);
			Assert.IsFalse (user.IsMuted);

			manager.Connect (server);
			manager.Join (server, user);
			user = manager.GetUser (server);
			Assert.IsNotNull (user);
			Assert.IsFalse (user.IsMuted);

			Assert.IsTrue (manager.ToggleMute (user));

			user = manager.GetUser (server);
			Assert.IsNotNull (user);
			Assert.IsTrue (user.IsMuted);
		}

		[Test]
		public void ToggleMuteMuted()
		{
			user = new UserInfo (user, true);
			Assert.IsTrue (user.IsMuted);

			manager.Connect (server);
			manager.Join (server, user);
			user = manager.GetUser (server);
			Assert.IsNotNull (user);
			Assert.IsTrue (user.IsMuted);

			Assert.IsFalse (manager.ToggleMute (user));

			user = manager.GetUser (server);
			Assert.IsNotNull (user);
			Assert.IsFalse (user.IsMuted);
		}

		[Test]
		public void SetStatusNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.SetStatus (null, UserStatus.Normal));
		}

		[Test]
		public void SetStatus()
		{
			manager.Connect (server);
			manager.Join (server, user);

			Assert.AreEqual (UserStatus.Normal, user.Status);
			user = manager.SetStatus (user, UserStatus.MutedSound);

			Assert.IsNotNull (user);
			Assert.AreEqual (UserStatus.MutedSound, user.Status);
		}

		[Test]
		public void SetStatusNotConnected()
		{
			Assert.IsNull (manager.SetStatus (user, UserStatus.Normal));
		}

		[Test]
		public void SetCommentNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.SetComment (null, "comment"));
			Assert.DoesNotThrow (() => manager.SetComment (user, null));
		}

		[Test]
		public void SetCommentNotConnected()
		{
			Assert.IsNull (manager.SetComment (user, "comment"));
		}

		[Test]
		public void SetComment()
		{
			manager.Connect (server);
			manager.Join (server, user);

			Assert.AreEqual (null, user.Comment);

			const string comment = "There are three monkeys in the barrel.";
			var u = manager.SetComment (user, comment);

			Assert.IsNotNull (u);
			Assert.AreEqual (comment, u.Comment);
		}

		[Test]
		public void ConnectNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Connect (null));
		}

		[Test]
		public void Connect()
		{
			Assert.IsFalse (manager.GetIsConnected (server));

			manager.Connect (server);

			Assert.IsTrue (manager.GetIsConnected (server));
		}

		[Test]
		public void GetIsConnectedNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetIsConnected ((IConnection)null));
			Assert.Throws<ArgumentNullException> (() => manager.GetIsConnected ((IUserInfo)null));
		}

		[Test]
		public void GetIsConnected()
		{
			Assert.IsFalse (manager.GetIsConnected (server));

			manager.Connect (server);

			Assert.IsTrue (manager.GetIsConnected (server));
			Assert.IsFalse (manager.GetIsConnected (provider.GetServerConnection()));

			manager.Disconnect (server);

			Assert.IsFalse (manager.GetIsConnected (server));
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
			manager.Connect (server);
			Assert.IsTrue (manager.GetIsConnected (server));

			manager.Disconnect (server);
			Assert.IsFalse (manager.GetIsConnected (server));
			Assert.IsFalse (manager.GetIsLoggedIn (server));
		}

		[Test]
		public void DisconnectJoinedUser()
		{
			manager.Connect (server);
			manager.Join (server, user);

			manager.Disconnect (server);

			Assert.IsNull (manager.GetConnection (user));
			Assert.IsNull (manager.GetUser (server));
			Assert.IsFalse (manager.GetIsJoined (server));
			Assert.IsFalse (manager.GetIsJoined (user));
			Assert.IsFalse (manager.GetIsConnected (server));
		}

		[Test]
		public void DisconnectLoggedInUser()
		{
			manager.Connect (server);
			manager.Login (server, user);

			manager.Disconnect (server);

			Assert.IsNull (manager.GetConnection (user));
			Assert.IsNull (manager.GetUser (server));
			Assert.IsFalse (manager.GetIsJoined (user));
			Assert.IsFalse (manager.GetIsLoggedIn (user));
			Assert.IsFalse (manager.GetIsConnected (server));
		}

		[Test]
		public void DisconnectPredicate()
		{
			manager.Connect (server);
			manager.Join (server, user);

			manager.Disconnect (c => c == server);

			Assert.IsNull (manager.GetConnection (user));
			Assert.IsNull (manager.GetUser (server));
			Assert.IsFalse (manager.GetIsJoined (user));
			Assert.IsFalse (manager.GetIsConnected (server));
		}

		[Test]
		public void GetUserNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetUser (null));
		}

		[Test]
		public void GetUser()
		{
			manager.Connect (server);
			manager.Join (server, user);

			Assert.AreEqual (user, manager.GetUser (server));
		}

		[Test]
		public void GetUserNotFound()
		{
			Assert.IsNull (manager.GetUser (server));
		}

		[Test]
		public void GetConnectionNull()
		{
			Assert.Throws<ArgumentNullException> (() => manager.GetConnection (null));
		}

		[Test]
		public void GetConnection()
		{
			manager.Connect (server);
			manager.Join (server, user);

			Assert.AreEqual (server, manager.GetConnection (user));
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
			manager.Connect (server);

			Assert.IsFalse (manager.GetIsNicknameInUse (user.Nickname));

			manager.Join (server, user);

			Assert.IsTrue (manager.GetIsNicknameInUse (user.Nickname));
			Assert.IsTrue (manager.GetIsNicknameInUse (user.Nickname + " "));
			Assert.IsTrue (manager.GetIsNicknameInUse (user.Nickname.ToUpper()));
			Assert.IsFalse (manager.GetIsNicknameInUse ("asdf"));
		}

		[Test]
		public void GetIsNicknameInUseNoNickname()
		{
			user = new UserInfo ("Username", 1, 2, true);

			manager.Connect (server);
			manager.Login (server, user);

			Assert.IsFalse (manager.GetIsNicknameInUse (user.Username));
		}
	}
}