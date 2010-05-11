using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Messages;
using Gablarski.Server;
using NUnit.Framework;

namespace Gablarski.Tests
{
	[TestFixture]
	public class ServerMessageTests
	{
		[SetUp]
		public void MessageSetup()
		{
			stream = new MemoryStream(new byte[20480], true);
			writer = new StreamValueWriter (stream);
			reader = new StreamValueReader (stream);
		}

		[TearDown]
		public void MessageTeardown()
		{
			writer.Dispose();
			reader.Dispose();
		}

		private MemoryStream stream;
		private IValueWriter writer;
		private IValueReader reader;

		private const string Nickname = "Foo";
		private const int UserId = 1;
		private const int ChannelId = 2;

		private const string Nickname2 = "Bar";
		private const int UserId2 = 2;
		private const int ChannelId2 = 3;

		private const bool Muted = true;

		[Test]
		public void LoginResult()
		{
			LoginResultState state = LoginResultState.Success;
			var msg = new LoginResultMessage (new LoginResult (UserId, state));

			Assert.IsTrue (msg.Result.Succeeded);
			Assert.AreEqual (state, msg.Result.ResultState);
			Assert.AreEqual (UserId, msg.Result.UserId);
		}

		[Test]
		public void JoinResult()
		{
			LoginResultState state = LoginResultState.Success;

			var msg = new JoinResultMessage (state, new UserInfo (Nickname, Nickname, UserId, ChannelId, true));

			Assert.AreEqual (state, msg.Result);
			Assert.AreEqual (UserId, msg.UserInfo.UserId);
			Assert.AreEqual (Nickname, msg.UserInfo.Nickname);
			Assert.AreEqual (ChannelId, msg.UserInfo.CurrentChannelId);
			Assert.AreEqual (Muted, msg.UserInfo.IsMuted);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new JoinResultMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (state, msg.Result);
			Assert.AreEqual (UserId, msg.UserInfo.UserId);
			Assert.AreEqual (Nickname, msg.UserInfo.Nickname);
			Assert.AreEqual (ChannelId, msg.UserInfo.CurrentChannelId);
			Assert.AreEqual (Muted, msg.UserInfo.IsMuted);
		}

		[Test]
		public void ChannelListFailed()
		{
			GenericResult result = GenericResult.FailedPermissions;
			var msg = new ChannelListMessage (result);
			Assert.AreEqual (result, msg.Result);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new ChannelListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (result, msg.Result);
		}

		[Test]
		public void ChannelList()
		{
			List<ChannelInfo> channels = new List<ChannelInfo>
			{
				new ChannelInfo (ChannelId) { ReadOnly = true },
				new ChannelInfo (ChannelId2)
			};

			var msg = new ChannelListMessage (channels, channels[0]);
			Assert.AreEqual(1, msg.Channels.Count (c => c.ChannelId.Equals (channels[0].ChannelId) && c.ReadOnly == channels[0].ReadOnly));
			Assert.AreEqual(1, msg.Channels.Count (c => c.ChannelId.Equals (channels[1].ChannelId) && c.ReadOnly == channels[1].ReadOnly));
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new ChannelListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (1, msg.Channels.Count (c => c.ChannelId.Equals (channels[0].ChannelId) && c.ReadOnly == channels[0].ReadOnly));
			Assert.AreEqual (1, msg.Channels.Count (c => c.ChannelId.Equals (channels[1].ChannelId) && c.ReadOnly == channels[1].ReadOnly));
		}

		[Test]
		public void UserList()
		{
			List<UserInfo> users = new List<UserInfo>
			{
				UserInfoTests.GetTestUser(1),
				UserInfoTests.GetTestUser(2)
			};

			var msg = new UserInfoListMessage (users);
			Assert.AreEqual (1, msg.Users.Count (ui => ui.UserId.Equals (users[0].UserId) && ui.CurrentChannelId.Equals (users[0].CurrentChannelId) && ui.Nickname == users[0].Nickname && ui.IsMuted == users[0].IsMuted));
			Assert.AreEqual (1, msg.Users.Count (ui => ui.UserId.Equals (users[1].UserId) && ui.CurrentChannelId.Equals (users[1].CurrentChannelId) && ui.Nickname == users[1].Nickname && ui.IsMuted == users[1].IsMuted));
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserInfoListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (1, msg.Users.Count (ui => ui.UserId.Equals (users[0].UserId) && ui.CurrentChannelId.Equals (users[0].CurrentChannelId) && ui.Nickname == users[0].Nickname && ui.IsMuted == users[0].IsMuted));
			Assert.AreEqual (1, msg.Users.Count (ui => ui.UserId.Equals (users[1].UserId) && ui.CurrentChannelId.Equals (users[1].CurrentChannelId) && ui.Nickname == users[1].Nickname && ui.IsMuted == users[1].IsMuted));
		}

