using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Gablarski.Server;
using Gablarski.Client;
using Gablarski.Messages;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerTests
	{
		[SetUp]
		public void ServerTestInitialize ()
		{
			this.permissions = new GuestPermissionProvider ();
			this.server = new GablarskiServer (new ServerSettings { Name = "Test Server", Description = "Test Server" }, new GuestUserProvider (), this.permissions, new LobbyChannelProvider());
			this.server.AddConnectionProvider (this.provider = new MockConnectionProvider ());
			this.server.Start ();
		}

		[TearDown]
		public void ServerTestCleanup ()
		{
			this.server.Shutdown ();
			this.server = null;
			this.provider = null;
		}

		private GuestPermissionProvider permissions;
		private GablarskiServer server;
		private MockConnectionProvider provider;

		[Test]
		public void TestOldVersionReject ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new ConnectMessage (new Version (0,0,0,1)));

			var msg = (connection.Client.DequeueMessage () as ConnectionRejectedMessage);
			Assert.IsNotNull (msg);
			Assert.AreEqual (ConnectionRejectedReason.IncompatibleVersion, msg.Reason);
		}

		[Test]
		public void TestRequestChannelList ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new RequestChannelListMessage ());

			var msg = (connection.Client.DequeueMessage () as ChannelListMessage);
			Assert.IsNotNull (msg);
			Assert.AreEqual (GenericResult.Success, msg.Result);
			Assert.IsNotNull (msg.Channels);
			Assert.IsTrue (msg.Channels.Count () > 0);
		}

		[Test]
		public void TestBadNickname ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new LoginMessage { Nickname = null, Username = null, Password = null });

			var msg = (connection.Client.DequeueMessage () as LoginResultMessage);
			Assert.IsFalse (msg.Result.Succeeded);
			Assert.AreEqual (LoginResultState.FailedInvalidNickname, msg.Result.ResultState);
		}

		[Test]
		public void TestGuestLogin ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new LoginMessage { Nickname = "Foo", Username = null, Password = null });

			var msg = (connection.Client.DequeueMessage () as LoginResultMessage);
			Assert.IsTrue (msg.Result.Succeeded);
			Assert.AreEqual ("Foo", msg.PlayerInfo.Nickname);
		}

		[Test]
		public void TestNicknameInUse ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new LoginMessage { Nickname = "Foo", Username = null, Password = null });
			var msg = (connection.Client.DequeueMessage () as LoginResultMessage);
			Assert.IsTrue (msg.Result.Succeeded);
			Assert.AreEqual ("Foo", msg.PlayerInfo.Nickname);

			connection.Client.Send (new LoginMessage { Nickname = "Foo", Username = null, Password = null });
			msg = (connection.Client.DequeueMessage () as LoginResultMessage);
			Assert.IsFalse (msg.Result.Succeeded);
			Assert.AreEqual (LoginResultState.FailedNicknameInUse, msg.Result.ResultState);
		}
	}
}