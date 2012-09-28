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
using Tempest;
using Tempest.Tests;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerSourceHandlerTests
	{
		private IGablarskiServerContext context;
		private IServerSourceManager manager;
		private IServerUserManager userManager;
		private ServerSourceHandler handler;
		private MockServerConnection server;
		private ConnectionBuffer client;
		private MockPermissionsProvider permissions;
		private UserInfo user;
		private MockConnectionProvider provider;

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

			this.provider = new MockConnectionProvider (GablarskiProtocol.Instance);

			userManager = new ServerUserManager();
			MockServerContext c;
			context = c = new MockServerContext (provider)
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
		
			var cs = provider.GetConnections (GablarskiProtocol.Instance);

			client = new ConnectionBuffer (cs.Item1);

			userManager.Connect (server);
			userManager.Join (server, user);
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

		[Test]
		public void Enumerable()
		{
			var source = manager.Create ("Name", user, AudioSourceTests.GetTestSource());
			var source2 = manager.Create ("Name2", user, AudioSourceTests.GetTestSource());

			Assert.Contains (source, handler.ToList());
			Assert.Contains (source2, handler.ToList());
		}

		#region RequestSourceMessage
		[Test]
		public void RequestSourceNotConnected()
		{
			var cs = this.provider.GetConnections (GablarskiProtocol.Instance);
			var cl = new ConnectionBuffer (cs.Item1);

			cs.Item2.Disconnect();

			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (cs.Item2,
				new RequestSourceMessage ("Name", AudioCodecArgsTests.GetTestArgs())));

			cl.AssertNoMessage();
		}

		[Test]
		public void RequestSourceNotJoined()
		{
			var cs = this.provider.GetConnections (GablarskiProtocol.Instance);
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (cs.Item2);

			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (cs.Item2,
				new RequestSourceMessage ("Name", AudioCodecArgsTests.GetTestArgs())));

			cl.AssertNoMessage();
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
			permissions.EnablePermissions (userManager.GetUser (connection).UserId,	PermissionName.RequestSource);

			var audioArgs = new AudioCodecArgs (AudioFormat.Mono16bitLPCM, 64000, 512, 10);
			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (connection,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);

			client.AssertNoMessage();

			return result.Source;
		}
		
		[Test]
		public void RequestSourceDefaultBitrate()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var audioArgs = new AudioCodecArgs (AudioFormat.Mono16bitLPCM, 0, 512, 10);
			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server, new RequestSourceMessage ("Name", audioArgs)));

			var result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			audioArgs.Bitrate = defaultBitrate;
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);

			client.AssertNoMessage();
		}
		
		[Test]
		public void RequestSourceExceedMaxBitrate()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var audioArgs = new AudioCodecArgs (AudioFormat.Mono16bitLPCM, 200000, 512, 10);
			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server, new RequestSourceMessage ("Name", audioArgs)));
			
			var result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			audioArgs.Bitrate = maxBitrate;
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);

			client.AssertNoMessage();
		}
		
		[Test]
		public void RequestSourceBelowMinBitrate()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var audioArgs = new AudioCodecArgs (AudioFormat.Mono16bitLPCM, 1, 512, 10);
			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server, new RequestSourceMessage ("Name", audioArgs)));
			
			var result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			audioArgs.Bitrate = minBitrate;
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);

			client.AssertNoMessage();
		}

		[Test]
		public void RequestSourceDuplicateSourceName()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var audioArgs = AudioCodecArgsTests.GetTestArgs();
			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);

			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server,
				new RequestSourceMessage ("Name", audioArgs)));

			result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.FailedDuplicateSourceName, result.SourceResult);
		}
		
		[Test]
		public void RequestSourceNotification()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2));

			var audioArgs = AudioCodecArgsTests.GetTestArgs();
			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var sourceAdded = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, sourceAdded.SourceResult);
			Assert.AreEqual ("Name", sourceAdded.SourceName);
			AudioCodecArgsTests.AssertAreEqual (audioArgs, sourceAdded.Source);

			cl.AssertNoMessage();
		}
		#endregion

		#region RequestSourceListMessage
		[Test]
		public void RequestSourceListNotConnected()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			c.Disconnect();

			handler.RequestSourceListMessage (new MessageEventArgs<RequestSourceListMessage> (c, new RequestSourceListMessage ()));
			
			cl.AssertNoMessage();
		}
		
		[Test]
		public void RequestSourceListEmpty()
		{
			handler.RequestSourceListMessage (new MessageEventArgs<RequestSourceListMessage> (server, new RequestSourceListMessage()));
			
			var list = client.DequeueAndAssertMessage<SourceListMessage>();
			Assert.IsEmpty (list.Sources.ToList());

			client.AssertNoMessage();
		}

		[Test]
		public void RequestSourceList()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);
			var audioArgs = AudioCodecArgsTests.GetTestArgs();
			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			client.AssertNoMessage();

			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server,
				new RequestSourceMessage ("Name2", audioArgs)));

			var result2 = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result2.SourceResult);
			client.AssertNoMessage();

			handler.RequestSourceListMessage (new MessageEventArgs<RequestSourceListMessage> (server, new RequestSourceListMessage()));
			var list = client.DequeueAndAssertMessage<SourceListMessage>();

			Assert.AreEqual (2, list.Sources.Count());
			Assert.Contains (result.Source, list.Sources.ToList());
			Assert.Contains (result2.Source, list.Sources.ToList());

			client.AssertNoMessage();
		}

		[Test]
		public void RequestSourceListFromViewer()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);
			var audioArgs = AudioCodecArgsTests.GetTestArgs();
			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);

			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server,
				new RequestSourceMessage ("Name2", audioArgs)));

			var result2 = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result2.SourceResult);
			client.AssertNoMessage();

			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2));

			handler.RequestSourceListMessage (new MessageEventArgs<RequestSourceListMessage> (c, new RequestSourceListMessage()));
			var list = cl.DequeueAndAssertMessage<SourceListMessage>();

			Assert.AreEqual (2, list.Sources.Count());
			Assert.Contains (result.Source, list.Sources.ToList());
			Assert.Contains (result2.Source, list.Sources.ToList());
			cl.AssertNoMessage();
		}

		[Test]
		public void RequestUpdatedSourceList()
		{
			permissions.EnablePermissions (user.UserId, PermissionName.RequestSource);
			var audioArgs = AudioCodecArgsTests.GetTestArgs();
			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);

			handler.RequestSourceListMessage (new MessageEventArgs<RequestSourceListMessage> (server, new RequestSourceListMessage()));
			var list = client.DequeueAndAssertMessage<SourceListMessage>();

			Assert.AreEqual (1, list.Sources.Count());
			Assert.Contains (result.Source, list.Sources.ToList());
			client.AssertNoMessage();

			handler.RequestSourceMessage (new MessageEventArgs<RequestSourceMessage> (server,
				new RequestSourceMessage ("Name2", audioArgs)));

			var result2 = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result2.SourceResult);

			handler.RequestSourceListMessage (new MessageEventArgs<RequestSourceListMessage> (server, new RequestSourceListMessage()));
			list = client.DequeueAndAssertMessage<SourceListMessage>();

			Assert.AreEqual (2, list.Sources.Count());
			Assert.Contains (result.Source, list.Sources.ToList());
			Assert.Contains (result2.Source, list.Sources.ToList());
			client.AssertNoMessage();
		}
		#endregion

		#region RequestMuteSourceMessage
		[Test]
		public void RequestMuteNotConnected()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			c.Disconnect();

			var source = GetSourceFromRequest();

			handler.RequestMuteSourceMessage (new MessageEventArgs<RequestMuteSourceMessage> (c,
				new RequestMuteSourceMessage (source, true)));

			cl.AssertNoMessage();
			client.AssertNoMessage();
		}

		[Test]
		public void RequestMuteNotJoined()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);

			var source = GetSourceFromRequest();

			Assert.AreEqual (SourceResult.NewSource, cl.DequeueAndAssertMessage<SourceResultMessage>().SourceResult);

			handler.RequestMuteSourceMessage (new MessageEventArgs<RequestMuteSourceMessage> (c,
				new RequestMuteSourceMessage (source, true)));

			var denied = cl.DequeueAndAssertMessage<PermissionDeniedMessage>();
			Assert.AreEqual (GablarskiMessageType.RequestMuteSource, denied.DeniedMessage);
			cl.AssertNoMessage();
		}

		[Test]
		public void RequestMuteWithoutPermission()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2));

			var source = GetSourceFromRequest();
			Assert.AreEqual (SourceResult.NewSource, cl.DequeueAndAssertMessage<SourceResultMessage>().SourceResult);
			cl.AssertNoMessage();

			handler.RequestMuteSourceMessage (new MessageEventArgs<RequestMuteSourceMessage> (c,
				new RequestMuteSourceMessage (source, true)));

			var denied = cl.DequeueAndAssertMessage<PermissionDeniedMessage>();
			Assert.AreEqual (GablarskiMessageType.RequestMuteSource, denied.DeniedMessage);
			cl.AssertNoMessage();
		}