		[Test]
		public void EmptyUserList()
		{
			var msg = new UserInfoListMessage(new List<UserInfo>());
			Assert.AreEqual (0, msg.Users.Count());
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserInfoListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (0, msg.Users.Count ());
		}

		[Test]
		public void SourceList()
		{
			var sources = new List<AudioSource>
			{
				new AudioSource ("voice", 1, UserId, AudioFormat.Mono16bitLPCM, 64000, 256, 10, false),
				new AudioSource ("voice", 2, UserId2, AudioFormat.Stereo16bitLPCM, 128000, 512, 10, false)
			};

			var msg = new SourceListMessage (sources);
			foreach (var s in msg.Sources)
				Assert.Contains (s, sources);

			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new SourceListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			foreach (var s in sources)
				Assert.Contains (s, msg.Sources.ToList());
		}

		[Test]
		public void EmptySourceList()
		{
			var msg = new SourceListMessage (new List<AudioSource>());
			Assert.AreEqual (0, msg.Sources.Count());
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new SourceListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (0, msg.Sources.Count());
		}

		[Test]
		public void UserLoggedIn()
		{
			var msg = new UserJoinedMessage (UserInfoTests.GetTestUser());
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserJoinedMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			UserInfoTests.AssertUserInfosMatch (UserInfoTests.GetTestUser(), msg.UserInfo);
		}

		[Test]
		public void UserDisconnected()
		{
			int id = 1;
			var msg = new UserDisconnectedMessage (id);
			Assert.AreEqual (id, msg.UserId);

			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserDisconnectedMessage();
			msg.ReadPayload (reader);

			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (id, msg.UserId);
		}

		[Test]
		public void SourcesRemoved()
		{
			var sources = new List<AudioSource>
			{
				new AudioSource ("voice", 1, UserId, AudioCodecArgsTests.GetTestArgs()),
				new AudioSource ("voice", 2, UserId2, AudioCodecArgsTests.GetTestArgs())
			};

			var msg = new SourcesRemovedMessage (sources);
			Assert.AreEqual (1, msg.SourceIds.Count (sid => sid == sources[0].Id));
			Assert.AreEqual (1, msg.SourceIds.Count (sid => sid == sources[1].Id));
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new SourcesRemovedMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (1, msg.SourceIds.Count (sid => sid == sources[0].Id));
			Assert.AreEqual (1, msg.SourceIds.Count (sid => sid == sources[1].Id));
		}

		[Test]
		public void UserJoined()
		{
			var user = new UserInfo ("Nickname", "Username", 1, 2, true);
			var msg = new UserJoinedMessage (user);
			Assert.AreEqual (user.UserId, msg.UserInfo.UserId);
			Assert.AreEqual (user.CurrentChannelId, msg.UserInfo.CurrentChannelId);
			Assert.AreEqual (user.Nickname, msg.UserInfo.Nickname);
			Assert.AreEqual (user.Username, msg.UserInfo.Username);
			Assert.AreEqual (user.IsMuted, msg.UserInfo.IsMuted);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserJoinedMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (user.UserId, msg.UserInfo.UserId);
			Assert.AreEqual (user.CurrentChannelId, msg.UserInfo.CurrentChannelId);
			Assert.AreEqual (user.Nickname, msg.UserInfo.Nickname);
			Assert.AreEqual (user.Username, msg.UserInfo.Username);
			Assert.AreEqual (user.IsMuted, msg.UserInfo.IsMuted);
		}

		[Test]
		public void UserChangedChannel()
		{
			var changeInfo = new ChannelChangeInfo (1, 2, 3);
			var msg = new UserChangedChannelMessage { ChangeInfo = changeInfo };
			Assert.AreEqual (changeInfo.TargetUserId, msg.ChangeInfo.TargetUserId);
			Assert.AreEqual (changeInfo.TargetChannelId, msg.ChangeInfo.TargetChannelId);
			Assert.AreEqual (changeInfo.RequestingUserId, msg.ChangeInfo.RequestingUserId);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserChangedChannelMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (changeInfo.TargetUserId, msg.ChangeInfo.TargetUserId);
			Assert.AreEqual (changeInfo.TargetChannelId, msg.ChangeInfo.TargetChannelId);
			Assert.AreEqual (changeInfo.RequestingUserId, msg.ChangeInfo.RequestingUserId);
		}

