using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Client;
using Gablarski.Messages;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientUserManagerTests
	{
		[SetUp]
		public void ManagerSetup ()
		{
			this.provider = new MockConnectionProvider ();
			this.server = this.provider.EstablishConnection ();

			var context = new MockClientContext { Connection = this.server.Client };

			var channels = new ClientChannelManager (context);
			ClientChannelManagerTests.PopulateChannels (channels, this.server);

			this.manager = new ClientUserManager (context);
			context.Users = this.manager;
			context.Channels = channels;
		}

		[TearDown]
		public void ManagerTearDown ()
		{
			this.manager = null;
			this.provider = null;
		}

		private static void CreateUsers (IClientConnection client, ClientUserManager manager)
		{
			manager.OnUserListReceivedMessage (new MessageReceivedEventArgs (client,
			                                                                 new UserListMessage (new[]
			                                                                 {
			                                                                 	new UserInfo ("Foo", "Foo", 1, 1, false),
			                                                                 	new UserInfo ("Bar", "Bar", 2, 1, false),
			                                                                 	new UserInfo ("Wee", "Wee", 3, 2, true),
			                                                                 })));
			Assert.AreEqual (3, manager.Count());
			VerifyDefaultUsers (manager);
		}

		private static void VerifyDefaultUsers (IEnumerable<ClientUser> manager)
		{
			Assert.AreEqual (1, manager.Count (u => u.UserId == 1 && u.Nickname == "Foo" && !u.IsMuted && u.CurrentChannelId == 1));
			Assert.AreEqual (1, manager.Count (u => u.UserId == 2 && u.Nickname == "Bar" && !u.IsMuted && u.CurrentChannelId == 1));
			Assert.AreEqual (1, manager.Count (u => u.UserId == 3 && u.Nickname == "Wee" && u.IsMuted && u.CurrentChannelId == 2));
		}

		private MockServerConnection server;
		private MockConnectionProvider provider;
		private ClientUserManager manager;

		[Test]
		public void NullConnection()
		{
			Assert.Throws<ArgumentNullException> (() => new ClientUserManager (null));
		}

		[Test]
		public void Enumerator()
		{
			CreateUsers (this.server.Client, this.manager);
		}

		[Test]
		public void UserLoggedIn()
		{
			CreateUsers (this.server.Client, this.manager);

			var newGuy = new UserInfo ("New", "new", 4, 3, false);
			manager.OnUserLoggedInMessage (new MessageReceivedEventArgs (this.server.Client,
				new UserJoinedMessage(newGuy)));

			VerifyDefaultUsers (this.manager);
			Assert.AreEqual (1, this.manager.Count ( u => u.UserId == newGuy.UserId && u.Nickname == newGuy.Nickname && u.CurrentChannelId == newGuy.CurrentChannelId));
		}

		[Test]
		public void UserDisconnected()
		{
			CreateUsers (this.server.Client, this.manager);

			manager.OnUserDisconnectedMessage (new MessageReceivedEventArgs (this.server.Client,
				new UserDisconnectedMessage (1)));

			Assert.AreEqual (2, manager.Count());
			Assert.AreEqual (0, manager.Count (u => (int)u.UserId == 1 || u.Nickname == "Foo"));
			Assert.AreEqual (1, manager.Count (u => (int)u.UserId == 2 && u.Nickname == "Bar" && (int)u.CurrentChannelId == 1));
			Assert.AreEqual (1, manager.Count (u => (int)u.UserId == 3 && u.Nickname == "Wee" && (int)u.CurrentChannelId == 2));
		}

		[Test]
		public void ChannelUpdate()
		{
			CreateUsers (this.server.Client, this.manager);

			var old = manager[2];

			manager.OnUserChangedChannelMessage (new MessageReceivedEventArgs (this.server.Client,
				new UserChangedChannelMessage
				{
					ChangeInfo = new ChannelChangeInfo (2, 2)
				}));

			Assert.AreEqual (manager[2].UserId, 2);
			Assert.AreEqual (manager[2].Nickname, old.Nickname);
			Assert.AreEqual (manager[2].CurrentChannelId, 2);
		}

		[Test]
		public void IgnoreUser()
		{
			CreateUsers (this.server.Client, this.manager);

			var user = manager.First();
			int userId = user.UserId;
			
			Assert.IsFalse (user.IsIgnored);
			Assert.IsTrue (user.ToggleIgnore());
			Assert.IsTrue (manager[userId].IsIgnored);
		}

		[Test]
		public void IgnoreUserPersists()
		{
			CreateUsers (this.server.Client, this.manager);

			var user = manager.First();
			int userId = user.UserId;
			
			Assert.IsFalse (user.IsIgnored);
			Assert.IsTrue (user.ToggleIgnore());
			Assert.IsTrue (manager[userId].IsIgnored);

			CreateUsers (this.server.Client, this.manager);

			Assert.IsTrue (manager[userId].IsIgnored);
		}
	}
}