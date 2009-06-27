using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Gablarski.Client;
using Gablarski.Messages;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientChannelManagerTests
	{
		[SetUp]
		public void ManagerSetup ()
		{
			this.provider = new MockConnectionProvider ();
			this.server = this.provider.EstablishConnection (new IdentifyingTypes (typeof(Int32), typeof(Int32)));
			this.manager = new ClientChannelManager (new MockClientContext { Connection = this.server.Client });
		}

		[TearDown]
		public void ManagerTearDown ()
		{
			this.manager = null;
			this.provider = null;
		}

		private MockServerConnection server;
		private ClientChannelManager manager;
		private MockConnectionProvider provider;

		[Test]
		public void NullConnection()
		{
			Assert.Throws<ArgumentNullException> (() => new ClientChannelManager (null));
		}

		[Test]
		public void CreateInvalidChannel ()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Create (null));
			Assert.Throws<ArgumentException>(() => manager.Create (new Channel (1)));
		}

		[Test]
		public void CreateChannel ()
		{
			Channel c = new Channel
			{
				Name = "Name",
				Description = "Description",
				ParentChannelId = 1
			};

			manager.Create (c);

			var msg = server.DequeueAndAssertMessage<ChannelEditMessage> ();
			Assert.AreEqual (c.Name, msg.Channel.Name);
			Assert.AreEqual (c.Description, msg.Channel.Description);
			Assert.AreEqual (c.ParentChannelId, msg.Channel.ParentChannelId);
		}

		[Test]
		public void UpdateInvalidChannel ()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Update (null));
			Assert.Throws<ArgumentException>(() => manager.Update (new Channel ()));
		}

		[Test]
		public void UpdateChannel ()
		{
			Channel c = new Channel (2)
			{
				Name = "Name",
				Description = "Description",
				ParentChannelId = 1
			};

			manager.Update (c);

			var msg = server.DequeueAndAssertMessage<ChannelEditMessage> ();
			Assert.AreEqual (c.ChannelId, msg.Channel.ChannelId);
			Assert.AreEqual (c.Name, msg.Channel.Name);
			Assert.AreEqual (c.Description, msg.Channel.Description);
			Assert.AreEqual (c.ParentChannelId, msg.Channel.ParentChannelId);
			Assert.IsFalse (msg.Delete);
		}

		[Test]
		public void DeleteInvalidChannel ()
		{
			Assert.Throws<ArgumentNullException> (() => manager.Delete (null));
			Assert.Throws<ArgumentException> (() => manager.Delete (new Channel (null)));
		}

		[Test]
		public void DeleteChannel ()
		{
			Channel c = new Channel (2)
			{
				Name = "Name",
				Description = "Description",
				ParentChannelId = 1
			};

			manager.Delete (c);

			var msg = server.DequeueAndAssertMessage<ChannelEditMessage> ();
			Assert.AreEqual (c.ChannelId, msg.Channel.ChannelId);
			Assert.AreEqual (c.Name, msg.Channel.Name);
			Assert.AreEqual (c.Description, msg.Channel.Description);
			Assert.AreEqual (c.ParentChannelId, msg.Channel.ParentChannelId);
			Assert.IsTrue (msg.Delete);
		}

		[Test]
		public void PopulateChannels()
		{
			PopulateChannels (this.manager, this.server);
		}

		public static void PopulateChannels (ClientChannelManager manager, IConnection server)
		{
			Channel c1 = new Channel (1)
			{
				Name = "Channel 1",
				Description = "Description 1"
			};

			Channel sc1 = new Channel (2)
			{
				Name = "SubChannel 1",
				Description = "Description 2",
				ParentChannelId = c1.ChannelId
			};

			Channel c2 = new Channel (3)
			{
				Name = "Channel 2",
				Description = "Description 3"
			};

			manager.OnChannelListReceivedMessage (new MessageReceivedEventArgs (server,
				new ChannelListMessage (new[] { c1, sc1, c2 })));

			Assert.AreEqual (3, manager.Count ());
			Assert.AreEqual (1, manager.Count (c => c.ChannelId == c1.ChannelId
				&& c.Name == c1.Name
				&& c.Description == c1.Description
				&& c.ParentChannelId == c1.ParentChannelId));

			Assert.AreEqual (1, manager.Count (c => c.ChannelId == sc1.ChannelId
				&& c.Name == sc1.Name
				&& c.Description == sc1.Description
				&& c.ParentChannelId == sc1.ParentChannelId));

			Assert.AreEqual (1, manager.Count (c => c.ChannelId == c2.ChannelId
				&& c.Name == c2.Name
				&& c.Description == c2.Description
				&& c.ParentChannelId == c2.ParentChannelId));
		}

		[Test]
		public void UpdateChannelFailed ()
		{
			Channel c1 = new Channel (1)
			{
				Name = "Channel 1",
				Description = "Description 1"
			};

			Channel sc1 = new Channel (2)
			{
				Name = "SubChannel 1",
				Description = "Description 2",
				ParentChannelId = c1.ChannelId
			};

			Channel c2 = new Channel (3)
			{
				Name = "Channel 2",
				Description = "Description 3"
			};

			manager.OnChannelListReceivedMessage (new MessageReceivedEventArgs (this.server,
				new ChannelListMessage (new[] { c1, sc1, c2 })));

			Channel updated = new Channel (1, c1) { Name = "Updated 1", Description = "U Description 1" };
			manager.OnChannelEditResultMessage (new MessageReceivedEventArgs (this.server,
				new ChannelEditResultMessage (updated, ChannelEditResult.FailedUnknown)));

			Assert.AreEqual (3, manager.Count ());
			Assert.AreEqual (0, manager.Count (c => c.ChannelId == c1.ChannelId
				&& c.Name == updated.Name && c.Description == updated.Description));
			
			Assert.AreEqual (1, manager.Count (c => c.ChannelId == c1.ChannelId
				&& c.Name == c1.Name && c.Description == c1.Description));
		}
	}
}