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
using System.Threading.Tasks;
using Gablarski.Messages;
using Gablarski.Server;
using Gablarski.Tests.Mocks;
using NUnit.Framework;
using Tempest;
using Tempest.Tests;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerUserHandlerTests
	{
		private ServerUserHandler handler;
		
		private MockConnectionProvider provider;

		private IServerConnection sobserver;
		private ConnectionBuffer observer;

		private ConnectionBuffer server;
		private ConnectionBuffer client;

		private MockServerContext context;
		private MockPermissionsProvider permissions;
		private MockUserProvider users;

		[SetUp]
		public void Setup()
		{
			permissions = new MockPermissionsProvider();
			users = new MockUserProvider();

			provider = new MockConnectionProvider (GablarskiProtocol.Instance);

			context = new MockServerContext (provider)
			{
				Settings = new ServerSettings(),
				UserProvider = users,
				PermissionsProvider = permissions,
				ChannelsProvider = new LobbyChannelProvider(),
			};

			context.Sources = new ServerSourceHandler (context, new ServerSourceManager (context));
			context.Channels = new ServerChannelHandler (context);
			context.UserManager = new ServerUserManager();
			context.Users = handler = new ServerUserHandler (context, context.UserManager);

			context.Start();

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			server = new ConnectionBuffer (cs.Item2);
			client = new ConnectionBuffer (cs.Item1);

			var observers = provider.GetConnections (GablarskiProtocol.Instance);
			sobserver = observers.Item2;
			observer = new ConnectionBuffer (observers.Item1);
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
			AsyncAssert.Throws<ArgumentNullException> (() => handler.DisconnectAsync ((IConnection)null));
			AsyncAssert.Throws<ArgumentNullException> (() => handler.DisconnectAsync ((Func<IConnection, bool>)null));
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

			client.AssertNoMessage();
		}

		[Test]
		public void MoveToSameChannel()
		{
			Connect();
			JoinAsGuest();

			IUserInfo user = handler.First();

			handler.Move (user, context.Channels[user.CurrentChannelId]);

			client.AssertNoMessage();
		}

		[Test]
		public void Move()
		{
			IChannelInfo altChannel = new ChannelInfo { Name = "Channel 2" };
			context.ChannelsProvider.SaveChannel (altChannel);
			client.DequeueAndAssertMessage<ChannelListMessage>();
			observer.DequeueAndAssertMessage<ChannelListMessage>();

			altChannel = context.ChannelsProvider.GetChannels().Single (c => c.Name == "Channel 2");

		    Connect();
		    JoinAsGuest();

		    IUserInfo user = handler.First();

			handler.Move (user, altChannel);

		    var move = client.DequeueAndAssertMessage<UserChangedChannelMessage>();
			Assert.AreEqual (user.UserId, move.ChangeInfo.TargetUserId);
			Assert.AreEqual (altChannel.ChannelId, move.ChangeInfo.TargetChannelId);
			Assert.AreEqual (1, move.ChangeInfo.PreviousChannelId);
			Assert.AreEqual (0, move.ChangeInfo.RequestingUserId);

			move = observer.DequeueAndAssertMessage<UserChangedChannelMessage>();
			Assert.AreEqual (user.UserId, move.ChangeInfo.TargetUserId);
			Assert.AreEqual (altChannel.ChannelId, move.ChangeInfo.TargetChannelId);
			Assert.AreEqual (1, move.ChangeInfo.PreviousChannelId);
			Assert.AreEqual (0, move.ChangeInfo.RequestingUserId);
		}

		[Test]
		public void SendAsyncNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.SendAsync (new ConnectMessage(), (Func<IConnection, bool>)null));
			Assert.Throws<ArgumentNullException> (() => handler.SendAsync (null, c => false));
			Assert.Throws<ArgumentNullException> (() => handler.SendAsync (new ConnectMessage(), (Func<IConnection, IUserInfo, bool>)null));
			Assert.Throws<ArgumentNullException> (() => handler.SendAsync (null, (c, u) => false));
		}

		[Test]
		public void SendConnection()
		{
			Connect();

			var msg = new ConnectMessage { ProtocolVersion = 42 };
			handler.SendAsync (msg, cc => cc == server);

			client.DequeueAndAssertMessage<ConnectMessage>();
		}

		[Test]
		public void SendJoinedUser()
		{
			Connect();
			JoinAsGuest();

			var user = handler.Manager.First();

			var msg = new ConnectMessage { ProtocolVersion = 42 };
			handler.SendAsync (msg, (cc, u) => cc == server && u == user);

			client.DequeueAndAssertMessage<ConnectMessage>();
		}

		[Test]
		public void Connect()
		{
			handler.OnConnectMessage (new MessageEventArgs<ConnectMessage> (server, 
				new ConnectMessage { ProtocolVersion = GablarskiProtocol.Instance.Version }));

			Assert.IsTrue (handler.Manager.GetIsConnected (server));

			client.DequeueAndAssertMessage<ServerInfoMessage>();
			client.DequeueAndAssertMessage<ChannelListMessage>();
			client.DequeueAndAssertMessage<UserInfoListMessage>();
			client.DequeueAndAssertMessage<SourceListMessage>();
		}
		
		[Test]
		public void JoinNotConnectedLiterally()
		{
			server.DisconnectAsync().Wait();

			context.Settings.AllowGuestLogins = true;
			
			handler.OnJoinMessage (new MessageEventArgs<JoinMessage> (server,
				new JoinMessage ("nickname", null)));

			client.AssertNoMessage();
		}

		[Test]
		public void JoinNotConnectedFormally()
		{
			context.Settings.AllowGuestLogins = true;
			
			handler.OnJoinMessage (new MessageEventArgs<JoinMessage> (server,
				new JoinMessage ("nickname", null)));

			var result = client.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.FailedNotConnected, result.Result);
		}

		[Test]
		public void JoinAsGuest()
		{
			JoinAsGuest (server, client, true, true, null, "Nickname", null);
		}
		
		[Test]
		public void JoinAsGuestWithServerPassword()
		{
			JoinAsGuest (server, client, true, true, "pass", "Nickname", "pass");
		}
		
		[Test]
		public void JoinAsGuestWhenNotAllowed()
		{
			JoinAsGuest (server, client, false, false, null, "Nickname", null);
		}
		
		[Test]
		public void JoinAsGuestWhenNotAllowedWithPassword()
		{
			JoinAsGuest (server, client, false, false, "pass", "Nickname", "pass");
		}
		
		[Test]
		public void JoinAsGuestWhenNotAllowedWithWrongPassword()
		{
			JoinAsGuest (server, client, false, false, "pass", "Nickname", "wrong");
		}
		
		[Test]
		public void JoinAsGuestWithNoPassword()
		{
			JoinAsGuest (server, client, false, true, "pass", "Nickname", null);
		}
		
		[Test]
		public void JoinAsGuestWithWrongPassword()
		{
			JoinAsGuest (server, client, false, true, "pass", "Nickname", "wrong");
		}

		[Test]
		public void JoinAsGuestBannedUsername()
		{
			users.AddBan (new BanInfo (null, "monkeys", TimeSpan.Zero));

			JoinAsGuest (server, client, false, true, null, "monkeys", null);
		}

		[Test]
		public void JoinAsGuestExpiredUsernameBan()
		{
			users.AddBan (new BanInfo (null, "monkeys", TimeSpan.FromSeconds (1)) { Created = new DateTime (2000, 1, 1) });

			JoinAsGuest (server, client, true, true, null, "monkeys", null);
		}

		public IUserInfo JoinAsGuest (ConnectionBuffer sc, ConnectionBuffer cc, string nickname)
		{
			return JoinAsGuest (sc, cc, true, true, null, nickname, null);
		}

		public IUserInfo JoinAsGuest (ConnectionBuffer sc, ConnectionBuffer cc, bool shouldWork, bool allowGuests, string serverPassword, string nickname, string userServerpassword)
		{
			return JoinAsGuest (sc, cc, true, shouldWork, allowGuests, serverPassword, nickname, userServerpassword);
		}

		public IUserInfo JoinAsGuest (ConnectionBuffer sc, ConnectionBuffer cc, bool connect, bool shouldWork, bool allowGuests, string serverPassword, string nickname, string userServerpassword)
		{
			context.Settings.AllowGuestLogins = allowGuests;
			context.Settings.ServerPassword = serverPassword;

			if (connect)
			{
				handler.Manager.Connect (sobserver);
				handler.Manager.Connect (sc);
			}

			handler.OnJoinMessage (new MessageEventArgs<JoinMessage> (sc,
				new JoinMessage (nickname, userServerpassword)));
			
			if (shouldWork)
			{
				Assert.IsTrue (handler.Manager.GetIsJoined (sc), "User is not joined");

				var msg = cc.DequeueAndAssertMessage<JoinResultMessage>();
				Assert.AreEqual (nickname, msg.UserInfo.Nickname);

				cc.DequeueAndAssertMessage<PermissionsMessage>();
				cc.DequeueAndAssertMessage<UserJoinedMessage>();

				observer.DequeueAndAssertMessage<UserJoinedMessage>();
				
				return msg.UserInfo;
			}
			else
			{
				Assert.IsFalse (handler.Manager.GetIsJoined (sc), "User joined");
				observer.AssertNoMessage();
				
				return null;
			}
		}

		[Test]
		public void JoinAsGuestAlreadyJoined()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);

			JoinAsGuest (server, client, true, true, null, "Nickname", null);
			JoinAsGuest (new ConnectionBuffer (cs.Item2), new ConnectionBuffer (cs.Item1),  false, false, true, null, "Nickname", null);
		}

		[Test]
		public void SetCommentNotConnected()
		{
			server.DisconnectAsync().Wait();

			handler.OnSetCommentMessage (new MessageEventArgs<SetCommentMessage> (server,
				new SetCommentMessage ("comment")));

			server.AssertNoMessage();
		}

		[Test]
		public void SetCommentSameComment()
		{
			JoinAsGuest (server, client, "Nickname");

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			JoinAsGuest (s, c, "Nickname2");

			client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnSetCommentMessage (new MessageEventArgs<SetCommentMessage> (server,
				new SetCommentMessage ("comment")));

			var update = c.DequeueAndAssertMessage<UserUpdatedMessage>();
			Assert.AreEqual ("comment", update.User.Comment);

			handler.OnSetCommentMessage (new MessageEventArgs<SetCommentMessage> (server,
				new SetCommentMessage ("comment")));

			c.AssertNoMessage();
		}

		[Test]
		public void SetComment()
		{
			JoinAsGuest (server, client, "Nickname");

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			JoinAsGuest (s, c, "Nickname2");

			client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnSetCommentMessage (new MessageEventArgs<SetCommentMessage> (server,
				new SetCommentMessage ("comment")));

			var update = c.DequeueAndAssertMessage<UserUpdatedMessage>();
			Assert.AreEqual ("comment", update.User.Comment);
		}

		[Test]
		public void SetStatusNotConnected()
		{
			server.DisconnectAsync().Wait();

			handler.OnSetStatusMessage (new MessageEventArgs<SetStatusMessage> (server,
				new SetStatusMessage (UserStatus.MutedMicrophone)));

			server.AssertNoMessage();
		}

		[Test]
		public void SetStatusSameStatus()
		{
			JoinAsGuest (server, client, "Nickname");
			
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			JoinAsGuest (s, c, "Nickname2");

			client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnSetStatusMessage (new MessageEventArgs<SetStatusMessage> (server,
				new SetStatusMessage (UserStatus.MutedMicrophone)));

			var update = c.DequeueAndAssertMessage<UserUpdatedMessage>();
			Assert.AreEqual (UserStatus.MutedMicrophone, update.User.Status);

			handler.OnSetStatusMessage (new MessageEventArgs<SetStatusMessage> (server,
				new SetStatusMessage (UserStatus.MutedMicrophone)));

			c.AssertNoMessage();
		}

		[Test]
		public void SetStatus ()
		{
			JoinAsGuest (server, client, "Nickname");
			
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			JoinAsGuest (s, c, "Nickname2");

			client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnSetStatusMessage (new MessageEventArgs<SetStatusMessage> (server,
				new SetStatusMessage (UserStatus.MutedMicrophone)));

			var update = c.DequeueAndAssertMessage<UserUpdatedMessage>();
			Assert.AreEqual (UserStatus.MutedMicrophone, update.User.Status);
		}

		[Test]
		public void RequestOnlineUserList()
		{
			permissions.EnablePermissions (0, PermissionName.RequestChannelList);

			var u1 = JoinAsGuest (server, client, "Nickname");

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u2 = JoinAsGuest (s, c, "Nickname2");

			client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnRequestUserListMessage (new MessageEventArgs<RequestUserListMessage> (server,
				new RequestUserListMessage (UserListMode.Current)));

			var list = client.DequeueAndAssertMessage<UserInfoListMessage>().Users.ToList();
			Assert.AreEqual (2, list.Count);
			Assert.IsTrue (list.Any (u => u.Nickname == "Nickname"), "User was not in returned list.");
			Assert.IsTrue (list.Any (u => u.Nickname == "Nickname2"), "User was not in returned list.");
		}

		[Test]
		public void RequestOnlineUserListWithoutPermission()
		{
			var u1 = JoinAsGuest (server, client, "Nickname");

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u2 = JoinAsGuest (s, c, "Nickname2");

			client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnRequestUserListMessage (new MessageEventArgs<RequestUserListMessage> (server,
				new RequestUserListMessage (UserListMode.Current)));

			Assert.AreEqual (GablarskiMessageType.RequestUserList,
				client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void RequestAllUserList()
		{
			permissions.EnablePermissions (0, PermissionName.RequestFullUserList);

			var u1 = JoinAsGuest (server, client, "Nickname");

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u2 = JoinAsGuest (s, c, "Nickname2");

			client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnRequestUserListMessage (new MessageEventArgs<RequestUserListMessage> (server,
				new RequestUserListMessage (UserListMode.All)));

			var list = client.DequeueAndAssertMessage<UserListMessage>().Users.ToList();
			Assert.AreEqual (0, list.Count);
		}

		[Test]
		public void RequestAllUserListWithoutPermission()
		{
			var u1 = JoinAsGuest (server, client, "Nickname");

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u2 = JoinAsGuest (s, c, "Nickname2");

			client.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnRequestUserListMessage (new MessageEventArgs<RequestUserListMessage> (server,
				new RequestUserListMessage (UserListMode.All)));

			Assert.AreEqual (GablarskiMessageType.RequestUserList,
				client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void SetPermissionsNotConnected()
		{
			permissions.UpdatedSupported = true;

			var u = UserInfoTests.GetTestUser();
			permissions.EnablePermissions (u.UserId, PermissionName.ModifyPermissions);

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = new ConnectionBuffer (cs.Item1);
			cs.Item2.DisconnectAsync().Wait();

			handler.OnSetPermissionsMessage (new MessageEventArgs<SetPermissionsMessage> (c,
				new SetPermissionsMessage (u, new Permission[0])));

			c.AssertNoMessage();
		}

		[Test]
		public void SetPermissionsNotAllowed()
		{
			permissions.UpdatedSupported = true;
			
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			handler.OnSetPermissionsMessage (new MessageEventArgs<SetPermissionsMessage> (s,
				new SetPermissionsMessage (u, new Permission[0])));

			Assert.AreEqual (GablarskiMessageType.SetPermissions, c.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void SetPermissionsNoPermissions()
		{
			permissions.UpdatedSupported = true;

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");
			permissions.EnablePermissions (u.UserId, PermissionName.ModifyPermissions);

			handler.OnSetPermissionsMessage (new MessageEventArgs<SetPermissionsMessage> (s,
				new SetPermissionsMessage (u, new Permission[0])));

			c.AssertNoMessage();
		}

		[Test]
		public void SetPermissionsUnsupported()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");
			permissions.EnablePermissions (u.UserId, PermissionName.ModifyPermissions);

			handler.OnSetPermissionsMessage (new MessageEventArgs<SetPermissionsMessage> (s,
				new SetPermissionsMessage (u, new []
				{
					new Permission (PermissionName.SendAudio, true), 
					new Permission (PermissionName.SendAudioToMultipleTargets, false), 
					new Permission (PermissionName.KickPlayerFromChannel, true) { ChannelId = 1 }, 
				})));

			c.AssertNoMessage();
		}

		[Test]
		public void SetPermissionsSelf()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			permissions.UpdatedSupported = true;
			permissions.EnablePermissions (u.UserId, PermissionName.ModifyPermissions);

			handler.OnSetPermissionsMessage (new MessageEventArgs<SetPermissionsMessage> (s,
				new SetPermissionsMessage (u, new []
				{
					new Permission (PermissionName.SendAudio, true), 
					new Permission (PermissionName.SendAudioToMultipleTargets, false), 
					new Permission (PermissionName.KickPlayerFromChannel, true) { ChannelId = 1 }, 
				})));

			var msg = c.DequeueAndAssertMessage<PermissionsMessage>();
			Assert.AreEqual (u.UserId, msg.OwnerId);

			var perms = msg.Permissions.ToList();
			perms.Single (p => p.ChannelId == 0 && p.Name == PermissionName.SendAudio && p.IsAllowed);
			perms.Single (p => p.ChannelId == 0 && p.Name == PermissionName.SendAudioToMultipleTargets && !p.IsAllowed);
			perms.Single (p => p.ChannelId == 1 && p.Name == PermissionName.KickPlayerFromChannel && p.IsAllowed);
		}

		[Test]
		public void SetPermissionsOtherConnected()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s1 = new ConnectionBuffer (cs.Item2);
			var c1 = new ConnectionBuffer (cs.Item1);

			var u1 = JoinAsGuest (s1, c1, "nick");

			permissions.UpdatedSupported = true;
			permissions.EnablePermissions (u1.UserId, PermissionName.ModifyPermissions);

			cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s2 = new ConnectionBuffer (cs.Item2);
			var c2 = new ConnectionBuffer (cs.Item1);

			var u2 = JoinAsGuest (s2, c2, "nick2");

			handler.OnSetPermissionsMessage (new MessageEventArgs<SetPermissionsMessage> (s1,
				new SetPermissionsMessage (u2, new []
				{
					new Permission (PermissionName.SendAudio, true), 
					new Permission (PermissionName.SendAudioToMultipleTargets, false), 
					new Permission (PermissionName.KickPlayerFromChannel, true) { ChannelId = 1 }, 
				})));

			var msg = c2.DequeueAndAssertMessage<PermissionsMessage>();
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

		[Test]
		public async Task RegisterNotConnected()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			await s.DisconnectAsync();

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (s, 
				new RegisterMessage ("username", "password")));

			c.AssertNoMessage();
		}

		[Test]
		public void RegisterNotJoinedUnsupported()
		{
			Connect();

			users.UpdateSupported = false;
			users.RegistrationMode = UserRegistrationMode.None;

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (server,
				new RegisterMessage ("username", "password")));

			var msg = client.DequeueAndAssertMessage<RegisterResultMessage>();
			Assert.AreEqual (RegisterResult.FailedUnsupported, msg.Result);
		}

		[Test]
		public void RegisterNotJoinedPreApproved()
		{
			Connect();

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.PreApproved;

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (server,
				new RegisterMessage ("username", "password")));

			client.AssertNoMessage();
		}

		[Test]
		public void RegisterNotJoined()
		{
			Connect();

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.Normal;

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (server,
				new RegisterMessage ("username", "password")));

			var msg = client.DequeueAndAssertMessage<RegisterResultMessage>();
			Assert.AreEqual (RegisterResult.Success, msg.Result);
		}

		[Test]
		public void RegisterUnsupported()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = false;
			users.RegistrationMode = UserRegistrationMode.Normal;

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (s, 
				new RegisterMessage ("username", "password")));

			var msg = c.DequeueAndAssertMessage<RegisterResultMessage>();
			Assert.AreEqual (RegisterResult.FailedUnsupported, msg.Result);
		}

		[Test]
		public void RegisterNoRegistration()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.None;

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (s, 
				new RegisterMessage ("username", "password")));

			var msg = c.DequeueAndAssertMessage<RegisterResultMessage>();
			Assert.AreEqual (RegisterResult.FailedUnsupported, msg.Result);
		}

		[Test]
		public void RegisterWebpage()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.WebPage;

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (s, 
				new RegisterMessage ("username", "password")));

			var msg = c.DequeueAndAssertMessage<RegisterResultMessage>();
			Assert.AreEqual (RegisterResult.FailedUnsupported, msg.Result);
		}

		[Test]
		public void RegisterMessage()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.Message;

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (s, 
				new RegisterMessage ("username", "password")));

			var msg = c.DequeueAndAssertMessage<RegisterResultMessage>();
			Assert.AreEqual (RegisterResult.FailedUnsupported, msg.Result);
		}

		[Test]
		public void Register()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.Normal;

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (s,
				new RegisterMessage ("username", "password")));

			var msg = c.DequeueAndAssertMessage<RegisterResultMessage>();
			Assert.AreEqual (RegisterResult.Success, msg.Result);
		}

		[Test]
		public void RegisterPreapprovedNotApproved()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.PreApproved;

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (s,
				new RegisterMessage ("username", "password")));

			var msg = c.DequeueAndAssertMessage<RegisterResultMessage>();
			Assert.AreEqual (RegisterResult.FailedNotApproved, msg.Result);
		}

		[Test]
		public void RegisterPreapprovedApproved()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.PreApproved;

			handler.ApproveRegistration (u);

			handler.OnRegisterMessage (new MessageEventArgs<RegisterMessage> (s, 
				new RegisterMessage ("username", "password")));

			var msg = c.DequeueAndAssertMessage<RegisterResultMessage>();
			Assert.AreEqual (RegisterResult.Success, msg.Result);
		}

		[Test]
		public void PreApproveRegistrationUnsupported()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = false;
			users.RegistrationMode = UserRegistrationMode.None;
			
			Assert.Throws<NotSupportedException> (() => handler.ApproveRegistration (u));
		}

		[Test]
		public void PreApproveRegistrationNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.ApproveRegistration ((IUserInfo)null));
		}

		[Test]
		public void ApproveRegistrationUnsupported()
		{
			users.UpdateSupported = false;
			users.RegistrationMode = UserRegistrationMode.None;

			Assert.Throws<NotSupportedException> (() => handler.ApproveRegistration ("monkeys"));
		}

		[Test]
		public void ApproveRegistrationNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.ApproveRegistration ((string)null));
		}

		[Test]
		public void RegistrationApprovalMessageNotSupported()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = false;
			users.RegistrationMode = UserRegistrationMode.None;

			permissions.UpdatedSupported = true;
			permissions.EnablePermissions (u.UserId, PermissionName.ApproveRegistrations);

			handler.OnRegistrationApprovalMessage (new MessageEventArgs<RegistrationApprovalMessage> (s,
				new RegistrationApprovalMessage { Approved = true, UserId = 2 }));

			client.AssertNoMessage();
		}

		[Test]
		public void RegistrationApprovalMessageBadUsername()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.Approved;

			permissions.UpdatedSupported = true;
			permissions.EnablePermissions (u.UserId, PermissionName.ApproveRegistrations);

			handler.OnRegistrationApprovalMessage (new MessageEventArgs<RegistrationApprovalMessage> (s,
				new RegistrationApprovalMessage { Approved = true, Username = null }));

			client.AssertNoMessage();
		}

		[Test]
		public void RegistrationApprovalMessageUnknownUser()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			users.UpdateSupported = true;
			users.RegistrationMode = UserRegistrationMode.PreApproved;

			permissions.UpdatedSupported = true;
			permissions.EnablePermissions (u.UserId, PermissionName.ApproveRegistrations);

			handler.OnRegistrationApprovalMessage (new MessageEventArgs<RegistrationApprovalMessage> (s,
				new RegistrationApprovalMessage { Approved = true, UserId = 2 }));

			client.AssertNoMessage();
		}

		[Test]
		public void KickUserNotConnected()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var u = JoinAsGuest (s, c, "nick");

			handler.OnKickUserMessage (new MessageEventArgs<KickUserMessage> (server,
				new KickUserMessage (u, true)));

			c.AssertNoMessage();
		}

		[Test]
		public void KickUserChannelNotAllowed()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var admin = JoinAsGuest (s, c, "admin");

			var target = JoinAsGuest (server, client, "target");
			c.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnKickUserMessage (new MessageEventArgs<KickUserMessage> (s,
				new KickUserMessage (target, false)));

			Assert.AreEqual (GablarskiMessageType.KickUser, c.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);


			permissions.EnablePermissions (admin.UserId, PermissionName.KickPlayerFromServer);

			handler.OnKickUserMessage (new MessageEventArgs<KickUserMessage> (s,
				new KickUserMessage (target, false)));

			Assert.AreEqual (GablarskiMessageType.KickUser, c.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void KickUserServerNotAllowed()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var admin = JoinAsGuest (s, c, "admin");

			var target = JoinAsGuest (server, client, "target");
			c.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnKickUserMessage (new MessageEventArgs<KickUserMessage> (s,
				new KickUserMessage (target, true)));

			Assert.AreEqual (GablarskiMessageType.KickUser, c.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);


			permissions.EnablePermissions (admin.UserId, PermissionName.KickPlayerFromChannel);

			handler.OnKickUserMessage (new MessageEventArgs<KickUserMessage> (s,
				new KickUserMessage (target, true)));

			Assert.AreEqual (GablarskiMessageType.KickUser, c.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void KickUserFromChannel()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var admin = JoinAsGuest (s, c, "admin");
			permissions.EnablePermissions (admin.UserId, PermissionName.KickPlayerFromChannel);
			
			var target = JoinAsGuest (server, client, "target");
			c.DequeueAndAssertMessage<UserJoinedMessage>();
			
			IChannelInfo altChannel = new ChannelInfo { Name = "Channel 2" };
			context.ChannelsProvider.SaveChannel (altChannel);
			c.DequeueAndAssertMessage<ChannelListMessage>();
			client.DequeueAndAssertMessage<ChannelListMessage>();
			altChannel = context.ChannelsProvider.GetChannels().Single (ch => ch.Name == "Channel 2");
			handler.Move (target, altChannel);
			var moved = client.DequeueAndAssertMessage<UserChangedChannelMessage>();
			Assert.AreEqual (altChannel.ChannelId, moved.ChangeInfo.TargetChannelId);
			Assert.AreEqual (target.UserId, moved.ChangeInfo.TargetUserId);

			handler.OnKickUserMessage (new MessageEventArgs<KickUserMessage> (s,
				new KickUserMessage (target, false)));

			var kicked = client.DequeueAndAssertMessage<UserKickedMessage>();
			Assert.AreEqual (target.UserId, kicked.UserId);
			Assert.AreEqual (false, kicked.FromServer);

			moved = client.DequeueAndAssertMessage<UserChangedChannelMessage>();
			Assert.AreEqual (context.ChannelsProvider.DefaultChannel.ChannelId, moved.ChangeInfo.TargetChannelId);
			Assert.AreEqual (target.UserId, moved.ChangeInfo.TargetUserId);
		}

		[Test]
		public void KickUserFromServer()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			var admin = JoinAsGuest (s, c, "admin");
			permissions.EnablePermissions (admin.UserId, PermissionName.KickPlayerFromServer);
			
			var target = JoinAsGuest (server, client, "target");
			c.DequeueAndAssertMessage<UserJoinedMessage>();

			handler.OnKickUserMessage (new MessageEventArgs<KickUserMessage> (s,
				new KickUserMessage (target, true)));

			var kicked = client.DequeueAndAssertMessage<UserKickedMessage>();
			Assert.AreEqual (target.UserId, kicked.UserId);
			Assert.AreEqual (true, kicked.FromServer);

			kicked = c.DequeueAndAssertMessage<UserKickedMessage>();
			Assert.AreEqual (target.UserId, kicked.UserId);
			Assert.AreEqual (true, kicked.FromServer);

			Assert.AreEqual (target.UserId, c.DequeueAndAssertMessage<UserDisconnectedMessage>().UserId);
		}

		[Test]
		public void BanNotConnected()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var s = new ConnectionBuffer (cs.Item2);
			var c = new ConnectionBuffer (cs.Item1);

			handler.OnBanUserMessage (new MessageEventArgs<BanUserMessage> (s,
				new BanUserMessage { BanInfo = new BanInfo (null, "username", TimeSpan.Zero)}));

			c.AssertNoMessage();
		}

		[Test]
		public void BanUserNotAllowed()
		{
			var user = JoinAsGuest (server, client, "user");

			handler.OnBanUserMessage (new MessageEventArgs<BanUserMessage> (server,
				new BanUserMessage { BanInfo = new BanInfo (null, "username", TimeSpan.Zero)}));

			Assert.AreEqual (GablarskiMessageType.BanUser,
			                 client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void BanUser()
		{
			var admin = JoinAsGuest (server, client, "admin");
			permissions.EnablePermissions (admin.UserId, PermissionName.BanUser);

			var ban = new BanInfo (null, "username", TimeSpan.Zero);
			users.BansChanged += (sender, e) =>
			{
				Assert.IsTrue (users.GetBans().Contains (ban), "User provider did not contain the new ban.");
				Assert.Pass();
			};

			handler.OnBanUserMessage (new MessageEventArgs<BanUserMessage> (server,
				new BanUserMessage { Removing = false, BanInfo = ban }));

			Assert.Fail ("Ban never reached the user provider");
		}

		[Test]
		public void BanUserRemove()
		{
			var admin = JoinAsGuest (server, client, "admin");
			permissions.EnablePermissions (admin.UserId, PermissionName.BanUser);

			var ban = new BanInfo (null, "username", TimeSpan.Zero);
			users.AddBan (ban);
			Assert.IsTrue (users.GetBans().Contains (ban), "User provider did not contain the new ban.");

			users.BansChanged += (sender, e) =>
			{
				Assert.IsFalse (users.GetBans().Contains (ban), "User provider still contained the ban.");
				Assert.Pass();
			};

			handler.OnBanUserMessage (new MessageEventArgs<BanUserMessage> (server,
				new BanUserMessage { Removing = true, BanInfo = ban }));

			Assert.Fail ("Ban never reached the user provider");
		}
	}
}