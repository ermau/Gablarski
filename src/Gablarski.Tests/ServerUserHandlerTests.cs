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
		private MockServerConnection observer;
		private MockServerConnection server;
		private MockServerContext context;
		private MockPermissionsProvider permissions;
		private MockUserProvider users;

		[SetUp]
		public void Setup()
		{
			permissions = new MockPermissionsProvider();
			users = new MockUserProvider();

			context = new MockServerContext
			{
				Settings = new ServerSettings(),
				UserProvider = users,
				PermissionsProvider = permissions,
				ChannelsProvider = new LobbyChannelProvider(),
				EncryptionParameters = new Decryption().PublicParameters
			};

			context.Sources = new ServerSourceHandler (context, new ServerSourceManager (context));
			context.Channels = new ServerChannelHandler (context);
			context.UserManager = new ServerUserManager();
			context.Users = handler = new ServerUserHandler (context, context.UserManager);

			server = new MockServerConnection();
			observer = new MockServerConnection();
		}

		[TearDown]
		public void Teardown()
		{
			handler = null;
			server = null;
			observer = null;
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

		[Test]
		public void MoveToUnknownChannel()
		{
			Connect();
			JoinAsGuest();

			IUserInfo user = handler.First();

			handler.Move (user, new ChannelInfo());

			server.Client.AssertNoMessage();
		}

		[Test]
		public void MoveToSameChannel()
		{
			Connect();
			JoinAsGuest();

			IUserInfo user = handler.First();

			handler.Move (user, context.Channels[user.CurrentChannelId]);

			server.Client.AssertNoMessage();
		}

		[Test]
		public void Move()
		{
			var altChannel = new ChannelInfo { Name = "Channel 2" };
			context.ChannelsProvider.SaveChannel (altChannel);
			altChannel = context.ChannelsProvider.GetChannels().Single (c => c.Name == "Channel 2");

		    Connect();
		    JoinAsGuest();

		    IUserInfo user = handler.First();

			handler.Move (user, altChannel);

		    var move = server.Client.DequeueAndAssertMessage<UserChangedChannelMessage>();
			Assert.AreEqual (user.UserId, move.ChangeInfo.TargetUserId);
			Assert.AreEqual (altChannel.ChannelId, move.ChangeInfo.TargetChannelId);
			Assert.AreEqual (1, move.ChangeInfo.PreviousChannelId);
			Assert.AreEqual (0, move.ChangeInfo.RequestingUserId);
		}

		[Test]
		public void SendNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.Send (new ConnectMessage(), (Func<IConnection, bool>)null));
			Assert.Throws<ArgumentNullException> (() => handler.Send (null, c => false));
			Assert.Throws<ArgumentNullException> (() => handler.Send (new ConnectMessage(), (Func<IConnection, IUserInfo, bool>)null));
			Assert.Throws<ArgumentNullException> (() => handler.Send (null, (c, u) => false));
		}

		[Test]
		public void SendConnection()
		{
			Connect();

			var msg = new ConnectMessage { ProtocolVersion = 42 };
			handler.Send (msg, cc => cc == server);

			server.Client.DequeueAndAssertMessage<ConnectMessage>();
		}

		[Test]
		public void SendJoinedUser()
		{
			Connect();
			JoinAsGuest();

			var user = handler.Manager.First();

			var msg = new ConnectMessage { ProtocolVersion = 42 };
			handler.Send (msg, (cc, u) => cc == server && u == user);

			server.Client.DequeueAndAssertMessage<ConnectMessage>();
		}

		[Test]
		public void Connect()
		{
			handler.ConnectMessage (new MessageReceivedEventArgs (server, 
				new ConnectMessage { ProtocolVersion = GablarskiServer.ProtocolVersion }));

			Assert.IsTrue (handler.Manager.GetIsConnected (server));

			server.Client.DequeueAndAssertMessage<ServerInfoMessage>();
		}

		[Test]
		public void ConnectInvalidVersion()
		{
			handler.ConnectMessage (new MessageReceivedEventArgs (server, 
				new ConnectMessage { ProtocolVersion = 1 }));

			Assert.IsFalse (handler.Manager.GetIsConnected (server));

			var rejected = server.Client.DequeueAndAssertMessage<ConnectionRejectedMessage>();
			Assert.AreEqual (ConnectionRejectedReason.IncompatibleVersion, rejected.Reason);
		}
		
		[Test]
		public void JoinNotConnectedLiterally()
		{
			server.Disconnect();

			context.Settings.AllowGuestLogins = true;
			
			handler.JoinMessage (new MessageReceivedEventArgs (server,
				new JoinMessage ("nickname", null)));

			server.Client.AssertNoMessage();
		}

		[Test]
		public void JoinNotConnectedFormally()
		{
			context.Settings.AllowGuestLogins = true;
			
			handler.JoinMessage (new MessageReceivedEventArgs (server,
				new JoinMessage ("nickname", null)));

			var result = server.Client.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.FailedNotConnected, result.Result);
		}

		[Test]
		public void JoinAsGuest()
		{
			JoinAsGuest (server, true, true, null, "Nickname", null);
		}
		
		[Test]
		public void JoinAsGuestWithServerPassword()
		{
			JoinAsGuest (server, true, true, "pass", "Nickname", "pass");
		}
		
		[Test]
		public void JoinAsGuestWhenNotAllowed()
		{
			JoinAsGuest (server, false, false, null, "Nickname", null);
		}
		
		[Test]
		public void JoinAsGuestWhenNotAllowedWithPassword()
		{
			JoinAsGuest (server, false, false, "pass", "Nickname", "pass");
		}
		
		[Test]
		public void JoinAsGuestWhenNotAllowedWithWrongPassword()
		{
			JoinAsGuest (server, false, false, "pass", "Nickname", "wrong");
		}
		
		[Test]
		public void JoinAsGuestWithNoPassword()
		{
			JoinAsGuest (server, false, true, "pass", "Nickname", null);
		}
		
		[Test]
		public void JoinAsGuestWithWrongPassword()
		{
			JoinAsGuest (server, false, true, "pass", "Nickname", "wrong");
		}

		public IUserInfo JoinAsGuest (MockServerConnection connection, string nickname)
		{
			return JoinAsGuest (connection, true, true, null, nickname, null);
		}

		public IUserInfo JoinAsGuest (MockServerConnection connection, bool shouldWork, bool allowGuests, string serverPassword, string nickname, string userServerpassword)
		{
			return JoinAsGuest (connection, true, shouldWork, allowGuests, serverPassword, nickname, userServerpassword);
		}

		public IUserInfo JoinAsGuest (MockServerConnection connection, bool connect, bool shouldWork, bool allowGuests, string serverPassword, string nickname, string userServerpassword)
		{
			context.Settings.AllowGuestLogins = allowGuests;
			context.Settings.ServerPassword = serverPassword;

			if (connect)
			{
				handler.Manager.Connect (observer);
				handler.Manager.Connect (connection);
			}

			handler.JoinMessage (new MessageReceivedEventArgs (connection,
				new JoinMessage (nickname, userServerpassword)));
			
			if (shouldWork)
			{
				Assert.IsTrue (handler.Manager.GetIsJoined (connection), "User is not joined");

				var msg = connection.Client.DequeueAndAssertMessage<JoinResultMessage>();
				Assert.AreEqual (nickname, msg.UserInfo.Nickname);

				connection.Client.DequeueAndAssertMessage<PermissionsMessage>();
				connection.Client.DequeueAndAssertMessage<UserJoinedMessage>();
				connection.Client.DequeueAndAssertMessage<ChannelListMessage>();
				connection.Client.DequeueAndAssertMessage<UserInfoListMessage>();
				connection.Client.DequeueAndAssertMessage<SourceListMessage>();

				observer.Client.DequeueAndAssertMessage<UserJoinedMessage>();
				
				return msg.UserInfo;
			}
			else
			{
				Assert.IsFalse (handler.Manager.GetIsJoined (connection));
				observer.AssertNoMessage();
				
				return null;
			}
		}

		[Test]
		public void JoinAsGuestAlreadyJoined()
		{
			JoinAsGuest (server, true, true, null, "Nickname", null);
			JoinAsGuest (server, false, false, true, null, "Nickname", null);
		}

		[Test]
		public void SetCommentNotConnected()
		{
			server.Disconnect();

			handler.SetCommentMessage(new MessageReceivedEventArgs (server,
				new SetCommentMessage ("comment")));

			server.AssertNoMessage();
		}

		[Test]
		public void SetCommentSameComment()
		{
			JoinAsGuest (server, "Nickname");
			
			var c = new MockServerConnection();
			JoinAsGuest (c, "Nickname2");

			server.Client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.SetCommentMessage (new MessageReceivedEventArgs (server,
				new SetCommentMessage ("comment")));

			var update = c.Client.DequeueAndAssertMessage<UserUpdatedMessage>();
			Assert.AreEqual ("comment", update.User.Comment);

			handler.SetCommentMessage (new MessageReceivedEventArgs (server,
				new SetCommentMessage ("comment")));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void SetComment()
		{
			JoinAsGuest (server, "Nickname");

			var c = new MockServerConnection();
			JoinAsGuest (c, "Nickname2");

			server.Client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.SetCommentMessage (new MessageReceivedEventArgs (server,
				new SetCommentMessage ("comment")));

			var update = c.Client.DequeueAndAssertMessage<UserUpdatedMessage>();
			Assert.AreEqual ("comment", update.User.Comment);
		}

		[Test]
		public void SetStatusNotConnected()
		{
			server.Disconnect();

			handler.SetStatusMessage(new MessageReceivedEventArgs (server,
				new SetStatusMessage (UserStatus.MutedMicrophone)));

			server.AssertNoMessage();
		}

		[Test]
		public void SetStatusSameStatus()
		{
			JoinAsGuest (server, "Nickname");
			
			var c = new MockServerConnection();
			JoinAsGuest (c, "Nickname2");

			server.Client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.SetStatusMessage (new MessageReceivedEventArgs (server,
				new SetStatusMessage (UserStatus.MutedMicrophone)));

			var update = c.Client.DequeueAndAssertMessage<UserUpdatedMessage>();
			Assert.AreEqual (UserStatus.MutedMicrophone, update.User.Status);

			handler.SetStatusMessage (new MessageReceivedEventArgs (server,
				new SetStatusMessage (UserStatus.MutedMicrophone)));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void SetStatus ()
		{
			JoinAsGuest (server, "Nickname");
			
			var c = new MockServerConnection();
			JoinAsGuest (c, "Nickname2");

			server.Client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.SetStatusMessage (new MessageReceivedEventArgs (server,
				new SetStatusMessage (UserStatus.MutedMicrophone)));

			var update = c.Client.DequeueAndAssertMessage<UserUpdatedMessage>();
			Assert.AreEqual (UserStatus.MutedMicrophone, update.User.Status);
		}

		[Test]
		public void RequestOnlineUserList()
		{
			permissions.EnablePermissions (0, PermissionName.RequestChannelList);

			var u1 = JoinAsGuest (server, "Nickname");

			var c = new MockServerConnection();
			var u2 = JoinAsGuest (c, "Nickname2");

			server.Client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.RequestUserListMessage (new MessageReceivedEventArgs (server,
				new RequestUserListMessage (UserListMode.Current)));

			var list = server.Client.DequeueAndAssertMessage<UserInfoListMessage>().Users.ToList();
			Assert.AreEqual (2, list.Count);
			Assert.IsTrue (list.Any (u => u.Nickname == "Nickname"), "User was not in returned list.");
			Assert.IsTrue (list.Any (u => u.Nickname == "Nickname2"), "User was not in returned list.");
		}

		[Test]
		public void RequestOnlineUserListWithoutPermission()
		{
			var u1 = JoinAsGuest (server, "Nickname");

			var c = new MockServerConnection();
			var u2 = JoinAsGuest (c, "Nickname2");

			server.Client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.RequestUserListMessage (new MessageReceivedEventArgs (server,
				new RequestUserListMessage (UserListMode.Current)));

			Assert.AreEqual (ClientMessageType.RequestUserList,
				server.Client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void RequestAllUserList()
		{
			permissions.EnablePermissions (0, PermissionName.RequestFullUserList);

			var u1 = JoinAsGuest (server, "Nickname");

			var c = new MockServerConnection();
			var u2 = JoinAsGuest (c, "Nickname2");

			server.Client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.RequestUserListMessage (new MessageReceivedEventArgs (server,
				new RequestUserListMessage (UserListMode.All)));

			var list = server.Client.DequeueAndAssertMessage<UserListMessage>().Users.ToList();
			Assert.AreEqual (0, list.Count);
		}

		[Test]
		public void RequestAllUserListWithoutPermission()
		{
			var u1 = JoinAsGuest (server, "Nickname");

			var c = new MockServerConnection();
			var u2 = JoinAsGuest (c, "Nickname2");

			server.Client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.RequestUserListMessage (new MessageReceivedEventArgs (server,
				new RequestUserListMessage (UserListMode.All)));

			Assert.AreEqual (ClientMessageType.RequestUserList,
				server.Client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}
		
		[Test]
		public void RegisterNotSupported()
		{
			users.RegistrationMode = UserRegistrationMode.None;

			var c = new MockServerConnection();
			context.UserManager.Connect (c);

			handler.RegisterMessage (new MessageReceivedEventArgs (c,
				new RegisterMessage ("Username", "Password")));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void RegisterNotConnected()
		{
			users.RegistrationMode = UserRegistrationMode.Normal;

			var c = new MockServerConnection();
			c.Disconnect();

			handler.RegisterMessage (new MessageReceivedEventArgs (c,
				new RegisterMessage ("Username", "Password")));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void SetPermissionsNotConnected()
		{
			permissions.UpdatedSupported = true;

			var u = UserInfoTests.GetTestUser();
			permissions.EnablePermissions (u.UserId, PermissionName.ModifyPermissions);

			var c = new MockServerConnection();
			c.Disconnect();

			handler.SetPermissionsMessage (new MessageReceivedEventArgs (c,
				new SetPermissionsMessage (u, new Permission[0])));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void SetPermissionsNotAllowed()
		{
			permissions.UpdatedSupported = true;
			MockServerConnection c = new MockServerConnection();
			var u = JoinAsGuest (c, "nick");

			handler.SetPermissionsMessage (new MessageReceivedEventArgs (c,
				new SetPermissionsMessage (u, new Permission[0])));

			Assert.AreEqual (ClientMessageType.SetPermissions,
			                 c.Client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void SetPermissionsNoPermissions()
		{
			permissions.UpdatedSupported = true;

			MockServerConnection c = new MockServerConnection();
			var u = JoinAsGuest (c, "nick");
			permissions.EnablePermissions (u.UserId, PermissionName.ModifyPermissions);

			handler.SetPermissionsMessage (new MessageReceivedEventArgs (c,
				new SetPermissionsMessage (u, new Permission[0])));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void SetPermissionsUnsupported()
		{
			MockServerConnection c = new MockServerConnection();
			var u = JoinAsGuest (c, "nick");
			permissions.EnablePermissions (u.UserId, PermissionName.ModifyPermissions);

			handler.SetPermissionsMessage (new MessageReceivedEventArgs (c,
				new SetPermissionsMessage (u, new []
				{
					new Permission (PermissionName.SendAudio, true), 
					new Permission (PermissionName.SendAudioToMultipleTargets, false), 
					new Permission (PermissionName.KickPlayerFromChannel, true) { ChannelId = 1 }, 
				})));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void SetPermissionsSelf()
		{
			MockServerConnection c = new MockServerConnection();
			var u = JoinAsGuest (c, "nick");

			permissions.UpdatedSupported = true;
			permissions.EnablePermissions (u.UserId, PermissionName.ModifyPermissions);

			handler.SetPermissionsMessage (new MessageReceivedEventArgs (c,
				new SetPermissionsMessage (u, new []
				{
					new Permission (PermissionName.SendAudio, true), 
					new Permission (PermissionName.SendAudioToMultipleTargets, false), 
					new Permission (PermissionName.KickPlayerFromChannel, true) { ChannelId = 1 }, 
				})));

			var msg = c.Client.DequeueAndAssertMessage<PermissionsMessage>();
			Assert.AreEqual (u.UserId, msg.OwnerId);

			var perms = msg.Permissions.ToList();
			perms.Single (p => p.ChannelId == 0 && p.Name == PermissionName.SendAudio && p.IsAllowed);
			perms.Single (p => p.ChannelId == 0 && p.Name == PermissionName.SendAudioToMultipleTargets && !p.IsAllowed);
			perms.Single (p => p.ChannelId == 1 && p.Name == PermissionName.KickPlayerFromChannel && p.IsAllowed);
		}

		[Test]
		public void SetPermissionsOtherConnected()
		{
			MockServerConnection c1 = new MockServerConnection();
			var u1 = JoinAsGuest (c1, "nick");

			permissions.UpdatedSupported = true;
			permissions.EnablePermissions (u1.UserId, PermissionName.ModifyPermissions);

			MockServerConnection c2 = new MockServerConnection();
			var u2 = JoinAsGuest (c2, "nick2");

			handler.SetPermissionsMessage (new MessageReceivedEventArgs (c1,
				new SetPermissionsMessage (u2, new []
				{
					new Permission (PermissionName.SendAudio, true), 
					new Permission (PermissionName.SendAudioToMultipleTargets, false), 
					new Permission (PermissionName.KickPlayerFromChannel, true) { ChannelId = 1 }, 
				})));

			var msg = c2.Client.DequeueAndAssertMessage<PermissionsMessage>();
			Assert.AreEqual (u2.UserId, msg.OwnerId);

			var perms = msg.Permissions.ToList();
			perms.Single (p => p.ChannelId == 0 && p.Name == PermissionName.SendAudio && p.IsAllowed);
			perms.Single (p => p.ChannelId == 0 && p.Name == PermissionName.SendAudioToMultipleTargets && !p.IsAllowed);
			perms.Single (p => p.ChannelId == 1 && p.Name == PermissionName.KickPlayerFromChannel && p.IsAllowed);
		}

		//[Test]
		//public void SetPermissionsOtherNotConnected()
		//{
			
		//}
	}
}