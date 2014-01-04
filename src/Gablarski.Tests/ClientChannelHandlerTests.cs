using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Gablarski.Client;
using Gablarski.Messages;
using Tempest;
using Tempest.Tests;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientChannelHandlerTests
	{
		[SetUp]
		public void ManagerSetup ()
		{
			this.provider = new MockConnectionProvider (GablarskiProtocol.Instance);
			this.provider.Start (MessageTypes.All);

			var cs = this.provider.GetConnections (GablarskiProtocol.Instance);

			this.server = new ConnectionBuffer (cs.Item2);
			this.handler = new ClientChannelHandler (new MockClientContext (cs.Item1));
		}

		[TearDown]
		public void ManagerTearDown ()
		{
			this.handler = null;
			this.provider = null;
		}

		private ConnectionBuffer server;
		private ClientChannelHandler handler;
		private MockConnectionProvider provider;

		[Test]
		public void NullConnection()
		{
			Assert.Throws<ArgumentNullException> (() => new ClientChannelHandler (null));
		}

		[Test]
		public void CreateInvalidChannel ()
		{
			Assert.Throws<ArgumentNullException> (() => this.handler.Create (null));
			Assert.Throws<ArgumentException>(() => this.handler.Create (new ChannelInfo (1)));
		}

		[Test]
		public void CreateChannel ()
		{
			ChannelInfo c = new ChannelInfo
			{
				Name = "Name",
				Description = "Description",
				ParentChannelId = 1
			};

			this.handler.Create (c);

			var msg = server.DequeueAndAssertMessage<ChannelEditMessage> ();
			Assert.AreEqual (c.Name, msg.Channel.Name);
			Assert.AreEqual (c.Description, msg.Channel.Description);
			Assert.AreEqual (c.ParentChannelId, msg.Channel.ParentChannelId);
		}

		[Test]
		public void UpdateInvalidChannel ()
		{
			Assert.Throws<ArgumentNullException> (() => this.handler.Update (null));
			Assert.Throws<ArgumentException>(() => this.handler.Update (new ChannelInfo ()));
		}

		[Test]
		public void UpdateChannel ()
		{
			ChannelInfo c = new ChannelInfo (2)
			{
				Name = "Name",
				Description = "Description",
				ParentChannelId = 1
			};

			this.handler.Update (c);

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
			Assert.Throws<ArgumentNullException> (() => this.handler.Delete (null));
			Assert.Throws<ArgumentException> (() => this.handler.Delete (new ChannelInfo ()));
		}

		[Test]
		public void DeleteChannel ()
		{
			ChannelInfo c = new ChannelInfo (2)
			{
				Name = "Name",
				Description = "Description",
				ParentChannelId = 1
			};

			this.handler.Delete (c);

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
			PopulateChannels (this.handler, this.server);
		}

		public static void PopulateChannels (ClientChannelHandler handler, IConnection server)
		{
			ChannelInfo c1 = new ChannelInfo (1)
			{
				Name = "Channel 1",
				Description = "Description 1"
			};

			ChannelInfo sc1 = new ChannelInfo (2)
			{
				Name = "SubChannel 1",
				Description = "Description 2",
				ParentChannelId = c1.ChannelId
			};

			ChannelInfo c2 = new ChannelInfo (3)
			{
				Name = "Channel 2",
				Description = "Description 3"
			};

			handler.OnChannelListReceivedMessage (new MessageEventArgs<ChannelListMessage> (server,
				new ChannelListMessage (new[] { c1, sc1, c2 }, sc1)));

			Assert.AreEqual (3, handler.Count ());
			Assert.AreEqual (1, handler.Count (c => c.ChannelId == c1.ChannelId
				&& c.Name == c1.Name
				&& c.Description == c1.Description
				&& c.ParentChannelId == c1.ParentChannelId));

			Assert.AreEqual (1, handler.Count (c => c.ChannelId == sc1.ChannelId
				&& c.Name == sc1.Name
				&& c.Description == sc1.Description
				&& c.ParentChannelId == sc1.ParentChannelId));

			Assert.AreEqual (1, handler.Count (c => c.ChannelId == c2.ChannelId
				&& c.Name == c2.Name
				&& c.Description == c2.Description
				&& c.ParentChannelId == c2.ParentChannelId));
		}

		[Test]
		public void UpdateChannelFailed ()
		{
			ChannelInfo c1 = new ChannelInfo (1)
			{
				Name = "Channel 1",
				Description = "Description 1"
			};

			ChannelInfo sc1 = new ChannelInfo (2)
			{
				Name = "SubChannel 1",
				Description = "Description 2",
				ParentChannelId = c1.ChannelId
			};

			ChannelInfo c2 = new ChannelInfo (3)
			{
				Name = "Channel 2",
				Description = "Description 3"
			};

			this.handler.OnChannelListReceivedMessage (new MessageEventArgs<ChannelListMessage> (this.server,
				new ChannelListMessage (new[] { c1, sc1, c2 }, sc1)));

			ChannelInfo updated = new ChannelInfo (1, c1) { Name = "Updated 1", Description = "U Description 1" };
			this.handler.OnChannelEditResultMessage (new ChannelEditResultMessage (updated, ChannelEditResult.FailedUnknown));

			Assert.AreEqual (3, this.handler.Count ());
			Assert.AreEqual (0, this.handler.Count (c => c.ChannelId == c1.ChannelId
				&& c.Name == updated.Name && c.Description == updated.Description));
			
			Assert.AreEqual (1, this.handler.Count (c => c.ChannelId == c1.ChannelId
				&& c.Name == c1.Name && c.Description == c1.Description));
		}
	}
}