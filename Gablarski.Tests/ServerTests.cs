using System;
using System.Linq;
using Gablarski.Messages;
using Gablarski.Server;
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

		private MockServerConnection Login (string nickname)
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new LoginMessage { Nickname = nickname, Username = null, Password = null });
			var message = connection.Client.DequeueMessage ();
			Assert.IsInstanceOf<LoginResultMessage> (message);
			var login = (LoginResultMessage)message;
			Assert.IsTrue (login.Result.Succeeded);
			Assert.AreEqual ("Foo", login.PlayerInfo.Nickname);

			Assert.IsInstanceOf<ServerInfoMessage> (connection.Client.DequeueMessage ());
			Assert.IsInstanceOf<ChannelListMessage> (connection.Client.DequeueMessage ());
			Assert.IsInstanceOf<PlayerListMessage> (connection.Client.DequeueMessage ());
			Assert.IsInstanceOf<SourceListMessage> (connection.Client.DequeueMessage ());

			return connection;
		}

		[Test]
		public void TestOldVersionReject ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new ConnectMessage (new Version (0,0,0,1)));

			MessageBase message = connection.Client.DequeueMessage ();
			Assert.IsInstanceOf<ConnectionRejectedMessage> (message);
			var rejected = (ConnectionRejectedMessage)message;

			Assert.AreEqual (ConnectionRejectedReason.IncompatibleVersion, rejected.Reason);
		}

		[Test]
		public void TestRequestChannelList ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new RequestChannelListMessage ());

			MessageBase message = connection.Client.DequeueMessage ();
			Assert.IsInstanceOf<ChannelListMessage> (message);
			var list = (ChannelListMessage)message;

			Assert.AreEqual (GenericResult.Success, list.Result);
			Assert.IsNotNull (list.Channels);
			Assert.IsTrue (list.Channels.Count () > 0);
		}

		[Test]
		public void TestBadNickname ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new LoginMessage { Nickname = null, Username = null, Password = null });

			MessageBase message = connection.Client.DequeueMessage ();
			Assert.IsInstanceOf<LoginResultMessage> (message);
			var login = (LoginResultMessage)message;

			Assert.IsFalse (login.Result.Succeeded);
			Assert.AreEqual (LoginResultState.FailedInvalidNickname, login.Result.ResultState);
		}

		[Test]
		public void TestGuestLogin ()
		{
			Login ("Foo");
		}

		[Test]
		public void TestNicknameInUse ()
		{
			var connection = Login ("Foo");

			connection.Client.Send (new LoginMessage { Nickname = "Foo", Username = null, Password = null });
			MessageBase message = connection.Client.DequeueMessage ();
			Assert.IsInstanceOf<LoginResultMessage> (message);

			var login = (LoginResultMessage)message;
			Assert.IsFalse (login.Result.Succeeded);
			Assert.AreEqual (LoginResultState.FailedNicknameInUse, login.Result.ResultState);
		}
	}
}