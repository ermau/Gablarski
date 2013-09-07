using System;
using System.Linq;
using Gablarski.Audio;
using Gablarski.Messages;
using Gablarski.Server;
using NUnit.Framework;
using System.Net;
using Tempest;
using Tempest.Tests;

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
			this.server.AddConnectionProvider (this.provider = new MockConnectionProvider (GablarskiProtocol.Instance));
			this.server.Start ();
		}

		[TearDown]
		public void ServerTestCleanup ()
		{
			this.server.Stop();
			this.server = null;
			this.provider = null;
			this.users = null;
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

		private IUserProvider users;
		private IChannelProvider channels;
		private ServerSettings settings;
		private GuestPermissionProvider permissions;
		private GablarskiServer server;
		private MockConnectionProvider provider;

		private ConnectionBuffer Login (string username, string password)
		{
			ConnectionBuffer connection = Connect();

			connection.SendAsync (new LoginMessage { Username = username, Password = password });
			var loginResultMessage = connection.DequeueAndAssertMessage<LoginResultMessage>();
			Assert.IsTrue (loginResultMessage.Result.Succeeded);

			connection.DequeueAndAssertMessage<PermissionsMessage>();

			return connection;
		}

		private ConnectionBuffer Connect()
		{
			var connection = new ConnectionBuffer (provider.GetClientConnection());
			connection.SendAsync (new ConnectMessage { ProtocolVersion = GablarskiProtocol.Instance.Version, Host = "test", Port = 42912 });
			connection.DequeueAndAssertMessage<ServerInfoMessage>();
			connection.DequeueAndAssertMessage<ChannelListMessage>();
			connection.DequeueAndAssertMessage<UserInfoListMessage>();
			connection.DequeueAndAssertMessage<SourceListMessage>();
			return connection;
		}

		private IUserInfo Join (bool loggedIn, ConnectionBuffer connection, string nickname)
		{
			return Join (loggedIn, connection, nickname, null);
		}

		private IUserInfo Join (bool loggedIn, ConnectionBuffer connection, string nickname, string serverPassword)
		{
			connection.SendAsync (new JoinMessage (nickname, serverPassword));

			var joinResultMessage = connection.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.Success, joinResultMessage.Result);

			if (!loggedIn)
			{
				connection.DequeueAndAssertMessage<PermissionsMessage>();
			}

			var userJoinedMessage = connection.DequeueAndAssertMessage<UserJoinedMessage>();
			IUserInfo user = userJoinedMessage.UserInfo;
			Assert.AreEqual (nickname, userJoinedMessage.UserInfo.Nickname);
			
			Assert.AreEqual (joinResultMessage.UserInfo.Username, userJoinedMessage.UserInfo.Username);
			Assert.AreEqual (joinResultMessage.UserInfo.UserId, userJoinedMessage.UserInfo.UserId);
			Assert.AreEqual (joinResultMessage.UserInfo.CurrentChannelId, userJoinedMessage.UserInfo.CurrentChannelId);

			//connection.Client.DequeueAndAssertMessage<ChannelListMessage>();
			//var usermsg = connection.Client.DequeueAndAssertMessage<UserInfoListMessage>();
			//Assert.IsNotNull (usermsg.Users.FirstOrDefault (u => u.UserId == userJoinedMessage.UserInfo.UserId));
			//connection.Client.DequeueAndAssertMessage<SourceListMessage>();

			return user;
		}

		//[Test]
		//public void IPBanned()
		//{
		//	this.users.AddBan (new BanInfo ("192.168.1.2", null, TimeSpan.Zero));

		//	var connection = provider.EstablishConnection (IPAddress.Parse ("192.168.1.2"));
		//	Assert.IsNull (connection);
		//}

		//[Test]
		//public void IPMaskBanned()
		//{
		//	this.users.AddBan (new BanInfo ("192.168.*.*", null, TimeSpan.Zero));

		//	var connection = provider.EstablishConnection (IPAddress.Parse ("192.168.1.2"));
		//	Assert.IsNull (connection);
		//}

		[Test]
		public void OldVersionReject ()
		{
			var connection = new ConnectionBuffer (provider.GetClientConnection());
			connection.SendAsync (new ConnectMessage { ProtocolVersion = 0 });

			Message message = connection.DequeueMessage ();
			Assert.IsInstanceOf<ConnectionRejectedMessage> (message);
			var rejected = (ConnectionRejectedMessage)message;

			Assert.AreEqual (ConnectionRejectedReason.IncompatibleVersion, rejected.Reason);
		}

		[Test]
		public void ServerInfo()
		{
			var connection =new ConnectionBuffer (provider.GetClientConnection());
			connection.SendAsync (new ConnectMessage { ProtocolVersion = GablarskiProtocol.Instance.Version });

			var msg = connection.DequeueAndAssertMessage<ServerInfoMessage>();
			Assert.AreEqual (this.settings.Name, msg.ServerInfo.Name);
			Assert.AreEqual (this.settings.Description, msg.ServerInfo.Description);
			Assert.AreEqual (String.Empty, msg.ServerInfo.Logo);
		}
		
		[Test]
		public void RedirectMatch()
		{
			server.AddRedirector (new MockRedirector ("monkeys.com", new IPEndPoint (IPAddress.Any, 6113)));
			
			var connection = new ConnectionBuffer (provider.GetClientConnection());
			connection.SendAsync (new ConnectMessage { ProtocolVersion = GablarskiProtocol.Instance.Version,
				Host = "monkeys.com", Port = 42912 });
				
			var msg = connection.DequeueAndAssertMessage<RedirectMessage>();
			Assert.AreEqual (IPAddress.Any.ToString(), msg.Host);
			Assert.AreEqual (6113, msg.Port);
		}

		[Test]
		public void RedirectNoMatch()
		{
			server.AddRedirector (new MockRedirector ("monkeys.com", new IPEndPoint (IPAddress.Any, 6113)));
			
			var connection = new ConnectionBuffer (provider.GetClientConnection());
			connection.SendAsync (new ConnectMessage { ProtocolVersion = GablarskiProtocol.Instance.Version,
				Host = "monkeys2.com", Port = 42912 });

			var msg = connection.DequeueAndAssertMessage<ServerInfoMessage>();
			Assert.AreEqual (this.settings.Name, msg.ServerInfo.Name);
			Assert.AreEqual (this.settings.Description, msg.ServerInfo.Description);
			Assert.AreEqual (String.Empty, msg.ServerInfo.Logo);
		}

		[Test]
		public void RequestChannelList ()
		{
			var connection = Connect();
			connection.SendAsync (new RequestChannelListMessage ());

			Message message = connection.DequeueMessage ();
			Assert.IsInstanceOf<ChannelListMessage> (message);
			var list = (ChannelListMessage)message;

			Assert.AreEqual (GenericResult.Success, list.Result);
			Assert.IsNotNull (list.Channels);
			CollectionAssert.IsNotEmpty (list.Channels);
		}

		[Test]
		public void BadNickname ()
		{
			var connection = Connect();
			connection.SendAsync (new JoinMessage { Nickname = String.Empty });

			var join = connection.DequeueAndAssertMessage<JoinResultMessage>();

			Assert.AreEqual (LoginResultState.FailedInvalidNickname, join.Result);
		}

		[Test]
		public void NicknameInUse ()
		{
			var c = Login (Username, Password);
			Join (true, c, Nickname);

			var connection = Connect();
			connection.SendAsync (new JoinMessage (Nickname, null));

			var join = connection.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.FailedNicknameInUse, join.Result);
		}

		[Test]
		public void ServerPassword()
		{
			this.server.Settings.ServerPassword = "foo";

			var connection = Connect();
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

			var connection = new ConnectionBuffer (provider.GetClientConnection());;
			connection.SendAsync (new JoinMessage (Nickname, null));

			var join = connection.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.FailedServerPassword, join.Result);
		}

		[Test]
		public void BadServerPasswordLoggedIn()
		{
			this.server.Settings.ServerPassword = "foo";

			var c = Login (Username, Password);
			c.SendAsync (new JoinMessage (Nickname, null));

			var join = c.DequeueAndAssertMessage<JoinResultMessage>();
			Assert.AreEqual (LoginResultState.FailedServerPassword, join.Result);
		}

		[Test]
		public void UserDisconnected()
		{
			var fooc = Login (Username, Password);
			IUserInfo foo = Join (true, fooc, Nickname);

			var barc = Login (Username2, Password2);

			fooc.DisconnectAsync().Wait();

			barc.DequeueAndAssertMessage<SourcesRemovedMessage>();

			var msg = barc.DequeueAndAssertMessage<UserDisconnectedMessage>();
			Assert.AreEqual (foo.UserId, msg.UserId);
		}

		[Test]
		public void RequestSource()
		{
			var c = Connect();
			var u = Join (false, c, Nickname);

			var args = AudioCodecArgsTests.GetTestArgs();
			c.SendAsync (new RequestSourceMessage ("source", args));

			var msg = c.DequeueAndAssertMessage<SourceResultMessage>();
			Assert.AreEqual (SourceResult.Succeeded, msg.SourceResult);
			Assert.AreEqual ("source", msg.Source.Name);
			Assert.AreEqual (false, msg.Source.IsMuted);
			AudioCodecArgsTests.AssertAreEqual (args, msg.Source);
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

		[Test]
		public void SourceStateChangeSameChannel()
		{
		    var c = Connect();
		    var u = Join (false, c, Nickname);

		    c.SendAsync (new RequestSourceMessage ("source", AudioCodecArgsTests.GetTestArgs()));
		    c.DequeueAndAssertMessage<SourceResultMessage>();

		    c.SendAsync (new ClientAudioSourceStateChangeMessage { Starting = true, SourceId = 1 });

		    //var msg = c.Client.DequeueAndAssertMessage<AudioSourceStateChangeMessage>();
		    //Assert.AreEqual (true, msg.Starting);
		    //Assert.AreEqual (1, msg.SourceId);
		}
	}
}