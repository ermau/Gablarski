using System;
using System.Linq;
using Gablarski.Messages;
using Gablarski.Server;
using NUnit.Framework;
using System.Net;

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
		private const string Username2 = "Bar2";
		private const string Password = "password";
		private const string Password2 = "password2";
		private const string Nickname = "Foo";
		private const string Nickname2 = "Foo2";

		private IAuthenticationProvider authentications;
		private IChannelProvider channels;
		private ServerSettings settings;
		private GuestPermissionProvider permissions;
		private GablarskiServer server;
		private MockConnectionProvider provider;

		private MockServerConnection Login (string username, string password)
		{
			MockServerConnection connection = Connect();

			connection.Client.Send (new LoginMessage { Username = username, Password = password });
			var loginResultMessage = connection.Client.DequeueAndAssertMessage<LoginResultMessage>();
			Assert.IsTrue (loginResultMessage.Result.Succeeded);

			connection.Client.DequeueAndAssertMessage<PermissionsMessage>();

			return connection;
		}

		private MockServerConnection Connect()
		{
			var connection = this.provider.EstablishConnection ();
			connection.Client.Send (new ConnectMessage { ProtocolVersion = GablarskiServer.ProtocolVersion, Host = "test", Port = 6112 });
			connection.Client.DequeueAndAssertMessage<ServerInfoMessage>();
			return connection;
		}

		private UserInfo Join (bool loggedIn, MockServerConnection connection, string nickname)
		{
			return Join (loggedIn, connection, nickname, null);
		}

		private UserInfo Join (bool loggedIn, MockServerConnection connection, string nickname, string serverPassword)
		{
			UserInfo user;
			connection.Client.Send (new JoinMessage (nickname, serverPassword));

			var joinResultMessage = connection.Client.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.Success, joinResultMessage.Result);

			if (!loggedIn)
			{
				connection.Client.DequeueAndAssertMessage<PermissionsMessage>();
			}

			var userJoinedMessage = connection.Client.DequeueAndAssertMessage<UserJoinedMessage>();
			user = userJoinedMessage.UserInfo;
			Assert.AreEqual (nickname, userJoinedMessage.UserInfo.Nickname);
			
			Assert.AreEqual (joinResultMessage.UserInfo.Username, userJoinedMessage.UserInfo.Username);
			Assert.AreEqual (joinResultMessage.UserInfo.UserId, userJoinedMessage.UserInfo.UserId);
			Assert.AreEqual (joinResultMessage.UserInfo.CurrentChannelId, userJoinedMessage.UserInfo.CurrentChannelId);

			connection.Client.DequeueAndAssertMessage<ChannelListMessage>();
			var usermsg = connection.Client.DequeueAndAssertMessage<UserListMessage>();
			Assert.IsNotNull (usermsg.Users.FirstOrDefault (u => u.UserId == userJoinedMessage.UserInfo.UserId));
			connection.Client.DequeueAndAssertMessage<SourceListMessage>();

			return user;
		}

		[Test]
		public void OldVersionReject ()
		{
			var connection = provider.EstablishConnection ();
			connection.Client.Send (new ConnectMessage { ProtocolVersion = 0 });

			MessageBase message = connection.Client.DequeueMessage ();
			Assert.IsInstanceOf<ConnectionRejectedMessage> (message);
			var rejected = (ConnectionRejectedMessage)message;

			Assert.AreEqual (ConnectionRejectedReason.IncompatibleVersion, rejected.Reason);
		}

		[Test]
		public void ServerInfo()
		{
			var connection = provider.EstablishConnection ();
			connection.Client.Send (new ConnectMessage { ProtocolVersion = GablarskiServer.ProtocolVersion });

			var msg = connection.Client.DequeueAndAssertMessage<ServerInfoMessage>();
			Assert.AreEqual (this.settings.Name, msg.ServerInfo.Name);
			Assert.AreEqual (this.settings.Description, msg.ServerInfo.Description);
			Assert.AreEqual (String.Empty, msg.ServerInfo.Logo);
		}
		
		[Test]
		public void RedirectMatch()
		{
			server.AddRedirector (new MockRedirector ("monkeys.com", new IPEndPoint (IPAddress.Any, 6113)));
			
			var connection = provider.EstablishConnection();
			connection.Client.Send (new ConnectMessage { ProtocolVersion = GablarskiServer.ProtocolVersion,
				Host = "monkeys.com", Port = 6112 });
				
			var msg = connection.Client.DequeueAndAssertMessage<RedirectMessage>();
			Assert.AreEqual (IPAddress.Any.ToString(), msg.Host);
			Assert.AreEqual (6113, msg.Port);
		}

		[Test]
		public void RedirectNoMatch()
		{
			server.AddRedirector (new MockRedirector ("monkeys.com", new IPEndPoint (IPAddress.Any, 6113)));
			
			var connection = provider.EstablishConnection();
			connection.Client.Send (new ConnectMessage { ProtocolVersion = GablarskiServer.ProtocolVersion,
				Host = "monkeys2.com", Port = 6112 });

			var msg = connection.Client.DequeueAndAssertMessage<ServerInfoMessage>();
			Assert.AreEqual (this.settings.Name, msg.ServerInfo.Name);
			Assert.AreEqual (this.settings.Description, msg.ServerInfo.Description);
			Assert.AreEqual (String.Empty, msg.ServerInfo.Logo);
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
			connection.Client.Send (new JoinMessage { Nickname = String.Empty });

			var join = connection.Client.DequeueAndAssertMessage<JoinResultMessage>();

			Assert.AreEqual (LoginResultState.FailedInvalidNickname, join.Result);
		}

		[Test]
		public void NicknameInUse ()
		{
			var c = Login (Username, Password);
			Join (true, c, Nickname);

			var connection = provider.EstablishConnection();
			connection.Client.Send (new JoinMessage (Nickname, null));

			var join = connection.Client.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.FailedNicknameInUse, join.Result);
		}

		[Test]
		public void ServerPassword()
		{
			this.server.Settings.ServerPassword = "foo";

			var connection = provider.EstablishConnection();
			Join (false, connection, Nickname, "foo");
		}

		[Test]
		public void ServerPasswordLoggedIn()
		{
			this.server.Settings.ServerPassword = "foo";

			var c = Login (Username, Password);
			Join (true, c, Nickname, "foo");
		}

		[Test]
		public void BadServerPassword()
		{
			this.server.Settings.ServerPassword = "foo";

			var connection = provider.EstablishConnection();
			connection.Client.Send (new JoinMessage (Nickname, null));

			var join = connection.Client.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.FailedServerPassword, join.Result);
		}

		[Test]
		public void BadServerPasswordLoggedIn()
		{
			this.server.Settings.ServerPassword = "foo";

			var c = Login (Username, Password);
			c.Client.Send (new JoinMessage (Nickname, null));

			var join = c.Client.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.FailedServerPassword, join.Result);
		}

		[Test]
		public void UserDisconnected()
		{
			UserInfo foo;
			var fooc = Login (Username, Password);
			foo = Join (true, fooc, Nickname);

			var barc = Login (Username2, Password2);

			fooc.Disconnect();

			var msg = barc.Client.DequeueAndAssertMessage<UserDisconnectedMessage>();
			Assert.AreEqual (foo.UserId, msg.UserId);
		}

		[Test]
		public void RequestSource()
		{
			var c = Connect();
			var u = Join (false, c, Nickname);

			c.Client.Send (new RequestSourceMessage ("source", 1, 64000, 512));

			var msg = c.Client.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, msg.SourceResult);
			Assert.AreEqual ("source", msg.Source.Name);
			Assert.AreEqual (1, msg.Source.Channels);
			Assert.AreEqual (64000, msg.Source.Bitrate);
			Assert.AreEqual (512, msg.Source.FrameSize);
			Assert.AreEqual (false, msg.Source.IsMuted);
		}

		//[Test]
		//public void SourceStateChangeSelf()
		//{
		//    var c = Connect();
		//    var u = Join(false, c, Nickname);

		//    c.Client.Send(new RequestSourceMessage ("source", 1, 64000, 512));
		//    c.Client.DequeueAndAssertMessage<SourceResultMessage>();

		//    c.Client.Send (new ClientAudioSourceStateChangeMessage { Starting = true, SourceId = 1 });

		//    //var msg = c.Client.DequeueAndAssertMessage<AudioSourceStateChangeMessage>();
		//    //Assert.AreEqual (true, msg.Starting);
		//    //Assert.AreEqual (1, msg.SourceId);
		//}
	}
}