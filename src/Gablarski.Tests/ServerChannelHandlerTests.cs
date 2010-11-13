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
	public class ServerChannelHandlerTests
	{
		private IChannelProvider channels;
		private IServerContext context;
		private IServerUserManager manager;
		private ServerChannelHandler handler;
		private MockPermissionsProvider permissions;
		private MockServerConnection server;
		private UserInfo user;

		[SetUp]
		public void Setup()
		{
			manager = new ServerUserManager();
			permissions = new MockPermissionsProvider();
			channels = new LobbyChannelProvider();

			MockServerContext mcontext;
			context = mcontext = new MockServerContext
			{
				ChannelsProvider = channels,
				PermissionsProvider = permissions,
				UserManager = new ServerUserManager (),
				Settings = new ServerSettings { Name = "Test Server" }
			};

			mcontext.Users = new ServerUserHandler (context, manager);
			mcontext.Channels = handler = new ServerChannelHandler (context);

			user = UserInfoTests.GetTestUser (1, 1, false);
			server = new MockServerConnection();
			manager.Connect (server);
			manager.Join (server, user);
		}

		[Test]
		public void CtorNull()
		{
			Assert.Throws<ArgumentNullException> (() => new ServerChannelHandler (null));
		}

		[Test]
		public void RequestChannelListMessageNotConnected()
		{
			var c = new MockServerConnection();
			c.Disconnect();
			
			handler.RequestChanneListMessage (new MessageReceivedEventArgs (c,
				new RequestChannelListMessage()));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void RequestChanneListMessageNotJoined()
		{
			permissions.EnablePermissions (0, PermissionName.RequestChannelList);

			var c = new MockServerConnection();
			manager.Connect (c);

			handler.RequestChanneListMessage (new MessageReceivedEventArgs (c,
				new RequestChannelListMessage()));

			var msg = c.Client.DequeueAndAssertMessage<ChannelListMessage>();
			Assert.AreEqual (GenericResult.Success, msg.Result);
			ChannelInfoTests.AssertChanelsAreEqual (channels.GetChannels().Single(), msg.Channels.Single());
		}

		[Test]
		public void DeleteChannelUserIsIn()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.ChangeChannel);

			Assert.AreEqual (ChannelEditResult.Success, channels.SaveChannel (new ChannelInfo { Name = "Channel 2" }));
			server.Client.DequeueAndAssertMessage<ChannelListMessage>();

			var channel = channels.GetChannels().Single (c => c.Name == "Channel 2");
			context.Users.Move (server, user, channel);
			server.Client.DequeueAndAssertMessage<UserChangedChannelMessage>();

			permissions.EnablePermissions (user.UserId, PermissionName.DeleteChannel, PermissionName.RequestChannelList);
			
			handler.ChannelEditMessage (new MessageReceivedEventArgs (server, new ChannelEditMessage
			{
				Channel = channel,
				Delete = true
			}));

			var moved = server.Client.DequeueAndAssertMessage<UserChangedChannelMessage>();
			Assert.AreEqual (user.UserId, moved.ChangeInfo.TargetUserId);
			Assert.AreEqual (channels.GetChannels().Single().ChannelId, moved.ChangeInfo.TargetChannelId);
			Assert.AreEqual (channel.ChannelId, moved.ChangeInfo.PreviousChannelId);
			Assert.AreEqual (0, moved.ChangeInfo.RequestingUserId);

			server.Client.DequeueAndAssertMessage<ChannelListMessage>();

			var result = server.Client.DequeueAndAssertMessage<ChannelEditResultMessage>();
			Assert.AreEqual (channel.ChannelId, result.ChannelId);
			Assert.AreEqual (ChannelEditResult.Success, result.Result);
		}

		[Test]
		public void JoinChannelAtLimit()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.ChangeChannel);

			Assert.AreEqual (ChannelEditResult.Success, channels.SaveChannel (new ChannelInfo { Name = "Channel 2", UserLimit = 1 }));
			server.Client.DequeueAndAssertMessage<ChannelListMessage>();

			var channel = channels.GetChannels().Single (c => c.Name == "Channel 2");
			context.Users.Move (user, channel);
			
			var moved = server.Client.DequeueAndAssertMessage<UserChangedChannelMessage>();
			Assert.AreEqual (user.UserId, moved.ChangeInfo.TargetUserId);
			Assert.AreEqual (channel.ChannelId, moved.ChangeInfo.TargetChannelId);
			Assert.AreEqual (channels.DefaultChannel.ChannelId, moved.ChangeInfo.PreviousChannelId);
			Assert.AreEqual (0, moved.ChangeInfo.RequestingUserId);

			var secondUser = UserInfoTests.GetTestUser (2, 1, false);
			var secondServer = new MockServerConnection();
			manager.Connect (secondServer);
			manager.Join (secondServer, secondUser);
			permissions.EnablePermissions (secondUser.UserId, PermissionName.ChangeChannel);

			context.Users.Move (secondServer, secondUser, channel);
			
			server.Client.AssertNoMessage();

			var result = secondServer.Client.DequeueAndAssertMessage<ChannelChangeResultMessage>();
			Assert.AreEqual (ChannelChangeResult.FailedFull, result.Result);
		}
	}
}