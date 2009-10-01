using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Gablarski.Audio;
using Gablarski.Messages;
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

			var msg = new ChannelListMessage (channels);
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
		public void EmptyChannelList()
		{
			var msg = new ChannelListMessage (new List<ChannelInfo>());
			Assert.AreEqual (0, msg.Channels.Count());
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new ChannelListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (0, msg.Channels.Count ());
		}

		[Test]
		public void UserList()
		{
			List<UserInfo> users = new List<UserInfo>
			{
				new UserInfo (Nickname, Nickname, UserId, ChannelId, false),
				new UserInfo (Nickname2, Nickname2, UserId2, ChannelId2, true)
			};

			var msg = new UserListMessage (users);
			Assert.AreEqual (1, msg.Users.Count (ui => ui.UserId.Equals (users[0].UserId) && ui.CurrentChannelId.Equals (users[0].CurrentChannelId) && ui.Nickname == users[0].Nickname && ui.IsMuted == users[0].IsMuted));
			Assert.AreEqual (1, msg.Users.Count (ui => ui.UserId.Equals (users[1].UserId) && ui.CurrentChannelId.Equals (users[1].CurrentChannelId) && ui.Nickname == users[1].Nickname && ui.IsMuted == users[1].IsMuted));
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (1, msg.Users.Count (ui => ui.UserId.Equals (users[0].UserId) && ui.CurrentChannelId.Equals (users[0].CurrentChannelId) && ui.Nickname == users[0].Nickname && ui.IsMuted == users[0].IsMuted));
			Assert.AreEqual (1, msg.Users.Count (ui => ui.UserId.Equals (users[1].UserId) && ui.CurrentChannelId.Equals (users[1].CurrentChannelId) && ui.Nickname == users[1].Nickname && ui.IsMuted == users[1].IsMuted));
		}

		[Test]
		public void EmptyUserList()
		{
			var msg = new UserListMessage(new List<UserInfo>());
			Assert.AreEqual (0, msg.Users.Count());
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (0, msg.Users.Count ());
		}

		[Test]
		public void SourceList()
		{
			var sources = new List<AudioSource>
			{
				new AudioSource ("voice", 1, UserId, 1, 64000, 44100, 256, 10, false),
				new AudioSource ("voice", 2, UserId2, 2, 128000, 48000, 512, 10, false)
			};

			var msg = new SourceListMessage (sources);
			Assert.AreEqual (1, msg.Sources.Count (s => s.Id == sources[0].Id && s.OwnerId.Equals (sources[0].OwnerId) && s.Bitrate == sources[0].Bitrate));
			Assert.AreEqual (1, msg.Sources.Count (s => s.Id == sources[1].Id && s.OwnerId.Equals (sources[1].OwnerId) && s.Bitrate == sources[1].Bitrate));

			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new SourceListMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (1, msg.Sources.Count (s => s.Id == sources[0].Id && s.OwnerId.Equals (sources[0].OwnerId) && s.Bitrate == sources[0].Bitrate));
			Assert.AreEqual (1, msg.Sources.Count (s => s.Id == sources[1].Id && s.OwnerId.Equals (sources[1].OwnerId) && s.Bitrate == sources[1].Bitrate));
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
			var info = new UserInfo (Nickname, Nickname, UserId, ChannelId, false);
			var msg = new UserJoinedMessage (info);
			msg.WritePayload (writer);
			long length = stream.Position;
			stream.Position = 0;

			msg = new UserJoinedMessage();
			msg.ReadPayload (reader);
			Assert.AreEqual (length, stream.Position);
			Assert.AreEqual (info.UserId, msg.UserInfo.UserId);
			Assert.AreEqual (info.Nickname, msg.UserInfo.Nickname);
			Assert.AreEqual (info.CurrentChannelId, msg.UserInfo.CurrentChannelId);
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
				new AudioSource ("voice", 1, UserId, 1, 64000, 44100, 256, 10, false),
				new AudioSource ("voice", 2, UserId2, 2, 128000, 48000, 512, 10, false)
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
	}
}