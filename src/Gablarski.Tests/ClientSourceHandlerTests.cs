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
using Gablarski.Client;
using Gablarski.Messages;
using NUnit.Framework;
using Tempest;
using Tempest.Tests;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientSourceHandlerTests
	{
		[SetUp]
		public void ManagerSetup()
		{
			this.provider = new MockConnectionProvider (GablarskiProtocol.Instance);
			this.provider.Start (MessageTypes.All);

			var connections = this.provider.GetConnections (GablarskiProtocol.Instance);
			this.server = new ConnectionBuffer (connections.Item2);
			this.client = connections.Item1;

			MockClientContext c;
			this.context = c = new MockClientContext (this.client);
			var channels = new ClientChannelManager (context);
			ClientChannelManagerTests.PopulateChannels (channels, server);

			c.Users = new ClientUserHandler (context, new ClientUserManager());
			c.Channels = channels;
			c.CurrentUser = new CurrentUser (context, 1, "Foo", channels.First().ChannelId);

			this.manager = new ClientSourceManager (context);
			this.handler = new ClientSourceHandler (context, manager);
		}

		private IGablarskiClientContext context;
		private ClientSourceHandler handler;
		private ClientSourceManager manager;
		private MockConnectionProvider provider;
		private ConnectionBuffer server;
		private MockClientConnection client;

		[Test]
		public void CtorNull()
		{
			Assert.Throws<ArgumentNullException> (() => new ClientSourceHandler (null, manager));
			Assert.Throws<ArgumentNullException> (() => new ClientSourceHandler (context, null));
		}

		[Test]
		public void GetIsIgnoredNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.GetIsIgnored (null));
		}

		[Test]
		public void ToggleIgnoreNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.ToggleIgnore (null));
		}

		[Test]
		public void ToggleIgnore()
		{
			var source = AudioSourceTests.GetTestSource();
			manager.Add (source);

			Assert.IsFalse (handler.GetIsIgnored (source));
			Assert.IsTrue (handler.ToggleIgnore (source));
			Assert.IsTrue (handler.GetIsIgnored (source));
			Assert.IsFalse (handler.ToggleIgnore (source));
			Assert.IsFalse (handler.GetIsIgnored (source));
		}

		[Test]
		public void ToggleMuteNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.ToggleMute (null));
		}

		[Test]
		public void BeginSendingNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.BeginSending (null));
		}

		[Test]
		public void SendAudioDataNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.SendAudioData (null, TargetType.User, new [] { 1 }, new byte[1][]));
			Assert.Throws<ArgumentNullException> (() => handler.SendAudioData (AudioSourceTests.GetTestSource(), TargetType.User, null, new byte[1][]));
			Assert.Throws<ArgumentNullException> (() => handler.SendAudioData (AudioSourceTests.GetTestSource(), TargetType.User, new [] { 1 }, null));
		}

		[Test]
		public void SendAudioDataInvalid()
		{
			Assert.Throws<ArgumentException> (() => handler.SendAudioData (AudioSourceTests.GetTestSource(), TargetType.User, new [] { 1 }, new byte[0][]));
			Assert.Throws<ArgumentException> (() => handler.SendAudioData (AudioSourceTests.GetTestSource (2), TargetType.User, new [] { 1 }, new byte[1][] ));
		}

		[Test]
		public void EndSendingNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.EndSending (null));
		}

		[Test]
		public void RequestNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.Request (null, AudioFormat.Mono16bitLPCM, AudioSourceTests.FrameSize));
			Assert.Throws<ArgumentNullException> (() => handler.Request ("name", null, AudioSourceTests.FrameSize));

			Assert.Throws<ArgumentNullException> (() => handler.Request (null, AudioFormat.Mono16bitLPCM, AudioSourceTests.FrameSize, 64000));
			Assert.Throws<ArgumentNullException> (() => handler.Request ("name", null, AudioSourceTests.FrameSize, 64000));
		}

		[Test]
		public void RequestDefaultBitrate()
		{
			this.handler.Request ("voice", AudioFormat.Mono16bitLPCM, 480);

			var msg = this.server.DequeueAndAssertMessage<RequestSourceMessage>();
			Assert.AreEqual ("voice", msg.Name);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.BitsPerSample, msg.AudioSettings.BitsPerSample);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.Channels, msg.AudioSettings.Channels);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.SampleRate, msg.AudioSettings.SampleRate);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.WaveEncoding, msg.AudioSettings.WaveEncoding);
			Assert.AreEqual (0,	msg.AudioSettings.Bitrate);
		}

		[Test]
		public void RequestFrequencyAndBitRate()
		{
			this.handler.Request ("voice", new AudioFormat (WaveFormatEncoding.LPCM, 1, 16, 48000), AudioSourceTests.FrameSize, 64000);

			var msg = this.server.DequeueAndAssertMessage<RequestSourceMessage>();
			Assert.AreEqual ("voice", msg.Name);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.BitsPerSample, msg.AudioSettings.BitsPerSample);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.Channels, msg.AudioSettings.Channels);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.WaveEncoding, msg.AudioSettings.WaveEncoding);
			Assert.AreEqual (480, msg.AudioSettings.FrameSize);
			Assert.AreEqual (48000, msg.AudioSettings.SampleRate);
			Assert.AreEqual (64000, msg.AudioSettings.Bitrate);
		}

		[Test]
		public void CreateFake()
		{
			string name = "fakeMonkeys";
			AudioFormat format = AudioFormat.Stereo16bitLPCM;
			short frameSize = 480;

			var s = this.handler.CreateFake (name, format, frameSize);

			Assert.AreEqual (name, s.Name);
			AudioCodecArgsTests.AssertAreEqual (new AudioCodecArgs (format, 0, frameSize, 10), s);
			
			Assert.AreSame (s, this.handler[s.Id]);
		}
	}
}