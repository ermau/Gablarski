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
			this.settings = new ServerSettings {Name = "Test Server", Description = "Test Server"};
			this.permissions = new GuestPermissionProvider ();
			this.channels = new LobbyChannelProvider();
			this.users = new GuestUserProvider();
			this.server = new GablarskiServer (this.settings, this.users, this.permissions, this.channels);
			this.server.AddConnectionProvider (this.provider = new MockConnectionProvider ());
			this.server.Start ();
		}

		[TearDown]
		public void ServerTestCleanup ()
		{
			this.server.Shutdown ();
			this.server = null;
			this.provider = null;
			this.users = null;
			this.channels = null;
			this.permissions = null;
			this.settings = null;
		}

		private IUserProvider users;
		private IChannelProvider channels;
		private ServerSettings settings;
		private GuestPermissionProvider permissions;
		private GablarskiServer server;
		private MockConnectionProvider provider;

		private MockServerConnection Login (string nickname)
		{
			MockServerConnection connection = provider.EstablishConnection ();
			connection.Client.Send (new ConnectMessage (GablarskiServer.MinimumApiVersion));
			connection.Client.DequeueAndAssertMessage<ServerInfoMessage>();

			connection.Client.Send (new LoginMessage { Nickname = nickname, Username = null, Password = null });
			var message = connection.Client.DequeueAndAssertMessage<LoginResultMessage>();

			Assert.IsTrue (message.Result.Succeeded);
			Assert.AreEqual (nickname, message.UserInfo.Nickname);

			var login = connection.Client.DequeueAndAssertMessage<UserLoggedInMessage>();
			Assert.AreEqual (nickname, login.UserInfo.Nickname);
			Assert.AreEqual (message.Result.UserId, login.UserInfo.UserId);
			Assert.AreEqual (message.UserInfo.UserId, login.UserInfo.UserId);
			Assert.AreEqual (message.UserInfo.CurrentChannelId, login.UserInfo.CurrentChannelId);

			connection.Client.DequeueAndAssertMessage<ChannelListMessage>();
			connection.Client.DequeueAndAssertMessage<UserListMessage>();
			connection.Client.DequeueAndAssertMessage<SourceListMessage>();

			return connection;
		}

		[Test]
		public void OldVersionReject ()
		{
			MockServerConnection connection = provider.EstablishConnection ();

			connection.Client.Send (new ConnectMessage (new Version (0,0,0,1)));

			MessageBase message = connection.Client.DequeueMessage ();
			Assert.IsInstanceOf<ConnectionRejectedMessage> (message);
			var rejected = (ConnectionRejectedMessage)message;

			Assert.AreEqual (ConnectionRejectedReason.IncompatibleVersion, rejected.Reason);
		}

		[Test]
		public void ServerInfo()
		{
			MockServerConnection connection = provider.EstablishConnection();

			connection.Client.Send (new ConnectMessage (GablarskiServer.MinimumApiVersion));

			var msg = connection.Client.DequeueAndAssertMessage<ServerInfoMessage>();
			Assert.AreEqual (this.settings.Name, msg.ServerInfo.ServerName);
			Assert.AreEqual (this.settings.Description, msg.ServerInfo.ServerDescription);
			Assert.AreEqual (this.channels.IdentifyingType, msg.ServerInfo.ChannelIdentifyingType);
			Assert.AreEqual (this.users.IdentifyingType, msg.ServerInfo.UserIdentifyingType);
		}

		[Test]
		public void RequestChannelList ()
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
		public void BadNickname ()
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
		public void GuestLogin ()
		{
			Login ("Foo");
		}

		[Test]
		public void NicknameInUse ()
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