		[Test]
		public void UserChangedChannelWithoutRequesting()
		{
			var changeInfo = new ChannelChangeInfo (1, 2, 1);
			var msg = new UserChangedChannelMessage { ChangeInfo = changeInfo };
			Assert.AreEqual (changeInfo.TargetUserId, msg.ChangeInfo.TargetUserId);
			Assert.AreEqual (changeInfo.TargetChannelId, msg.ChangeInfo.TargetChannelId);
			Assert.AreEqual (changeInfo.PreviousChannelId, msg.ChangeInfo.PreviousChannelId);
			Assert.AreEqual (changeInfo.RequestingUserId, msg.ChangeInfo.RequestingUserId);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserChangedChannelMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (changeInfo.TargetUserId, msg.ChangeInfo.TargetUserId);
			Assert.AreEqual (changeInfo.TargetChannelId, msg.ChangeInfo.TargetChannelId);
			Assert.AreEqual (changeInfo.PreviousChannelId, msg.ChangeInfo.PreviousChannelId);
			Assert.AreEqual (changeInfo.RequestingUserId, msg.ChangeInfo.RequestingUserId);
		}

		[Test]
		public void SourceResult()
		{
			const string name = "Name";
			var result = Messages.SourceResult.Succeeded;
			var source = new AudioSource (name, 1, 2, AudioCodecArgsTests.GetTestArgs());
			var msg = new SourceResultMessage (name, result, source);
			Assert.AreEqual (result, msg.SourceResult);
			Assert.AreEqual (name, msg.SourceName);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new SourceResultMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (result, msg.SourceResult);
			Assert.AreEqual (name, msg.SourceName);
		}

		[Test]
		public void SourceResultWithoutSource ()
		{
			const string name = "Name";
			var result = Messages.SourceResult.FailedPermissions;
			var msg = new SourceResultMessage (name, result, null);
			Assert.AreEqual (result, msg.SourceResult);
			Assert.AreEqual (name, msg.SourceName);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new SourceResultMessage ();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (result, msg.SourceResult);
			Assert.AreEqual (name, msg.SourceName);
		}

		[Test]
		public void SourceResultInvalid ()
		{
			Assert.Throws<ArgumentNullException> (() => new SourceResultMessage (null, Messages.SourceResult.Succeeded, new AudioSource ("name", 1, 2, AudioCodecArgsTests.GetTestArgs())));
			Assert.Throws<ArgumentNullException> (() => new SourceResultMessage ("name", Messages.SourceResult.Succeeded, null));
			Assert.Throws<ArgumentNullException> (() => new SourceResultMessage ("name", Messages.SourceResult.SourceRemoved, null));
			Assert.Throws<ArgumentNullException> (() => new SourceResultMessage ("name", Messages.SourceResult.NewSource, null));
			Assert.DoesNotThrow (() => new SourceResultMessage ("name", Messages.SourceResult.FailedLimit, null));
		}

		[Test]
		public void ServerInfo()
		{
			var msg = new ServerInfoMessage (new ServerInfo (new ServerSettings
			{
				ServerLogo = "logo",
				Name = "name",
				Description = "description"
			}, new GuestUserProvider(), new Decryption().PublicParameters));
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new ServerInfoMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
		}

		[Test]
		public void UserUpdatedMessage()
		{
			var msg = new UserUpdatedMessage (UserInfoTests.GetTestUser());
			UserInfoTests.AssertUserInfosMatch (UserInfoTests.GetTestUser(), msg.User);

			msg = AssertLengthMatches (msg);
			UserInfoTests.AssertUserInfosMatch (UserInfoTests.GetTestUser(), msg.User);
		}

		[Test]
		public void UserListMessage()
		{
			var msg = new UserListMessage (new[] {new User (1, "One"), new User (2, "Two")});
			Assert.AreEqual (msg.Users.First().UserId, 1);
			Assert.AreEqual (msg.Users.Skip (1).First().UserId, 2);

			msg = AssertLengthMatches (msg);
			Assert.AreEqual (msg.Users.First().UserId, 1);
			Assert.AreEqual (msg.Users.Skip (1).First().UserId, 2);
		}

		private T AssertLengthMatches<T> (T msg)
			where T : MessageBase, new()
		{
			msg.WritePayload (writer);
			long len = stream.Position;
			stream.Position = 0;

			msg = new T();
			msg.ReadPayload (reader);
			Assert.AreEqual (len, stream.Position);

			return msg;
		}
	}
}