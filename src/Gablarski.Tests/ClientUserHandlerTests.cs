using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gablarski.Client;
using Gablarski.Messages;
using Gablarski.Server;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ClientUserHandlerTests
	{
		[SetUp]
		public void ManagerSetup ()
		{
			this.provider = new MockConnectionProvider ();
			this.server = this.provider.EstablishConnection ();

			userProvider = new MockUserProvider();
			context = new MockClientContext { Connection = this.server.Client, ServerInfo = new ServerInfo (new ServerSettings(), userProvider, new PublicRSAParameters()) };

			var channels = new ClientChannelManager (context);
			ClientChannelManagerTests.PopulateChannels (channels, this.server);

			this.userManager = new ClientUserManager();
			this.handler = new ClientUserHandler (context, userManager);
			context.Users = this.handler;
			context.Channels = channels;
		}

		[TearDown]
		public void ManagerTearDown ()
		{
			this.userProvider = null;
			this.server = null;
			this.handler = null;
			this.userManager = null;
			this.provider = null;
			this.context = null;
		}

		private static void CreateUsers (IClientConnection client, ClientUserHandler handler)
		{
			handler.OnUserListReceivedMessage (new MessageReceivedEventArgs (client,
			                                                                 new UserInfoListMessage (new[]
			                                                                 {
			                                                                 	new UserInfo ("Foo", "Foo", 1, 1, false),
			                                                                 	new UserInfo ("Bar", "Bar", 2, 1, false),
			                                                                 	new UserInfo ("Wee", "Wee", 3, 2, true),
			                                                                 })));
			Assert.AreEqual (3, handler.Count());
			VerifyDefaultUsers (handler);
		}

		private static void VerifyDefaultUsers (IEnumerable<IUserInfo> manager)
		{
			Assert.AreEqual (1, manager.Count (u => u.UserId == 1 && u.Nickname == "Foo" && !u.IsMuted && u.CurrentChannelId == 1));
			Assert.AreEqual (1, manager.Count (u => u.UserId == 2 && u.Nickname == "Bar" && !u.IsMuted && u.CurrentChannelId == 1));
			Assert.AreEqual (1, manager.Count (u => u.UserId == 3 && u.Nickname == "Wee" && u.IsMuted && u.CurrentChannelId == 2));
		}

		private MockServerConnection server;
		private MockConnectionProvider provider;
		private ClientUserHandler handler;
		private MockClientContext context;
		private MockUserProvider userProvider;
		private ClientUserManager userManager;

		[Test]
		public void NullConnection()
		{
			Assert.Throws<ArgumentNullException> (() => new ClientUserHandler (null, new ClientUserManager()));
			Assert.Throws<ArgumentNullException> (() => new ClientUserHandler (new MockClientContext(), null));
		}

		[Test]
		public void Enumerator()
		{
			CreateUsers (this.server.Client, this.handler);
		}

		[Test]
		public void UserJoined()
		{
			CreateUsers (this.server.Client, this.handler);

			var newGuy = new UserInfo ("New", "new", 4, 3, false);
			this.handler.OnUserJoinedMessage (new MessageReceivedEventArgs (this.server.Client,
				new UserJoinedMessage(newGuy)));

			VerifyDefaultUsers (this.handler);
			Assert.AreEqual (1, this.handler.Count ( u => u.UserId == newGuy.UserId && u.Nickname == newGuy.Nickname && u.CurrentChannelId == newGuy.CurrentChannelId));
		}

		[Test]
		public void UserDisconnected()
		{
			CreateUsers (this.server.Client, this.handler);

			this.handler.OnUserDisconnectedMessage (new MessageReceivedEventArgs (this.server.Client,
				new UserDisconnectedMessage (1)));

			Assert.AreEqual (2, this.handler.Count());
			Assert.AreEqual (0, this.handler.Count (u => (int)u.UserId == 1 || u.Nickname == "Foo"));
			Assert.AreEqual (1, this.handler.Count (u => (int)u.UserId == 2 && u.Nickname == "Bar" && (int)u.CurrentChannelId == 1));
			Assert.AreEqual (1, this.handler.Count (u => (int)u.UserId == 3 && u.Nickname == "Wee" && (int)u.CurrentChannelId == 2));
		}

		[Test]
		public void ChannelUpdate()
		{
			CreateUsers (this.server.Client, this.handler);

			var old = this.handler[2];

			this.handler.OnUserChangedChannelMessage (new MessageReceivedEventArgs (this.server.Client,
				new UserChangedChannelMessage
				{
					ChangeInfo = new ChannelChangeInfo (2, 2, 1)
				}));

			Assert.AreEqual (this.handler[2].UserId, 2);
			Assert.AreEqual (this.handler[2].Nickname, old.Nickname);
			Assert.AreEqual (this.handler[2].CurrentChannelId, 2);
		}

		[Test]
		public void IgnoreUser()
		{
			CreateUsers (this.server.Client, this.handler);

			var user = this.handler.First();
			int userId = user.UserId;
			
			Assert.IsFalse (this.handler.GetIsIgnored (user));
			Assert.IsTrue (this.handler.ToggleIgnore (user));
			Assert.IsTrue (this.handler.GetIsIgnored (user));
		}

		[Test]
		public void IgnoreUserPersists ()
		{
			CreateUsers (this.server.Client, this.handler);

			var user = this.handler.First ();
			int userId = user.UserId;

			Assert.IsFalse (this.handler.GetIsIgnored (user));
			Assert.IsTrue (this.handler.ToggleIgnore (user));
			Assert.IsTrue (this.handler.GetIsIgnored (user));

			CreateUsers (this.server.Client, this.handler);

			Assert.IsTrue (this.handler.GetIsIgnored (user));
		}

		[Test]
		public void ApproveRegistrationNull()
		{
			Assert.Throws<ArgumentNullException> (() => this.handler.ApproveRegistration ((string)null));
			Assert.Throws<ArgumentNullException> (() => this.handler.ApproveRegistration ((IUserInfo)null));
		}

		[Test]
		public void PreApproveRegistration()
		{
			userProvider.UpdateSupported = true;
			userProvider.RegistrationMode = UserRegistrationMode.PreApproved;
			context = new MockClientContext { Connection = this.server.Client, ServerInfo = new ServerInfo (new ServerSettings(), userProvider, new PublicRSAParameters()) };
			this.handler = new ClientUserHandler (context, this.userManager);

			CreateUsers (this.server.Client, this.handler);

			var user = this.handler.First();
			int userId = user.UserId;

			this.handler.ApproveRegistration (user);

			var msg = this.server.DequeueAndAssertMessage<RegistrationApprovalMessage>();
			Assert.AreEqual (userId, msg.UserId);
		}

		//[Test]
		//public void ApproveRegistrationUnsupported()
		//{
			
		//}

		[Test]
		public void RejectRegistrationNull()
		{
			Assert.Throws<ArgumentNullException> (() => this.handler.RejectRegistration (null));
		}
	}
}