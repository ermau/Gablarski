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
			this.authentications = new GuestAuthProvider();
			this.server = new GablarskiServer (this.settings, this.authentications, this.permissions, this.channels);
			this.server.AddConnectionProvider (this.provider = new MockConnectionProvider ());
			this.server.Start ();
		}

		[TearDown]
		public void ServerTestCleanup ()
		{
			this.server.Shutdown ();
			this.server = null;
			this.provider = null;
			this.authentications = null;
			this.channels = null;
			this.permissions = null;
			this.settings = null;
		}

		private const string Username = "Bar";
		private const string Nickname = "Foo";

		private IAuthenticationProvider authentications;
		private IChannelProvider channels;
		private ServerSettings settings;
		private GuestPermissionProvider permissions;
		private GablarskiServer server;
		private MockConnectionProvider provider;

		private MockServerConnection Login (string username, string nickname)
		{
			UserInfo user;
			return Login (username, nickname, out user);
		}

		private MockServerConnection Login (string nickname)
		{
			UserInfo user;
			return Login (null, nickname, out user);
		}

		private MockServerConnection Login (string nickname, out UserInfo user)
		{
			return Login (null, nickname, out user);
		}

		private MockServerConnection Login (string username, string nickname, out UserInfo user)
		{
			var connection = provider.EstablishConnection ();
			connection.Client.Send (new ConnectMessage (GablarskiServer.MinimumApiVersion));
			connection.Client.DequeueAndAssertMessage<ServerInfoMessage>();

			connection.Client.Send (new LoginMessage { Nickname = nickname, Username = username, Password = (username.IsEmpty()) ? null : "password" });
			var message = connection.Client.DequeueAndAssertMessage<LoginResultMessage>();

			Assert.IsTrue (message.Result.Succeeded);
			Assert.AreEqual (nickname, message.UserInfo.Nickname);
			Assert.AreEqual ((username.IsEmpty() ? nickname : username), message.UserInfo.Username);

			connection.Client.DequeueAndAssertMessage<PermissionsMessage>();

			var login = connection.Client.DequeueAndAssertMessage<UserLoggedInMessage>();
			user = login.UserInfo;
			Assert.AreEqual (nickname, login.UserInfo.Nickname);
			Assert.AreEqual (message.Result.UserId, login.UserInfo.UserId);
			Assert.AreEqual (message.UserInfo.Username, login.UserInfo.Username);
			Assert.AreEqual (message.UserInfo.UserId, login.UserInfo.UserId);
			Assert.AreEqual (message.UserInfo.CurrentChannelId, login.UserInfo.CurrentChannelId);

			connection.Client.DequeueAndAssertMessage<ChannelListMessage>();
			var usermsg = connection.Client.DequeueAndAssertMessage<UserListMessage>();
			Assert.IsNotNull (usermsg.Users.FirstOrDefault (u => u.UserId.Equals (login.UserInfo.UserId)));
			connection.Client.DequeueAndAssertMessage<SourceListMessage>();

			return connection;
		}

		[Test]
		public void OldVersionReject ()
		{
			var connection = provider.EstablishConnection ();
			connection.Client.Send (new ConnectMessage (new Version (0,0,0,1)));

			MessageBase message = connection.Client.DequeueMessage ();
			Assert.IsInstanceOf<ConnectionRejectedMessage> (message);
			var rejected = (ConnectionRejectedMessage)message;

			Assert.AreEqual (ConnectionRejectedReason.IncompatibleVersion, rejected.Reason);
		}

		[Test]
		public void ServerInfo()
		{
			var connection = provider.EstablishConnection ();
			connection.Client.Send (new ConnectMessage (GablarskiServer.MinimumApiVersion));

			var msg = connection.Client.DequeueAndAssertMessage<ServerInfoMessage>();
			Assert.AreEqual (this.settings.Name, msg.ServerInfo.ServerName);
			Assert.AreEqual (this.settings.Description, msg.ServerInfo.ServerDescription);
			Assert.AreEqual (String.Empty, msg.ServerInfo.ServerLogo);
		}

		[Test]
		public void RequestChannelList ()
		{
			var connection = provider.EstablishConnection ();
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
			var connection = provider.EstablishConnection ();
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
			Login (Nickname);
		}

		[Test]
		public void NicknameInUse ()
		{
			var connection = Login (Username, Nickname);

			connection.Client.Send (new LoginMessage { Nickname = "Foo", Username = null, Password = null });
			MessageBase message = connection.Client.DequeueMessage ();
			Assert.IsInstanceOf<LoginResultMessage> (message);

			var login = (LoginResultMessage)message;
			Assert.IsFalse (login.Result.Succeeded);
			Assert.AreEqual (LoginResultState.FailedNicknameInUse, login.Result.ResultState);
		}

		[Test]
		public void UserDisconnected()
		{
			UserInfo foo;
			var fooc = Login (Nickname, out foo);
			var barc = Login (Username);

			fooc.Disconnect();

			var msg = barc.Client.DequeueAndAssertMessage<UserDisconnectedMessage>();
			Assert.AreEqual (foo.UserId, msg.UserId);
		}
	}
}