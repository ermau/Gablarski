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
using Gablarski.Audio;
using Gablarski.Messages;
using Gablarski.Server;
using Gablarski.Tests.Mocks;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerSourceHandlerTests
	{
		private IServerContext context;
		private IServerSourceManager manager;
		private ServerSourceHandler handler;
		private MockServerConnection server;
		private MockPermissionsProvider permissions;
		private UserInfo user;
		
		private const int defaultBitrate = 96000;
		private const int maxBitrate = 192000;
		private const int minBitrate = 32000;

		[SetUp]
		public void Setup()
		{
			permissions = new MockPermissionsProvider();

			LobbyChannelProvider channels = new LobbyChannelProvider();
			channels.SaveChannel (new ChannelInfo
			{
				Name = "Channel 2"
			});

			ServerUserManager userManager = new ServerUserManager();
			MockServerContext c;
			context = c = new MockServerContext
			{
				Settings = new ServerSettings
				{
					Name = "Server",
					DefaultAudioBitrate = defaultBitrate,
					MaximumAudioBitrate = maxBitrate,
					MinimumAudioBitrate = minBitrate
				},

				UserManager = userManager,
				PermissionsProvider = permissions,
				ChannelsProvider = channels 
			};
			c.Users = new ServerUserHandler (context, userManager);

			manager = new ServerSourceManager (context);
			handler = new ServerSourceHandler (context, manager);

			user = UserInfoTests.GetTestUser (1, 1, false);
			server = new MockServerConnection();

			context.UserManager.Connect (server);
			context.UserManager.Join (server, user);
		}
        
		[TearDown]
		public void Teardown()
		{
			handler = null;
			manager = null;
			context = null;
			server = null;
			permissions = null;
		}

		[Test]
		public void CtorNull()
		{
			Assert.Throws<ArgumentNullException> (() => new ServerSourceHandler (null, manager));
			Assert.Throws<ArgumentNullException> (() => new ServerSourceHandler (context, null));
		}

		#region RequestSourceMessage
		[Test]
		public void RequestSourceNotConnected()
		{
			var c = new MockServerConnection();
			c.Disconnect();

			handler.RequestSourceMessage (new MessageReceivedEventArgs (c,
				new RequestSourceMessage ("Name", new AudioCodecArgs (1, 64000, 44100, 512, 10))));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void RequestSourceNotJoined()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);

			handler.RequestSourceMessage (new MessageReceivedEventArgs (c,
				new RequestSourceMessage ("Name", new AudioCodecArgs (1, 64000, 44100, 512, 10))));

			c.Client.AssertNoMessage();
		}

		[Test]
		public void RequestSource()
		{
			GetSourceFromRequest();
		}

		public AudioSource GetSourceFromRequest()
		{
			return GetSourceFromRequest (server);
		}

		public AudioSource GetSourceFromRequest (MockServerConnection connection)
		{
			permissions.EnablePermissions (context.UserManager.GetUser (connection).UserId,	PermissionName.RequestSource);

			var audioArgs = new AudioCodecArgs (1, 64000, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (connection,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = connection.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);

			connection.Client.AssertNoMessage();

			return result.Source;
		}
		
		[Test]
		public void RequestSourceDefaultBitrate()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var audioArgs = new AudioCodecArgs (1, 0, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server, new RequestSourceMessage ("Name", audioArgs)));

			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			audioArgs.Bitrate = defaultBitrate;
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);

			server.Client.AssertNoMessage();
		}
		
		[Test]
		public void RequestSourceExceedMaxBitrate()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var audioArgs = new AudioCodecArgs (1, 200000, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server, new RequestSourceMessage ("Name", audioArgs)));
			
			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			audioArgs.Bitrate = maxBitrate;
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);

			server.Client.AssertNoMessage();
		}
		
		[Test]
		public void RequestSourceBelowMinBitrate()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var audioArgs = new AudioCodecArgs (1, 1, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server, new RequestSourceMessage ("Name", audioArgs)));
			
			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			audioArgs.Bitrate = minBitrate;
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);

			server.Client.AssertNoMessage();
		}

		[Test]
		public void RequestSourceDuplicateSourceName()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var audioArgs = new AudioCodecArgs (1, 64000, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);

			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name", audioArgs)));

			result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.FailedDuplicateSourceName, result.SourceResult);
		}
		
		[Test]
		public void RequestSourceNotification()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2));

			var audioArgs = new AudioCodecArgs (1, 64000, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var sourceAdded = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, sourceAdded.SourceResult);
			Assert.AreEqual ("Name", sourceAdded.SourceName);
			AudioCodecArgsTests.AssertAreEqual (audioArgs, sourceAdded.Source);

			c.Client.AssertNoMessage();
		}
		#endregion

		#region RequestSourceListMessage
		[Test]
		public void RequestSourceListNotConnected()
		{
			var c = new MockServerConnection();
			c.Disconnect();

			handler.RequestSourceListMessage (new MessageReceivedEventArgs (c, new RequestSourceListMessage ()));
			
			c.Client.AssertNoMessage();
		}
		
		[Test]
		public void RequestSourceListEmpty()
		{
			handler.RequestSourceListMessage (new MessageReceivedEventArgs (server, new RequestSourceListMessage()));
			
			var list = server.Client.DequeueAndAssertMessage<SourceListMessage>();
			Assert.IsEmpty (list.Sources.ToList());

			server.Client.AssertNoMessage();
		}

		[Test]
		public void RequestSourceList()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);
			var audioArgs = new AudioCodecArgs (1, 64000, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			server.Client.AssertNoMessage();

			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name2", audioArgs)));

			var result2 = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result2.SourceResult);
			server.Client.AssertNoMessage();

			handler.RequestSourceListMessage (new MessageReceivedEventArgs (server, new RequestSourceListMessage()));
			var list = server.Client.DequeueAndAssertMessage<SourceListMessage>();

			Assert.AreEqual (2, list.Sources.Count());
			Assert.Contains (result.Source, list.Sources.ToList());
			Assert.Contains (result2.Source, list.Sources.ToList());

			server.Client.AssertNoMessage();
		}

		[Test]
		public void RequestSourceListFromViewer()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);
			var audioArgs = new AudioCodecArgs (1, 64000, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);

			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name2", audioArgs)));

			var result2 = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result2.SourceResult);
			server.Client.AssertNoMessage();

			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2));

			handler.RequestSourceListMessage (new MessageReceivedEventArgs (c, new RequestSourceListMessage()));
			var list = c.Client.DequeueAndAssertMessage<SourceListMessage>();

			Assert.AreEqual (2, list.Sources.Count());
			Assert.Contains (result.Source, list.Sources.ToList());
			Assert.Contains (result2.Source, list.Sources.ToList());
			c.Client.AssertNoMessage();
		}

		[Test]
		public void RequestUpdatedSourceList()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);
			var audioArgs = new AudioCodecArgs (1, 64000, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);

			handler.RequestSourceListMessage (new MessageReceivedEventArgs (server, new RequestSourceListMessage()));
			var list = server.Client.DequeueAndAssertMessage<SourceListMessage>();

			Assert.AreEqual (1, list.Sources.Count());
			Assert.Contains (result.Source, list.Sources.ToList());
			server.Client.AssertNoMessage();

			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name2", audioArgs)));

			var result2 = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result2.SourceResult);

			handler.RequestSourceListMessage (new MessageReceivedEventArgs (server, new RequestSourceListMessage()));
			list = server.Client.DequeueAndAssertMessage<SourceListMessage>();

			Assert.AreEqual (2, list.Sources.Count());
			Assert.Contains (result.Source, list.Sources.ToList());
			Assert.Contains (result2.Source, list.Sources.ToList());
			server.Client.AssertNoMessage();
		}
		#endregion

		#region RequestMuteSourceMessage
		[Test]
		public void RequestMuteNotConnected()
		{
			var c = new MockServerConnection();
			c.Disconnect();

			var source = GetSourceFromRequest();

			handler.RequestMuteSourceMessage (new MessageReceivedEventArgs (c,
				new RequestMuteSourceMessage (source, true)));

			c.Client.AssertNoMessage();
			server.Client.AssertNoMessage();
		}

		[Test]
		public void RequestMuteNotJoined()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);

			var source = GetSourceFromRequest();

			Assert.AreEqual (SourceResult.NewSource, c.Client.DequeueAndAssertMessage<SourceResultMessage>().SourceResult);

			handler.RequestMuteSourceMessage (new MessageReceivedEventArgs (c,
				new RequestMuteSourceMessage (source, true)));

			var denied = c.Client.DequeueAndAssertMessage<PermissionDeniedMessage>();
			Assert.AreEqual (ClientMessageType.RequestMuteSource, denied.DeniedMessage);
			c.Client.AssertNoMessage();
		}

		[Test]
		public void RequestMuteWithoutPermission()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2));

			var source = GetSourceFromRequest();
			Assert.AreEqual (SourceResult.NewSource, c.Client.DequeueAndAssertMessage<SourceResultMessage>().SourceResult);
			c.Client.AssertNoMessage();

			handler.RequestMuteSourceMessage (new MessageReceivedEventArgs (c,
				new RequestMuteSourceMessage (source, true)));

			var denied = c.Client.DequeueAndAssertMessage<PermissionDeniedMessage>();
			Assert.AreEqual (ClientMessageType.RequestMuteSource, denied.DeniedMessage);
			c.Client.AssertNoMessage();
		}

		[Test]
		public void RequestMuteWithPermission()
		{
			var u = UserInfoTests.GetTestUser (2);
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, u);
			permissions.SetPermissions (u.UserId, new[] { new Permission (PermissionName.MuteAudioSource, true) });

			var source = GetSourceFromRequest();
			Assert.AreEqual (SourceResult.NewSource, c.Client.DequeueAndAssertMessage<SourceResultMessage>().SourceResult);

			handler.RequestMuteSourceMessage (new MessageReceivedEventArgs (c,
				new RequestMuteSourceMessage (source, true)));

			var mute = c.Client.DequeueAndAssertMessage<MutedMessage>();
			Assert.AreEqual (false, mute.Unmuted);
			Assert.AreEqual (source.Id, mute.Target);
			Assert.AreEqual (MuteType.AudioSource, mute.Type);
			c.Client.AssertNoMessage();

			mute = server.Client.DequeueAndAssertMessage<MutedMessage>();
			Assert.AreEqual (false, mute.Unmuted);
			Assert.AreEqual (source.Id, mute.Target);
			Assert.AreEqual (MuteType.AudioSource, mute.Type);
			c.Client.AssertNoMessage();
		}
		#endregion

		#region ClientAudioSourceStateChangeMessage
		[Test]
		public void ClientAudioSourceStateChangeNotConnected()
		{
			var c = new MockServerConnection();
			c.Disconnect();

			handler.ClientAudioSourceStateChangeMessage (new MessageReceivedEventArgs (c,
				new ClientAudioSourceStateChangeMessage { SourceId = 1, Starting = true }));

			server.Client.AssertNoMessage();
			c.Client.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangeNotJoined()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);

			handler.ClientAudioSourceStateChangeMessage (new MessageReceivedEventArgs (c,
				new ClientAudioSourceStateChangeMessage { SourceId = 1, Starting = true }));

			server.Client.AssertNoMessage();
			c.Client.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangeNotOwnedSource()
		{
			var u = UserInfoTests.GetTestUser (2, 1, false);
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, u);

			var s = manager.Create ("Name", user, new AudioCodecArgs());

			handler.ClientAudioSourceStateChangeMessage (new MessageReceivedEventArgs (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			server.Client.AssertNoMessage();
			c.Client.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangedUserMuted()
		{
			var u = UserInfoTests.GetTestUser (2, 1, true);
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, u);

			var s = manager.Create ("Name", u, new AudioCodecArgs ());

			handler.ClientAudioSourceStateChangeMessage (new MessageReceivedEventArgs (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			server.Client.AssertNoMessage();
			c.Client.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangedSourceMuted()
		{
			var u = UserInfoTests.GetTestUser (2, 1, false);
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, u);

			var s = manager.Create ("Name", u, new AudioCodecArgs ());
			manager.ToggleMute (s);

			handler.ClientAudioSourceStateChangeMessage (new MessageReceivedEventArgs (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			server.Client.AssertNoMessage();
			c.Client.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangedSameChannel()
		{
			var u = UserInfoTests.GetTestUser (2, 1, false);
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, u);

			var s = manager.Create ("Name", u, new AudioCodecArgs ());

			handler.ClientAudioSourceStateChangeMessage (new MessageReceivedEventArgs (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			c.Client.AssertNoMessage();

			var change = server.Client.DequeueAndAssertMessage<AudioSourceStateChangeMessage>();
			Assert.AreEqual (s.Id, change.SourceId);
			Assert.AreEqual (true, change.Starting);
		}

		[Test]
		public void ClientAudioSourceStateChangedDifferentChannel()
		{
			var u = UserInfoTests.GetTestUser (2, 2, false);
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, u);

			var s = manager.Create ("Name", u, new AudioCodecArgs ());

			handler.ClientAudioSourceStateChangeMessage (new MessageReceivedEventArgs (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			c.Client.AssertNoMessage();

			var change = server.Client.DequeueAndAssertMessage<AudioSourceStateChangeMessage>();
			Assert.AreEqual (s.Id, change.SourceId);
			Assert.AreEqual (true, change.Starting);
		}
		#endregion

		#region SendAudioDataMessage
		[Test]
		public void SendAudioDataMessageNotConnected()
		{			
			var c = new MockServerConnection();
			c.Disconnect();

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (c,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = 1,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			c.Client.AssertNoMessage();
			server.Client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageNotJoined()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (c,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = 1,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			c.Client.AssertNoMessage();
			server.Client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageUnknownSource()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = 1,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			c.Client.AssertNoMessage();
			server.Client.AssertNoMessage();
		}
        
		[Test]
		public void SendAudioDataMessageSourceNotOwned()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var s = GetSourceFromRequest();
			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (c,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			c.Client.AssertNoMessage();
			server.Client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageSourceMuted ()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var s = GetSourceFromRequest();
			manager.ToggleMute (s);

			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			c.Client.AssertNoMessage();
			server.Client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageUserMuted()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2, 1, true));

			var s = GetSourceFromRequest (c);
			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			server.Client.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (c,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			c.Client.AssertNoMessage();
			server.Client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageToUser()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			var u = UserInfoTests.GetTestUser (2, 2, false);
			context.UserManager.Join (c, u);

			var s = GetSourceFromRequest();
			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio);

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.User,
					TargetIds = new [] { u.UserId }
				}));

			server.Client.AssertNoMessage();
			Assert.AreEqual (s.Id, c.Client.DequeueAndAssertMessage<AudioDataReceivedMessage>().SourceId);
		}

		[Test]
		public void SendAudioDataMessageToUserWithoutPermission()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			var u = UserInfoTests.GetTestUser (2, 2, false);
			context.UserManager.Join (c, u);

			var s = GetSourceFromRequest();
			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.User,
					TargetIds = new [] { u.UserId }
				}));

			c.Client.AssertNoMessage();
			Assert.AreEqual (ClientMessageType.AudioData, server.Client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void SendAudioDataMessageToCurrentChannel()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var s = GetSourceFromRequest();
			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio);

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { user.CurrentChannelId }
				}));

			server.Client.AssertNoMessage();
			Assert.AreEqual (s.Id, c.Client.DequeueAndAssertMessage<AudioDataReceivedMessage>().SourceId);
		}

		[Test]
		public void SendAudioDataMessageToCurrentChannelWithoutPermission()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var s = GetSourceFromRequest();
			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { user.CurrentChannelId }
				}));

			c.Client.AssertNoMessage();
			Assert.AreEqual (ClientMessageType.AudioData, server.Client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void SendAudioDataMessageToMultipleChannels()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var c2 = new MockServerConnection();
			context.UserManager.Connect (c2);
			context.UserManager.Join (c2, UserInfoTests.GetTestUser (3, 2, false));

			var s = GetSourceFromRequest();
			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			result = c2.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c2.Client.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio, PermissionName.SendAudioToMultipleTargets);

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1, 2 }
				}));

			server.Client.AssertNoMessage();
			Assert.AreEqual (s.Id, c.Client.DequeueAndAssertMessage<AudioDataReceivedMessage>().SourceId);
			Assert.AreEqual (s.Id, c2.Client.DequeueAndAssertMessage<AudioDataReceivedMessage>().SourceId);
		}

		[Test]
		public void SendAudioDataMessageToMultipleChannelsWithoutPermission()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			context.UserManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var c2 = new MockServerConnection();
			context.UserManager.Connect (c2);
			context.UserManager.Join (c2, UserInfoTests.GetTestUser (3, 2, false));

			var s = GetSourceFromRequest();
			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			result = c2.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c2.Client.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1, 2 }
				}));

			c.Client.AssertNoMessage();
			c2.Client.AssertNoMessage();
			Assert.AreEqual (ClientMessageType.AudioData, server.Client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void SendAudioDataMessageToMultipleUsers()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			var u1 = UserInfoTests.GetTestUser (2, 1, false);
			context.UserManager.Join (c, u1);

			var c2 = new MockServerConnection();
			context.UserManager.Connect (c2);
			var u2 = UserInfoTests.GetTestUser (3, 2, false);
			context.UserManager.Join (c2, u2);

			var s = GetSourceFromRequest();
			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			result = c2.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c2.Client.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio, PermissionName.SendAudioToMultipleTargets);

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.User,
					TargetIds = new [] { u1.UserId, u2.UserId }
				}));

			server.Client.AssertNoMessage();
			Assert.AreEqual (s.Id, c.Client.DequeueAndAssertMessage<AudioDataReceivedMessage>().SourceId);
			Assert.AreEqual (s.Id, c2.Client.DequeueAndAssertMessage<AudioDataReceivedMessage>().SourceId);
		}

		[Test]
		public void SendAudioDataMessageToMultipleUsersWithoutPermission()
		{
			var c = new MockServerConnection();
			context.UserManager.Connect (c);
			var u1 = UserInfoTests.GetTestUser (2, 1, false);
			context.UserManager.Join (c, u1);

			var c2 = new MockServerConnection();
			context.UserManager.Connect (c2);
			var u2 = UserInfoTests.GetTestUser (3, 1, false);
			context.UserManager.Join (c2, u2);

			var s = GetSourceFromRequest();
			var result = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			c.Client.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio);

			handler.SendAudioDataMessage (new MessageReceivedEventArgs (server,
				new SendAudioDataMessage
				{
					Data = new byte[512],
					SourceId = s.Id,
					TargetType = TargetType.User,
					TargetIds = new [] { u1.UserId, u2.UserId }
				}));

			c.Client.AssertNoMessage();
			Assert.AreEqual (ClientMessageType.AudioData, server.Client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}
		#endregion
	}
}