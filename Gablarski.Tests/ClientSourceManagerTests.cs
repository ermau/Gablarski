using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Client;
using Gablarski.Messages;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientSourceManagerTests
	{
		[SetUp]
		public void ManagerSetup()
		{
			this.provider = new MockConnectionProvider();
			this.server = this.provider.EstablishConnection();
			this.client = this.server.Client;

			var context = new MockClientContext { Connection = this.server.Client };
			var channels = new ClientChannelManager (context);
			ClientChannelManagerTests.PopulateChannels (channels, server);

			context.Users = new ClientUserManager (context);
			context.Channels = channels;
			context.CurrentUser = new CurrentUser (context, 1, "Foo", channels.First().ChannelId);

			this.manager = new ClientSourceManager (context);
		}

		private ClientSourceManager manager;
		private MockConnectionProvider provider;
		private MockServerConnection server;
		private MockClientConnection client;

		[Test]
		public void NullContext()
		{
			Assert.Throws<ArgumentNullException> (() => new ClientSourceManager (null));
		}

		[Test]
		public void RequestDefaultBitrate()
		{
			this.manager.Request ("voice", 1);

			var msg = this.server.DequeueAndAssertMessage<RequestSourceMessage>();
			Assert.AreEqual (1, msg.Channels);
			Assert.AreEqual (0, msg.TargetBitrate);
		}

		[Test]
		public void RequestBitRate()
		{
			this.manager.Request ("voice", 2, 64000);

			var msg = this.server.DequeueAndAssertMessage<RequestSourceMessage>();
			Assert.AreEqual (2, msg.Channels);
			Assert.AreEqual (64000, msg.TargetBitrate);
		}

		[Test]
		public void SourceListReceived()
		{
			manager.OnSourceListReceivedMessage (new MessageReceivedEventArgs (this.client, 
				new SourceListMessage (new []
				{
					new AudioSource ("ownvoice", 1, 1, 1, 64000, 44100, 256, 10, false),
					new AudioSource ("voice", 2, 2, 1, 96000, 44100, 512, 10, true),
				})
			));

			var csource = manager.OfType<ClientAudioSource>().FirstOrDefault (s => s.Id == 1);
			Assert.IsNotNull (csource, "Source not found");
			Assert.AreEqual ("ownvoice", csource.Name, "Name not matching");
			Assert.AreEqual (1, csource.OwnerId, "OwnerId not matching");
			Assert.AreEqual (1, csource.Channels, "Channels not matching");
			Assert.AreEqual (64000, csource.Bitrate, "Bitrate not matching");
			Assert.AreEqual (44100, csource.Frequency, "Frequency not matching");
			Assert.AreEqual (256, csource.FrameSize, "FrameSize not matching");
			Assert.AreEqual (10, csource.Complexity, "Complexity not matching.");
			Assert.AreEqual (false, csource.Muted, "Muted not matching");

			var source = manager.OfType<AudioSource>().FirstOrDefault (s => s.Id == 2);
			Assert.IsNotNull (source, "Source not found");
			Assert.AreEqual ("voice", source.Name, "Name not matching");
			Assert.AreEqual (2, source.OwnerId, "OwnerId not matching");
			Assert.AreEqual (1, source.Channels, "Channels not matching");
			Assert.AreEqual (96000, source.Bitrate, "Bitrate not matching");
			Assert.AreEqual (44100, source.Frequency, "Frequency not matching");
			Assert.AreEqual (512, source.FrameSize, "FrameSize not matching");
			Assert.AreEqual (10, source.Complexity, "Complexity not matching");
			Assert.AreEqual (true, source.Muted, "Muted not matching");
		}

		//manager.OnSourceResultMessage (new SourceResultMessage (SourceResult.));
	}
}