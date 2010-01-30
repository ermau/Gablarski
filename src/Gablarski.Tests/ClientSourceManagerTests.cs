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

			context.Users = new ClientUserHandler (context, new ClientUserManager());
			context.Channels = channels;
			context.CurrentUser = new CurrentUser (context, 1, "Foo", channels.First().ChannelId);

			this.manager = new ClientSourceManager (context);
		}

		private void CreateSources()
		{
			manager.OnSourceListReceivedMessage (new MessageReceivedEventArgs (this.client, 
			                                                                   new SourceListMessage (new []
			                                                                   {
																				   AudioSourceTests.GetTestSource (1, 0),
																				   AudioSourceTests.GetTestSource (2, 1),
			                                                                   })
			                                     	));
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
			this.manager.Request ("voice", 1, 512);

			var msg = this.server.DequeueAndAssertMessage<RequestSourceMessage>();
			Assert.AreEqual (1, msg.AudioSettings.Channels);
			Assert.AreEqual (0, msg.AudioSettings.Bitrate);
		}

		[Test]
		public void RequestBitRate()
		{
			this.manager.Request ("voice", 2, 512, 64000);

			var msg = this.server.DequeueAndAssertMessage<RequestSourceMessage>();
			Assert.AreEqual (2, msg.AudioSettings.Channels);
			Assert.AreEqual (64000, msg.AudioSettings.Bitrate);
		}

		[Test]
		public void SourceListReceived()
		{
			CreateSources();

			var csource = manager.FirstOrDefault (s => s.Id == 1);
			Assert.IsNotNull (csource, "Source not found");
			AudioSourceTests.AssertSourcesMatch (AudioSourceTests.GetTestSource (1, 0), csource);

			var source = manager.FirstOrDefault (s => s.Id == 2);
			Assert.IsNotNull (source, "Source not found");
			AudioSourceTests.AssertSourcesMatch (AudioSourceTests.GetTestSource (2, 1), source);
		}

		[Test]
		public void ToggleIgnore()
		{
			CreateSources();

			var source = manager.First();
			Assert.IsFalse (manager.GetIsIgnored (source));
			Assert.IsTrue (manager.ToggleIgnore (source));
			Assert.IsTrue (manager.GetIsIgnored (source));
			Assert.IsFalse (manager.ToggleIgnore (source));
			Assert.IsFalse (manager.GetIsIgnored (source));
		}

		[Test]
		public void ToggleIgnorePersisted()
		{
			CreateSources();

			var source = manager.First();
			Assert.IsFalse (manager.GetIsIgnored (source));
			Assert.IsTrue (manager.ToggleIgnore (source));
			Assert.IsTrue (manager.GetIsIgnored (source));

			CreateSources();

			Assert.IsTrue (manager.GetIsIgnored (source));
			Assert.IsFalse (manager.ToggleIgnore (source));
			Assert.IsFalse (manager.GetIsIgnored (source));
		}
	}
}