//		[Test]
//		public void RequestMuteWithPermission()
//		{
//			var u = UserInfoTests.GetTestUser (2);
//			var c = new MockServerConnection();
//			userManager.Connect (c);
//			userManager.Join (c, u);
//			permissions.SetPermissions (u.UserId, new[] { new Permission (PermissionName.MuteAudioSource, true) });
//
//			var source = GetSourceFromRequest();
//			Assert.AreEqual (SourceResult.NewSource, cl.DequeueAndAssertMessage<SourceResultMessage>().SourceResult);
//
//			handler.RequestMuteSourceMessage (new MessageReceivedEventArgs (c,
//				new RequestMuteSourceMessage (source, true)));
//
//			var mute = cl.DequeueAndAssertMessage<MutedMessage>();
//			Assert.AreEqual (false, mute.Unmuted);
//			Assert.AreEqual (source.Id, mute.Target);
//			Assert.AreEqual (MuteType.AudioSource, mute.Type);
//			cl.AssertNoMessage();
//
//			mute = client.DequeueAndAssertMessage<MutedMessage>();
//			Assert.AreEqual (false, mute.Unmuted);
//			Assert.AreEqual (source.Id, mute.Target);
//			Assert.AreEqual (MuteType.AudioSource, mute.Type);
//			cl.AssertNoMessage();
//		}
		#endregion

		#region ClientAudioSourceStateChangeMessage
		[Test]
		public void ClientAudioSourceStateChangeNotConnected()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			c.Disconnect();

			handler.ClientAudioSourceStateChangeMessage (new MessageEventArgs<ClientAudioSourceStateChangeMessage> (c,
				new ClientAudioSourceStateChangeMessage { SourceId = 1, Starting = true }));

			client.AssertNoMessage();
			cl.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangeNotJoined()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);

			handler.ClientAudioSourceStateChangeMessage (new MessageEventArgs<ClientAudioSourceStateChangeMessage> (c,
				new ClientAudioSourceStateChangeMessage { SourceId = 1, Starting = true }));

			client.AssertNoMessage();
			cl.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangeNotOwnedSource()
		{
			var u = UserInfoTests.GetTestUser (2, 1, false);
			
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, u);

			var s = manager.Create ("Name", user, new AudioCodecArgs());

			handler.ClientAudioSourceStateChangeMessage (new MessageEventArgs<ClientAudioSourceStateChangeMessage> (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			client.AssertNoMessage();
			cl.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangedUserMuted()
		{
			var u = UserInfoTests.GetTestUser (2, 1, true);
			
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, u);

			var s = manager.Create ("Name", u, new AudioCodecArgs ());

			handler.ClientAudioSourceStateChangeMessage (new MessageEventArgs<ClientAudioSourceStateChangeMessage> (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			client.AssertNoMessage();
			cl.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangedSourceMuted()
		{
			var u = UserInfoTests.GetTestUser (2, 1, false);
			
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, u);

			var s = manager.Create ("Name", u, new AudioCodecArgs ());
			manager.ToggleMute (s);

			handler.ClientAudioSourceStateChangeMessage (new MessageEventArgs<ClientAudioSourceStateChangeMessage> (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			client.AssertNoMessage();
			cl.AssertNoMessage();
		}

		[Test]
		public void ClientAudioSourceStateChangedSameChannel()
		{
			var u = UserInfoTests.GetTestUser (2, 1, false);
			
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, u);

			var s = manager.Create ("Name", u, new AudioCodecArgs ());

			handler.ClientAudioSourceStateChangeMessage (new MessageEventArgs<ClientAudioSourceStateChangeMessage> (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			cl.AssertNoMessage();

			var change = client.DequeueAndAssertMessage<AudioSourceStateChangeMessage>();
			Assert.AreEqual (s.Id, change.SourceId);
			Assert.AreEqual (true, change.Starting);
		}

		[Test]
		public void ClientAudioSourceStateChangedDifferentChannel()
		{
			var u = UserInfoTests.GetTestUser (2, 2, false);
			
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, u);

			var s = manager.Create ("Name", u, new AudioCodecArgs ());

			handler.ClientAudioSourceStateChangeMessage (new MessageEventArgs<ClientAudioSourceStateChangeMessage> (c,
				new ClientAudioSourceStateChangeMessage { SourceId = s.Id, Starting = true }));

			cl.AssertNoMessage();

			var change = client.DequeueAndAssertMessage<AudioSourceStateChangeMessage>();
			Assert.AreEqual (s.Id, change.SourceId);
			Assert.AreEqual (true, change.Starting);
		}
		#endregion

		#region ClientAudioDataMessage
		[Test]
		public void SendAudioDataMessageNotConnected()
		{			
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			c.Disconnect();

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (c,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = 1,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			cl.AssertNoMessage();
			client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageNotJoined()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (c,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = 1,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			cl.AssertNoMessage();
			client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageUnknownSource()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = 1,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			cl.AssertNoMessage();
			client.AssertNoMessage();
		}
        
		[Test]
		public void SendAudioDataMessageSourceNotOwned()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var s = GetSourceFromRequest();
			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (c,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			cl.AssertNoMessage();
			client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageSourceMuted ()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var s = GetSourceFromRequest();
			manager.ToggleMute (s);

			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			cl.AssertNoMessage();
			client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageUserMuted()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2, 1, true));

			var s = GetSourceFromRequest (c);
			var result = client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			client.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (c,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1 }
				}));

			cl.AssertNoMessage();
			client.AssertNoMessage();
		}

		[Test]
		public void SendAudioDataMessageToUser()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			var u = UserInfoTests.GetTestUser (2, 2, false);
			userManager.Join (c, u);

			var s = GetSourceFromRequest();
			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio);

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.User,
					TargetIds = new [] { u.UserId }
				}));

			client.AssertNoMessage();
			Assert.AreEqual (s.Id, cl.DequeueAndAssertMessage<ServerAudioDataMessage>().SourceId);
		}

		[Test]
		public void SendAudioDataMessageToUserWithoutPermission()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			var u = UserInfoTests.GetTestUser (2, 2, false);
			userManager.Join (c, u);

			var s = GetSourceFromRequest();
			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.User,
					TargetIds = new [] { u.UserId }
				}));

			cl.AssertNoMessage();
			Assert.AreEqual (GablarskiMessageType.AudioData, client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void SendAudioDataMessageToCurrentChannel()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var s = GetSourceFromRequest();
			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio);

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { user.CurrentChannelId }
				}));

			client.AssertNoMessage();
			Assert.AreEqual (s.Id, cl.DequeueAndAssertMessage<ServerAudioDataMessage>().SourceId);
		}

		[Test]
		public void SendAudioDataMessageToCurrentChannelWithoutPermission()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var s = GetSourceFromRequest();
			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { user.CurrentChannelId }
				}));

			cl.AssertNoMessage();
			Assert.AreEqual (GablarskiMessageType.AudioData, client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void SendAudioDataMessageToMultipleChannels()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var cs2 = provider.GetConnections (GablarskiProtocol.Instance);
			var c2 = cs2.Item2;
			var cl2 = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c2);
			userManager.Join (c2, UserInfoTests.GetTestUser (3, 2, false));

			var s = GetSourceFromRequest();
			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			result = cl2.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl2.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio, PermissionName.SendAudioToMultipleTargets);

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1, 2 }
				}));

			client.AssertNoMessage();
			Assert.AreEqual (s.Id, cl.DequeueAndAssertMessage<ServerAudioDataMessage>().SourceId);
			Assert.AreEqual (s.Id, cl2.DequeueAndAssertMessage<ServerAudioDataMessage>().SourceId);
		}

		[Test]
		public void SendAudioDataMessageToMultipleChannelsWithoutPermission()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			userManager.Join (c, UserInfoTests.GetTestUser (2, 1, false));

			var cs2 = provider.GetConnections (GablarskiProtocol.Instance);
			var c2 = cs2.Item2;
			var cl2 = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c2);
			userManager.Join (c2, UserInfoTests.GetTestUser (3, 2, false));

			var s = GetSourceFromRequest();
			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			result = cl2.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl2.AssertNoMessage();

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.Channel,
					TargetIds = new [] { 1, 2 }
				}));

			cl.AssertNoMessage();
			cl2.AssertNoMessage();
			Assert.AreEqual (GablarskiMessageType.AudioData, client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}

		[Test]
		public void SendAudioDataMessageToMultipleUsers()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			var u1 = UserInfoTests.GetTestUser (2, 1, false);
			userManager.Join (c, u1);

			var cs2 = provider.GetConnections (GablarskiProtocol.Instance);
			var c2 = cs2.Item2;
			var cl2 = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c2);
			var u2 = UserInfoTests.GetTestUser (3, 2, false);
			userManager.Join (c2, u2);

			var s = GetSourceFromRequest();
			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			result = cl2.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl2.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio, PermissionName.SendAudioToMultipleTargets);

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.User,
					TargetIds = new [] { u1.UserId, u2.UserId }
				}));

			client.AssertNoMessage();
			Assert.AreEqual (s.Id, cl.DequeueAndAssertMessage<ServerAudioDataMessage>().SourceId);
			Assert.AreEqual (s.Id, cl2.DequeueAndAssertMessage<ServerAudioDataMessage>().SourceId);
		}

		[Test]
		public void SendAudioDataMessageToMultipleUsersWithoutPermission()
		{
			var cs = provider.GetConnections (GablarskiProtocol.Instance);
			var c = cs.Item2;
			var cl = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c);
			var u1 = UserInfoTests.GetTestUser (2, 1, false);
			userManager.Join (c, u1);

			var cs2 = provider.GetConnections (GablarskiProtocol.Instance);
			var c2 = cs2.Item2;
			var cl2 = new ConnectionBuffer (cs.Item1);

			userManager.Connect (c2);
			var u2 = UserInfoTests.GetTestUser (3, 1, false);
			userManager.Join (c2, u2);

			var s = GetSourceFromRequest();
			var result = cl.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.NewSource, result.SourceResult);
			cl.AssertNoMessage();

			permissions.EnablePermissions (user.UserId, PermissionName.SendAudio);

			handler.SendAudioDataMessage (new MessageEventArgs<ClientAudioDataMessage> (server,
				new ClientAudioDataMessage
				{
					Data = new [] { new byte[512] },
					SourceId = s.Id,
					TargetType = TargetType.User,
					TargetIds = new [] { u1.UserId, u2.UserId }
				}));

			cl.AssertNoMessage();
			Assert.AreEqual (GablarskiMessageType.AudioData, client.DequeueAndAssertMessage<PermissionDeniedMessage>().DeniedMessage);
		}
		#endregion
	}
}