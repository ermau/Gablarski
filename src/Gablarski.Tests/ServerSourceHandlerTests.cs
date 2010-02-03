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
		private UserInfo user;
		
		private const int defaultBitrate = 96000;
		private const int maxBitrate = 192000;
		private const int minBitrate = 32000;

		[SetUp]
		public void Setup()
		{
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
				PermissionsProvider = new GuestPermissionProvider()
			
			};
			c.Users = new ServerUserHandler (context, userManager);

			manager = new ServerSourceManager (context);
			handler = new ServerSourceHandler (context, manager);

			user = UserInfoTests.GetTestUser();
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
		}

		[Test]
		public void CtorNull()
		{
			Assert.Throws<ArgumentNullException> (() => new ServerSourceHandler (null, manager));
			Assert.Throws<ArgumentNullException> (() => new ServerSourceHandler (context, null));
		}

		[Test]
		public void RequestSourceNotConnected()
		{
			var c = new MockServerConnection();
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
			var audioArgs = new AudioCodecArgs (1, 64000, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server,
				new RequestSourceMessage ("Name", audioArgs)));

			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);
		}
		
		[Test]
		public void RequestSourceDefaultBitrate()
		{
			var audioArgs = new AudioCodecArgs (1, 0, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server, new RequestSourceMessage ("Name", audioArgs)));
			
			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			audioArgs.Bitrate = defaultBitrate;
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);
		}
		
		[Test]
		public void RequestSourceExceedMaxBitrate()
		{
			var audioArgs = new AudioCodecArgs (1, 200000, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server, new RequestSourceMessage ("Name", audioArgs)));
			
			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			audioArgs.Bitrate = maxBitrate;
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);
		}
		
		[Test]
		public void RequestSourceBelowMinBitrate()
		{
			var audioArgs = new AudioCodecArgs (1, 1, 44100, 512, 10);
			handler.RequestSourceMessage (new MessageReceivedEventArgs (server, new RequestSourceMessage ("Name", audioArgs)));
			
			var result = server.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, result.SourceResult);
			Assert.AreEqual ("Name", result.SourceName);
			Assert.AreEqual ("Name", result.Source.Name);
			
			audioArgs.Bitrate = minBitrate;
			AudioCodecArgsTests.AssertAreEqual (audioArgs, result.Source);
		}
		
		[Test]
		public void RequestSourceListNotConnected()
		{
			var c = new MockServerConnection();			
			handler.RequestSourceListMessage (new MessageReceivedEventArgs (c, new RequestSourceListMessage ()));
			
			c.Client.AssertNoMessage();
		}
		
		[Test]
		public void RequestSourceListEmpty()
		{
			handler.RequestSourceListMessage (new MessageReceivedEventArgs (server, new RequestSourceListMessage()));
			
			var list = server.Client.DequeueAndAssertMessage<SourceListMessage>();
			Assert.IsEmpty (list.Sources.ToList());
		}
	}
}