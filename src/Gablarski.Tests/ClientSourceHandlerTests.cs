using System;
using System.Collections.Generic;
using System.Linq;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Messages;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientSourceHandlerTests
	{
		[SetUp]
		public void ManagerSetup()
		{
			this.provider = new MockConnectionProvider();
			this.server = this.provider.EstablishConnection();
			this.client = this.server.Client;

			MockClientContext c;
			this.context = c = new MockClientContext { Connection = this.server.Client };
			var channels = new ClientChannelManager (context);
			ClientChannelManagerTests.PopulateChannels (channels, server);

			c.Users = new ClientUserHandler (context, new ClientUserManager());
			c.Channels = channels;
			c.CurrentUser = new CurrentUser (context, 1, "Foo", channels.First().ChannelId);

			this.manager = new ClientSourceManager (context);
			this.handler = new ClientSourceHandler (context, manager);
		}

		private IClientContext context;
		private ClientSourceHandler handler;
		private ClientSourceManager manager;
		private MockConnectionProvider provider;
		private MockServerConnection server;
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
		public void ToggleMuteNull()
		{
			Assert.Throws<ArgumentNullException> (() => handler.ToggleMute (null));
		}

		[Test]
		public void RequestDefaultBitrate()
		{
			this.handler.Request ("voice", AudioFormat.Mono16bitLPCM, 512);

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
			this.handler.Request ("voice", new AudioFormat (WaveFormatEncoding.LPCM, 1, 16, 48000), 512, 64000);

			var msg = this.server.DequeueAndAssertMessage<RequestSourceMessage>();
			Assert.AreEqual ("voice", msg.Name);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.BitsPerSample, msg.AudioSettings.BitsPerSample);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.Channels, msg.AudioSettings.Channels);
			Assert.AreEqual (AudioFormat.Mono16bitLPCM.WaveEncoding, msg.AudioSettings.WaveEncoding);
			Assert.AreEqual (48000, msg.AudioSettings.SampleRate);
			Assert.AreEqual (64000, msg.AudioSettings.Bitrate);
		}
	}
}